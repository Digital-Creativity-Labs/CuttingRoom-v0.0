using System.IO;
using System.Xml.Serialization;

public static class XmlSerialization
{
	public static string SerializeToXmlString<T>(T obj)
	{
		using (StringWriter stringWriter = new StringWriter())
		{
			XmlSerializer xmlSerializer = new XmlSerializer(typeof(T));

			xmlSerializer.Serialize(stringWriter, obj);

			return stringWriter.ToString();
		}
	}

	public static T DeserializeFromXmlString<T>(string state)
	{
		using (StringReader stringReader = new StringReader(state))
		{
			XmlSerializer xmlSerializer = new XmlSerializer(typeof(T));

			return (T)xmlSerializer.Deserialize(stringReader);
		}
	}

	public static void SerializeToXmlFile<T>(string filePath, T obj, bool append = false)
	{
		using (TextWriter textWriter = new StreamWriter(filePath, append))
		{
			XmlSerializer xmlSerializer = new XmlSerializer(typeof(T));

			xmlSerializer.Serialize(textWriter, obj);
		}
	}

	public static T DeserializeFromXmlFile<T>(string filePath)
	{
		using (TextReader textReader = new StreamReader(filePath))
		{
			XmlSerializer xmlSerializer = new XmlSerializer(typeof(T));

			return (T)xmlSerializer.Deserialize(textReader);
		}
	}
}