using System.Xml.Serialization;


namespace Citadel
{
    // key - number of players, value - number of dropped cards at open and at close
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
        public Game(int numberOfPlayers, IViewer? viewer = null)
        {
            _players = new List<Player>();
            GamePrerequisite gp = new BaseGamePrerequisite();
            _playerCardsDeck = gp.GeneratePlayerCardDeck();
            _townCardsDeck = gp.GenerateTownCardDeck();
            _dropPlayerCardsOpen = new List<IPlayerCard>();
            _dropPlayerCardsClose = new List<IPlayerCard>();
            _roundNumber = 0;
            _viewer = viewer ?? new ConsoleViewer();
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
            // Starting a new game must discard the previous game's state.
            foreach (var oldPlayer in _players)
                oldPlayer.OnGameChoosedPlayerCard -= OnChoosedPlayerCard;
            ResetTurnState();
            _selectionOrder.Clear();
            _selectionIndex = 0;
            _charactersSelected = false;
            _players.Clear();
            GamePrerequisite prerequisite = new BaseGamePrerequisite();
            _playerCardsDeck = prerequisite.GeneratePlayerCardDeck();
            _townCardsDeck = prerequisite.GenerateTownCardDeck();
            _dropPlayerCardsOpen.Clear();
            _dropPlayerCardsClose.Clear();
            OnAddGold = null;
            OnAddTownCards = null;
            OnCoronation = null;
            OnDecoronation = null;
            OnChoosePlayerCard = null;
            for (int i = 0; i < numberOfPlayers; i++)
            {
                Player playerToAdd = new(0, false, "DUMMY", null, null, null, _viewer);
                playerToAdd.OnGameChoosedPlayerCard += OnChoosedPlayerCard;
                _players.Add(playerToAdd);
            }
            _roundNumber = 0;
            RandomizeDecks();
            ChoosingCrown();
            Preparation();
            _roundNumber = 0;
            Phase = GamePhase.ReadyForCharacterSelection;
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
                    break;
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
            // Adding start cards to all players and gold
            for (int i = 0; i < _players.Count; i++)
            {
                PlayerAddTownCardsEventArgs e = new PlayerAddTownCardsEventArgs();
                e.addTownCardDeck.AddRange(_townCardsDeck.Slice(0, prepareStartTownCardCount));
                _townCardsDeck.RemoveRange(0, prepareStartTownCardCount);
                OnAddTownCards = _players[i].OnAddTownCards;
                OnAddTownCards(this, e);

                OnAddGold = _players[i].OnAddGold;
                OnAddGold(this, new PlayerAddGoldEventArgs() { goldToAdd = prepareStartPlayerGold });
            }
        }

    }
}