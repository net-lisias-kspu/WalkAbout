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
using KspWalkAbout.Extensions;
using KspWalkAbout.KspFiles;
using System.Collections.Generic;
using UnityEngine;

namespace KspWalkAbout.WalkAboutFiles
{
    /// <summary>Represents the settings file used to hold information for the WalkAbout mod.</summary>
    public class WalkAboutSettings : SettingsFile
    {
        /// <summary>The key that activates the main GUI.</summary>
        [Persistent]
        public KeyCode ActivationHotKey;

        /// <summary>
        /// Any additional keys needs that need to be pressed at the same time as the <seealso cref="ActivationHotKey"/>
        /// (e.g. Shift, Ctrl, etc).
        /// </summary>
        [Persistent]
        public List<KeyCode> ActivationHotKeyModifiers;

        /// <summary>
        /// Indicates whether mod only allows the placement of kerbals ("normal") or also allows new
        /// locations to be defined ("utility").
        /// </summary>
        [Persistent]
        public string Mode;

        /// <summary>The x-coordinate the GUI's top left corner</summary>
        [Persistent]
        public int ScreenX;

        /// <summary>The y-coordinate of the GUI's top left corner</summary>
        [Persistent]
        public int ScreenY;

        /// <summary>The width of the GUI.</summary>
        [Persistent]
        public int ScreenWidth;

        /// <summary>The height of the GUI.</summary>
        [Persistent]
        public int ScreenHeight;

        /// <summary>The max number of locations to display in the GUI as the most likely locations the user wants to select from.</summary>
        [Persistent]
        public int TopFew;

        [Persistent]
        public int MaxInventoryItems;

        [Persistent]
        public float MaxInventoryVolume;

        /// <summary>Loads the settings from disk.</summary>
        /// <param name="filePath">The full path of the file containing the settings information.</param>
        /// <param name="defaultNode">The default values for the settings.</param>
        /// <returns>A value indicating whether the settings were read (or set from default).</returns>
        internal new bool Load(string filePath, ConfigNode defaultNode = null)
        {
            var result = base.Load(filePath, defaultNode);

            if (TopFew == 0)
            {
                TopFew = 5;
                IsChanged = true;
            }

            if (MaxInventoryItems == 0)
            {
                MaxInventoryItems = 6;
                IsChanged = true;
            }

            if (MaxInventoryVolume == 0)
            {
                MaxInventoryVolume = 300f;
                IsChanged = true;
            }

            return result;
        }

        /// <summary>Obtains the current position and dimensions of the GUI.</summary>
        /// <returns>A rectangle corresponding to the current position and dimensions of the GUI.</returns>
        internal Rect GetScreenPosition()
        {
            return new Rect(ScreenX, ScreenY, ScreenWidth, ScreenHeight);
        }

        /// <summary>Sets the position and dimensions of the GUI.</summary>
        /// <param name="newPosition">A rectangle corresponding to the desired position and dimensions of the GUI.</param>
        internal void SetScreenPosition(Rect newPosition)
        {
            var oldPosition = GetScreenPosition();
            IsChanged |= (newPosition != oldPosition);
            if (newPosition != oldPosition)
            {
                IsChanged = true;
                ScreenX = (int)newPosition.xMin;
                ScreenY = (int)newPosition.yMin;
                ScreenWidth = (int)newPosition.width;
                ScreenHeight = (int)newPosition.height;
                $"IsChanged = {IsChanged} - screen moved from {oldPosition} to {newPosition}".Debug();
            }
        }
    }
}
