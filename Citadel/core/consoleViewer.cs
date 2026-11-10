using System;
namespace Citadel
{
    public class ConsoleViewer : IViewer
    {
        private readonly TextWriter _output;
        private readonly TextWriter _error;
        private readonly bool _clearTerminal;
        private int? _displayedPlayerId;
        private int _displayedRound;

        private readonly Dictionary<int, int> _gold = new();
        private readonly Dictionary<int, IReadOnlyList<ITownCard>> _hands = new();
        private readonly Dictionary<int, IReadOnlyList<ITownCard>> _cities = new();

        public ConsoleViewer(TextWriter? output = null, TextWriter? error = null)
        {
            _output = output ?? Console.Out;
            _error = error ?? Console.Error;
            _clearTerminal = output == null && !Console.IsOutputRedirected;
        }

        public void ShowWelcome()
        {
            _output.WriteLine("Citadels: basic turns. Character abilities, special district effects and scoring are not implemented yet.");
            _output.WriteLine("Pass the terminal to the indicated player. Enter 'quit' to stop.");
        }

        public void ShowUsage() => _error.WriteLine("Usage: Citadel [players: 4-7]");
        public void ShowStopped() => _output.WriteLine("Game stopped.");
        public void ShowError(string message) => _output.WriteLine(message);
        public void ShowUnknownCommand() => _output.WriteLine("Unknown command.");
        public void ShowInvalidCharacter() => _output.WriteLine("Enter the rank of an available character.");
        public void ShowInvalidCardNumber() => _output.WriteLine("Enter a number from the displayed list.");

        public void ClearScreen()
        {
            if (_clearTerminal) Console.Clear();
        }

        public void OnCharacterChanged(object? sender, Player.ViewerCharacterChangedEventArgs e)
        {
            if (e.Character != null && ((Player)sender!).GetId() == _displayedPlayerId)
                _output.WriteLine($"Player {((Player)sender!).GetId()} chose {e.Character.Name} (rank {e.Rank}).");
        }

        public void OnTurnStateChanged(object? sender, Player.ViewerTurnStateChangedEventArgs e)
        {
            if (e.Stage == PlayerTurnStage.Inactive) return;
            int id = ((Player)sender!).GetId();
            PrepareDisplay(id, e.RoundNumber);
            _output.WriteLine($"Round {e.RoundNumber}. Player {id}, {e.CharacterName} (rank {e.CharacterRank}). Gold: {_gold.GetValueOrDefault(id)}.");
            switch (e.Stage)
            {
                case PlayerTurnStage.TakeResources:
                    _output.WriteLine("Your hand:");
                    ShowCards(_hands.GetValueOrDefault(id) ?? Array.Empty<ITownCard>());
                    _output.WriteLine("Your city:");
                    ShowCards(_cities.GetValueOrDefault(id) ?? Array.Empty<ITownCard>());
                    _output.WriteLine("Take resources: gold or cards.");
                    break;
                case PlayerTurnStage.KeepTownCard:
                    ShowCards(e.DrawnCards);
                    _output.WriteLine("Choose a drawn district: keep <number>.");
                    break;
                case PlayerTurnStage.TurnActions:
                    _output.WriteLine("Your hand:");
                    ShowCards(_hands.GetValueOrDefault(id) ?? Array.Empty<ITownCard>());
                    _output.WriteLine("Your city:");
                    ShowCards(_cities.GetValueOrDefault(id) ?? Array.Empty<ITownCard>());
                    _output.WriteLine(e.CanBuild ? "Build a district: build <number>, or end your turn: end." : "End your turn: end.");
                    break;
            }
        }

        public void OnActionRejected(Exception error)
        {
            if (error is WrongPlayerCard) ShowInvalidCharacter();
            else if (error is ArgumentOutOfRangeException) ShowInvalidCardNumber();
            else ShowError(error.Message);
        }

