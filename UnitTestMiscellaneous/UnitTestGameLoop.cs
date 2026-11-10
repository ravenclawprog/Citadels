namespace UnitTestMiscellaneous;

using Citadel;
using Moq;

public class UnitTestGameLoop
{
    private sealed class TestController(Func<Game.PlayerInteractionEventArgs, PlayerAction> respond) : IController
    {
        public PlayerAction ChooseCharacter(Game.PlayerChooseCharacterEventArgs message) => respond(message);
        public PlayerAction TakeResources(Game.PlayerTakeResourcesEventArgs message) => respond(message);
        public PlayerAction KeepTownCard(Game.PlayerKeepTownCardEventArgs message) => respond(message);
        public PlayerAction ChooseTurnAction(Game.PlayerTurnActionEventArgs message) => respond(message);
    }

    [Theory]
    [InlineData(4)]
    [InlineData(7)]
    public void GamePollsEachPlayersOwnControllerWithoutConsole(int count)
    {
        var game = new Game(count, new Mock<IViewer>().Object);
        var calls = new Dictionary<int, int>();
        foreach (var player in game.Players)
        {
            calls.Add(player.GetId(), 0);
            player.SetController(new TestController(message =>
            {
                Assert.Equal(player.GetId(), message.PlayerId);
                calls[player.GetId()]++;
                if (message.RoundNumber == 2) return new(PlayerActionKind.Quit);
                switch (message)
                {
                    case Game.PlayerChooseCharacterEventArgs choose:
                        Assert.Equal(GamePhase.CharacterSelection, game.Phase);
                        Assert.NotEmpty(choose.Characters);
                        return new(PlayerActionKind.ChooseCharacter, choose.Characters[0].Rank);
                    case Game.PlayerTakeResourcesEventArgs resources:
                        Assert.Equal(GamePhase.PlayerTurns, game.Phase);
                        Assert.Equal(2, resources.Gold);
                        Assert.NotEmpty(resources.CharacterName);
                        return new(PlayerActionKind.TakeGold);
                    case Game.PlayerTurnActionEventArgs turn:
                        Assert.Equal(4, turn.Gold);
                        Assert.Equal(4, turn.Hand.Count);
                        Assert.True(turn.CanBuild);
                        return new(PlayerActionKind.EndTurn);
                    default: throw new Xunit.Sdk.XunitException("Unexpected message type.");
                }
            }));
        }

        game.Run();

        Assert.Equal(2, game.RoundNumber);
        Assert.Equal(GamePhase.CharacterSelection, game.Phase);
        Assert.All(calls.Values, value => Assert.InRange(value, 3, 4));
        Assert.All(game.Players, player => Assert.Equal(4, game.GetPlayerGold(player.GetId())));
    }

    [Fact]
    public void RejectedActionReturnsToSameControllerWithFreshState()
    {
        var game = new Game(4, new Mock<IViewer>().Object);
        int attempts = 0;
        int? active = null;
        var errors = new List<Exception>();
        foreach (var player in game.Players)
        {
            var viewer = new Mock<IViewer>();
            viewer.Setup(v => v.OnActionRejected(It.IsAny<Exception>())).Callback<Exception>(errors.Add);
            player.SetViewer(viewer.Object);
        }
        var messages = new List<Game.PlayerInteractionEventArgs>();
        var controller = new TestController(message =>
        {
            if (message is Game.PlayerChooseCharacterEventArgs choose)
                return new(PlayerActionKind.ChooseCharacter, choose.Characters[0].Rank);
            active ??= message.PlayerId;
            Assert.Equal(active, message.PlayerId);
            messages.Add(message);
            attempts++;
            return attempts switch
            {
                1 => new(PlayerActionKind.TakeGold),
                2 => new(PlayerActionKind.TakeGold),
                _ => new(PlayerActionKind.Quit)
            };
        });
        foreach (var player in game.Players) player.SetController(controller);

        game.Run();

        Assert.IsType<InvalidOperationException>(Assert.Single(errors));
        Assert.IsType<Game.PlayerTakeResourcesEventArgs>(messages[0]);
        var before = Assert.IsType<Game.PlayerTurnActionEventArgs>(messages[1]);
        var after = Assert.IsType<Game.PlayerTurnActionEventArgs>(messages[2]);
        Assert.NotSame(before, after);
        Assert.Equal(4, after.Gold);
        Assert.Equal(4, game.GetPlayerGold(active!.Value));
        Assert.True(game.HasTakenResources);
        Assert.Equal(active, game.CurrentPlayerId);
    }

    [Fact]
    public void QuitCanResumeSameSelection()
    {
        var game = new Game(4, new Mock<IViewer>().Object);
        var quit = new TestController(_ => new(PlayerActionKind.Quit));
        foreach (var player in game.Players) player.SetController(quit);
        game.Run();
        int id = game.CurrentChoosingPlayerId!.Value;
        int round = game.RoundNumber;
        game.Run();
        Assert.Equal(id, game.CurrentChoosingPlayerId);
        Assert.Equal(round, game.RoundNumber);
        Assert.False(game.CharactersSelected);
    }

