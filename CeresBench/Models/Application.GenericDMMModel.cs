using System;
using System.Threading;
using System.Xml;

namespace CeresBench.Models.Application;

public partial class CeresGenericDMMMModel
{
    private VISAResourceManagerModel _resourceManager;
    public CeresGenericDMMMModel(VISAResourceManagerModel resourceManager)
    {
        _resourceManager = resourceManager;

        XmlDocument xmlDocument = new ();
        xmlDocument.Load("/Assets/ModelApplications.xml");
        var rootElement= xmlDocument.DocumentElement;

        if (rootElement == null)
        {
            return;
        }

        foreach (XmlNode? node in rootElement.ChildNodes)
        {
            if (node?.Name == "Application" && node.Attributes?.GetNamedItem("name")?.Value == "GenericDMMView")
            {
                foreach(XmlNode? childNode in node.ChildNodes)
                {
                    switch (childNode?.Name)
                    {
                        case "MatchRule":
                            switch (childNode?.Attributes?.GetNamedItem("type")?.Value)
                            {
                                case "model":
                                default:
                                    throw new NotImplementedException();
                            }
                        case "Initlization":
                            resourceManager.FormattedIO?.WriteLine(childNode.Value);
                            break;
                        default:
                            throw new NotImplementedException();
                    }
                }
            }
        }
    }
}
