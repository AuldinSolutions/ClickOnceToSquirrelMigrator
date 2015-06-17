﻿using System;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using Squirrel.Shell;

namespace ClickOnceToSquirrelMigrator
{
    internal static class TaskbarHelper
    {
        public static bool IsPinnedToTaskbar(string executablePath, string shortcutExtension)
        {
            var taskbarPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "Microsoft\\Internet Explorer\\Quick Launch\\User Pinned\\TaskBar");

            return Directory
                .GetFiles(taskbarPath, "*" + shortcutExtension)
                .Select(pinnedShortcut => new ShellLink(pinnedShortcut))
                .Any(shortcut => String.Equals(shortcut.Target, executablePath, StringComparison.OrdinalIgnoreCase));
        }

        public static void PinToTaskbar(string executablePath)
        {
            pinUnpin(executablePath, "pin to taskbar");
        }

        public static void UnpinFromTaskbar(string executablePath)
        {
            pinUnpin(executablePath, "unpin from taskbar");
        }

        private static void pinUnpin(string executablePath, string verbToExecute)
        {
            if (!File.Exists(executablePath))
            {
                throw new FileNotFoundException(executablePath);
            }

            dynamic shellApplication = Activator.CreateInstance(Type.GetTypeFromProgID("Shell.Application"));

            try
            {
                var path = Path.GetDirectoryName(executablePath);
                var fileName = Path.GetFileName(executablePath);

                dynamic directory = shellApplication.NameSpace(path);
                dynamic link = directory.ParseName(fileName);

                dynamic verbs = link.Verbs();

                for (var i = 0; i < verbs.Count(); i++)
                {
                    dynamic verb = verbs.Item(i);
                    string verbName = verb.Name.Replace(@"&", String.Empty).ToLower();

                    if (verbName.Equals(verbToExecute))
                    {
                        verb.DoIt();
                    }
                }
            }
            finally
            {
                Marshal.ReleaseComObject(shellApplication);
            }
        }
    }
}