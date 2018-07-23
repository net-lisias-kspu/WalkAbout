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
using UnityEngine;

namespace KspWalkAbout.Guis
{
    /// <summary>Represents a resizing button for Unity windows drawn with the GUILayout functions.</summary>
    internal static class GuiResizer
    {
        private static Rect _resizingGui;
        private static Vector2 _minGuiSize;

        private const int ButtonSize = 15;

        public static bool IsResizing { get; private set; } = false;

        /// <summary>Draws a resizing button.</summary>
        /// <param name="guiCoordinates">A rectangle representing the coordinates and dimensions of an existing GUI.</param>
        /// <param name="minSize">The minimum width and height allowed for the existing GUI.</param>
        internal static void DrawResizingButton(Rect guiCoordinates, Vector2 minSize = new Vector2())
        {
            if (GUI.RepeatButton(
                new Rect(guiCoordinates.width - ButtonSize, guiCoordinates.height - ButtonSize, ButtonSize, ButtonSize), "//") &&
                !IsResizing)
            {
                IsResizing = true;
                _resizingGui = guiCoordinates;
                _minGuiSize = new Vector2(Math.Max(minSize.x, ButtonSize * 2), Math.Max(minSize.y, ButtonSize * 2));
            }
        }

        /// <summary>Changes the size of the displayed GUI to match the mouse position after the resize button has been dragged.</summary>
        /// <param name="guiCoordinates">The screen coordinates of the GUI to be resized.</param>
        /// <returns>The screen coordinates and dimensions of the resized GUI.</returns>
        internal static Rect HandleResizing(Rect guiCoordinates)
        {
            if (Input.GetMouseButtonUp(0))
            {
                IsResizing = false;
            }

            if (IsResizing && (guiCoordinates.x == _resizingGui.x) && (guiCoordinates.y == _resizingGui.y))
            {
                _resizingGui.width = Math.Max(Input.mousePosition.x - _resizingGui.x + ButtonSize / 2, _minGuiSize.x);
                _resizingGui.height = Math.Max(Screen.height - Input.mousePosition.y - _resizingGui.y + ButtonSize / 2, _minGuiSize.y);
                return _resizingGui;
            }

            return guiCoordinates;
        }
    }
}
