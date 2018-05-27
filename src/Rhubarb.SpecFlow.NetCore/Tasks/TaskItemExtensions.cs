using System;
using Microsoft.Build.Framework;

namespace Rhubarb.SpecFlow.NetCore.Tasks
{ 
    public static class TaskItemExtensions
    {
        public static string GetFullPath(this ITaskItem item)
        {
            if (item == null)
            {
                throw new ArgumentNullException(nameof(item));
            }

            return item.GetMetadata("FullPath");
        }

        public static string GetRootDir(this ITaskItem item)
        {
            if (item == null)
            {
                throw new ArgumentNullException(nameof(item));
            }

            return item.GetMetadata("RootDir");
        }

        public static string GetDirectory(this ITaskItem item)
        {
            if (item == null)
            {
                throw new ArgumentNullException(nameof(item));
            }

            return item.GetMetadata("Directory");
        }
    }
}
