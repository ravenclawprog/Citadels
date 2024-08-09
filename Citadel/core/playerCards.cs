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
    }

}