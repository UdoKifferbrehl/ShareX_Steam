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

        public static void Run(string[] args)
        {
            bool isFirstTimeRunning = false;

            if (!IsShareXRunning())
            {
                bool isSteamInit = false;

                if (SteamAPI.IsSteamRunning())
                {
                    isSteamInit = SteamAPI.Init();
                }

                if (!Directory.Exists(ContentFolderPath))
                {
                    isFirstTimeRunning = true;
                    DoUpdate();
                }
                else if (IsUpdateRequired())
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

                if (isFirstTimeRunning)
                {
                    arguments = "-SteamConfig";
                }
                else if (Helpers.IsCommandExist(args, "-silent"))
                {
                    arguments = "-silent";
                }

                RunShareX(arguments);
            }
        }

        private static bool IsShareXRunning()
        {
            return Helpers.IsRunning("82E6AC09-0FEF-4390-AD9F-0DD3F5561EFC");
        }

        private static bool IsUpdateRequired()
        {
            try
            {
                if (!File.Exists(ContentExecutablePath) || File.Exists(UpdatingTempFilePath))
                {
                    return true;
                }

                FileVersionInfo contentVersionInfo = FileVersionInfo.GetVersionInfo(ContentExecutablePath);
                FileVersionInfo updateVersionInfo = FileVersionInfo.GetVersionInfo(UpdateExecutablePath);

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

                File.Create(UpdatingTempFilePath).Dispose(); // In case updating terminate middle of it, so in next Launcher start it can repair it
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
                // Workaround for don't show "In-app"
                ProcessStartInfo startInfo = new ProcessStartInfo()
                {
                    Arguments = $"/C start \"\" \"{ContentExecutablePath}\" {arguments}",
                    CreateNoWindow = true,
                    FileName = "cmd.exe",
                    UseShellExecute = false
                };

                Process process = new Process();
                process.StartInfo = startInfo;
                process.Start();
            }
            catch (Exception e)
            {
                Helpers.ShowError(e);
            }
        }
    }
}