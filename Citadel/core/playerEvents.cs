namespace Citadel
{
    public partial class Player
    {
        public class ViewerAttachEventArgs : EventArgs
        {
            public int playerId = 0;
            public string playerName = "";
        }
        protected EventHandler<ViewerAttachEventArgs>? OnViewerAttach;
        public class ViewerChangeNameEventArgs : EventArgs
        {
            public string newPlayerName = "";
        }
        protected EventHandler<ViewerChangeNameEventArgs>? OnViewerChangeName;

        public class ViewerDetachEventArgs : EventArgs
        {
        }
        protected EventHandler<ViewerDetachEventArgs>? OnViewerDetach;

        public class ViewerGoldUpdatedEventArgs : EventArgs
        {
            public int currentGoldOfPlayer = 0;
        }
        protected EventHandler<ViewerGoldUpdatedEventArgs>? OnViewerGoldUpdated;
        public class ViewerTownCardDeckUpdatedEventArgs : EventArgs
        {
            public List<ITownCard> townCardDeck = new List<ITownCard>();
        }
        protected EventHandler<ViewerTownCardDeckUpdatedEventArgs>? OnViewerTownCardDeckUpdated;
        public class ViewerBuildedTownUpdatedEventArgs : EventArgs
        {
            public List<ITownCard> buildedTown = new List<ITownCard>();
        }
        protected EventHandler<ViewerBuildedTownUpdatedEventArgs>? OnViewerBuildedTownUpdatedEventArgs;
        public class ViewerCoronationChange : EventArgs
        {
            public bool hasCrown = false;
        }
        protected EventHandler<ViewerCoronationChange>? OnViewerCoronationChange;
        public class GameChoosedPlayerCardEventArgs : EventArgs
        {
            public IPlayerCard? choosedPlayerCard = null;
        }
        protected EventHandler<GameChoosedPlayerCardEventArgs>? OnGameChoosedPlayerCard;
        public class ViewerPlayerCardsToChooseEventArgs : EventArgs
        {
            public List<IPlayerCard> playerCardToChoose = new List<IPlayerCard>();
            public List<IPlayerCard> playerCardExcludedOpen = new List<IPlayerCard>();
            public int numberOfPlayerCardExcludedClose = 0;
        }
        protected EventHandler<ViewerPlayerCardsToChooseEventArgs>? OnViewerPlayerCardsToChoose;
    }
}