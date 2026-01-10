using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSC418ConsoleApp
{
    public static class Globals
    {
        public readonly static string _projectRoot = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", ".."));
        public readonly static string _targetFolder = Path.Combine(_projectRoot, "temp");
    }
}
