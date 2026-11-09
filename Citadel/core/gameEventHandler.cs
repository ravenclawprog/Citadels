namespace Citadel
{
    public partial class Game
    {
        public void OnChoosedPlayerCard(object? sender, Player.GameChoosedPlayerCardEventArgs e)
        {
            ArgumentNullException.ThrowIfNull(e);
            if (e.choosedPlayerCard == null)
                throw new UnchoosedPlayerCard("player didn't choosed a player card.");
            if (sender is not Player player || !_players.Contains(player))
                throw new InvalidOperationException("The player does not belong to this game.");
            ChooseCharacter(player.GetId(), new BaseGamePrerequisite().Sequence(e.choosedPlayerCard));
        }
    }
}
