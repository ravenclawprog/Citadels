using System.ComponentModel;
namespace Citadel
{
    public partial class Game
    {
        public class PlayerAddGoldEventArgs : EventArgs
        {
            public int goldToAdd = 0;
        }
        protected EventHandler<PlayerAddGoldEventArgs>? OnAddGold;
        public class PlayerAddTownCardsEventArgs : EventArgs
        {
            public List<ITownCard> addTownCardDeck = new List<ITownCard>();
        }
        protected EventHandler<PlayerAddTownCardsEventArgs>? OnAddTownCards;

        public class PlayerCoronationEventArgs : EventArgs
        {
        }
        protected EventHandler<PlayerCoronationEventArgs>? OnCoronation;
        public class PlayerDecoronationEventArgs : EventArgs
        {
        }
        protected EventHandler<PlayerDecoronationEventArgs>? OnDecoronation;
        public class PlayerChoosePlayerCardEventArgs : EventArgs
        {
            public int roundNumber;
            public IReadOnlyList<CharacterChoice> characters = Array.Empty<CharacterChoice>();
            public List<IPlayerCard> playerCardToChoose = new List<IPlayerCard>();
            public List<IPlayerCard> playerCardExcludedOpen = new List<IPlayerCard>();
            public int numberOfPlayerCardExcludedClose = 0;
        }
        protected EventHandler<PlayerChoosePlayerCardEventArgs>? OnChoosePlayerCard;

        public sealed class PlayerCharacterAssignedEventArgs(IPlayerCard? character, int? rank) : EventArgs
        {
            public IPlayerCard? Character { get; } = character;
            public int? Rank { get; } = rank;
        }
        protected EventHandler<PlayerCharacterAssignedEventArgs>? OnAssignCharacter;

        public sealed class PlayerTurnStateChangedEventArgs(int roundNumber, PlayerTurnStage stage,
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
        protected EventHandler<PlayerTurnStateChangedEventArgs>? OnTurnStateChanged;

        public abstract class PlayerInteractionEventArgs(int playerId, int roundNumber) : EventArgs
        {
            public int PlayerId { get; } = playerId;
            public int RoundNumber { get; } = roundNumber;
        }

        public sealed class PlayerChooseCharacterEventArgs(int playerId, int roundNumber,
            IReadOnlyList<CharacterChoice> characters) : PlayerInteractionEventArgs(playerId, roundNumber)
        {
            public IReadOnlyList<CharacterChoice> Characters { get; } = characters;
        }

        public abstract class PlayerTurnEventArgs(int playerId, int roundNumber, string characterName,
            int characterRank, int gold) : PlayerInteractionEventArgs(playerId, roundNumber)
        {
            public string CharacterName { get; } = characterName;
            public int CharacterRank { get; } = characterRank;
            public int Gold { get; } = gold;
        }

        public sealed class PlayerTakeResourcesEventArgs(int playerId, int roundNumber, string characterName,
            int characterRank, int gold) : PlayerTurnEventArgs(playerId, roundNumber, characterName, characterRank, gold);

        public sealed class PlayerKeepTownCardEventArgs(int playerId, int roundNumber, string characterName,
            int characterRank, int gold, IReadOnlyList<ITownCard> cards)
            : PlayerTurnEventArgs(playerId, roundNumber, characterName, characterRank, gold)
        {
            public IReadOnlyList<ITownCard> Cards { get; } = cards;
        }

        public sealed class PlayerTurnActionEventArgs(int playerId, int roundNumber, string characterName,
            int characterRank, int gold, IReadOnlyList<ITownCard> hand,
            IReadOnlyList<ITownCard> city, bool canBuild)
            : PlayerTurnEventArgs(playerId, roundNumber, characterName, characterRank, gold)
        {
            public IReadOnlyList<ITownCard> Hand { get; } = hand;
            public IReadOnlyList<ITownCard> City { get; } = city;
            public bool CanBuild { get; } = canBuild;
        }

        public sealed class PlayerActionRejectedEventArgs(int playerId, int roundNumber, Exception error)
            : PlayerInteractionEventArgs(playerId, roundNumber)
        {
            public Exception Error { get; } = error;
        }

        protected EventHandler<PlayerChooseCharacterEventArgs>? OnChooseCharacter;
        protected EventHandler<PlayerTakeResourcesEventArgs>? OnTakeResources;
        protected EventHandler<PlayerKeepTownCardEventArgs>? OnKeepTownCard;
        protected EventHandler<PlayerTurnActionEventArgs>? OnTurnAction;
        protected EventHandler<PlayerActionRejectedEventArgs>? OnRejectAction;
    }
}