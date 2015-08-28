using System.IO;
using System.Text.RegularExpressions;

namespace Infinni.Node.Packaging
{
	/// <summary>
	/// Элемент каталога установки.
	/// </summary>
	internal sealed class InstallDirectoryItem
	{
		public InstallDirectoryItem(PackageName packageName, string instance = null)
		{
			PackageName = packageName;
			Instance = instance;
		}


		/// <summary>
		/// Наименование пакета.
		/// </summary>
		public readonly PackageName PackageName;

		/// <summary>
		/// Экземпляр пакета.
		/// </summary>
		public readonly string Instance;


		/// <summary>
		/// Возвращает путь к каталогу.
		/// </summary>
		public string GetPath(string rootPath)
		{
			var packageDir = PackageName.ToString();

			if (!string.IsNullOrWhiteSpace(Instance))
			{
				packageDir += "$" + Instance.Trim();
			}

			return Path.Combine(rootPath, packageDir);
		}


		public static InstallDirectoryItem Parse(string path)
		{
			if (!string.IsNullOrWhiteSpace(path))
			{
				var directoryName = Path.GetFileName(path);

				if (!string.IsNullOrWhiteSpace(directoryName))
				{
					var directoryInfo = Regex.Match(directoryName, @"^(?<Id>.*?)\.(?<Version>[0-9]+(\.[0-9]+(\.[0-9]+(\.[0-9]+){0,1}){0,1}){0,1}(\-.*?){0,1})(\$(?<Instance>.*?)){0,1}$", RegexOptions.Compiled);

					if (directoryInfo.Success)
					{
						var packageId = directoryInfo.Groups["Id"].Value;
						var packageVersion = directoryInfo.Groups["Version"].Value;
						var packageInstance = directoryInfo.Groups["Instance"].Value;

						return new InstallDirectoryItem(new PackageName(packageId, packageVersion), packageInstance);
					}
				}
			}

			return null;
		}
	}
}