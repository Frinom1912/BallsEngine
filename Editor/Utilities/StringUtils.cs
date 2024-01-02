using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Editor.Utilities
{
    public static class StringUtils
    {
        public static string EnsureDirectorySeparator(string path)
        {
            string separator1 = Path.DirectorySeparatorChar.ToString();
            string separator2 = Path.AltDirectorySeparatorChar.ToString();
            path = path.TrimEnd();
            if (path.EndsWith(separator1) || path.EndsWith(separator2))
                return path;

            if (path.Contains(separator2))
                return path + separator2;

            return path + separator1;
        }
    }
}
