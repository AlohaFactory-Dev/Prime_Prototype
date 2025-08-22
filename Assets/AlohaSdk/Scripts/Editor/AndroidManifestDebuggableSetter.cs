using System.Collections.Generic;
using System.IO;
using System.Xml.Linq;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;

namespace Aloha.Sdk.Editor
{
    class AndroidManifestDebuggableSetter : IPreprocessBuildWithReport
    {
        public int callbackOrder { get { return 100; } }

        public void OnPreprocessBuild(BuildReport report)
        {
            XDocument manifest = XDocument.Load("Assets/Plugins/Android/AndroidManifest.xml");
        
            List<XNode> nodeList = new List<XNode>(manifest.Descendants());
            foreach (XNode node in nodeList)
            {
                XElement element = node.Parent;
            
                if (element == null) continue;
                if (element.Name != "application") continue;
            
                List<XAttribute> attributes = new List<XAttribute>(element.Attributes());
                foreach (XAttribute attribute in attributes)
                {
                    if (attribute.Name.LocalName == "debuggable")
                    {
                        attribute.SetValue("false");
                        break;
                    }
                }
            
                break;
            }

            using StreamWriter writer = new StreamWriter("Assets/Plugins/Android/AndroidManifest.xml");
            manifest.Save(writer);
        }
    }
}