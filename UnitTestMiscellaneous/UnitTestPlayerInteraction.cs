namespace UnitTestMiscellaneous;

using Citadel;
using Moq;

public class UnitTestPlayerInteraction
{
    private sealed class CallbackReader(Func<string?> read) : TextReader
    {
        public override string? ReadLine() => read();
    }

    [Fact]
    public void PlayerNotifiesChoicesOnceAndReportsInvalidInputIndependently()
    {
        var sequence = new List<string>();
        var viewer = new Mock<IViewer>();
        viewer.Setup(v => v.OnPlayerCardToChoose(It.IsAny<object>(), It.IsAny<Player.ViewerPlayerCardsToChooseEventArgs>()))
            .Callback(() => sequence.Add("show"));
        viewer.Setup(v => v.OnActionRejected(It.IsAny<Exception>()))
            .Callback<Exception>(error =>
            {
                Assert.IsType<FormatException>(error);
                sequence.Add("error");
            });
        var commands = new Queue<string>(["invalid", "quit"]);
        var controller = new ConsoleController(new CallbackReader(() =>
        {
            sequence.Add("read");
            return commands.Dequeue();
        }));
        var game = new Game(4, viewer.Object);
        foreach (var player in game.Players) player.SetController(controller);

        game.Run();

        Assert.Equal(new[] { "show", "read", "error", "read" }, sequence);
        Assert.False(game.CharactersSelected);
        Assert.Equal(1, game.RoundNumber);
    }

    [Fact]
    public void GameRejectionIsDisplayedByPlayersViewer()
    {
        var sequence = new List<string>();
        var viewer = new Mock<IViewer>();
        viewer.Setup(v => v.OnTurnStateChanged(It.IsAny<object>(), It.IsAny<Player.ViewerTurnStateChangedEventArgs>()))
            .Callback<object, Player.ViewerTurnStateChangedEventArgs>((_, e) =>
                sequence.Add(e.Stage == PlayerTurnStage.TakeResources ? "resources" : "turn"));
        viewer.Setup(v => v.OnActionRejected(It.IsAny<Exception>()))
            .Callback<Exception>(error =>
            {
                Assert.IsType<InvalidOperationException>(error);
                sequence.Add("error");
            });
        var game = new Game(4, viewer.Object);
        game.StartCharacterSelection();
        while (game.CurrentChoosingPlayerId is int id)
            game.ChooseCharacter(id, new BaseGamePrerequisite().Sequence(game.GetAvailableCharacters(id)[0]));
        int active = game.CurrentPlayerId!.Value;
        var input = new Queue<string>(["gold", "gold", "quit"]);
        var controller = new ConsoleController(new CallbackReader(() =>
        {
            sequence.Add("read");
            return input.Dequeue();
        }));
        game.GetPlayer(active).SetController(controller);

        game.Run();

        Assert.Equal(new[] { "resources", "read", "turn", "read", "error", "read" }, sequence);
        Assert.Equal(4, game.GetPlayerGold(active));
        Assert.Equal(active, game.CurrentPlayerId);
    }

    [Fact]
    public void SharedControllerWorksWithDifferentViewersForEachPlayer()
    {
        var game = new Game(4, new Mock<IViewer>().Object);
        var shown = new List<int>();
        var viewers = new Dictionary<int, Mock<IViewer>>();
        int reads = 0;
        var controller = new ConsoleController(new CallbackReader(() =>
        {
            reads++;
            if (reads == 2) return "quit";
            int id = game.CurrentChoosingPlayerId!.Value;
            return new BaseGamePrerequisite().Sequence(game.GetAvailableCharacters(id)[0]).ToString();
        }));
        foreach (var player in game.Players)
        {
            var viewer = new Mock<IViewer>();
            viewer.Setup(v => v.OnPlayerCardToChoose(It.IsAny<object>(), It.IsAny<Player.ViewerPlayerCardsToChooseEventArgs>()))
                .Callback<object, Player.ViewerPlayerCardsToChooseEventArgs>((sender, _) =>
                {
                    Assert.Same(player, sender);
                    shown.Add(player.GetId());
                });
            player.SetViewer(viewer.Object);
            player.SetController(controller);
            viewers.Add(player.GetId(), viewer);
        }

        game.Run();

        Assert.Equal(2, reads);
        Assert.Equal(2, shown.Distinct().Count());
        foreach (var (id, viewer) in viewers)
            viewer.Verify(v => v.OnPlayerCardToChoose(It.IsAny<object>(), It.IsAny<Player.ViewerPlayerCardsToChooseEventArgs>()),
                Times.Exactly(shown.Contains(id) ? 1 : 0));
    }

