using NuGet.Common;
using NuGet.Protocol;
using NuGet.Protocol.Core.Types;
using NuGet.Versioning;

namespace CoreXTInstallPackage
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Task.Run(async () =>
            {
             
              var list =  await PackageHelper.GetDependencysAll("Azure.Identity");
                foreach (var item in list)
                {
                    Console.WriteLine($"Found dependency {item.Id}  {item.VersionRange}");
                }
                //Console.ReadKey();
            }).GetAwaiter().GetResult();
        }
    }
}