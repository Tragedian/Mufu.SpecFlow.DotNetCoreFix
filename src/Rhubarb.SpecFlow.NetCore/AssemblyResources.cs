using System.Resources;

namespace Rhubarb.SpecFlow.NetCore
{
    internal static class AssemblyResources
    {
        static AssemblyResources()
        {
            var thisType = typeof(AssemblyResources);

            PrimaryResources = new ResourceManager($"{thisType.Namespace}.Resources.Strings", thisType.Assembly);
        }

        public static ResourceManager PrimaryResources { get; }
    }
}
