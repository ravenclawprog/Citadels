namespace Citadel;

class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine("Hello, World!");

        IPlayerCard myCard = new KingPlayerCard();

        myCard.Action();
        Console.WriteLine($"My Player Card Name: {myCard.Name}!");
    }
}

