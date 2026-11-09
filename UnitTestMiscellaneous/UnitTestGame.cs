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
    public void RestartCreatesFreshPlayersAndDecks()
    {
        var game = new Game(8);
        for (int i = 0; i < 5; i++)
        {
            game.StartGame(4);
            Assert.Equal(4, XDocument.Parse(game.Serialize()).Root!.Elements("player").Count());
            game.StartGame(8);
            Assert.Equal(8, XDocument.Parse(game.Serialize()).Root!.Elements("player").Count());
        }
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(0)]
    [InlineData(3)]
    [InlineData(9)]
    public void InvalidPlayerCountDoesNotChangeExistingGame(int count)
    {
        var game = new Game(4);
        var before = game.Serialize();
        var error = Assert.Throws<IncorrectNumberOfPlayers>(() => game.StartGame(count));
        Assert.Equal("Number of players must be more or equal 4 and less or equal 8", error.Message);
        Assert.Equal(before, game.Serialize());
        Assert.Throws<IncorrectNumberOfPlayers>(() => new Game(count));
    }
}
