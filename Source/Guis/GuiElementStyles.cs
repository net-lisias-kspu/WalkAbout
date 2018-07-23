using UnityEngine;

namespace KspWalkAbout.Guis
{
    /// <summary>Represents usable styles for Unity windows drawn with the GUILayout functions.</summary>
    internal class GuiElementStyles
    {
        public GUIStyle ValidButton { get; } = new GUIStyle(GUI.skin.button);
        public GUIStyle ActionableButton { get; } = new GUIStyle(GUI.skin.button) { normal = { textColor = Color.green } };
        public GUIStyle InvalidButton { get; } = new GUIStyle(GUI.skin.button) { normal = { textColor = Color.yellow } };

        public GUIStyle ValidLabel { get; } = new GUIStyle(GUI.skin.label);
        public GUIStyle InvalidLabel { get; } = new GUIStyle(GUI.skin.label) { normal = { textColor = Color.yellow } };

        public GUIStyle ValidTextInput { get; } = new GUIStyle(GUI.skin.label) { normal = { textColor = Color.black, background = Texture2D.whiteTexture } };
        public GUIStyle InvalidTextInput { get; } = new GUIStyle(GUI.skin.label) { normal = { textColor = Color.blue, background = Texture2D.whiteTexture } };
    }
}