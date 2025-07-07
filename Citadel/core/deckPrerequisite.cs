namespace Citadel
{
    public abstract class GamePrerequisite
    {
        public abstract List<IPlayerCard> GeneratePlayerCardDeck();
        public abstract List<ITownCard> GenerateTownCardDeck();
        public abstract int Sequence(IPlayerCard playerCard);
    }

    public class BaseGamePrerequisite : GamePrerequisite
    {

        public override List<IPlayerCard> GeneratePlayerCardDeck()
        {
            List<IPlayerCard> ret =
            [
                BasicPlayerCardCreator.CreateCard<AssassinPlayerCard>(),
            BasicPlayerCardCreator.CreateCard<ThiefPlayerCard>(),
            BasicPlayerCardCreator.CreateCard<SorcererPlayerCard>(),
            BasicPlayerCardCreator.CreateCard<KingPlayerCard>(),
            BasicPlayerCardCreator.CreateCard<BishopPlayerCard>(),
            BasicPlayerCardCreator.CreateCard<MerchantPlayerCard>(),
            BasicPlayerCardCreator.CreateCard<ArchitectPlayerCard>(),
            BasicPlayerCardCreator.CreateCard<СondottierePlayerCard>(),
        ];
            return ret;
        }
        public override List<ITownCard> GenerateTownCardDeck()
        {
            TownCardCreator townGenerator = new BasicTownCardCreator();
            List<ITownCard> ret =
            [
                .. townGenerator.CreateSeveralCard(QuarterType.Commercial, 1, "Tavern", 5),
            .. townGenerator.CreateSeveralCard(QuarterType.Commercial, 2, "Market", 4),
            .. townGenerator.CreateSeveralCard(QuarterType.Commercial, 2, "Trading Post", 3),
            .. townGenerator.CreateSeveralCard(QuarterType.Commercial, 3, "Docks", 3),
            .. townGenerator.CreateSeveralCard(QuarterType.Commercial, 4, "Harbour", 3),
            .. townGenerator.CreateSeveralCard(QuarterType.Commercial, 5, "Town Hall", 2),
            .. townGenerator.CreateSeveralCard(QuarterType.Religious, 1, "Temple", 3),
            .. townGenerator.CreateSeveralCard(QuarterType.Religious, 2, "Church", 3),
            .. townGenerator.CreateSeveralCard(QuarterType.Religious, 3, "Monastery", 3),
            .. townGenerator.CreateSeveralCard(QuarterType.Religious, 5, "Cathedral", 2),
            .. townGenerator.CreateSeveralCard(QuarterType.Military, 1, "Watchtower", 3),
            .. townGenerator.CreateSeveralCard(QuarterType.Military, 2, "Prison", 3),
            .. townGenerator.CreateSeveralCard(QuarterType.Military, 3, "Battlefield", 3),
            .. townGenerator.CreateSeveralCard(QuarterType.Military, 5, "Fortress", 2),
            .. townGenerator.CreateSeveralCard(QuarterType.Royal, 3, "Manor", 5),
            .. townGenerator.CreateSeveralCard(QuarterType.Royal, 4, "Castle", 4),
            .. townGenerator.CreateSeveralCard(QuarterType.Royal, 5, "Palace", 3),
            .. townGenerator.CreateSeveralCard(QuarterType.Special, 1, "Haunted City", 2),
            .. townGenerator.CreateSeveralCard(QuarterType.Special, 2, "Keep", 3),
            .. townGenerator.CreateSeveralCard(QuarterType.Special, 5, "Laboratory", 1),
            .. townGenerator.CreateSeveralCard(QuarterType.Special, 5, "Smithy", 1),
            .. townGenerator.CreateSeveralCard(QuarterType.Special, 5, "Graveyard", 1),
            .. townGenerator.CreateSeveralCard(QuarterType.Special, 5, "Observatory", 1),
            .. townGenerator.CreateSeveralCard(QuarterType.Special, 6, "School of magic", 1),
            .. townGenerator.CreateSeveralCard(QuarterType.Special, 6, "Library", 1),
            .. townGenerator.CreateSeveralCard(QuarterType.Special, 6, "Great Wall", 1),
            .. townGenerator.CreateSeveralCard(QuarterType.Special, 7, "University", 1),
            .. townGenerator.CreateSeveralCard(QuarterType.Special, 8, "Dragon Gate", 1),
        ];
            return ret;
        }
        public override int Sequence(IPlayerCard playerCard) {
            if (playerCard is AssassinPlayerCard)
            {
                return 1;
            }
            if (playerCard is ThiefPlayerCard)
            {
                return 2;
            }
            if (playerCard is SorcererPlayerCard)
            {
                return 3;
            }
            if (playerCard is KingPlayerCard)
            {
                return 4;
            }
            if (playerCard is BishopPlayerCard)
            {
                return 5;
            }
            if (playerCard is MerchantPlayerCard)
            {
                return 6;
            }
            if (playerCard is ArchitectPlayerCard)
            {
                return 7;
            }
            if (playerCard is СondottierePlayerCard)
            {
                return 8;
            }
            throw new WrongPlayerCard("Unknown type of Player card:" + playerCard.GetType().ToString());
        }
    }
    
}