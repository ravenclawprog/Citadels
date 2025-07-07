namespace Citadel
{
    public partial class Player
    {
        public void OnPreparation(object? sender, Game.PreparationsEventArgs e)
        {
            AddGold(e.goldToAdd);
            _townCardsDeck.AddRange(e.startTownCardDeck);
        }
    }
}