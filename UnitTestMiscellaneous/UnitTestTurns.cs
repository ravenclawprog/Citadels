namespace UnitTestMiscellaneous;

using Citadel;
using Moq;

public class UnitTestTurns
{
    private sealed record TestGame(Game Game, Dictionary<int, Player> Players, Mock<IViewer> Viewer);

    private static TestGame CreateGame(int count = 4)
    {
        var players = new Dictionary<int, Player>();
        var viewer = new Mock<IViewer>();
        viewer.Setup(v => v.OnAttach(It.IsAny<object>(), It.IsAny<Player.ViewerAttachEventArgs>()))
            .Callback<object, Player.ViewerAttachEventArgs>((sender, e) => players.Add(e.playerId, (Player)sender));
        var game = new Game(count, viewer.Object);
        game.StartCharacterSelection();
        ChooseCharacters(game);
        return new TestGame(game, players, viewer);
    }

    private static void ChooseCharacters(Game game)
    {
        while (game.CurrentChoosingPlayerId is int playerId)
        {
            var card = game.GetAvailableCharacters(playerId)[0];
            game.ChooseCharacter(playerId, new BaseGamePrerequisite().Sequence(card));
        }
    }

    private static void FinishRound(Game game)
    {
        int round = game.RoundNumber;
        while (game.RoundNumber == round)
        {
            int id = game.CurrentPlayerId!.Value;
            if (!game.HasTakenResources) game.TakeGold(id);
            game.EndTurn(id);
        }
    }

    private static string Key(ITownCard card) => $"{card.Name}|{card.Price}|{card.TypeOfQuarter}";

    [Fact]
    public void LastCharacterChoiceStartsFirstTurnAndPublishesIt()
    {
        var game = new Game(4, new Mock<IViewer>().Object);
        var turns = new List<Game.TurnStartedEventArgs>();
        game.TurnStarted += (_, e) => turns.Add(e);
        Assert.Equal(GamePhase.ReadyForCharacterSelection, game.Phase);
        Assert.Null(game.CurrentPlayerId);
        game.StartCharacterSelection();
        Assert.Equal(GamePhase.CharacterSelection, game.Phase);
        ChooseCharacters(game);

        Assert.Equal(GamePhase.PlayerTurns, game.Phase);
        Assert.Equal(game.GetTurnOrder()[0], game.CurrentPlayerId);
        var started = Assert.Single(turns);
        Assert.Equal(game.CurrentPlayerId, started.PlayerId);
        Assert.Equal(game.CurrentCharacterRank, started.CharacterRank);
        Assert.Equal(1, started.RoundNumber);
        Assert.False(game.HasTakenResources);
        Assert.False(game.HasBuiltThisTurn);
    }

    [Fact]
    public void GoldCanOnlyBeTakenOnceAndOnlyByActivePlayer()
    {
        var game = CreateGame().Game;
        int id = game.CurrentPlayerId!.Value;
        int other = game.GetTurnOrder()[1];
        Assert.Equal(2, game.GetPlayerGold(id));
        Assert.Throws<InvalidOperationException>(() => game.TakeGold(other));
        Assert.Throws<InvalidOperationException>(() => game.EndTurn(id));
        Assert.Throws<InvalidOperationException>(() => game.BuildQuarter(id, 0));
        Assert.False(game.HasTakenResources);
        game.TakeGold(id);

        Assert.Equal(4, game.GetPlayerGold(id));
        Assert.Equal(2, game.GetPlayerGold(other));
        Assert.True(game.HasTakenResources);
        Assert.Throws<InvalidOperationException>(() => game.TakeGold(id));
        Assert.Throws<InvalidOperationException>(() => game.DrawTownCards(id));
        Assert.Equal(4, game.GetPlayerGold(id));
    }

    [Fact]
    public void DrawingCardsRequiresKeepingOneBeforeOtherActions()
    {
        var game = CreateGame().Game;
        int id = game.CurrentPlayerId!.Value;
        int other = game.GetTurnOrder()[1];
        int remaining = game.RemainingTownCards;
        var hand = game.GetPlayerHand(id).Select(Key).ToList();
        var drawn = game.DrawTownCards(id);
        Assert.Equal(2, drawn.Count);
        Assert.Equal(remaining - 2, game.RemainingTownCards);
        Assert.Equal(hand, game.GetPlayerHand(id).Select(Key));
        Assert.NotSame(drawn[0], game.GetDrawnTownCards(id)[0]);
        Assert.Throws<InvalidOperationException>(() => game.DrawTownCards(id));
        Assert.Throws<InvalidOperationException>(() => game.TakeGold(id));
        Assert.Throws<InvalidOperationException>(() => game.BuildQuarter(id, 0));
        Assert.Throws<InvalidOperationException>(() => game.EndTurn(id));
        Assert.Throws<InvalidOperationException>(() => game.KeepTownCard(other, 0));
        Assert.Throws<InvalidOperationException>(() => game.GetDrawnTownCards(other));
        Assert.Throws<ArgumentOutOfRangeException>(() => game.KeepTownCard(id, -1));
        Assert.Throws<ArgumentOutOfRangeException>(() => game.KeepTownCard(id, 2));
        Assert.Equal(2, game.GetDrawnTownCards(id).Count);

        game.KeepTownCard(id, 1);
        hand.Add(Key(drawn[1]));
        Assert.Equal(hand, game.GetPlayerHand(id).Select(Key));
        Assert.Equal(remaining - 1, game.RemainingTownCards);
        Assert.Equal(2, game.GetPlayerGold(id));
        Assert.Empty(game.GetDrawnTownCards(id));
        Assert.Throws<InvalidOperationException>(() => game.KeepTownCard(id, 0));
        game.EndTurn(id);
        Assert.Equal(other, game.CurrentPlayerId);
        Assert.False(game.HasTakenResources);
    }

