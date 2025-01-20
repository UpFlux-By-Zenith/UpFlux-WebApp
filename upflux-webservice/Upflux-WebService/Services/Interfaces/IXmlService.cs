using System.Xml.Linq;

namespace Upflux_WebService.Services.Interfaces
{
	public interface IXmlService
	{
		XDocument ParseXml(string xml);

		string NormalizeXml(XDocument doc);

		string SignXml(XDocument doc, string signature);
	}
}
