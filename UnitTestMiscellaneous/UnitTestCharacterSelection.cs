namespace UnitTestMiscellaneous;

using Citadel;
using Moq;
using System.Xml.Linq;

public class UnitTestCharacterSelection
{
    private static List<int> PlayerIds(Game game) => XDocument.Parse(game.Serialize())
        .Root!.Elements("player").Select(player => (int)player.Attribute("id")!).ToList();

    [Theory]
    [InlineData(4, 5)]
    [InlineData(5, 6)]
    [InlineData(6, 7)]
    [InlineData(7, 7)]
    public void SelectionStartsWithCrownAndEndsWithRankedTurnOrder(int count, int firstChoices)
    {
        var game = new Game(count);
        var ids = PlayerIds(game);
        Assert.Null(game.CurrentChoosingPlayerId);
        game.StartCharacterSelection();
        Assert.Equal(1, game.RoundNumber);
        var chosen = new Dictionary<int, int>();
        for (int i = 0; i < count; i++)
        {
            int expectedId = ids[(count - 1 + i) % count];
            Assert.Equal(expectedId, game.CurrentChoosingPlayerId);
            var options = game.GetAvailableCharacters(expectedId);
            int expectedOptions = firstChoices - i;
            if (count == 7 && i == 6) expectedOptions++;
            Assert.Equal(expectedOptions, options.Count);
            var ranks = options.Select(card => new BaseGamePrerequisite().Sequence(card)).ToList();
            Assert.DoesNotContain(ranks, rank => chosen.ContainsValue(rank));
            int rank = ranks.Max();
            game.ChooseCharacter(expectedId, rank);
            chosen.Add(expectedId, rank);
        }
        Assert.True(game.CharactersSelected);
        Assert.Null(game.CurrentChoosingPlayerId);
        Assert.Equal(chosen.OrderBy(pair => pair.Value).Select(pair => pair.Key), game.GetTurnOrder());
    }

    [Fact]
    public void InvalidChoiceDoesNotAdvanceOrChangeOptions()
    {
        var game = new Game(4);
        game.StartCharacterSelection();
        int id = game.CurrentChoosingPlayerId!.Value;
        var names = game.GetAvailableCharacters(id).Select(card => card.Name).ToArray();
        Assert.Throws<InvalidOperationException>(() => game.ChooseCharacter(-1, 1));
        Assert.Throws<InvalidOperationException>(() => game.GetAvailableCharacters(-1));
        Assert.Throws<WrongPlayerCard>(() => game.ChooseCharacter(id, 99));
        Assert.Throws<InvalidOperationException>(() => game.GetTurnOrder());
        Assert.Throws<InvalidOperationException>(() => game.StartCharacterSelection());
        Assert.Equal(id, game.CurrentChoosingPlayerId);
        Assert.Equal(names, game.GetAvailableCharacters(id).Select(card => card.Name));
    }

    [Fact]
    public void SelectedCardCannotBeChosenAgain()
    {
        var game = new Game(4);
        game.StartCharacterSelection();
        int id = game.CurrentChoosingPlayerId!.Value;
        int rank = new BaseGamePrerequisite().Sequence(game.GetAvailableCharacters(id)[0]);
        game.ChooseCharacter(id, rank);
        int next = game.CurrentChoosingPlayerId!.Value;
        Assert.Throws<WrongPlayerCard>(() => game.ChooseCharacter(next, rank));
        Assert.Throws<InvalidOperationException>(() => game.ChooseCharacter(id, rank));
        Assert.Equal(next, game.CurrentChoosingPlayerId);
    }

