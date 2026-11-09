namespace Citadel;

public partial class Player
{
    internal int CharacterRank => _playerCard == null
        ? throw new InvalidOperationException("The player has not chosen a character.")
        : new BaseGamePrerequisite().Sequence(_playerCard);

    internal void AssignCharacter(IPlayerCard card) => _playerCard = card;

    // A UI may submit a choice after receiving OnPlayerCardToChoose.
    public void ChoosePlayerCard(IPlayerCard card)
    {
        ArgumentNullException.ThrowIfNull(card);
        if (OnGameChoosedPlayerCard == null)
            throw new InvalidOperationException("The player is not attached to a game.");
        OnGameChoosedPlayerCard(this, new GameChoosedPlayerCardEventArgs { choosedPlayerCard = card });
    }
}
