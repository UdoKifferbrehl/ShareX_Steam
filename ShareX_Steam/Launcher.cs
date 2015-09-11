#region License Information (GPL v3)

/*
    ShareX - A program that allows you to take screenshots and share any file type
    Copyright (c) 2007-2015 ShareX Team

    This program is free software; you can redistribute it and/or
    modify it under the terms of the GNU General Public License
    as published by the Free Software Foundation; either version 2
    of the License, or (at your option) any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with this program; if not, write to the Free Software
    Foundation, Inc., 51 Franklin Street, Fifth Floor, Boston, MA  02110-1301, USA.

    Optionally you can also view the license at <http://www.gnu.org/licenses/>.
*/

#endregion License Information (GPL v3)

using Steamworks;
using System;
using System.Diagnostics;
using System.IO;

namespace ShareX.Steam
{
    public static class Launcher
    {
        private static string ContentFolderPath => Helpers.GetAbsolutePath("ShareX");
        private static string ContentExecutablePath => Path.Combine(ContentFolderPath, "ShareX.exe");
        private static string UpdateFolderPath => Helpers.GetAbsolutePath("Updates");
        private static string UpdateExecutablePath => Path.Combine(UpdateFolderPath, "ShareX.exe");
        private static string UpdatingTempFilePath => Path.Combine(ContentFolderPath, "Updating");

        private static bool IsFirstTimeRunning { get; set; }

        public static void Run(string[] args)
        {
            if (!IsShareXRunning())
            {
                bool isSteamInit = false;

                if (SteamAPI.IsSteamRunning())
                {
                    isSteamInit = SteamAPI.Init();
                }

                if (IsUpdateRequired())
                {
                    DoUpdate();
                }

                if (isSteamInit)
                {
                    SteamAPI.Shutdown();
                }
            }

            if (File.Exists(ContentExecutablePath))
            {
                string arguments = "";

                if (IsFirstTimeRunning)
                {
                    // Show first time config window
                    arguments = "-SteamConfig";
                }
                else if (Helpers.IsCommandExist(args, "-silent"))
                {
                    // Don't show ShareX main window
                    arguments = "-silent";
                }

                RunShareX(arguments);
            }
        }

        private static bool IsShareXRunning()
        {
            // Check ShareX mutex
            return Helpers.IsRunning("82E6AC09-0FEF-4390-AD9F-0DD3F5561EFC");
        }

        private static bool IsUpdateRequired()
        {
            try
            {
                // First time running?
                if (!Directory.Exists(ContentFolderPath) || !File.Exists(ContentExecutablePath))
                {
                    IsFirstTimeRunning = true;
                    return true;
                }

                // Need repair?
                if (File.Exists(UpdatingTempFilePath))
                {
                    return true;
                }

                // Need update?
                FileVersionInfo contentVersionInfo = FileVersionInfo.GetVersionInfo(ContentExecutablePath);
                FileVersionInfo updateVersionInfo = FileVersionInfo.GetVersionInfo(UpdateExecutablePath);

                // For testing purposes
                if (Helpers.CompareVersion(contentVersionInfo.FileVersion, "10.2.2.0") <= 0)
                {
                    IsFirstTimeRunning = true;
                }

                return Helpers.CompareVersion(contentVersionInfo.FileVersion, updateVersionInfo.FileVersion) < 0;
            }
            catch (Exception e)
            {
                Helpers.ShowError(e);
            }

            return false;
        }

        private static void DoUpdate()
        {
            try
            {
                if (!Directory.Exists(ContentFolderPath))
                {
                    Directory.CreateDirectory(ContentFolderPath);
                }

                // In case updating terminate middle of it, in next Launcher start it can repair
                File.Create(UpdatingTempFilePath).Dispose();
                Helpers.CopyAll(UpdateFolderPath, ContentFolderPath);
                File.Delete(UpdatingTempFilePath);
            }
            catch (Exception e)
            {
                Helpers.ShowError(e);
            }
        }

        private static void RunShareX(string arguments = "")
        {
            try
            {
                ProcessStartInfo startInfo;

                // Show "In-app"?
                if (File.Exists(Path.Combine(ContentFolderPath, "Steam")))
                {
                    startInfo = new ProcessStartInfo()
                    {
                        Arguments = arguments,
                        FileName = ContentExecutablePath,
                        UseShellExecute = true
                    };
                }
                else
                {
                    string path = Path.Combine(Environment.SystemDirectory, "cmd.exe");

                    if (!File.Exists(path))
                    {
                        path = "cmd.exe";
                    }

                    // Workaround for don't show "In-app"
                    startInfo = new ProcessStartInfo()
                    {
                        Arguments = $"/C start \"\" \"{ContentExecutablePath}\" {arguments}",
                        CreateNoWindow = true,
                        FileName = path,
                        UseShellExecute = false
                    };
                }

                Process.Start(startInfo);
            }
            catch (Exception e)
            {
                Helpers.ShowError(e);
            }
        }
    }
}