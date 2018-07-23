/*  Copyright 2016 Clive Pottinger
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
using System;
using System.IO;

namespace KspWalkAbout.KspFiles

{
    public class SettingsFile
    {
        public string FilePath { get; private set; }
        public bool IsChanged { get; internal set; }
        public string StatusMessage { get; private set; }

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
                    ConfigNode.LoadObjectFromConfig(this, defaultNode);
                    StatusMessage = $"Loaded from default due to {StatusMessage}";
                    isLoaded = true;
                }
            }

            return isLoaded;
        }

        public bool Save()
        {
            var saved = ConfigNode.CreateConfigFromObject(this, new ConfigNode()).Save(FilePath);
            $"Saved {FilePath} = {saved}".Debug();
            IsChanged = IsChanged && !saved;
            return saved;
        }
    }
}
