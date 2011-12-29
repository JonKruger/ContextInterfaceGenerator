using System.Xml;

namespace ContextInterfaceGenerator
{
    public class ContextDefinition
    {
        public ContextDefinition(XmlNode node)
        {
            SetEntityNamespace(node);
            SetClassName(node);
            SetAccessModifier(node);

        }

        private void SetAccessModifier(XmlNode node)
        {
            XmlAttribute accessModifier = node.Attributes["AccessModifier"];
            AccessModifier = accessModifier != null 
                ? accessModifier.InnerText.ToLower()
                : "public";
        }

        private void SetClassName(XmlNode node)
        {
            XmlAttribute className = node.Attributes["Class"];
            if (className != null)
                ClassName = className.InnerText;
        }

        private void SetEntityNamespace(XmlNode node)
        {
            XmlAttribute nameSpace = node.Attributes["EntityNamespace"];
            if (nameSpace != null)
                EntityNamespace = nameSpace.InnerText;
        }

        public string ClassName { get; set; }
        public string EntityNamespace { get; set; }
        public string AccessModifier { get; set; }
    }
}