    [Fact]
    public void QuitDuringCardChoicePreservesPendingCardsAndStopsPolling()
    {
        var game = new Game(4, new Mock<IViewer>().Object);
        Game.PlayerKeepTownCardEventArgs? pending = null;
        int quitMessages = 0;
        var controller = new TestController(message =>
        {
            switch (message)
            {
                case Game.PlayerChooseCharacterEventArgs choose:
                    return new(PlayerActionKind.ChooseCharacter, choose.Characters[0].Rank);
                case Game.PlayerTakeResourcesEventArgs:
                    return new(PlayerActionKind.DrawTownCards);
                case Game.PlayerKeepTownCardEventArgs keep:
                    Assert.Equal(1, ++quitMessages);
                    pending = keep;
                    return new(PlayerActionKind.Quit);
                default: throw new Xunit.Sdk.XunitException("Unexpected message after quitting.");
            }
        });
        foreach (var player in game.Players) player.SetController(controller);

        game.Run();

        Assert.NotNull(pending);
        Assert.Equal(1, quitMessages);
        Assert.Equal(GamePhase.PlayerTurns, game.Phase);
        Assert.Equal(pending.PlayerId, game.CurrentPlayerId);
        Assert.Equal(pending.Cards.Select(card => card.Name),
            game.GetDrawnTownCards(pending.PlayerId).Select(card => card.Name));
        Assert.Equal(4, game.GetPlayerHand(pending.PlayerId).Count);
        Assert.Equal(2, game.GetPlayerGold(pending.PlayerId));
        Assert.True(game.HasTakenResources);
    }

    [Fact]
    public void MissingControllerFailsInsteadOfRetryingForever()
    {
        var game = new Game(4, new Mock<IViewer>().Object);
        var error = Assert.Throws<InvalidOperationException>(() => game.Run());
        Assert.Contains("no controller", error.Message);
        Assert.Throws<InvalidOperationException>(() => new Game().Run());
    }

    [Fact]
    public void CardChoiceHasItsOwnMessageAndInvalidChoicePreservesCards()
    {
        var game = new Game(4, new Mock<IViewer>().Object);
        int? active = null;
        var choices = new List<Game.PlayerKeepTownCardEventArgs>();
        var errors = new List<Exception>();
        foreach (var player in game.Players)
        {
            var viewer = new Mock<IViewer>();
            viewer.Setup(v => v.OnActionRejected(It.IsAny<Exception>())).Callback<Exception>(errors.Add);
            player.SetViewer(viewer.Object);
        }
        var controller = new TestController(message =>
        {
            if (message is Game.PlayerChooseCharacterEventArgs choose)
                return new(PlayerActionKind.ChooseCharacter, choose.Characters[0].Rank);
            active ??= message.PlayerId;
            if (message.PlayerId != active) return new(PlayerActionKind.Quit);
            switch (message)
            {
                case Game.PlayerTakeResourcesEventArgs: return new(PlayerActionKind.DrawTownCards);
                case Game.PlayerKeepTownCardEventArgs keep:
                    choices.Add(keep);
                    Assert.Equal(2, keep.Cards.Count);
                    Assert.Equal(2, keep.Gold);
                    Assert.Throws<NotSupportedException>(() => ((IList<ITownCard>)keep.Cards).Clear());
                    return new(PlayerActionKind.KeepTownCard, choices.Count == 1 ? 99 : 1);
                case Game.PlayerTurnActionEventArgs turn:
                    Assert.Equal(5, turn.Hand.Count);
                    Assert.Equal(choices[0].Cards[1].Name, turn.Hand[^1].Name);
                    return new(PlayerActionKind.EndTurn);
                default: throw new Xunit.Sdk.XunitException("Unexpected message type.");
            }
        });
        foreach (var player in game.Players) player.SetController(controller);

        game.Run();

        Assert.Equal(2, choices.Count);
        Assert.NotSame(choices[0].Cards[0], choices[1].Cards[0]);
        Assert.Equal(choices[0].Cards.Select(card => card.Name), choices[1].Cards.Select(card => card.Name));
        Assert.IsType<ArgumentOutOfRangeException>(Assert.Single(errors));
        Assert.Equal(5, game.GetPlayerHand(active!.Value).Count);
        Assert.Equal(new BaseGamePrerequisite().GenerateTownCardDeck().Count - 17, game.RemainingTownCards);
    }

    [Fact]
    public void BuildingUpdatesNextTurnActionMessage()
    {
        var game = new Game(4, new Mock<IViewer>().Object);
        foreach (var player in game.Players)
            player.OnAddTownCards(null, new Game.PlayerAddTownCardsEventArgs
            {
                addTownCardDeck = [new TownCard(QuarterType.Commercial, 1, "Message district")]
            });
        int? active = null;
        var actions = new List<Game.PlayerTurnActionEventArgs>();
        var controller = new TestController(message =>
        {
            if (message is Game.PlayerChooseCharacterEventArgs choose)
                return new(PlayerActionKind.ChooseCharacter, choose.Characters[0].Rank);
            active ??= message.PlayerId;
            if (message.PlayerId != active) return new(PlayerActionKind.Quit);
            if (message is Game.PlayerTakeResourcesEventArgs) return new(PlayerActionKind.TakeGold);
            var turn = Assert.IsType<Game.PlayerTurnActionEventArgs>(message);
            actions.Add(turn);
            return turn.CanBuild ? new(PlayerActionKind.BuildQuarter, turn.Hand.Count - 1) : new(PlayerActionKind.EndTurn);
        });
        foreach (var player in game.Players) player.SetController(controller);

        game.Run();

        Assert.Equal(2, actions.Count);
        Assert.True(actions[0].CanBuild);
        Assert.False(actions[1].CanBuild);
        Assert.Equal(4, actions[0].Gold);
        Assert.Equal(3, actions[1].Gold);
        Assert.Equal(5, actions[0].Hand.Count);
        Assert.Equal(4, actions[1].Hand.Count);
        Assert.Empty(actions[0].City);
        Assert.Equal("Message district", Assert.Single(actions[1].City).Name);
    }
}
