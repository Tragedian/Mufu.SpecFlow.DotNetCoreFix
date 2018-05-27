using System.Xml;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

namespace Rhubarb.SpecFlow.NetCore.Tasks
{
    public class SpecFlowGenerateAppConfig : Task
    {
        [Required]
        public ITaskItem OutputAppConfigFile { get; set; }

        [Required]
        public string UnitTestProvider { get; set; }

        public SpecFlowGenerateAppConfig()
            : base(AssemblyResources.PrimaryResources)
        {

        }

        private bool ValidateInputs()
        {
            if (OutputAppConfigFile == null)
            {
                Log.LogErrorWithCodeFromResources(nameof(SpecFlowGenerateAppConfig) + ".NeedsOutputAppConfigFile", nameof(OutputAppConfigFile));
                return false;
            }

            return true;
        }

        public override bool Execute()
        {
            if (!ValidateInputs())
            {
                return false;
            }

            var settings = new XmlWriterSettings
            {
                Indent = true
            };

            using (var w = XmlWriter.Create(OutputAppConfigFile.ItemSpec, settings))
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
