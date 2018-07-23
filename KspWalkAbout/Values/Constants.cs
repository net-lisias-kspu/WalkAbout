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
using KspWalkAbout.Entities;
using KspWalkAbout.WalkAboutFiles;
using System.Collections.Generic;
using UnityEngine;

namespace KspWalkAbout.Values
{
    /// <summary>Represents immutable values used by the WalkAbout mod.</summary>
    internal class Constants
    {
        internal static readonly string ModName = "WalkAbout";
        internal static readonly string Version =
            System.Diagnostics.FileVersionInfo
            .GetVersionInfo(System.Reflection.Assembly.GetExecutingAssembly().Location)
            .ProductVersion;
        
        internal static readonly ConfigNode DefaultSettings =
            ConfigNode.CreateConfigFromObject(
                new WalkAboutSettings
                {
                    ActivationHotKey = KeyCode.W,
                    ActivationHotKeyModifiers = new List<KeyCode> { KeyCode.LeftControl, KeyCode.RightControl, },
                    Mode = "normal",
                    ScreenX = (int)new Rect().xMin,
                    ScreenY = (int)new Rect().yMin,
                    ScreenWidth = (int)new Rect().width,
                    ScreenHeight = (int)new Rect().height,
                },
                new ConfigNode());

        internal static readonly ConfigNode DefaultItems =
            ConfigNode.CreateConfigFromObject(
                new ItemsFile
                {
                    Items = new List<InventoryItem>(),
                },
                new ConfigNode());
    }
}