        private void PrepareDisplay(int playerId, int roundNumber)
        {
            if (_displayedPlayerId != playerId || _displayedRound != roundNumber)
                ClearScreen();
            _displayedPlayerId = playerId;
            _displayedRound = roundNumber;
        }

        private void ShowCards(IReadOnlyList<ITownCard> cards)
        {
            if (cards.Count == 0) _output.WriteLine("(empty)");
            for (int i = 0; i < cards.Count; i++)
                _output.WriteLine($"{i + 1}: {cards[i].Name}, cost {cards[i].Price}, {cards[i].TypeOfQuarter}");
        }

        public void OnAttach(object? sender, Player.ViewerAttachEventArgs e)
        {
            _output.WriteLine("Attached player with id:{0}; name:{1}", e.playerId, e.playerName);
        }
        public void OnDettach(object? sender, Player.ViewerDetachEventArgs e)
        {
            Player? detachedPlayer = (Player?)sender;
            if (detachedPlayer != null)
            {
                int id = detachedPlayer.GetId();
                _gold.Remove(id);
                _hands.Remove(id);
                _cities.Remove(id);
                if (_displayedPlayerId == id) _displayedPlayerId = null;
            }
            _output.WriteLine("Player detached with id:{0}", detachedPlayer?.GetId());
        }
        public void OnChangeName(object? sender, Player.ViewerChangeNameEventArgs e)
        {
            Player? player = (Player?)sender;
            _output.WriteLine("Player detached with id:{0}", player?.GetId());
            _output.WriteLine("New player name:{0}", e.newPlayerName);
        }
        public void OnGoldUpdated(object? sender, Player.ViewerGoldUpdatedEventArgs e)
        {
            Player? player = (Player?)sender;
            if (player != null) _gold[player.GetId()] = e.currentGoldOfPlayer;
            _output.WriteLine("Gold updated for player with id:{0}", player?.GetId());
            _output.WriteLine("Current gold:{0}", e.currentGoldOfPlayer);
        }
        public void OnCoronationChange(object? sender, Player.ViewerCoronationChange e)
        {
            Player? player = (Player?)sender;
            _output.WriteLine("Coronation changed for player with id:{0}", player?.GetId());
            _output.WriteLine("Has crown:{0}", e.hasCrown);
        }
        public void OnTownCardDeckUpdated(object? sender, Player.ViewerTownCardDeckUpdatedEventArgs e)
        {
            Player? player = (Player?)sender;
            if (player != null) _hands[player.GetId()] = e.townCardDeck;
            if (player?.GetId() == _displayedPlayerId)
            {
                _output.WriteLine("Your hand:");
                ShowCards(e.townCardDeck);
            }
        }
        public void OnBuildedTownUpdated(object? sender, Player.ViewerBuildedTownUpdatedEventArgs e)
        {
            Player? player = (Player?)sender;
            if (player != null) _cities[player.GetId()] = e.buildedTown;
            if (player?.GetId() == _displayedPlayerId)
            {
                _output.WriteLine("Your city:");
                ShowCards(e.buildedTown);
            }
        }
        public void OnPlayerCardToChoose(object? sender, Player.ViewerPlayerCardsToChooseEventArgs e)
        {
            Player? player = (Player?)sender;
            PrepareDisplay(player!.GetId(), e.roundNumber);
            _output.WriteLine($"Round {e.roundNumber}. Player {player.GetId()}, choose a character by rank:");
            foreach (var card in e.characters)
                _output.WriteLine($"{card.Rank}: {card.Name}");
            _output.WriteLine("Number of excluded open cards:{0}", e.playerCardExcludedOpen.Count);
            for (int i = 0; i < e.playerCardExcludedOpen.Count; i++)
            {
                _output.WriteLine("\t{0}:{1}", i, e.playerCardExcludedOpen[i].Name);
            }
            _output.WriteLine("Number of excluded closed cards:{0}", e.numberOfPlayerCardExcludedClose);
            _output.WriteLine("Number of player cards to choose:{0}", e.playerCardToChoose.Count);
        }
    }
}