    [Fact]
    public void RestartResetsSelectionState()
    {
        var game = new Game(4);
        game.StartCharacterSelection();
        int oldId = game.CurrentChoosingPlayerId!.Value;
        game.StartGame(5);
        Assert.False(game.CharactersSelected);
        Assert.Null(game.CurrentChoosingPlayerId);
        Assert.Equal(0, game.RoundNumber);
        game.StartCharacterSelection();
        Assert.Equal(6, game.GetAvailableCharacters(game.CurrentChoosingPlayerId!.Value).Count);
        Assert.Throws<InvalidOperationException>(() => game.GetAvailableCharacters(oldId));
    }

    [Fact]
    public void UnsupportedSetupFailsBeforeStartingSelection()
    {
        var empty = new Game();
        Assert.Throws<InvalidOperationException>(() => empty.StartCharacterSelection());
        var game = new Game(8);
        Assert.Throws<NotSupportedException>(() => game.StartCharacterSelection());
        Assert.Null(game.CurrentChoosingPlayerId);
        Assert.Equal(0, game.RoundNumber);
    }

    [Fact]
    public void PlayerPassesAvailableCardsAndPublicDiscardsToViewer()
    {
        var viewer = new Mock<IViewer>();
        Player.ViewerPlayerCardsToChooseEventArgs? received = null;
        viewer.Setup(v => v.OnPlayerCardToChoose(It.IsAny<object>(),
                It.IsAny<Player.ViewerPlayerCardsToChooseEventArgs>()))
            .Callback<object, Player.ViewerPlayerCardsToChooseEventArgs>((_, e) => received = e);
        var player = new Player(2, false, viewer: viewer.Object);
        player.OnChoosePlayerCard(null, new Game.PlayerChoosePlayerCardEventArgs
        {
            playerCardToChoose = [new KingPlayerCard()],
            playerCardExcludedOpen = [new AssassinPlayerCard()],
            numberOfPlayerCardExcludedClose = 1
        });
        Assert.NotNull(received);
        Assert.IsType<KingPlayerCard>(Assert.Single(received.playerCardToChoose));
        Assert.IsType<AssassinPlayerCard>(Assert.Single(received.playerCardExcludedOpen));
        Assert.Equal(1, received.numberOfPlayerCardExcludedClose);
    }

    [Fact]
    public void ViewerCanSubmitChoicesThroughPlayerEvents()
    {
        var viewer = new Mock<IViewer>();
        var chosen = new Dictionary<int, int>();
        viewer.Setup(v => v.OnPlayerCardToChoose(It.IsAny<object>(),
                It.IsAny<Player.ViewerPlayerCardsToChooseEventArgs>()))
            .Callback<object, Player.ViewerPlayerCardsToChooseEventArgs>((sender, e) =>
            {
                var player = Assert.IsType<Player>(sender);
                Assert.DoesNotContain(e.playerCardExcludedOpen, card => card is KingPlayerCard);
                var card = e.playerCardToChoose[0];
                chosen.Add(player.GetId(), new BaseGamePrerequisite().Sequence(card));
                // Options are detached snapshots; changes cannot alter the game deck.
                e.playerCardToChoose.Clear();
                player.ChoosePlayerCard(card);
            });
        var game = new Game(4, viewer.Object);
        game.StartCharacterSelection();

        Assert.True(game.CharactersSelected);
        Assert.Equal(4, chosen.Count);
        Assert.Equal(chosen.OrderBy(pair => pair.Value).Select(pair => pair.Key), game.GetTurnOrder());
        Assert.Throws<InvalidOperationException>(() => game.StartCharacterSelection());
    }

    [Fact]
    public void EventRejectsPlayerOutsideGameAndMissingCard()
    {
        var game = new Game(4);
        game.StartCharacterSelection();
        Assert.Throws<InvalidOperationException>(() => game.OnChoosedPlayerCard(new Player(0, false),
            new Player.GameChoosedPlayerCardEventArgs { choosedPlayerCard = new KingPlayerCard() }));
        Assert.Throws<UnchoosedPlayerCard>(() => game.OnChoosedPlayerCard(null,
            new Player.GameChoosedPlayerCardEventArgs()));
    }
}
