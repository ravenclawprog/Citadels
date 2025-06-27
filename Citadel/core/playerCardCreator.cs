namespace Citadel
{
    public class BasicPlayerCardCreator
    {
        static public IPlayerCard CreateCard<T>() where T : new()
        {
            return (IPlayerCard)new T();
        }
    }
}