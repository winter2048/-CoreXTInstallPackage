using NuGet.Common;
using NuGet.Configuration;
using NuGet.Frameworks;
using NuGet.Packaging;
using NuGet.Packaging.Core;
using NuGet.Protocol;
using NuGet.Protocol.Core.Types;
using NuGet.Versioning;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static NuGet.Frameworks.FrameworkConstants;

namespace CoreXTInstallPackage
{
	public static class PackageHelper
	{
		static ILogger logger = NullLogger.Instance;
		static CancellationToken cancellationToken = CancellationToken.None;
		static SourceCacheContext cache = new SourceCacheContext();
		static SourceRepository repository = Repository.Factory.GetCoreV3("https://api.nuget.org/v3/index.json");

		public static HashSet<IPackageSearchMetadata> packagesCache = new HashSet<IPackageSearchMetadata>();

		public static async Task<IPackageSearchMetadata> GetPackage(string packageId, string? version = null) {
			var packageMetadataResource = await repository.GetResourceAsync<PackageMetadataResource>();
			var findPackageByIdResource = await repository.GetResourceAsync<FindPackageByIdResource>();
			NuGetVersion nuGetVersion;
			if (version != null)
			{
				nuGetVersion = NuGetVersion.Parse(version);
			}
			else
			{
				IEnumerable<NuGetVersion> versions = await findPackageByIdResource.GetAllVersionsAsync(
				packageId,
				cache,
				logger,
				cancellationToken);
				nuGetVersion = versions.Last();
			}

			var packageCache = packagesCache.FirstOrDefault(p => p.Identity.Equals(new PackageIdentity(packageId, nuGetVersion)));
			if (packageCache == null)
			{
				packageCache = await packageMetadataResource.GetMetadataAsync(new PackageIdentity(packageId, nuGetVersion),
				 cache,
				 logger,
				 cancellationToken);
				packagesCache.Add(packageCache);
			}
			return packageCache;
		}

		public static async Task<List<PackageInfo>> GetDependencys(string packageId, NuGetFramework tfm, string? version = null)
		{
			var package = await GetPackage(packageId, version);
			var dependencyGroups = package.DependencySets.Where(p => p.TargetFramework.IsCompatibleFramework(tfm)).OrderByDescending(p => p.TargetFramework.Version).ToList();

			if (dependencyGroups == null || dependencyGroups.Count == 0)
			{
				return new List<PackageInfo>();
			}
			else
			{
				if (dependencyGroups.First().Packages.Any(p => p.Id == "System.Runtime.CompilerServices.Unsafe"))
				{
					await Console.Out.WriteLineAsync("sss");
				}
				var list = new List<PackageInfo>();
				foreach (var item in dependencyGroups.First().Packages)
				{
					var pkg = await GetPackage(item.Id, item.VersionRange.MinVersion?.OriginalVersion);
					list.Add(new PackageInfo
					{
						Id = item.Id,
						Version = item.VersionRange.MinVersion,
						Tfm = pkg.DependencySets.Where(p => p.TargetFramework.IsCompatibleFramework(tfm)).OrderByDescending(p => p.TargetFramework.Version)?.FirstOrDefault().TargetFramework
					});
				}
				return list;
			}
		}

		public static async Task<List<PackageInfo>> GetDependencysAll(string packageId, NuGetFramework tfm, string? version = null)
		{
			List<PackageInfo> list = new List<PackageInfo>();
			var des = await PackageHelper.GetDependencys(packageId, tfm, version);
			foreach (var item in des)
			{
				list.Add(item);
				var ss = await PackageHelper.GetDependencysAll(item.Id, tfm, item.Version.OriginalVersion);
				if (ss.Count > 0)
				{
					list.AddRange(ss);
				}
			}
			return list.GroupBy(p => p.Id).Select(p => p.OrderByDescending(p => p.Version).FirstOrDefault()).ToList();
		}

        public static async Task<List<PackageInfo>> GetPackageAndDependencysAll(string packageId, NuGetFramework tfm, string? version = null)
        {
            List<PackageInfo> list = new List<PackageInfo>();
            var pkg = await GetPackage(packageId, version);
            var dependencyGroups = pkg.DependencySets.Where(p => p.TargetFramework.IsCompatibleFramework(tfm)).OrderByDescending(p => p.TargetFramework.Version).ToList();
            list.Add(new PackageInfo()
            {
                Id = pkg.Identity.Id,
                Version = pkg.Identity.Version,
                Tfm = dependencyGroups.FirstOrDefault()?.TargetFramework
            });
            list.AddRange(await GetDependencysAll(packageId, tfm, version));
            return list;
        }


        static bool IsCompatibleFramework(this NuGetFramework cTfk, NuGetFramework tfm)
		{
			if (tfm.Framework == FrameworkIdentifiers.Net)
			{
				return (cTfk.Framework == FrameworkIdentifiers.Net && cTfk.Version <= tfm.Version) ||
					   (cTfk.Framework == FrameworkIdentifiers.NetStandard && tfm.Version >= new Version(4, 8, 0, 0) && cTfk.Version <= new Version(2, 1, 0, 0)) ||
					   (cTfk.Framework == FrameworkIdentifiers.NetStandard && cTfk.Version <= new Version(2, 0, 0, 0));
			}
			if (tfm.Framework == FrameworkIdentifiers.NetStandard)
			{
				return (cTfk.Framework == FrameworkIdentifiers.NetStandard && cTfk.Version <= tfm.Version);
			}
			if (tfm.Framework == FrameworkIdentifiers.NetCore)
			{
				return (cTfk.Framework == FrameworkIdentifiers.NetCore && cTfk.Version == tfm.Version) ||
					   (cTfk.Framework == FrameworkIdentifiers.NetStandard && cTfk.Version <= new Version(2, 1, 0, 0));
			}
			if (tfm.Framework == FrameworkIdentifiers.NetCoreApp)
			{
				return (cTfk.Framework == FrameworkIdentifiers.NetCoreApp && cTfk.Version == tfm.Version) ||
					   (cTfk.Framework == FrameworkIdentifiers.NetStandard && cTfk.Version <= new Version(2, 1, 0, 0));
			}
			return false;
		}

		public static string GetNetPath(this NuGetFramework cTfk)
		{
			if (cTfk.Framework == FrameworkIdentifiers.Net)
			{
				return $"net{cTfk.Version.Major}{cTfk.Version.Minor}{cTfk.Version.Build}".Replace("0","");
			}
			if (cTfk.Framework == FrameworkIdentifiers.NetStandard)
			{
				return $"netstandard{cTfk.Version.Major}.{cTfk.Version.Minor}";
			}
			return "";
		}
	}
}
