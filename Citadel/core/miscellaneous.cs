using System;
using System.IO;
using System.Xml;
using System.Xml.Serialization;

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
    public static class XmlExtension
    {
        public static string Serialize<T>(this T value)
        {
            if (value == null) return string.Empty;

            var xmlSerializer = new XmlSerializer(typeof(T));

            using (var stringWriter = new StringWriter())
            {
                using (var xmlWriter = XmlWriter.Create(stringWriter, new XmlWriterSettings { Indent = true }))
                {
                    xmlSerializer.Serialize(xmlWriter, value);
                    return stringWriter.ToString();
                }
            }
        }
    }
    
}
