using System;
using System.Globalization;
using System.IO;

namespace SqlUtils
{
    internal class PathUtil
    {
        internal static string EnsureFullPath(string filePath)
        {
            return Path.GetFullPath(filePath);
        }

        internal static bool IsContained(string root, string path)
        {
            if (!Path.IsPathRooted(root) || !Path.IsPathRooted(path))
            {
                throw new ArgumentException("Path must be rooted.");
            }
            path = Path.GetDirectoryName(path);
            if (!path.EndsWith(Path.DirectorySeparatorChar.ToString(), true, CultureInfo.InvariantCulture))
            {
                path = path + Path.DirectorySeparatorChar.ToString();
            }
            root = Path.GetFullPath(root);
            path = Path.GetFullPath(path);
            return path.StartsWith(root, true, CultureInfo.InvariantCulture);
        }
    }
}

