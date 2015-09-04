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

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace ShareX.Steam
{
    public static class Launcher
    {
        private static string ContentFolderPath => Helpers.GetAbsolutePath("ShareX");
        private static string UpdateFolderPath => Helpers.GetAbsolutePath("Updates");
        private static string ExecutablePath => Path.Combine(ContentFolderPath, "ShareX.exe");

        public static void Run()
        {
            if (IsShareXRunning())
            {
                return;
            }

            // Init Steam API

            if (!Directory.Exists(ContentFolderPath))
            {
                // First time running
                DoUpdate();
                Application.Run(new LauncherForm());
            }
            else if (IsUpdateRequired())
            {
                DoUpdate();
            }

            RunShareX();
        }

        private static bool IsShareXRunning()
        {
            return Helpers.IsRunning("82E6AC09-0FEF-4390-AD9F-0DD3F5561EFC");
        }

        private static bool IsUpdateRequired()
        {
            try
            {
                FileVersionInfo contentVersionInfo = FileVersionInfo.GetVersionInfo(ContentFolderPath);
                FileVersionInfo updateVersionInfo = FileVersionInfo.GetVersionInfo(UpdateFolderPath);

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
                Helpers.CopyAll(UpdateFolderPath, ContentFolderPath);
            }
            catch (Exception e)
            {
                Helpers.ShowError(e);
            }
        }

        private static void RunShareX()
        {
            try
            {
                Process.Start(ExecutablePath);
            }
            catch (Exception e)
            {
                Helpers.ShowError(e);
            }
        }
    }
}