namespace Citadel;

public enum GamePhase
{
    NotStarted,
    ReadyForCharacterSelection,
    CharacterSelection,
    PlayerTurns
}

public partial class Game
{
    private readonly List<Player> _turnOrder = new();
    private readonly List<ITownCard> _drawnTownCards = new();
    private int _turnIndex;

    public GamePhase Phase { get; private set; } = GamePhase.NotStarted;
    public int? CurrentPlayerId => Phase == GamePhase.PlayerTurns ? _turnOrder[_turnIndex].GetId() : null;
    public int? CurrentCharacterRank => Phase == GamePhase.PlayerTurns ? _turnOrder[_turnIndex].CharacterRank : null;
    public string? CurrentCharacterName => Phase == GamePhase.PlayerTurns ? _turnOrder[_turnIndex].CharacterName : null;
    public bool HasTakenResources { get; private set; }
    public bool HasBuiltThisTurn { get; private set; }
    public int RemainingTownCards => _townCardsDeck.Count;

    public sealed class TurnStartedEventArgs(int playerId, int characterRank, int roundNumber) : EventArgs
    {
        public int PlayerId { get; } = playerId;
        public int CharacterRank { get; } = characterRank;
        public int RoundNumber { get; } = roundNumber;
    }

    public event EventHandler<TurnStartedEventArgs>? TurnStarted;

    public int GetPlayerGold(int playerId) => FindPlayer(playerId).GoldCount;
    public IReadOnlyList<ITownCard> GetPlayerHand(int playerId) => FindPlayer(playerId).HandSnapshot;
    public IReadOnlyList<ITownCard> GetPlayerCity(int playerId) => FindPlayer(playerId).CitySnapshot;

    public void TakeGold(int playerId)
    {
        var player = RequireActivePlayer(playerId);
        RequireResourcesNotTaken();
        HasTakenResources = true;
        player.OnAddGold(this, new PlayerAddGoldEventArgs { goldToAdd = 2 });
        NotifyTurnState();
    }

    public IReadOnlyList<ITownCard> DrawTownCards(int playerId)
    {
        RequireActivePlayer(playerId);
        RequireResourcesNotTaken();
        if (_townCardsDeck.Count == 0)
            throw new InvalidOperationException("The district deck is empty. Take gold instead.");
        int count = Math.Min(2, _townCardsDeck.Count);
        _drawnTownCards.AddRange(_townCardsDeck.GetRange(0, count));
        _townCardsDeck.RemoveRange(0, count);
        HasTakenResources = true;
        NotifyTurnState();
        return GetDrawnTownCards(playerId);
    }

    public IReadOnlyList<ITownCard> GetDrawnTownCards(int playerId)
    {
        RequireActivePlayer(playerId);
        return _drawnTownCards.Select(card => (ITownCard)card.Clone()).ToList().AsReadOnly();
    }

    public void KeepTownCard(int playerId, int index)
    {
        var player = RequireActivePlayer(playerId);
        if (_drawnTownCards.Count == 0)
            throw new InvalidOperationException("Draw district cards before choosing one.");
        if (index < 0 || index >= _drawnTownCards.Count)
            throw new ArgumentOutOfRangeException(nameof(index));
        var keptCard = _drawnTownCards[index];
        for (int i = 0; i < _drawnTownCards.Count; i++)
            if (i != index)
                _townCardsDeck.Add(_drawnTownCards[i]);
        _drawnTownCards.Clear();
        player.OnAddTownCards(this, new PlayerAddTownCardsEventArgs { addTownCardDeck = [keptCard] });
        NotifyTurnState();
    }

    public void BuildQuarter(int playerId, int handIndex)
    {
        var player = RequireActivePlayer(playerId);
        RequireResourcesResolved();
        if (HasBuiltThisTurn)
            throw new InvalidOperationException("Only one district may be built per turn.");
        player.ValidateBuilding(handIndex);
        HasBuiltThisTurn = true;
        player.Build(handIndex);
        NotifyTurnState();
    }

    public void EndTurn(int playerId)
    {
        RequireActivePlayer(playerId);
        RequireResourcesResolved();
        NotifyTurnState(PlayerTurnStage.Inactive);
        _turnIndex++;
        if (_turnIndex < _turnOrder.Count)
        {
            BeginTurn();
            return;
        }

        ResetTurnState();
        Phase = GamePhase.ReadyForCharacterSelection;
        StartCharacterSelection();
    }

    private Player FindPlayer(int playerId) => _players.SingleOrDefault(player => player.GetId() == playerId)
        ?? throw new ArgumentException("The player does not belong to this game.", nameof(playerId));

    private Player RequireActivePlayer(int playerId)
    {
        if (Phase != GamePhase.PlayerTurns || CurrentPlayerId != playerId)
            throw new InvalidOperationException("It is not this player's turn.");
        return _turnOrder[_turnIndex];
    }

    private void RequireResourcesNotTaken()
    {
        if (HasTakenResources)
            throw new InvalidOperationException("Resources may only be taken once per turn.");
    }

    private void RequireResourcesResolved()
    {
        if (!HasTakenResources)
            throw new InvalidOperationException("Take resources before building or ending the turn.");
        if (_drawnTownCards.Count != 0)
            throw new InvalidOperationException("Choose a drawn district card first.");
    }

    private void StartTurns()
    {
        _turnOrder.Clear();
        _turnOrder.AddRange(_selectionOrder.OrderBy(player => player.CharacterRank));
        _turnIndex = 0;
        Phase = GamePhase.PlayerTurns;
        BeginTurn();
    }

    private void BeginTurn()
    {
        HasTakenResources = false;
        HasBuiltThisTurn = false;
        var player = _turnOrder[_turnIndex];
        NotifyTurnState();
        TurnStarted?.Invoke(this, new TurnStartedEventArgs(player.GetId(), player.CharacterRank, _roundNumber));
    }

    private void NotifyTurnState()
    {
        var stage = _drawnTownCards.Count != 0 ? PlayerTurnStage.KeepTownCard
            : !HasTakenResources ? PlayerTurnStage.TakeResources : PlayerTurnStage.TurnActions;
        NotifyTurnState(stage);
    }

    private void NotifyTurnState(PlayerTurnStage stage)
    {
        var player = _turnOrder[_turnIndex];
        OnTurnStateChanged = player.OnTurnStateChanged;
        OnTurnStateChanged(this, new PlayerTurnStateChangedEventArgs(
            RoundNumber, stage, player.CharacterName!, player.CharacterRank,
            stage == PlayerTurnStage.TurnActions && !HasBuiltThisTurn,
            _drawnTownCards.Select(card => (ITownCard)card.Clone()).ToList().AsReadOnly()));
    }

    private void ResetTurnState()
    {
        _turnOrder.Clear();
        _turnIndex = 0;
        _drawnTownCards.Clear();
        HasTakenResources = false;
        HasBuiltThisTurn = false;
        Phase = GamePhase.NotStarted;
    }
}
