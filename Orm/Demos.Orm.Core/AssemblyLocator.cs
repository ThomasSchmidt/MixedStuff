using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Web;
using System.Web.Compilation;

namespace Demos.Orm.Core
{
	public static class AssemblyLocator
	{
		private static readonly ReadOnlyCollection<Assembly> AllAssemblies = null;
		private static readonly ReadOnlyCollection<Assembly> BinFolderAssemblies = null;

		static AssemblyLocator()
		{
			if (!string.IsNullOrEmpty(HttpRuntime.AppDomainId))
			{
				try
				{
					AllAssemblies = new ReadOnlyCollection<Assembly>(BuildManager.GetReferencedAssemblies().Cast<Assembly>().ToList());
				}
				catch //fallback to appdomain way
				{
					AllAssemblies = new ReadOnlyCollection<Assembly>(AppDomain.CurrentDomain.GetAssemblies().ToList());
				}


				IList<Assembly> binFolderAssemblies = new List<Assembly>();

				string binFolder = HttpRuntime.BinDirectory;
				IList<string> dllFiles = Directory.GetFiles(binFolder, "*.dll", SearchOption.TopDirectoryOnly).ToList();

				foreach (string dllFile in dllFiles)
				{
					AssemblyName assemblyName = AssemblyName.GetAssemblyName(dllFile);

					Assembly locatedAssembly = AllAssemblies.FirstOrDefault(a => AssemblyName.ReferenceMatchesDefinition(a.GetName(), assemblyName));

					if (locatedAssembly != null)
					{
						binFolderAssemblies.Add(locatedAssembly);
					}
				}

				BinFolderAssemblies = new ReadOnlyCollection<Assembly>(binFolderAssemblies);
			}
			else
			{
				//fake it so it works in unit tests
				AllAssemblies = new ReadOnlyCollection<Assembly>(AppDomain.CurrentDomain.GetAssemblies().Cast<Assembly>().ToList());
				List<Assembly> binFolderAssemblies = new List<Assembly>(AllAssemblies);
				BinFolderAssemblies = new ReadOnlyCollection<Assembly>(binFolderAssemblies);
			}
		}

		public static ReadOnlyCollection<Assembly> GetAssemblies()
		{
			return AllAssemblies;
		}

		public static ReadOnlyCollection<Assembly> GetBinFolderAssemblies()
		{
			return BinFolderAssemblies;
		}
	}
}
