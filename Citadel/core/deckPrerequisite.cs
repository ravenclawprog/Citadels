namespace Citadel
{
    public abstract class GamePrerequisite
    {
        public abstract List<IPlayerCard> GeneratePlayerCardDeck();
        public abstract List<ITownCard> GenerateTownCardDeck();
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
    }
}