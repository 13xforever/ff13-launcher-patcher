using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Microsoft.Win32;

namespace FF13LauncherPatcher
{
    internal static class Program
    {
        private const string LauncherFilename = "FFXiiiLauncher.exe";
        private static readonly byte[] patchSequence = { 0x80, 0x1D, 0x00, 0x00, 0x04, };

        static void Main(string[] args)
        {
            try
            {
                var pathToLauncher = "";
                if (args.Length > 0 && File.Exists(args[0]))
                    pathToLauncher = args[0];
                else if (File.Exists(LauncherFilename))
                    pathToLauncher = Path.GetFullPath(LauncherFilename);
                else
                {
                    // Computer\HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\Steam App 292120
                    // @InstallLocation
                    try
                    {
                        var folderPath = Registry.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\Steam App 292120", "InstallLocation", null) as string;
                        var path = Path.Combine(folderPath, LauncherFilename);
                        if (File.Exists(path))
                            pathToLauncher = path;
                    }
                    catch
                    {
                    }
                }
                if (string.IsNullOrEmpty(pathToLauncher))
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"Couldn't find {LauncherFilename}.");
                    Console.ResetColor();
                    Console.WriteLine("You can drag-and-drop the launcher executable on this patcher.");
                    return;
                }

                Console.WriteLine($"Patching {pathToLauncher}...");
                try
                {
                    var bytes = File.ReadAllBytes(pathToLauncher);
                    for (int i = 0; i < bytes.Length - patchSequence.Length; i++)
                    {
                        if ((bytes[i] == 0x16 || bytes[i] == 0x17) && SequenceEqual(bytes, i + 1, patchSequence, 0, patchSequence.Length))
                        {
                            var switchToAsian = bytes[i] == 0x16;

                            bytes[i] = (byte)(switchToAsian ? 0x17 : 0x16);
                            File.WriteAllBytes(pathToLauncher, bytes);
                            Console.ForegroundColor = ConsoleColor.Green;
                            Console.WriteLine($"Switched to {(switchToAsian ? "Asian" : "Western")} launcher");
                            Console.ResetColor();
                            return;
                        }
                    }
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine("Couldn't find the sequence, patch might already be applied");
                    Console.ResetColor();
                }
                catch (Exception e)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine(e.Message);
                    Console.ResetColor();
                }
            }
            finally
            {
                Console.ReadKey(true);
            }
        }

        private static bool SequenceEqual(byte[] arr1, int offset1, byte[] arr2, int offset2, int len)
        {
            for (int i = offset1, j = offset2, k = 0; k < len; i++, j++, k++)
                if (arr1[i] != arr2[j])
                    return false;
            return true;
        }
    }
}
