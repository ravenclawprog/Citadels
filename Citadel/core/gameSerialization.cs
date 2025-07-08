using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace Citadel
{
    public partial class Game
    {
        public void WriteXml(XmlWriter writer)
        {
            foreach (var player in _players)
            {
                writer.WriteStartElement("player");
                writer.WriteAttributeString("id", player.GetId().ToString());
                writer.WriteEndElement();
            }
        }

        public void ReadXml(XmlReader reader)
        {
            _players.Clear();
            while (reader.ReadToFollowing("player"))
            {
                for (int i = 0; i < reader.AttributeCount; i++)
                {
                    Console.WriteLine("player: {0}",reader.GetAttribute(i));
                }
                // _players.Add(new Player())
                }
            
        }
        
        public XmlSchema GetSchema()
        {
            return new XmlSchema();
        }
    }
}