﻿using System.Management.Automation;
using Sitecore.Install;
using Sitecore.Install.Files;
using Sitecore.Install.Framework;
using Sitecore.Install.Items;
using Sitecore.Install.Utils;

namespace Cognifide.PowerShell.Commandlets.Packages
{
    [Cmdlet(VerbsLifecycle.Install, "Package", SupportsShouldProcess = true)]
    public class InstallPackageCommand : BasePackageCommand
    {
        [Parameter(Position = 0)]
        [Alias("FullName", "FileName")]
        public string Path { get; set; }

        [Parameter(HelpMessage = "Undefined, Overwrite, Merge, Skip, SideBySide")]
        public InstallMode InstallMode { get; set; }

        [Parameter(HelpMessage = "Undefined, Clear, Append, Merge")]
        public MergeMode MergeMode { get; set; }

        protected override void ProcessRecord()
        {
            var fileName = Path;
            PerformInstallAction(
                () =>
                {
                    if (!System.IO.Path.IsPathRooted(fileName))
                    {
                        fileName = FullPackagePath(fileName);
                    }

                    if (ShouldProcess(fileName, "Install package"))
                    {
                        IProcessingContext context = new SimpleProcessingContext();
                        IItemInstallerEvents events =
                            new DefaultItemInstallerEvents(new BehaviourOptions(InstallMode, MergeMode));
                        context.AddAspect(events);
                        IFileInstallerEvents events1 = new DefaultFileInstallerEvents(true);
                        context.AddAspect(events1);
                        var installer = new Installer();
                        installer.InstallPackage(fileName, context);
                    }
                });
        }
    }
}