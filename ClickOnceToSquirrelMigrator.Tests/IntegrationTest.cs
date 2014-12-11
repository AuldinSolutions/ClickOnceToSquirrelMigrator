﻿using System;
using System.IO;
using System.Threading.Tasks;
using Xunit;

namespace ClickOnceToSquirrelMigrator.Tests
{
    public class IntegrationTest
    {
        [Fact]
        public async Task FirstStepInstallsSquirrelApp()
        {
            string rootDir;

            using (IntegrationTestHelper.WithTempDirectory(out rootDir))
            {
                using (var updateManager = IntegrationTestHelper.GetSquirrelUpdateManager(rootDir))
                {
                    var migrator = new ClickOnceToSquirrelMigrator(updateManager, IntegrationTestHelper.ClickOnceAppName);

                    await migrator.InstallSquirrel();

                    Assert.True(File.Exists(Path.Combine(rootDir, IntegrationTestHelper.SquirrelAppName, "packages", "RELEASES")));
                }
            }
        }

        [Fact]
        public async Task FirstStepRemovesClickOnceShortcut()
        {
            using (IntegrationTestHelper.WithClickOnceApp())
            {
                var clickOnceInfo = UninstallInfo.Find(IntegrationTestHelper.ClickOnceAppName);

                Assert.True(File.Exists(clickOnceInfo.GetShortcutPath()));

                string rootDir;
                using (IntegrationTestHelper.WithTempDirectory(out rootDir))
                {
                    using (var updateManager = IntegrationTestHelper.GetSquirrelUpdateManager(rootDir))
                    {
                        var migrator = new ClickOnceToSquirrelMigrator(updateManager, IntegrationTestHelper.ClickOnceAppName);

                        await migrator.InstallSquirrel();

                        Assert.False(File.Exists(clickOnceInfo.GetShortcutPath()));
                    }
                }
            }
        }

        [Fact]
        public async Task UninstallsClickOnceApp()
        {
            var installer = new ClickOnceInstaller();
            await installer.InstallClickOnceApp(new Uri(IntegrationTestHelper.ClickOnceTestAppPath));

            UninstallInfo theApp = UninstallInfo.Find(IntegrationTestHelper.ClickOnceAppName);

            Assert.NotNull(theApp);

            var uninstaller = new Uninstaller();
            uninstaller.Uninstall(theApp);

            UninstallInfo shouldBeNull = UninstallInfo.Find(IntegrationTestHelper.ClickOnceAppName);

            Assert.Null(shouldBeNull);
        }
    }
}