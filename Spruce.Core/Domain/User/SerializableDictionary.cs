using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace Spruce.Core
{
	/// <summary>
	/// Provides a <see cref="Dictionary&lt;TKey, TValue&gt;"/> that is serializable as XML.
	/// </summary>
	/// <remarks>
	/// The dictionary is serialized as the following XML:
	/// <code>
	/// <![CDATA[
	/// <Item>
	///     <Key>MyKey</Key>
	///     <Value>The value</Value>
	/// </Item>
	/// ]]>
	/// </code>
	/// </remarks>
	/// <typeparam name="TKey">The key type</typeparam>
	/// <typeparam name="TValue">The value type</typeparam>
	[XmlRoot("dictionary")]
	public class SerializableDictionary<TKey, TValue> : Dictionary<TKey, TValue>, IXmlSerializable
	{
		#region IXmlSerializable Members
		public System.Xml.Schema.XmlSchema GetSchema()
		{
			return null;
		}

		/// <summary>
		/// Generates an object from its XML representation.
		/// </summary>
		/// <param name="reader">The <see cref="T:System.Xml.XmlReader"/> stream from which the object is deserialized.</param>
		public void ReadXml(System.Xml.XmlReader reader)
		{
			XmlSerializer keySerializer = new XmlSerializer(typeof(TKey));
			XmlSerializer valueSerializer = new XmlSerializer(typeof(TValue));

			bool wasEmpty = reader.IsEmptyElement;
			reader.Read();

			if (wasEmpty)
				return;

			while (reader.NodeType != System.Xml.XmlNodeType.EndElement)
			{
				reader.ReadStartElement("Item");

				reader.ReadStartElement("Key");
				TKey key = (TKey)keySerializer.Deserialize(reader);
				reader.ReadEndElement();

				reader.ReadStartElement("Value");
				TValue value = (TValue)valueSerializer.Deserialize(reader);
				reader.ReadEndElement();

				this.Add(key, value);

				reader.ReadEndElement();
				reader.MoveToContent();
			}
			reader.ReadEndElement();
		}

		/// <summary>
		/// Converts an object into its XML representation.
		/// </summary>
		/// <param name="writer">The <see cref="T:System.Xml.XmlWriter"/> stream to which the object is serialized.</param>
		public void WriteXml(System.Xml.XmlWriter writer)
		{
			XmlSerializer keySerializer = new XmlSerializer(typeof(TKey));
			XmlSerializer valueSerializer = new XmlSerializer(typeof(TValue));

			foreach (TKey key in this.Keys)
			{
				writer.WriteStartElement("Item");

				writer.WriteStartElement("Key");
				keySerializer.Serialize(writer, key);
				writer.WriteEndElement();

				writer.WriteStartElement("Value");
				TValue value = this[key];
				valueSerializer.Serialize(writer, value);
				writer.WriteEndElement();

				writer.WriteEndElement();
			}
		}
		#endregion
	}
}
