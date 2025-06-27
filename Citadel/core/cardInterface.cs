namespace Citadel
{

    public interface ICard : ICloneable
    {
        public string Name { get; }
        public string Description { get; }
        public void Action();
    }
}