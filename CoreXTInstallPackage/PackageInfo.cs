using NuGet.Frameworks;
using NuGet.Versioning;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoreXTInstallPackage
{
	public class PackageInfo
	{
        public string Id { get; set; }

        public NuGetVersion Version { get; set; }

        public NuGetFramework? Tfm { get; set; }
    }
}
