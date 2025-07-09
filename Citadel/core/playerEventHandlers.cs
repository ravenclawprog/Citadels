namespace Citadel
{
    public partial class Player
    {
        public void OnAddGold(object? sender, Game.PlayerAddGoldEventArgs e)
        {
            AddGold(e.goldToAdd);
        }
        public void OnAddTownCards(object? sender, Game.PlayerAddTownCardsEventArgs e)
        {
            AppendTownCards(e.addTownCardDeck);
        }
        public void OnCoronation(object? sender, Game.PlayerCoronationEventArgs e)
        {
            Crown = true;
        }

        public void OnDecoronation(object? sender, Game.PlayerDecoronationEventArgs e)
        {
            Crown = false;
        }
        public void OnChoosePlayerCard(object? sender, Game.PlayerDecoronationEventArgs e)
        {
            
        }
    }
}