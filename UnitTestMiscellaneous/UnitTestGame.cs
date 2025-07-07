namespace UnitTestMiscellaneous;

using Citadel;
using System.Text.RegularExpressions;

public class UnitTestGame
{

    [Fact]
    public void TestGameStartGracefull()
    {
        int numberOfPlayers = 4;
        Game game = new Game(numberOfPlayers);
        var gameState = XmlExtension.Serialize(game);
        System.Console.WriteLine("{0}", gameState);
        Regex rg = new Regex("player");
        var match = rg.Matches(gameState);
        System.Console.WriteLine("Number of players:{0}", match.Count);
        Assert.Equal(match.Count, numberOfPlayers);
    }
    [Fact]
    public void TestGameStartFail()
    {
        var expectedMessage = "Number of players must be more or equal 4 and less or equal 8";
        var ex = Assert.Throws<IncorrectNumberOfPlayers>(() => new Game(3));
        if (ex != null)
        {
            Console.WriteLine("get exception with message:{0}", ex.Message);
        }
        else
        {
            Assert.False(false,"No throw");
            return;
        }
        Assert.Equal(expectedMessage, ex.Message);
    }
}