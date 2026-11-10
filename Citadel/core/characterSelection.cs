namespace Citadel;

public partial class Game
{
    private readonly List<Player> _selectionOrder = new();
    private int _selectionIndex;
    private bool _charactersSelected;

    public int RoundNumber => _roundNumber;
    public bool CharactersSelected => _charactersSelected;
    public int? CurrentChoosingPlayerId => _selectionIndex < _selectionOrder.Count
        ? _selectionOrder[_selectionIndex].GetId() : null;

    public void StartCharacterSelection()
    {
        if (_players.Count == 0)
            throw new InvalidOperationException("Start a game before choosing characters.");
        if (_players.Count > 7)
            throw new NotSupportedException("Eight players require a ninth character, which is not implemented yet.");
        if (Phase != GamePhase.ReadyForCharacterSelection)
            throw new InvalidOperationException("Character selection has already started for this round.");

        _selectionOrder.Clear();
        foreach (var player in _players)
            AssignPlayerCharacter(player, null);
        PrepareCharacterDeck();
        var crownedPlayer = FindCrownedPlayer()
            ?? throw new InvalidOperationException("The game has no crowned player.");
        int first = _players.IndexOf(crownedPlayer);
        for (int i = 0; i < _players.Count; i++)
            _selectionOrder.Add(_players[(first + i) % _players.Count]);
        _selectionIndex = 0;
        _charactersSelected = false;
        _roundNumber++;
        Phase = GamePhase.CharacterSelection;
        NotifyCharacterChoice();
    }

    // Only the choosing player's options are returned; selected and hidden cards stay private.
    public IReadOnlyList<IPlayerCard> GetAvailableCharacters(int playerId)
    {
        RequireChoosingPlayer(playerId);
        return _playerCardsDeck.Select(card => (IPlayerCard)card.Clone()).ToList().AsReadOnly();
    }

    public void ChooseCharacter(int playerId, int rank)
    {
        Player player = RequireChoosingPlayer(playerId);
        var card = _playerCardsDeck.SingleOrDefault(candidate => GetCharacterRank(candidate) == rank)
            ?? throw new WrongPlayerCard("This character is not available for selection.");
        _playerCardsDeck.Remove(card);
        AssignPlayerCharacter(player, card, rank);
        _selectionIndex++;
        if (_selectionIndex == _selectionOrder.Count)
        {
            _dropPlayerCardsClose.AddRange(_playerCardsDeck);
            _playerCardsDeck.Clear();
            _charactersSelected = true;
            StartTurns();
            return;
        }

        // With seven players, the last player also receives the initial facedown discard.
        if (_players.Count == 7 && _selectionIndex == 6)
        {
            _playerCardsDeck.AddRange(_dropPlayerCardsClose);
            _dropPlayerCardsClose.Clear();
        }
        NotifyCharacterChoice();
    }

    // Engine-level order for the subsequent turn phase; unavailable during selection.
    public IReadOnlyList<int> GetTurnOrder()
    {
        if (!_charactersSelected)
            throw new InvalidOperationException("All players must choose a character first.");
        return _selectionOrder.OrderBy(player => player.CharacterRank)
            .Select(player => player.GetId()).ToList().AsReadOnly();
    }

    private Player RequireChoosingPlayer(int playerId)
    {
        if (Phase != GamePhase.CharacterSelection || CurrentChoosingPlayerId != playerId)
            throw new InvalidOperationException("It is not this player's turn to choose a character.");
        return _selectionOrder[_selectionIndex];
    }

    private void NotifyCharacterChoice()
    {
        var player = _selectionOrder[_selectionIndex];
        OnChoosePlayerCard = player.OnChoosePlayerCard;
        OnChoosePlayerCard(this, new PlayerChoosePlayerCardEventArgs
        {
            roundNumber = RoundNumber,
            characters = _playerCardsDeck.Select(card => new CharacterChoice(GetCharacterRank(card), card.Name)).ToList().AsReadOnly(),
            playerCardToChoose = _playerCardsDeck.Select(card => (IPlayerCard)card.Clone()).ToList(),
            playerCardExcludedOpen = _dropPlayerCardsOpen.Select(card => (IPlayerCard)card.Clone()).ToList(),
            numberOfPlayerCardExcludedClose = _dropPlayerCardsClose.Count
        });
    }

    private void AssignPlayerCharacter(Player player, IPlayerCard? card, int? rank = null)
    {
        OnAssignCharacter = player.OnCharacterAssigned;
        OnAssignCharacter(this, new PlayerCharacterAssignedEventArgs(card, rank));
    }

    private int GetCharacterRank(IPlayerCard card) =>
        _characterRanks.TryGetValue(card.GetType(), out int rank)
            ? rank
            : throw new WrongPlayerCard("Unknown type of Player card:" + card.GetType());

    private void PrepareCharacterDeck()
    {
        _playerCardsDeck = _characterCards.Select(card => (IPlayerCard)card.Clone()).ToList();
        ListRandomizer.Permutate(ref _playerCardsDeck);
        _dropPlayerCardsOpen.Clear();
        _dropPlayerCardsClose.Clear();
        // Discard facedown first, so every character (including King) can be hidden.
        _dropPlayerCardsClose.Add(_playerCardsDeck[0]);
        _playerCardsDeck.RemoveAt(0);
        for (int i = 0; i < dumpCardRule[_players.Count].open; i++)
        {
            int index = _playerCardsDeck.FindIndex(card => card is not KingPlayerCard);
            _dropPlayerCardsOpen.Add(_playerCardsDeck[index]);
            _playerCardsDeck.RemoveAt(index);
        }
    }
}
