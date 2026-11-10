namespace Citadel
{
    public enum PlayerTurnStage
    {
        Inactive,
        TakeResources,
        KeepTownCard,
        TurnActions
    }

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
        public class ViewerPlayerCardsToChooseEventArgs : EventArgs
        {
            public int roundNumber;
            public IReadOnlyList<CharacterChoice> characters = Array.Empty<CharacterChoice>();
            public List<IPlayerCard> playerCardToChoose = new List<IPlayerCard>();
            public List<IPlayerCard> playerCardExcludedOpen = new List<IPlayerCard>();
            public int numberOfPlayerCardExcludedClose = 0;
        }
        protected EventHandler<ViewerPlayerCardsToChooseEventArgs>? OnViewerPlayerCardsToChoose;

        public sealed class ViewerCharacterChangedEventArgs(IPlayerCard? character, int? rank) : EventArgs
        {
            public IPlayerCard? Character { get; } = character;
            public int? Rank { get; } = rank;
        }
        protected event EventHandler<ViewerCharacterChangedEventArgs>? OnViewerCharacterChanged;

        public sealed class GameActionEventArgs(int playerId, PlayerAction action) : EventArgs
        {
            public int PlayerId { get; } = playerId;
            public PlayerAction Action { get; } = action ?? throw new ArgumentNullException(nameof(action));
        }

        public event EventHandler<GameActionEventArgs>? OnGameAction;

        public sealed class ViewerTurnStateChangedEventArgs(int roundNumber, PlayerTurnStage stage,
            string characterName, int characterRank, bool canBuild,
            IReadOnlyList<ITownCard> drawnCards) : EventArgs
        {
            public int RoundNumber { get; } = roundNumber;
            public PlayerTurnStage Stage { get; } = stage;
            public string CharacterName { get; } = characterName;
            public int CharacterRank { get; } = characterRank;
            public bool CanBuild { get; } = canBuild;
            public IReadOnlyList<ITownCard> DrawnCards { get; } = drawnCards;
        }
        protected event EventHandler<ViewerTurnStateChangedEventArgs>? OnViewerTurnStateChanged;
    }
}