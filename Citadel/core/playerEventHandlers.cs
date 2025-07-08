namespace Citadel
{
    public partial class Player
    {
        public void OnSendGold(object? sender, Game.SendGoldToPlayerEventArgs e)
        {
            AddGold(e.goldToAdd);
        }
        public void OnSendTownCards(object? sender, Game.SendTownCardsToPlayerEventArgs e)
        {
            _townCardsDeck.AddRange(e.startTownCardDeck);
        }
        public void OnCoronation(object? sender, Game.CoronationEventArgs e)
        {
            Crown = true;
        }

        public void OnDecoronation(object? sender, Game.DecoronationEventArgs e)
        {
            Crown = false;
        }
    }
}