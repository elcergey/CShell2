using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Versioning;
using NuGet;
using ScriptCs.Contracts;

namespace CShell.Hosting.Package
{
    internal class PackageObject : IPackageObject
    {
        private const string Dll = ".dll";
        private const string Exe = ".exe";
        private readonly IPackage _package;

        public PackageObject(IPackage package, FrameworkName frameworkName)
        {
            _package = package;
            FrameworkName = frameworkName;
            Id = package.Id;
            Version = package.Version.Version;
            TextVersion = package.Version.ToString();
            FrameworkAssemblies = package.FrameworkAssemblies
                .Where(x => x.SupportedFrameworks.Any(y => y == frameworkName))
                .Select(x => x.AssemblyName);

            var dependencies = _package.GetCompatiblePackageDependencies(frameworkName);
            if (dependencies != null)
            {
                Dependencies = dependencies.Select(i => new PackageObject(i.Id) { FrameworkName = frameworkName });
            }
        }

        public PackageObject(string packageId)
        {
            Id = packageId;
        }

        public IEnumerable<string> FrameworkAssemblies { get; private set; }

        public string Id { get; private set; }

        public string TextVersion { get; private set; }

        public Version Version { get; private set; }

        public FrameworkName FrameworkName { get; private set; }

        public IEnumerable<IPackageObject> Dependencies { get; set; }

        public string FullName => Id + "." + TextVersion;

        public IEnumerable<string> GetCompatibleDlls(FrameworkName frameworkName)
        {
            var dlls = _package.GetLibFiles().Where(i => i.EffectivePath.EndsWith(Dll) || i.EffectivePath.EndsWith(Exe));
            IEnumerable<IPackageFile> compatibleFiles;
            VersionUtility.TryGetCompatibleItems(frameworkName, dlls, out compatibleFiles);

            return compatibleFiles != null ? compatibleFiles.Select(i => i.Path) : null;
        }

        public IEnumerable<string> GetContentFiles()
        {
            foreach (var file in _package.GetContentFiles())
            {
                yield return file.Path;
            }
        }
    }
}