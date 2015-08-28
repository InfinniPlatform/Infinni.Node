namespace Infinni.Node.Packaging
{
	/// <summary>
	/// Наименование пакета.
	/// </summary>
	internal sealed class PackageName
	{
		public PackageName(string id, string version)
		{
			Id = id;
			Version = version;
		}


		/// <summary>
		/// ID пакета.
		/// </summary>
		public readonly string Id;

		/// <summary>
		/// Версия пакета.
		/// </summary>
		public readonly string Version;


		public override string ToString()
		{
			return string.Format("{0}.{1}", Id.Trim(), Version.Trim());
		}
	}
}