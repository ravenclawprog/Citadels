using System.ComponentModel;
namespace Citadel
{
    public partial class Game
    {
        public class PlayerAddGoldEventArgs : EventArgs
        {
            public int goldToAdd = 0;
        }
        protected EventHandler<PlayerAddGoldEventArgs> OnAddGold;
        public class PlayerAddTownCardsEventArgs : EventArgs
        {
            public List<ITownCard> addTownCardDeck = new List<ITownCard>();
        }
        protected EventHandler<PlayerAddTownCardsEventArgs> OnAddTownCards;

        public class PlayerCoronationEventArgs : EventArgs
        {
        }
        protected EventHandler<PlayerCoronationEventArgs> OnCoronation;
        public class PlayerDecoronationEventArgs : EventArgs
        {
        }
        protected EventHandler<PlayerDecoronationEventArgs> OnDecoronation;
        public class PlayerChoosePlayerCardEventArgs : EventArgs
        {
            List<IPlayerCard> playerCardToChoose = new List<IPlayerCard>();
            List<IPlayerCard> playerCardExcludedOpen = new List<IPlayerCard>();
            int numberOfPlayerCardExcludedClose = 0;
        }
        protected EventHandler<PlayerChoosePlayerCardEventArgs> OnChoosePlayerCard;
    }
}