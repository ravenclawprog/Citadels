namespace Citadel;

public class Game
{
    public Game()
    {
        _players = new List<Player>();
        GamePrerequisite gp = new BaseGamePrerequisite();
        _playerCardsDeck = gp.GeneratePlayerCardDeck();
        _townCardsDeck = gp.GenerateTownCardDeck();
        _dropPlayerCards = new List<IPlayerCard>();
        _currentPlayer = null;
    }
    List<Player> _players;
    Player? _currentPlayer;
    List<IPlayerCard> _playerCardsDeck;
    List<ITownCard> _townCardsDeck;
    List<IPlayerCard> _dropPlayerCards;
    public void StartGame(int numberOfPlayers)
    {
        if (numberOfPlayers < 4 || numberOfPlayers > 8)
        {
            throw new IncorrectNumberOfPlayers("Number of players must be more or equal 4 and less or equal 8");
        }
        for (int i = 0; i < numberOfPlayers; i++)
        {
            _players.Add(new Player(0, false));
        }
        _currentPlayer = _players[0];
        RandomizeDecks();
        ChoosingCrown();
    }
    public void EndGame()
    {

    }
    private void RandomizeDecks()
    {
        ListRandomizer.Permutate<ITownCard>(ref _townCardsDeck);
        ListRandomizer.Permutate<IPlayerCard>(ref _playerCardsDeck);
    }
    private void ChoosingCrown()
    {
        // TODO: strategy pattern
        int maxId = 0;
        Player crownedPlayer = _players[0];
        foreach (var player in _players)
        {
            if (player.getId() > maxId)
            {
                maxId = player.getId();
                crownedPlayer = player;
            }
        }
        crownedPlayer.setCrown();
    }
    private void Preparation()
    {
        
    }  

}