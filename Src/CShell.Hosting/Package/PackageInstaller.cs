﻿using System;
using System.Collections.Generic;
using System.Linq;
using ScriptCs.Contracts;

namespace CShell.Hosting.Package
{
    public class PackageInstaller : IPackageInstaller
    {
        private readonly IInstallationProvider _installer;
        private readonly ILog _logger;

        public PackageInstaller(IInstallationProvider installer, ILogProvider logProvider)
        {
            _installer = installer;
            _logger = logProvider.For<PackageInstaller>();
        }

        public void InstallPackages(IEnumerable<IPackageReference> packageIds, bool allowPreRelease = false)
        {
            packageIds = packageIds?.ToList() ?? throw new ArgumentNullException(nameof(packageIds));

            if (!packageIds.Any())
            {
                _logger.Info("Nothing to install.");
                return;
            }

            var exceptions = new List<Exception>();
            foreach (var packageId in packageIds)
            {
                if (_installer.IsInstalled(packageId, allowPreRelease))
                {
                    continue;
                }

                try
                {
                    _installer.InstallPackage(packageId, allowPreRelease);
                }
                catch (Exception ex)
                {
                    _logger.ErrorException(ex.Message, ex);
                    exceptions.Add(ex);
                }
            }

            if (exceptions.Any())
            {
                throw new AggregateException(exceptions);
            }
        }
    }
}
