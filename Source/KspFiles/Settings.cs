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
using System.Collections.Generic;
using UnityEngine;

namespace KspWalkAbout.KspFiles
{
    public class Settings : SettingsFile
    {
        [Persistent]
        public KeyCode ActivationHotKey;

        [Persistent]
        public List<KeyCode> ActivationHotKeyModifiers;

        [Persistent]
        public string Mode;

        [Persistent]
        public int ScreenX;

        [Persistent]
        public int ScreenY;

        [Persistent]
        public int ScreenWidth;

        [Persistent]
        public int ScreenHeight;

        [Persistent]
        public int TopFew;

        internal new bool Load(string fileName, ConfigNode defaultNode = null)
        {
            var result = base.Load(fileName, defaultNode);
            if (TopFew == 0)
            {
                TopFew = 5;
                IsChanged = true;
            }

            return result;
        }

        internal Rect GetScreenPosition()
        {
            return new Rect(ScreenX, ScreenY, ScreenWidth, ScreenHeight);
        }

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
