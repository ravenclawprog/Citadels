using System;
namespace Citadel
{
    public class ListRandomizer
    {
        public static int seed = 0;
        public static void Permutate<T>(ref List<T> values)
        {
            seed = (int)DateTime.UtcNow.Ticks;
            Random rnd = new Random(seed);
            int numberOfPermuataion = values.Count * 3;
            for (int i = 0; i < numberOfPermuataion; i++)
            {
                int indexFirst = rnd.Next(0, values.Count);
                int indexSecond = rnd.Next(0, values.Count);
                T tmp = values[indexFirst];
                values[indexFirst] = values[indexSecond];
                values[indexSecond] = tmp;
            }
        }
    }
}
