using System.Xml.Linq;
using System.Xml;
using Upflux_WebService.Services.Interfaces;

namespace Upflux_WebService.Services
{
	/// <summary>
	/// Service that deals with XML files
	/// </summary>
	public class XmlService : IXmlService
	{
		/// <summary>
		/// Parses a string containing XML into an <see cref="XDocument"/> object.
		/// </summary>
		/// <param name="xml">The XML string to parse.</param>
		/// <returns>
		/// An <see cref="XDocument"/> representing the parsed XML structure.
		/// </returns>
		/// <exception cref="FormatException">
		/// Thrown if the provided XML string is not in a valid XML format.
		/// </exception>
		/// <remarks>
		/// This method uses <see cref="XDocument.Parse"/> for parsing and wraps any exceptions
		/// in a <see cref="FormatException"/> for clarity.
		/// </remarks>
		public XDocument ParseXml(string xml)
		{
			try
			{
				return XDocument.Parse(xml);
			}
			catch (Exception ex)
			{
				throw new FormatException("Invalid XML format", ex);
			}
		}

		/// <summary>
		/// Parses a string containing XML into an <see cref="XDocument"/> object.
		/// </summary>
		/// <param name="xml">The XML string to parse.</param>
		/// <returns>
		/// An <see cref="XDocument"/> representing the parsed XML structure.
		/// </returns>
		/// <exception cref="FormatException">
		/// Thrown if the provided XML string is not in a valid XML format.
		/// </exception>
		/// <remarks>
		/// This method uses <see cref="XDocument.Parse"/> for parsing and wraps any exceptions
		/// in a <see cref="FormatException"/> for clarity.
		/// </remarks>
		public string NormalizeXml(XDocument doc)
		{
			using var stringWriter = new StringWriter();
			using var xmlWriter = XmlWriter.Create(stringWriter, new XmlWriterSettings
			{
				OmitXmlDeclaration = true,
				Indent = false
			});
			doc.Save(xmlWriter);
			return stringWriter.ToString();
		}

		/// <summary>
		/// Adds a digital signature to the root element of an XML document.
		/// </summary>
		/// <param name="doc">The <see cref="XDocument"/> to sign.</param>
		/// <param name="signature">The digital signature to add as a string.</param>
		/// <returns>
		/// A string representation of the signed XML document, with the signature included.
		/// </returns>
		/// <remarks>
		/// The signature is added as a new element named "Signature" within the root element of the XML document.
		/// The XML is returned without formatting or indentation to maintain a consistent structure.
		/// </remarks>
		public string SignXml(XDocument doc, string signature)
		{
			doc.Root?.Add(new XElement("Signature", signature));
			return doc.ToString(SaveOptions.DisableFormatting);
		}
	}
}