    [Fact]
    public void ReplacingViewerUnsubscribesOldStateNotifications()
    {
        var oldViewer = new Mock<IViewer>();
        var newViewer = new Mock<IViewer>();
        var player = new Player(2, false, viewer: oldViewer.Object);
        player.SetViewer(newViewer.Object);
        oldViewer.Invocations.Clear();
        newViewer.Invocations.Clear();

        player.OnAddGold(null, new Game.PlayerAddGoldEventArgs { goldToAdd = 2 });

        oldViewer.Verify(v => v.OnGoldUpdated(It.IsAny<object>(), It.IsAny<Player.ViewerGoldUpdatedEventArgs>()), Times.Never);
        newViewer.Verify(v => v.OnGoldUpdated(player, It.Is<Player.ViewerGoldUpdatedEventArgs>(e => e.currentGoldOfPlayer == 4)), Times.Once);
    }
    [Fact]
    public void TurnChangesNotifyViewerWithoutPollingAController()
    {
        var updates = new List<(int PlayerId, Player.ViewerTurnStateChangedEventArgs State)>();
        var viewer = new Mock<IViewer>();
        viewer.Setup(v => v.OnTurnStateChanged(It.IsAny<object>(), It.IsAny<Player.ViewerTurnStateChangedEventArgs>()))
            .Callback<object, Player.ViewerTurnStateChangedEventArgs>((sender, state) =>
                updates.Add((((Player)sender).GetId(), state)));
        var game = new Game(4, viewer.Object);
        var controller = new Mock<IController>(MockBehavior.Strict);
        foreach (var player in game.Players) player.SetController(controller.Object);
        game.StartCharacterSelection();
        while (game.CurrentChoosingPlayerId is int id)
            game.ChooseCharacter(id, new BaseGamePrerequisite().Sequence(game.GetAvailableCharacters(id)[0]));
        int active = game.CurrentPlayerId!.Value;
        game.GetPlayer(active).OnAddTownCards(null, new Game.PlayerAddTownCardsEventArgs
        {
            addTownCardDeck = [new TownCard(QuarterType.Commercial, 1, "Notification district")]
        });

        game.DrawTownCards(active);
        Assert.Throws<ArgumentOutOfRangeException>(() => game.KeepTownCard(active, 99));
        Assert.Equal(2, updates.Count);
        game.KeepTownCard(active, 0);
        game.BuildQuarter(active, game.GetPlayerHand(active).Count - 2);
        game.EndTurn(active);

        Assert.Equal(new[] { PlayerTurnStage.TakeResources, PlayerTurnStage.KeepTownCard,
            PlayerTurnStage.TurnActions, PlayerTurnStage.TurnActions, PlayerTurnStage.Inactive },
            updates.Where(update => update.PlayerId == active).Select(update => update.State.Stage));
        Assert.Equal(2, updates[1].State.DrawnCards.Count);
        Assert.Empty(updates[2].State.DrawnCards);
        Assert.True(updates[2].State.CanBuild);
        Assert.False(updates[3].State.CanBuild);
        Assert.Equal(game.CurrentPlayerId, updates[^1].PlayerId);
        Assert.Equal(PlayerTurnStage.TakeResources, updates[^1].State.Stage);
        controller.VerifyNoOtherCalls();
    }

