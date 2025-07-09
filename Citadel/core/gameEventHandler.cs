namespace Citadel
{
    public partial class Game
    {
        public void OnChoosedPlayerCard(object? sender, Player.GameChoosedPlayerCardEventArgs e)
        {
            if (e.choosedPlayerCard != null)
            {
                _playerCardsDeck.Remove(e.choosedPlayerCard);
            }
            else
            {
                throw new UnchoosedPlayerCard("player didn't choosed a player card.");
            }
            
        }
    }
}