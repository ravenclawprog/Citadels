namespace Citadel
{
    public class KingPlayerCard : IPlayerCard
    {
        public string Name { get { return "King"; } }
        public string Description { get { return "I can rule!"; } }
        public void Action()
        {
            Console.WriteLine("I get the crown!");
        }
        public QuarterType FavoriteQuarterType { get { return QuarterType.Royal; } }
        public object Clone()
        {
            var copy = new KingPlayerCard();
            return copy;
        }
    }
    public class AssassinPlayerCard : IPlayerCard
    {
        public string Name { get { return "Assassin"; } }
        public string Description { get { return "Can kill person by a sight"; } }
        public void Action()
        {
            Console.WriteLine("Oh yes, here i go again");
        }
        public QuarterType FavoriteQuarterType { get { return QuarterType.Default; } }
        public object Clone()
        {
            var copy = new AssassinPlayerCard();
            return copy;
        }
    }
    public class ThiefPlayerCard : IPlayerCard
    {
        public string Name { get { return "Thief"; } }
        public string Description { get { return "Can stole everything. But only gold"; } }
        public void Action()
        {
            Console.WriteLine("All your gold are belong to mine");
        }
        public QuarterType FavoriteQuarterType { get { return QuarterType.Default; } }
        public object Clone()
        {
            var copy = new ThiefPlayerCard();
            return copy;
        }
    }
    public class SorcererPlayerCard : IPlayerCard
    {
        public string Name { get { return "Sorcerer"; } }
        public string Description { get { return "I can do some magic. Get the card to me!"; } }
        public void Action()
        {
            Console.WriteLine("Now i owe a big deck");
        }
        public QuarterType FavoriteQuarterType { get { return QuarterType.Default; } }
        public object Clone()
        {
            var copy = new SorcererPlayerCard();
            return copy;
        }
    }
    public class BishopPlayerCard : IPlayerCard
    {
        public string Name { get { return "Bishop"; } }
        public string Description { get { return "Сondottiere can do no harm. Get gold for Religious Quarter type"; } }
        public void Action()
        {
            Console.WriteLine("!!!");
        }
        public QuarterType FavoriteQuarterType { get { return QuarterType.Religious; } }
        public object Clone()
        {
            var copy = new BishopPlayerCard();
            return copy;
        }
    }
    public class СondottierePlayerCard : IPlayerCard
    {
        public string Name { get { return "Сondottiere"; } }
        public string Description { get { return "Can destroy town quartal"; } }
        public void Action()
        {
            Console.WriteLine("Wreck it all");
        }
        public QuarterType FavoriteQuarterType { get { return QuarterType.Military; } }
        public object Clone()
        {
            var copy = new СondottierePlayerCard();
            return copy;
        }
    }
    public class ArchitectPlayerCard : IPlayerCard
    {
        public string Name { get { return "Architect"; } }
        public string Description { get { return "Can build 3 quarter at once. Also can get gold and card simultaniously."; } }
        public void Action()
        {
            Console.WriteLine("We can built it all");
        }
        public QuarterType FavoriteQuarterType { get { return QuarterType.Default; } }
        public object Clone()
        {
            var copy = new ArchitectPlayerCard();
            return copy;
        }
    }
    public class MerchantPlayerCard : IPlayerCard
    {
        public string Name { get { return "Merchant"; } }
        public string Description { get { return "Give me more gold."; } }
        public void Action()
        {
            Console.WriteLine("I can buy everything");
        }
        public QuarterType FavoriteQuarterType { get { return QuarterType.Commercial; } }
        public object Clone()
        {
            var copy = new MerchantPlayerCard();
            return copy;
        }
    }
}