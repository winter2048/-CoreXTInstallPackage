using NuGet.Common;
using NuGet.Frameworks;
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

				//				var list = await PackageHelper.GetDependencysAll("Azure.Identity", NuGetFramework.Parse("net48"), "1.3.0");
				//				await Console.Out.WriteLineAsync("corext.config: ");
				//				foreach (var item in list)
				//				{
				//					await Console.Out.WriteLineAsync(@$"<package id=""{item.Id}"" version=""{item.Version.MinVersion?.OriginalVersion}"" />");
				//				}

				//                await Console.Out.WriteLineAsync("\nReference: ");
				//                foreach (var item in list)
				//				{
				//					//Console.WriteLine($"Found dependency {item.Tfm}  {item.Id}  {item.Version}");

				//                    await Console.Out.WriteLineAsync(@$"<Reference Include=""{item.Id}"">
				//    <HintPath>$(Pkg{item.Id.Replace(".","_")})\lib\{item.Tfm.GetNetPath()}\{item.Id}.dll</HintPath>
				//</Reference>");
				//                }

				CsprojHelper csprojHelper = new CsprojHelper(@"C:\Users\v-haitaojin\OneDrive - Microsoft\other\ConsoleApp2\TeskPkg3\TeskPkg3.csproj");
				var pp = csprojHelper.GetReference();
			}).GetAwaiter().GetResult();
        }
    }
}