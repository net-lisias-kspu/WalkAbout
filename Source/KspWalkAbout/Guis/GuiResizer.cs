/*
	This file is part of Walk About /L Unleashed
		© 2023 Lisias T : http://lisias.net <support@lisias.net>
		© 2016-2017 Antipodes (Clive Pottinger)

	Walk About /L Unleashed is licensed as follows:
		* GPL 3.0 : https://www.gnu.org/licenses/gpl-3.0.txt

	Walk About /L Unleashed is distributed in the hope that
	it will be useful, but WITHOUT ANY WARRANTY; without even the implied
	warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.

	You should have received a copy of the GNU General Public License 3.0
	along with Walk About /L Unleashed. If not, see <https://www.gnu.org/licenses/>.

*/

using System;
using UnityEngine;

namespace KspWalkAbout.Guis
{
    /// <summary>Represents a resizing button for windows drawn with Unity's GUILayout functions.</summary>
    internal static class GuiResizer
    {
        private static Rect _resizingGui;
        private static Vector2 _minGuiSize;
        private const int ButtonSize = 15;

        /// <summary>Whether or not the user is still dragging the resizing button.</summary>
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

        /// <summary>
        /// Changes the size of the displayed GUI to match the mouse position while the resize button 
        /// is being dragged.
        /// </summary>
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
