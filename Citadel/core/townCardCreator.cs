namespace Citadel
{

    public abstract class TownCardCreator
    {
        public abstract ITownCard CreateCard(QuarterType q, int price, String name);
        public abstract List<ITownCard> CreateSeveralCard(QuarterType q, int price, String name, int quantity);
    }

    public class BasicTownCardCreator : TownCardCreator
    {
        public override ITownCard CreateCard(QuarterType q, int price, String name)
        {
            return new TownCard(q, price, name);
        }
        public override List<ITownCard> CreateSeveralCard(QuarterType q, int price, String name, int quantity = 1)
        {
            List<ITownCard> ret = new List<ITownCard>();
            for (int i = 0; i < quantity; i++)
            {
                ret.Add(new TownCard(q, price, name));
            }
            return ret;
        }
    }
}
