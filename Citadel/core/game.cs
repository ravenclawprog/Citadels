using System.Xml.Serialization;


namespace Citadel
{
    using DumpCardRuleType = Dictionary<int, (int open, int close)>;
    public partial class Game : IXmlSerializable
    {
        public const int maxNumOfBuildingToWin = 7;
        public const int prepareStartPlayerGold = 2;
        public const int prepareStartTownCardCount = 4;
        public readonly DumpCardRuleType dumpCardRule = new()
        {
            {4, new (2,1) },
            {5, new (1,1) },
            {6, new (0,1) },
            {7, new (0,1) },
            {8, new (0,1) }
        };
        public Game()
        {
            _players = new List<Player>();
            GamePrerequisite gp = new BaseGamePrerequisite();
            _playerCardsDeck = gp.GeneratePlayerCardDeck();
            _townCardsDeck = gp.GenerateTownCardDeck();
            _dropPlayerCardsOpen = new List<IPlayerCard>();
            _dropPlayerCardsClose = new List<IPlayerCard>();
            _viewer = new ConsoleViewer();
            _roundNumber = 0;
        }
        public Game(int numberOfPlayers)
        {
            _players = new List<Player>();
            GamePrerequisite gp = new BaseGamePrerequisite();
            _playerCardsDeck = gp.GeneratePlayerCardDeck();
            _townCardsDeck = gp.GenerateTownCardDeck();
            _dropPlayerCardsOpen = new List<IPlayerCard>();
            _dropPlayerCardsClose = new List<IPlayerCard>();
            _roundNumber = 0;
            _viewer = new ConsoleViewer();
            StartGame(numberOfPlayers);
        }
        List<Player> _players;
        List<IPlayerCard> _playerCardsDeck;
        List<ITownCard> _townCardsDeck;
        List<IPlayerCard> _dropPlayerCardsOpen;
        List<IPlayerCard> _dropPlayerCardsClose;
        IViewer _viewer;
        int _roundNumber;
        public void StartGame(int numberOfPlayers)
        {
            if (numberOfPlayers < 4 || numberOfPlayers > 8)
            {
                throw new IncorrectNumberOfPlayers("Number of players must be more or equal 4 and less or equal 8");
            }
            for (int i = 0; i < numberOfPlayers; i++)
            {
                Player playerToAdd = new(0, false, "DUMMY", null, null, null, _viewer);
                _players.Add(playerToAdd);
            }
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
        private Player? FindCrownedPlayer()
        {
            Player? crownedPlayer = null;
            foreach (var player in _players)
            {
                if (player.HasCrown())
                {
                    crownedPlayer = player;
                }
            }
            return crownedPlayer;
        }
        protected void ChoosingCrown()
        {
            // TODO: strategy pattern
            int maxId = 0;
            Player crownedPlayer = _players[0];
            foreach (var player in _players)
            {
                if (player.GetId() > maxId)
                {
                    maxId = player.GetId();
                    crownedPlayer = player;
                }
            }
            OnCoronation = crownedPlayer.OnCoronation;
            OnCoronation(this, new PlayerCoronationEventArgs());
        }
        protected void Preparation()
        {
            // TODO: pattern strategy
            for (int i = 0; i < _players.Count; i++)
            {
                PlayerAddTownCardsEventArgs e = new PlayerAddTownCardsEventArgs();
                e.addTownCardDeck.AddRange(_townCardsDeck.Slice(0, prepareStartTownCardCount));
                _townCardsDeck.RemoveRange(0, prepareStartTownCardCount);
                OnAddTownCards = _players[i].OnAddTownCards;
                OnAddTownCards(this, e);

                OnAddGold = _players[i].OnAddGold;
                OnAddGold(this, new PlayerAddGoldEventArgs(){ goldToAdd = prepareStartPlayerGold});
            }
            int numberOfOpened = dumpCardRule[_players.Count].open;
            int numberOfClosed = dumpCardRule[_players.Count].close;

            for (int i = 0; i < numberOfOpened; i++)
            {
                if (_playerCardsDeck[0].FavoriteQuarterType == QuarterType.Royal)
                {
                    _dropPlayerCardsOpen.Add(_playerCardsDeck[1]);
                    _playerCardsDeck.Remove(_playerCardsDeck[1]);
                }
                else
                {
                    _dropPlayerCardsOpen.Add(_playerCardsDeck[0]);
                    _playerCardsDeck.Remove(_playerCardsDeck[0]);
                }
            }
            for (int i = 0; i < numberOfClosed; i++)
            {
                _dropPlayerCardsClose.Add(_playerCardsDeck[0]);
                _playerCardsDeck.Remove(_playerCardsDeck[0]);
            }
        }

    }
}