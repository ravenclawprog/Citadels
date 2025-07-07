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
        
    }
}