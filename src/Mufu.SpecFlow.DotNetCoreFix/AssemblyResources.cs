using System.Resources;

namespace Mufu.SpecFlow.DotNetCoreFix
{
    internal static class AssemblyResources
    {
        public static ResourceManager PrimaryResources { get; }
            = new ResourceManager("Mufu.SpecFlow.DotNetCoreFix.Resources.Strings", typeof(AssemblyResources).Assembly);
    }
}
