using System.Xml;

namespace ContextInterfaceGenerator
{
    public class ContextType
    {
        public ContextType(XmlNode node)
        {
            SetMember(node);
            SetClass(node);
        }

        private void SetClass(XmlNode node)
        {
            XmlElement typeNode = node["Type"];
            if (typeNode != null)
            {
                XmlAttribute typeName = typeNode.Attributes["Name"];
                if (typeName != null)
                    ClassName = typeName.InnerText;
            }
        }

        private void SetMember(XmlNode node)
        {
            XmlAttribute member = node.Attributes["Member"];
            if (member != null)
                MemberName = member.InnerText;
        }

        public string ClassName { get; set; }
        public string MemberName { get; set; }
    }
}
