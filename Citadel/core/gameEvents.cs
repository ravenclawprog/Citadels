using System.ComponentModel;
namespace Citadel
{
    public partial class Game
    {
        public class PreparationsEventArgs : EventArgs
        {
            public int goldToAdd = 0;
            public List<ITownCard> startTownCardDeck = new List<ITownCard>();
        }
        // protected EventHandler<PreparationsEventArgs> OnPreparation;
        public class SendGoldToPlayerEventArgs : EventArgs
        {
            public int goldToAdd = 0;
        }
        protected EventHandler<SendGoldToPlayerEventArgs> OnSendGold;
        public class SendTownCardsToPlayerEventArgs : EventArgs
        {
            public List<ITownCard> startTownCardDeck = new List<ITownCard>();
        }
        protected EventHandler<SendTownCardsToPlayerEventArgs> OnSendTownCards;

        public class CoronationEventArgs : EventArgs
        {
        }
        protected EventHandler<CoronationEventArgs> OnCoronation;
        public class DecoronationEventArgs : EventArgs
        {
        }
        protected EventHandler<DecoronationEventArgs> OnDecoronation;
    }
}