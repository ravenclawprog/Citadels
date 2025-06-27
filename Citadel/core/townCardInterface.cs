namespace Citadel
{

    public enum QuarterType
    {
        Default,
        Royal,
        Commercial,
        Religious,
        Military,
        Special
    };
    public interface ITownCard : ICard
    {
        public QuarterType TypeOfQuarter { get; }
        public int Price { get; }
    }
}
