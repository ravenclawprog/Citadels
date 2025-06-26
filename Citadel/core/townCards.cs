namespace Citadel;

public class TownCard : ITownCard
{

    public QuarterType TypeOfQuarter { get { return _quarterType; } }
    public int Price { get { return _price; } }

    public String Name { get { return _name; } }
    public string Description { get { return "I can rule!"; } }
    public void Action()
    {
        Console.WriteLine("Name:{0}\nDescription:{1}\nQuearter type:{2}", _name, _description, _quarterType.ToString());
    }
    public TownCard(QuarterType q, int price, String name = "Default town card", String description = "Default town card description")
    {
        _quarterType = q;
        _price = price;
        _name = name;
        _description = description;
    }
    private QuarterType _quarterType;
    private int _price;
    private String _name;
    private String _description;
}