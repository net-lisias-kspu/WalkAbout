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
using UnityEngine;

namespace KspWalkAbout.Guis
{
    /// <summary>Represents usable styles for windows drawn with Unity's GUILayout functions.</summary>
    internal class GuiElementStyles
    {
        public GUIStyle ValidButton { get; } = new GUIStyle(GUI.skin.button);
        public GUIStyle SelectedButton { get; } = new GUIStyle(GUI.skin.button) { normal = { textColor = Color.green } };
        public GUIStyle InvalidButton { get; } = new GUIStyle(GUI.skin.button) { normal = { textColor = Color.yellow } };
        public GUIStyle HighlightedButton { get; } = new GUIStyle(GUI.skin.button) { normal = { textColor = Color.green, background = Texture2D.whiteTexture } };

        public GUIStyle ValidLabel { get; } = new GUIStyle(GUI.skin.label);
        public GUIStyle InvalidLabel { get; } = new GUIStyle(GUI.skin.label) { normal = { textColor = Color.yellow } };

        public GUIStyle ValidTextInput { get; } = new GUIStyle(GUI.skin.label) { normal = { textColor = Color.black, background = Texture2D.whiteTexture } };
        public GUIStyle InvalidTextInput { get; } = new GUIStyle(GUI.skin.label) { normal = { textColor = Color.blue, background = Texture2D.whiteTexture } };
    }
}