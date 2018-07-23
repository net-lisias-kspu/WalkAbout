
using System;
using UnityEngine;

namespace KspWalkAbout.Guis
{
    internal static class GuiResizer
    {
        private static Rect _resizingGui;
        private static Vector2 _minGuiSize;

        private const int ButtonSize = 15;

        public static bool IsResizing { get; private set; } = false;

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
