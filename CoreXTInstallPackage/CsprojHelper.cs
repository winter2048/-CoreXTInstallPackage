using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace CoreXTInstallPackage
{
	public class CsprojHelper
	{
		public XDocument projectXml;
		protected string projectFilePath;
		protected HashSet<string> packageNames = new HashSet<string>();
		protected XNamespace projectNamespace;

		protected string replacePattern = @"(?=\.\.\\)(.*?)(?<=[0-9]\\)";

		public CsprojHelper(string filePath)
		{
			this.projectFilePath = filePath;
			projectXml = XDocument.Load(projectFilePath);
			this.projectNamespace = projectXml.Root.GetDefaultNamespace();
			projectXml.Root.Attributes().Where(a => a.IsNamespaceDeclaration).Remove();
			projectXml.Descendants().Attributes().Where(a => a.IsNamespaceDeclaration).Remove();
			projectXml.Descendants().Where(e => e.Name.Namespace != XNamespace.None).ToList()
				.ForEach(e => e.Name = e.Name.LocalName);
		}

		private string PackageName(string hintPath)
		{
			string packageName = "";
			// $(PkgAzure_FrontDoor_SharedSchemas_DataConversion)\lib\netstandard2.0...
			//if (hintPath.StartsWith("$(Pkg"))
			//{
			//	Match match = Regex.Match(hintPath, @"(?<=\$\(Pkg)(.*?)(?=_[0-9]|\))", RegexOptions.IgnoreCase);
			//	packageName = match.Groups[0].Value.Replace("_", ".");
			//}
			// ..\..\..\..\packages\ApSecretStore.1.10.5134758.5257447\lib\net4.0...
			// $(ROOT)\packages\Microsoft.Cloud.InstrumentationFramework.Metrics.Extensions.$(Microsoft_Cloud_InstrumentationFramework_Extensions_Version)
			if (hintPath.Contains("packages\\"))
			{
				Match match = Regex.Match(hintPath, @"(?<=packages\\)(.*?)(?=\.[0-9]|\.\$\()", RegexOptions.IgnoreCase);
				packageName = match.Groups[0].Value;
			}

			return packageName;
		}


		public string GetReference()
		{
			string sss = "";
			foreach (var itemGroup in projectXml.Root.Descendants("ItemGroup").ToList())
			{
				foreach (var reference in itemGroup.Descendants( "Reference").ToList())
				{
					if (reference.Element("HintPath") != null)
					{
						var hintPath = reference.Element( "HintPath");
						var pkgName = PackageName(hintPath.Value);
						if (pkgName != "")
						{
							hintPath.Value = Regex.Replace(hintPath.Value, replacePattern, @$"$(Pkg{pkgName.Replace('.', '_')})\");
						}

						sss += "\n" + reference.ToString();
					}
				}
			}

			return sss;
		}
	}
}
