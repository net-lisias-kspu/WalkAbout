/*  Copyright 2017 Clive Pottinger
    This file is part of the WalkAbout Mod.

    WalkAbout is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    WalkAbout is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with WalkAbout.  If not, see<http://www.gnu.org/licenses/>.
*/

using KspWalkAbout.Extensions;
using System;
using System.IO;

namespace KspWalkAbout.KspFiles

{
    /// <summary>
    /// Represents a set of values that can read from, or written to, a standard KSP settings file.
    /// </summary>
    public class SettingsFile
    {
        /// <summary>
        /// Where the settings file is stored on disk.
        /// </summary>
        public string FilePath { get; private set; }

        /// <summary>
        /// Indicates if any values in the settings file have changed.
        /// </summary>
        public bool IsChanged { get; internal set; }

        /// <summary>
        /// Text summary of the result of the last action performed on the settings file.
        /// </summary>
        public string StatusMessage { get; private set; }

        /// <summary>
        /// Sets the properties of the SettingsFile object to match those read from a physical disk file.
        /// </summary>
        /// <param name="settingsFilePath">The full path of the file containing the settings information.</param>
        /// <param name="defaultNode">
        /// A representation of the values to be used as defaults in case the <paramref
        /// name="settingsFilePath"/> could not be found or read.
        /// </param>
        /// <returns>
        /// A value indicating whether the settings where loaded (either from the file or from the defaults)
        /// </returns>
        public bool Load(string settingsFilePath, ConfigNode defaultNode = null)
        {
            var isLoaded = false;
            try
            {
                var di = new DirectoryInfo(settingsFilePath); $"Load called for {settingsFilePath}".Debug();
                FilePath = di.FullName;
                if (File.Exists(FilePath))
                {
                    var loadNode = ConfigNode.Load(FilePath);
                    isLoaded = ConfigNode.LoadObjectFromConfig(this, loadNode);
                    StatusMessage = isLoaded
                        ? $"Loaded [{FilePath}]"
                        : $"error - unable to open settings file [{FilePath}]";
                }
                else
                {
                    StatusMessage = $"error - did not find file [{FilePath}]";
                }
            }
            catch (Exception) { } // yes, this looks bad,  but the exception handling is covered by the finally clause
            finally
            {
                IsChanged = !isLoaded;

                if (!isLoaded && defaultNode != null)
                {
                    isLoaded = ConfigNode.LoadObjectFromConfig(this, defaultNode);
                    StatusMessage = $"Loaded from default due to {StatusMessage}";
                }
            }

            return isLoaded;
        }

        /// <summary>
        /// Writes the current settings to the associated disk file (see <see cref="Load(string, ConfigNode)"/>)
        /// </summary>
        /// <returns>
        /// A value indicating whether or not the settings information was written to disk.
        /// </returns>
        public bool Save()
        {
            var saved = ConfigNode.CreateConfigFromObject(this, new ConfigNode()).Save(FilePath);
            $"Saved {FilePath} = {saved}".Debug();
            IsChanged = IsChanged && !saved;
            return saved;
        }
    }
}