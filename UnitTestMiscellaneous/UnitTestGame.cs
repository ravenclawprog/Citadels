namespace UnitTestMiscellaneous;

using Citadel;
using System.Xml.Linq;

public class UnitTestGame
{
    [Theory]
    [InlineData(4)]
    [InlineData(5)]
    [InlineData(6)]
    [InlineData(7)]
    [InlineData(8)]
    public void GameStartsWithRequestedPlayers(int count)
    {
        var game = new Game(count);
        var players = XDocument.Parse(game.Serialize()).Root!.Elements("player").ToList();
        Assert.Equal(count, players.Count);
        Assert.Equal(count, players.Select(p => (int)p.Attribute("id")!).Distinct().Count());
    }

    [Fact]
    public void StartGamePreparesDecksAndCharacterRanksOnAnEmptyGame()
    {
        var game = new Game();
        Assert.Equal(GamePhase.NotStarted, game.Phase);
        Assert.Equal(0, game.RemainingTownCards);

        game.StartGame(4);

        Assert.Equal(GamePhase.ReadyForCharacterSelection, game.Phase);
        Assert.Equal(4, game.Players.Count);
        Assert.Equal(new BaseGamePrerequisite().GenerateTownCardDeck().Count - 16,
            game.RemainingTownCards);
        game.StartCharacterSelection();
        var ranks = new BaseGamePrerequisite();
        while (!game.CharactersSelected)
        {
            int id = game.CurrentChoosingPlayerId!.Value;
            game.ChooseCharacter(id, ranks.Sequence(game.GetAvailableCharacters(id)[0]));
        }
        Assert.Equal(GamePhase.PlayerTurns, game.Phase);
        Assert.Equal(4, game.GetTurnOrder().Count);
        Assert.InRange(game.CurrentCharacterRank!.Value, 1, 8);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(0)]
    [InlineData(3)]
    [InlineData(9)]
    public void InvalidPlayerCountIsRejected(int count)
    {
        var game = new Game();
        var error = Assert.Throws<IncorrectNumberOfPlayers>(() => game.StartGame(count));
        Assert.Equal("Number of players must be more or equal 4 and less or equal 8", error.Message);
        Assert.Throws<IncorrectNumberOfPlayers>(() => new Game(count));
    }
}
