namespace Infinni.Node.Packaging
{
    /// <summary>
    /// Фабрика для создания <see cref="IPackageRepositoryManager"/>.
    /// </summary>
    public interface IPackageRepositoryManagerFactory
    {
        /// <summary>
        /// Создает хранилище пакетов.
        /// </summary>
        /// <param name="packageSources">Список источников пакетов.</param>
        IPackageRepositoryManager Create(params string[] packageSources);
    }
}