    [Fact]
    public void BuildingPaysCostMovesCardAndNotifiesViewer()
    {
        var setup = CreateGame();
        var game = setup.Game;
        int id = game.CurrentPlayerId!.Value;
        var player = setup.Players[id];
        var card = new TownCard(QuarterType.Commercial, 3, "Test district");
        player.OnAddTownCards(null, new Game.PlayerAddTownCardsEventArgs { addTownCardDeck = [card] });
        game.TakeGold(id);
        int index = game.GetPlayerHand(id).Count - 1;
        var hands = new List<List<ITownCard>>();
        var cities = new List<List<ITownCard>>();
        var gold = new List<int>();
        setup.Viewer.Setup(v => v.OnTownCardDeckUpdated(player, It.IsAny<Player.ViewerTownCardDeckUpdatedEventArgs>()))
            .Callback<object, Player.ViewerTownCardDeckUpdatedEventArgs>((_, e) => hands.Add(e.townCardDeck));
        setup.Viewer.Setup(v => v.OnBuildedTownUpdated(player, It.IsAny<Player.ViewerBuildedTownUpdatedEventArgs>()))
            .Callback<object, Player.ViewerBuildedTownUpdatedEventArgs>((_, e) => cities.Add(e.buildedTown));
        setup.Viewer.Setup(v => v.OnGoldUpdated(player, It.IsAny<Player.ViewerGoldUpdatedEventArgs>()))
            .Callback<object, Player.ViewerGoldUpdatedEventArgs>((_, e) => gold.Add(e.currentGoldOfPlayer));

        game.BuildQuarter(id, index);

        Assert.Equal(1, game.GetPlayerGold(id));
        Assert.Equal(4, game.GetPlayerHand(id).Count);
        Assert.Equal(Key(card), Key(Assert.Single(game.GetPlayerCity(id))));
        Assert.True(game.HasBuiltThisTurn);
        Assert.Equal(new[] { 1 }, gold);
        Assert.Equal(4, Assert.Single(hands).Count);
        Assert.Equal(Key(card), Key(Assert.Single(Assert.Single(cities))));
        hands[0].Clear();
        cities[0].Clear();
        Assert.Equal(4, game.GetPlayerHand(id).Count);
        Assert.Single(game.GetPlayerCity(id));
        Assert.Throws<InvalidOperationException>(() => game.BuildQuarter(id, 0));
        Assert.Equal(1, game.GetPlayerGold(id));
    }

    [Fact]
    public void InvalidBuildingDoesNotConsumeGoldCardsOrBuildingAllowance()
    {
        var setup = CreateGame();
        var game = setup.Game;
        int id = game.CurrentPlayerId!.Value;
        setup.Players[id].OnAddTownCards(null, new Game.PlayerAddTownCardsEventArgs
        {
            addTownCardDeck = [new TownCard(QuarterType.Royal, 100, "Expensive"),
                new TownCard(QuarterType.Commercial, 1, "Affordable")]
        });
        game.TakeGold(id);
        var hand = game.GetPlayerHand(id).Select(Key).ToList();
        Assert.Throws<ArgumentOutOfRangeException>(() => game.BuildQuarter(id, -1));
        Assert.Throws<ArgumentOutOfRangeException>(() => game.BuildQuarter(id, hand.Count));
        Assert.Throws<InvalidOperationException>(() => game.BuildQuarter(id, hand.Count - 2));
        Assert.Throws<InvalidOperationException>(() => game.BuildQuarter(game.GetTurnOrder()[1], 0));
        Assert.Equal(4, game.GetPlayerGold(id));
        Assert.Equal(hand, game.GetPlayerHand(id).Select(Key));
        Assert.Empty(game.GetPlayerCity(id));
        Assert.False(game.HasBuiltThisTurn);
        game.BuildQuarter(id, hand.Count - 1);
        Assert.Equal(3, game.GetPlayerGold(id));
        Assert.Single(game.GetPlayerCity(id));
    }

