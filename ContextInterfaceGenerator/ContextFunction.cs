using System.Collections.Generic;
using System.Linq;
using System.Xml;

namespace ContextInterfaceGenerator
{
    public class ContextFunction
    {
        public ContextFunction()
        {
            Parameters = new List<FunctionParameter>();
        }

        public ContextFunction(XmlNode node)
            : this()
        {
            SetIsComposable(node);
            SetReturnType(node);
            SetReturnElement(node);
            SetMethodName(node);
            SetParameters(node);
        }

        private void SetMethodName(XmlNode node)
        {
            XmlAttribute methodAttribute = node.Attributes["Method"];
            if (methodAttribute != null)
                MethodName = methodAttribute.InnerText;
        }

        private void SetParameters(XmlNode node)
        {
            IEnumerable<XmlNode> nodes = node.ChildNodes.OfType<XmlNode>().Where(nd => nd.Name == "Parameter");
            foreach (var parameterNode in nodes)
            {
                string name = parameterNode.Attributes["Parameter"] != null
                                  ? parameterNode.Attributes["Parameter"].InnerText
                                  : parameterNode.Attributes["Name"].InnerText;

                Parameters.Add(new FunctionParameter
                                   {
                                       ByRef = parameterNode.Attributes["Direction"] != null,
                                       Name = name,
                                       Type = parameterNode.Attributes["Type"].InnerText
                                   });
            }
        }

        private void SetReturnElement(XmlNode node)
        {
            XmlElement elementNode = node["ElementType"];
            if (elementNode != null)
            {
                XmlAttribute elementTypeAttribute = elementNode.Attributes["Name"];
                if (elementTypeAttribute != null)
                    ReturnElement = elementTypeAttribute.InnerText;
            }
        }

        private void SetReturnType(XmlNode node)
        {
            XmlElement returnNode = node["Return"];
            if (returnNode != null)
            {
                XmlAttribute returnTypeAttribute = returnNode.Attributes["Type"];
                if (returnTypeAttribute != null)
                    ReturnType = returnTypeAttribute.InnerText;
            }
        }

        private void SetIsComposable(XmlNode node)
        {
            XmlAttribute isComposable = node.Attributes["IsComposable"];
            if (isComposable != null)
                IsComposable = isComposable.InnerText.ToLower() == "true";
        }

        public bool IsComposable { get; set; }
        public string ReturnType { get; set; }
        public string ReturnElement { get; set; }
        public string MethodName { get; set; }
        public List<FunctionParameter> Parameters { get; set; }

        public string GetSignature()
        {
            var list =
                Parameters.Select(
                    prm =>
                    string.Format("{0}{1} {2}", prm.ByRef ? "ref " : string.Empty, ConvertTypeString(prm.Type), prm.Name));
            return string.Join(", ", list.ToArray());
        }

        public string GetCall()
        {
            var list = Parameters.Select(prm => string.Format("{0}{1}", prm.ByRef ? "ref " : string.Empty, prm.Name));
            return string.Join(", ", list.ToArray());
        }

        private string ConvertTypeString(string typeString)
        {
            if (typeString == "System.String")
                return "string";
            else if (typeString.EndsWith("XElement"))
                return typeString;

            return string.Format("{0}?", typeString);
        }

        public string GetSignatureForVB()
        {
            var list =
                Parameters.Select(prm => string.Format("{0}{2} As {1}", prm.ByRef ? "ByRef " : "ByVal ",
                                         ConvertTypeStringForVB(prm.Type), prm.Name));
            return string.Join(", ", list.ToArray());
        }

        public string GetCallForVB()
        {
            var list = Parameters.Select(prm => prm.Name);
            return string.Join(", ", list.ToArray());
        }

        private string ConvertTypeStringForVB(string typeString)
        {
            if (typeString == "System.String")
                return "String";
            else if (typeString.EndsWith("XElement"))
                return typeString;

            return string.Format("Nullable(Of {0})", typeString);
        }
    }
}
