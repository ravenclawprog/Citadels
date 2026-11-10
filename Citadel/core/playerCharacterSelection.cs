namespace Citadel;

public partial class Player
{
    internal string? CharacterName => _playerCard?.Name;

    private int? _characterRank;

    internal int CharacterRank => _characterRank
        ?? throw new InvalidOperationException("The player has not chosen a character.");

    private void AssignCharacter(IPlayerCard? card, int? rank = null)
    {
        _characterChoices = null;
        _turnState = null;
        _playerCard = card;
        _characterRank = card == null ? null : rank;
        OnViewerCharacterChanged?.Invoke(this, new ViewerCharacterChangedEventArgs(
            card == null ? null : (IPlayerCard)card.Clone(), _characterRank));
    }
}
