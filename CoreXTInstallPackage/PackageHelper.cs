using NuGet.Common;
using NuGet.Packaging.Core;
using NuGet.Protocol;
using NuGet.Protocol.Core.Types;
using NuGet.Versioning;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoreXTInstallPackage
{
    public static class PackageHelper
    {
        static ILogger logger = NullLogger.Instance;
        static CancellationToken cancellationToken = CancellationToken.None;
        static SourceCacheContext cache = new SourceCacheContext();
        static SourceRepository repository = Repository.Factory.GetCoreV3("https://api.nuget.org/v3/index.json");

        static List<string> net48 = new List<string>{ ".NETStandard,Version=v2.1", ".NETStandard,Version=v2.0" };

        public static async Task<List<PackageDependency>> GetDependencys(string packageId, string? version = null)
        {
            FindPackageByIdResource resource = await repository.GetResourceAsync<FindPackageByIdResource>();
            NuGetVersion nuGetVersion = null;
            if (version != null)
            {
                nuGetVersion = NuGetVersion.Parse(version);
            }
            else
            {
                IEnumerable<NuGetVersion> versions = await resource.GetAllVersionsAsync(
                packageId,
                cache,
                logger,
                cancellationToken);
                nuGetVersion = versions.Last();
            }

            var dependencys = await resource.GetDependencyInfoAsync(packageId,
                 nuGetVersion,
                 cache,
                 logger,
                 cancellationToken);

            var list = new List<PackageDependency>();
            var list2 = dependencys.DependencyGroups.Where(p => net48.Contains(p.TargetFramework.DotNetFrameworkName)).ToList();

            if (list2 == null || list2.Count == 0)
            {
                return list;
            }
            else
            {
                return list2[0].Packages.ToList();
            }
        }

        public static async Task<List<PackageDependency>> GetDependencysAll(string packageId, string? version = null)
        {
            List<PackageDependency> list = new List<PackageDependency>();
           //await Task.Delay(2000);
            var des = await PackageHelper.GetDependencys(packageId, version);
            foreach (var item in des)
            {
                list.Add(item);
                var ss = await PackageHelper.GetDependencysAll(item.Id, item.VersionRange.OriginalString);
                if (ss.Count > 0)
                {
                    list.AddRange(ss);
                }
            }
            return list.GroupBy(p => p.Id).Select(p => p.OrderByDescending(p => p.VersionRange.MinVersion).FirstOrDefault()).ToList();
            //return list;
        }
    }
}
