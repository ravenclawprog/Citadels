namespace Citadel;

public abstract class TownCardCreator
{
    public abstract ITownCard CreateCard(QuarterType q, int price, String name);
}

public class BasicTownCardCreator : TownCardCreator
{
    public override ITownCard CreateCard(QuarterType q, int price, String name)
    {
        return new TownCard(q,price,name);
    }
}