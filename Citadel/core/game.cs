namespace Citadel;

public class Game
{
    public Game()
    {
        _players = new List<Player>();
        GamePrerequisite gp = new BaseGamePrerequisite();
        _playerCardsDeck = gp.GeneratePlayerCardDeck();
        _townCardsDeck = gp.GenerateTownCardDeck();
    }
    List<Player> _players;
    Player _currentPlayer;
    List<IPlayerCard> _playerCardsDeck;
    List<ITownCard> _townCardsDeck;
    void startGame()
    { }
    void endGame()
    { }
}