    [Fact]
    public void NextRoundRetainsCityAndRejectsDuplicateDistrict()
    {
        var setup = CreateGame();
        var game = setup.Game;
        int id = game.CurrentPlayerId!.Value;
        setup.Players[id].OnAddTownCards(null, new Game.PlayerAddTownCardsEventArgs
        {
            addTownCardDeck = [new TownCard(QuarterType.Commercial, 1, "Duplicate"),
                new TownCard(QuarterType.Commercial, 1, "Duplicate")]
        });
        game.TakeGold(id);
        game.BuildQuarter(id, game.GetPlayerHand(id).Count - 1);
        var expectedHand = game.GetPlayerHand(id).Select(Key).ToList();
        int remaining = game.RemainingTownCards;
        FinishRound(game);
        Assert.Equal(2, game.RoundNumber);
        Assert.Equal(GamePhase.CharacterSelection, game.Phase);
        Assert.False(game.CharactersSelected);
        Assert.Null(game.CurrentPlayerId);
        Assert.Equal(3, game.GetPlayerGold(id));
        Assert.Equal(expectedHand, game.GetPlayerHand(id).Select(Key));
        Assert.Equal(remaining, game.RemainingTownCards);
        Assert.Single(game.GetPlayerCity(id));
        Assert.Throws<InvalidOperationException>(() => game.TakeGold(id));
        ChooseCharacters(game);
        while (game.CurrentPlayerId != id)
        {
            int other = game.CurrentPlayerId!.Value;
            game.TakeGold(other);
            game.EndTurn(other);
        }
        game.TakeGold(id);
        Assert.Throws<InvalidOperationException>(() => game.BuildQuarter(id, game.GetPlayerHand(id).Count - 1));
        Assert.Equal(5, game.GetPlayerGold(id));
        Assert.False(game.HasBuiltThisTurn);
        Assert.Single(game.GetPlayerCity(id));
    }

    [Theory]
    [InlineData(4)]
    [InlineData(7)]
    public void TurnsFollowRankOrderAndNextRoundStartsAtCrown(int count)
    {
        var game = CreateGame(count).Game;
        var order = game.GetTurnOrder().ToList();
        var ranks = new List<int>();
        var events = new List<int>();
        game.TurnStarted += (_, e) => events.Add(e.PlayerId);
        foreach (int id in order)
        {
            Assert.Equal(id, game.CurrentPlayerId);
            ranks.Add(game.CurrentCharacterRank!.Value);
            Assert.False(game.HasTakenResources);
            game.TakeGold(id);
            game.EndTurn(id);
        }
        Assert.Equal(ranks.OrderBy(rank => rank), ranks);
        Assert.Equal(order.Skip(1), events);
        Assert.Equal(2, game.RoundNumber);
        Assert.Equal(GamePhase.CharacterSelection, game.Phase);
        Assert.Throws<InvalidOperationException>(() => game.GetTurnOrder());
        Assert.Throws<InvalidOperationException>(() => game.EndTurn(order[^1]));
        ChooseCharacters(game);
        Assert.Equal(GamePhase.PlayerTurns, game.Phase);
        Assert.Equal(game.GetTurnOrder()[0], game.CurrentPlayerId);
    }

    [Fact]
    public void ExhaustingDeckPreservesEveryCardAndStillAllowsGold()
    {
        var setup = CreateGame(7);
        var game = setup.Game;
        int initialRemaining = game.RemainingTownCards;
        string? lastDiscard = null;
        bool sawSingleCard = false;
        for (int i = 0; game.RemainingTownCards > 0; i++)
        {
            Assert.True(i < initialRemaining + 1, "Each resource draw must reduce the deck by one card.");
            if (game.Phase == GamePhase.CharacterSelection) ChooseCharacters(game);
            int id = game.CurrentPlayerId!.Value;
            int before = game.RemainingTownCards;
            var drawn = game.DrawTownCards(id);
            if (before == 1)
            {
                sawSingleCard = true;
                Assert.Equal(lastDiscard, Key(Assert.Single(drawn)));
            }
            else lastDiscard = Key(drawn[1]);
            game.KeepTownCard(id, 0);
            Assert.Equal(before - 1, game.RemainingTownCards);
            game.EndTurn(id);
        }
        Assert.True(sawSingleCard);
        var expectedCards = new BaseGamePrerequisite().GenerateTownCardDeck().Select(Key).OrderBy(key => key);
        var actualCards = setup.Players.Keys.SelectMany(game.GetPlayerHand).Select(Key).OrderBy(key => key);
        Assert.Equal(expectedCards, actualCards);
        if (game.Phase == GamePhase.CharacterSelection) ChooseCharacters(game);
        int active = game.CurrentPlayerId!.Value;
        Assert.Throws<InvalidOperationException>(() => game.DrawTownCards(active));
        Assert.False(game.HasTakenResources);
        game.TakeGold(active);
        game.EndTurn(active);
    }
}