    [Fact]
    public void ReplacingViewerSynchronizesCurrentStateAndPendingChoice()
    {
        var oldViewer = new Mock<IViewer>();
        var game = new Game(4, oldViewer.Object);
        game.StartCharacterSelection();
        while (game.CurrentChoosingPlayerId is int id)
            game.ChooseCharacter(id, new BaseGamePrerequisite().Sequence(game.GetAvailableCharacters(id)[0]));
        int active = game.CurrentPlayerId!.Value;
        game.DrawTownCards(active);
        var player = game.GetPlayer(active);
        var newViewer = new Mock<IViewer>();

        player.SetViewer(newViewer.Object);

        newViewer.Verify(v => v.OnGoldUpdated(player,
            It.Is<Player.ViewerGoldUpdatedEventArgs>(e => e.currentGoldOfPlayer == 2)), Times.Once);
        newViewer.Verify(v => v.OnTownCardDeckUpdated(player,
            It.Is<Player.ViewerTownCardDeckUpdatedEventArgs>(e => e.townCardDeck.Count == 4)), Times.Once);
        newViewer.Verify(v => v.OnTurnStateChanged(player,
            It.Is<Player.ViewerTurnStateChangedEventArgs>(e => e.Stage == PlayerTurnStage.KeepTownCard && e.DrawnCards.Count == 2)), Times.Once);
        oldViewer.Invocations.Clear();
        game.KeepTownCard(active, 0);
        oldViewer.Verify(v => v.OnTurnStateChanged(It.IsAny<object>(),
            It.IsAny<Player.ViewerTurnStateChangedEventArgs>()), Times.Never);
        newViewer.Verify(v => v.OnTurnStateChanged(player,
            It.Is<Player.ViewerTurnStateChangedEventArgs>(e => e.Stage == PlayerTurnStage.TurnActions)), Times.Once);
    }

    [Fact]
    public void ExplicitDetachStopsNotificationsAndCanBeRepeated()
    {
        var viewer = new Mock<IViewer>();
        var player = new Player(2, false, viewer: viewer.Object);
        viewer.Invocations.Clear();

        player.DetachViewer();
        player.DetachViewer();
        player.OnAddGold(null, new Game.PlayerAddGoldEventArgs { goldToAdd = 2 });
        player.OnCharacterAssigned(null, new Game.PlayerCharacterAssignedEventArgs(new KingPlayerCard(), 4));

        viewer.Verify(v => v.OnDettach(player, It.IsAny<Player.ViewerDetachEventArgs>()), Times.Once);
        viewer.Verify(v => v.OnGoldUpdated(It.IsAny<object>(), It.IsAny<Player.ViewerGoldUpdatedEventArgs>()), Times.Never);
        viewer.Verify(v => v.OnCharacterChanged(It.IsAny<object>(), It.IsAny<Player.ViewerCharacterChangedEventArgs>()), Times.Never);
        var replacement = new Mock<IViewer>();
        player.SetViewer(replacement.Object);
        replacement.Verify(v => v.OnGoldUpdated(player,
            It.Is<Player.ViewerGoldUpdatedEventArgs>(e => e.currentGoldOfPlayer == 4)), Times.Once);
        replacement.Verify(v => v.OnCharacterChanged(player,
            It.Is<Player.ViewerCharacterChangedEventArgs>(e => e.Character is KingPlayerCard && e.Rank == 4)), Times.Once);
    }

    [Fact]
    public void ReplacingViewerAfterCharacterChoiceDoesNotReplayOldOptions()
    {
        var game = new Game(4, new Mock<IViewer>().Object);
        game.StartCharacterSelection();
        int id = game.CurrentChoosingPlayerId!.Value;
        var card = game.GetAvailableCharacters(id)[0];
        int rank = new BaseGamePrerequisite().Sequence(card);
        game.ChooseCharacter(id, rank);
        var player = game.GetPlayer(id);
        var viewer = new Mock<IViewer>();

        player.SetViewer(viewer.Object);

        viewer.Verify(v => v.OnCharacterChanged(player,
            It.Is<Player.ViewerCharacterChangedEventArgs>(e => e.Character != null && e.Character.Name == card.Name && e.Rank == rank)), Times.Once);
        viewer.Verify(v => v.OnPlayerCardToChoose(It.IsAny<object>(),
            It.IsAny<Player.ViewerPlayerCardsToChooseEventArgs>()), Times.Never);
    }

}
