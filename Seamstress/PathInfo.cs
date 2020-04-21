using System;
using System.IO;

namespace Seamstress
{
    public static class PathInfo
    {
        public static string project_directory = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"..\..\.."));
        public static string resources_directory = project_directory + @"\Resources";
        public static string output_directory = project_directory + @"\Output";
    }
}
