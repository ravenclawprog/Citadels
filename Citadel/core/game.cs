using System.Xml.Serialization;

namespace Citadel;

public partial class Game: IXmlSerializable
{
    private const int maxNumOfBuildingToWin = 7;
    private const int prepareStartPlayerGold = 2;
    private const int prepareStartTownCardCount = 4;
    public Game()
    {
        _players = new List<Player>();
        GamePrerequisite gp = new BaseGamePrerequisite();
        _playerCardsDeck = gp.GeneratePlayerCardDeck();
        _townCardsDeck = gp.GenerateTownCardDeck();
        _dropPlayerCards = new List<IPlayerCard>();
        _currentPlayer = null;
        _roundNumber = 0;
    }
    public Game(int numberOfPlayers)
    {
        _players = new List<Player>();
        GamePrerequisite gp = new BaseGamePrerequisite();
        _playerCardsDeck = gp.GeneratePlayerCardDeck();
        _townCardsDeck = gp.GenerateTownCardDeck();
        _dropPlayerCards = new List<IPlayerCard>();
        _currentPlayer = null;
        _roundNumber = 0;
        StartGame(numberOfPlayers);
    }
    List<Player> _players;
    Player? _currentPlayer;
    List<IPlayerCard> _playerCardsDeck;
    List<ITownCard> _townCardsDeck;
    List<IPlayerCard> _dropPlayerCards;
    int _roundNumber;
    public void StartGame(int numberOfPlayers)
    {
        if (numberOfPlayers < 4 || numberOfPlayers > 8)
        {
            throw new IncorrectNumberOfPlayers("Number of players must be more or equal 4 and less or equal 8");
        }
        for (int i = 0; i < numberOfPlayers; i++)
        {
            Player playerToAdd = new(0, false);
            _players.Add(playerToAdd);
            // OnPreparation += playerToAdd.OnPreparation;
        }
        _currentPlayer = _players[0];
        _roundNumber = 0;
        RandomizeDecks();
        ChoosingCrown();
        Preparation();
        _roundNumber = 0;
    }
    public void EndGame()
    {

    }
    private void RandomizeDecks()
    {
        ListRandomizer.Permutate<ITownCard>(ref _townCardsDeck);
        ListRandomizer.Permutate<IPlayerCard>(ref _playerCardsDeck);
    }
    protected void ChoosingCrown()
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
    protected void Preparation()
    {
        // TODO: pattern strategy
        for (int i = 0; i < _players.Count; i++)
        {
            PreparationsEventArgs e = new PreparationsEventArgs();
            e.goldToAdd = prepareStartPlayerGold;
            e.startTownCardDeck.AddRange(_townCardsDeck.Slice(0, prepareStartTownCardCount));
            _townCardsDeck.RemoveRange(0, prepareStartTownCardCount);
            _players[0].OnPreparation(this, e);
        }
    }  

}