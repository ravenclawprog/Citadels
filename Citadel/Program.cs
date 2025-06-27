using System.Collections.Generic;
using Citadel;
class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine("Hello, World!");

        IPlayerCard myCard = new KingPlayerCard();

        myCard.Action();
        Console.WriteLine($"My Player Card Name: {myCard.Name}!");
        TownCardCreator creator = new BasicTownCardCreator();
        List<ITownCard> cardDeck = new List<ITownCard>();
        cardDeck.Add(creator.CreateCard(QuarterType.Military, 2, "Coliseum"));
        cardDeck[0].Action();

    }
}

