namespace Citadel;

public partial class Game
{
    public IReadOnlyList<Player> Players => _players.AsReadOnly();

    private bool _stopRequested;

    public void Run()
    {
        if (Phase == GamePhase.NotStarted)
            throw new InvalidOperationException("Start a game before running it.");
        if (Phase == GamePhase.ReadyForCharacterSelection)
            StartCharacterSelection();

        _stopRequested = false;
        while (!_stopRequested && (Phase is GamePhase.CharacterSelection or GamePhase.PlayerTurns))
        {
            var player = Phase == GamePhase.CharacterSelection
                ? _selectionOrder[_selectionIndex] : _turnOrder[_turnIndex];
            SendInteraction(player);
        }
    }

    private void SendInteraction(Player player)
    {
        int playerId = player.GetId();
        if (Phase == GamePhase.CharacterSelection)
        {
            var characters = _playerCardsDeck
                .Select(card => new CharacterChoice(GetCharacterRank(card), card.Name))
                .ToList().AsReadOnly();
            OnChooseCharacter = player.OnChooseCharacter;
            OnChooseCharacter(this, new PlayerChooseCharacterEventArgs(playerId, RoundNumber, characters));
            return;
        }

        string characterName = CurrentCharacterName!;
        int rank = CurrentCharacterRank!.Value;
        int gold = player.GoldCount;
        var drawn = _drawnTownCards.Select(card => (ITownCard)card.Clone()).ToList().AsReadOnly();
        if (drawn.Count != 0)
        {
            OnKeepTownCard = player.OnKeepTownCard;
            OnKeepTownCard(this, new PlayerKeepTownCardEventArgs(playerId, RoundNumber, characterName, rank, gold, drawn));
        }
        else if (!HasTakenResources)
        {
            OnTakeResources = player.OnTakeResources;
            OnTakeResources(this, new PlayerTakeResourcesEventArgs(playerId, RoundNumber, characterName, rank, gold));
        }
        else
        {
            OnTurnAction = player.OnTurnAction;
            OnTurnAction(this, new PlayerTurnActionEventArgs(playerId, RoundNumber, characterName, rank, gold,
                player.HandSnapshot, player.CitySnapshot, !HasBuiltThisTurn));
        }
    }
}
