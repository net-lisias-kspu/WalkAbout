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