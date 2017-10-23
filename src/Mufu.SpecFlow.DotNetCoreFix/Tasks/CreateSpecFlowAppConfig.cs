using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using System.Xml;

namespace Mufu.SpecFlow.DotNetCoreFix.Tasks
{
    public class CreateSpecFlowAppConfig : Task
    {
        [Required]
        public ITaskItem OutputFile { get; set; }

        [Required]
        public string UnitTestProvider { get; set; }
        
        public override bool Execute()
        {
            var settings = new XmlWriterSettings
            {
                Indent = true
            };

            using (var w = XmlWriter.Create(OutputFile.ItemSpec, settings))
            {
                w.WriteStartElement("configuration");

                w.WriteStartElement("configSections");

                w.WriteStartElement("section");
                w.WriteAttributeString("name", "specFlow");
                w.WriteAttributeString("type", "TechTalk.SpecFlow.Configuration.ConfigurationSectionHandler, TechTalk.SpecFlow");
                w.WriteEndElement();

                w.WriteEndElement();

                w.WriteStartElement("specFlow");

                w.WriteStartElement("unitTestProvider");
                w.WriteAttributeString("name", UnitTestProvider);
                w.WriteEndElement();

                w.WriteEndElement();

                w.WriteEndElement();
            }

            return true;
        }
    }
}
