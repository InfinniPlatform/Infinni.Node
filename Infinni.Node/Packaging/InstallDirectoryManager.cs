using System;
using System.Collections.Generic;
using System.IO;

using Infinni.Node.Logging;
using Infinni.Node.Properties;

namespace Infinni.Node.Packaging
{
	/// <summary>
	/// Менеджер по работе с каталогом установки.
	/// </summary>
	internal sealed class InstallDirectoryManager : IInstallDirectoryManager
	{
		public InstallDirectoryManager(string rootInstallPath)
		{
			if (string.IsNullOrWhiteSpace(rootInstallPath))
			{
				throw new ArgumentNullException("rootInstallPath");
			}

			_rootInstallPath = rootInstallPath;
		}


		private readonly string _rootInstallPath;


		public string GetPath(PackageName packageName, string instance = null)
		{
			if (packageName == null)
			{
				throw new ArgumentNullException("packageName");
			}

			var installDir = new InstallDirectoryItem(packageName, instance);

			return installDir.GetPath(_rootInstallPath);
		}

		public bool Exists(PackageName packageName, string instance = null)
		{
			var installPath = GetPath(packageName, instance);

			return Directory.Exists(installPath);
		}

		public void Create(PackageName packageName, string instance = null)
		{
			var installPath = GetPath(packageName, instance);

			Directory.CreateDirectory(installPath);
		}

		public void Delete(PackageName packageName, string instance = null)
		{
			var installPath = GetPath(packageName, instance);

			Directory.Delete(installPath, true);
		}

		public void Install(Package package, string instance = null, params string[] files)
		{
			var installPath = GetPath(package.Name, instance);

			foreach (var content in package.Contents)
			{
				foreach (var contentPartItem in content.Parts)
				{
					var part = contentPartItem.Value;

					// Example: lib, src, content
					var partName = contentPartItem.Key;

					// Example: packages/SomePackage.1.2.3/lib/
					var partPath = part.Path.TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar;

					// Example: packages/SomePackage.1.2.3/lib/net45/
					var partFrameworkPath = Path.Combine(part.Path, package.FrameworkName ?? "").TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar;

					// Файлы из раздела 'lib' копируются в корень каталога установки
					if (string.Equals(partName, "lib", StringComparison.OrdinalIgnoreCase))
					{
						foreach (var sourcePath in part.Files)
						{
							// Example: 'packages/SomePackage.1.2.3/lib/path/to/file.dll' -> 'file.dll'
							// Example: 'packages/SomePackage.1.2.3/lib/net45/path/to/file.dll' -> 'file.dll'
							var relativeDestinationPath = Path.GetFileName(sourcePath) ?? "";

							// Example: 'install/SomeApp.4.5.6/file.dll'
							var destinationPath = Path.Combine(installPath, relativeDestinationPath);

							CopyFileWithOverwrite(sourcePath, destinationPath);
						}
					}
					// Файлы остальных разделов копируются с сохранением структуры каталогов
					else
					{
						foreach (var sourcePath in part.Files)
						{
							// Example: 'packages/SomePackage.1.2.3/content/path/to/file' -> 'path/to/file'
							// Example: 'packages/SomePackage.1.2.3/content/net45/path/to/file' -> 'path/to/file'
							var relativeDestinationPath = sourcePath.Substring(sourcePath.StartsWith(partFrameworkPath) ? partFrameworkPath.Length : partPath.Length);

							// Example: 'install/SomeApp.4.5.6/content/SomePackage/path/to/file'
							var destinationPath = Path.Combine(installPath, partName, content.Name.Id, relativeDestinationPath);

							CopyFileWithOverwrite(sourcePath, destinationPath);
						}
					}
				}
			}

			if (files != null)
			{
				// Дополнительные файлы копируются в корень каталога установки
				foreach (var file in files)
				{
					if (!string.IsNullOrWhiteSpace(file))
					{
						var relativeDestinationPath = Path.GetFileName(file);
						var destinationPath = Path.Combine(installPath, relativeDestinationPath);

						CopyFileWithOverwrite(file, destinationPath);
					}
				}
			}
		}

		private static void CopyFileWithOverwrite(string sourcePath, string destinationPath)
		{
			if (File.Exists(destinationPath))
			{
				Log.Default.WarnFormat(Resources.FileHasRewritten, destinationPath, sourcePath);
			}

			var destinationDir = Path.GetDirectoryName(destinationPath) ?? "";

			if (!Directory.Exists(destinationDir))
			{
				Directory.CreateDirectory(destinationDir);
			}

			File.Copy(sourcePath, destinationPath, true);
		}

		public IEnumerable<InstallDirectoryItem> GetItems()
		{
			var directories = Directory.EnumerateDirectories(_rootInstallPath);

			foreach (var path in directories)
			{
				var installDir = InstallDirectoryItem.Parse(path);

				if (installDir != null)
				{
					yield return installDir;
				}
			}
		}
	}
}