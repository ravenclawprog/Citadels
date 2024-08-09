namespace Citadel;

public enum QuarterType
{
    Royal,
    Commercial,
    Religious,
    Military
};
public interface ITownCard  : ICard
{
    public QuarterType TypeOfQuarter{get;set;}
    public int Price{get;set;}
}