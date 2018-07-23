/*  Copyright 2017 Clive Pottinger
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
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace KspAccess
{
    /// <summary>Presents common internal methods to access and manipulate KSP's game state.</summary>
    internal static class CommonKspAccess
    {
        /// <summary>Gets the object representing the planet on which the KSC is located (e.g. Kerbin).</summary>
        public static CelestialBody Homeworld { get; } = Planetarium.fetch.Home;

        /// <summary>Gets a value indicating whether the game is currently paused.</summary>
        internal static bool IsPauseMenuOpen
        {
            get
            {
                try { return PauseMenu.isOpen; }
                catch { return false; }
            }
        }

        /// <summary>Determines if a specified mod is included in the current game.</summary>
        /// <param name="modName">The text id of the mod.</param>
        /// <returns>A value indicating whether the mod is installed.</returns>
        internal static bool IsModInstalled(string modName)
        {
            try
            {
                var searchText = $"{modName},";
                foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
                {
                    if (assembly.FullName.StartsWith(searchText))
                    {
                        return true;
                    }
                }
                return false;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>Get the object representing an installed mod.</summary>
        /// <param name="modName">The name of the mod to obtain.</param>
        /// <returns>An object that can be used to access the mod.</returns>
        internal static Assembly GetMod(string modName)
        {
            try
            {
                var searchText = $"{modName},";
                foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
                {
                    if (assembly.FullName.StartsWith(searchText))
                    {
                        return assembly;
                    }
                }
                return null;
            }
            catch
            {
                return null;
            }
        }

        /// <summary>Obtains the directory where the mod is currently installed.</summary>
        /// <returns>Returns a directoy path (or an empty string if the mod is not found).</returns>
        internal static string GetModDirectory(string modName)
        {
            return (IsModInstalled(modName)) ? $"{KSPUtil.ApplicationRootPath}GameData/{modName}" : string.Empty;
        }

        internal static bool IsKeyCombinationPressed(KeyCode key, List<KeyCode> modifiers = null)
        {
            var requiredKeysPressed = (modifiers?.Count ?? 0) == 0;

            if (Input.GetKeyDown(key))
            {
                if (!requiredKeysPressed)
                {
                    foreach (var modifier in modifiers)
                    {
                        requiredKeysPressed |= Input.GetKey(modifier);
                    }
                }

                return requiredKeysPressed;
            }

            return false;
        }

        /// <summary>
        /// Determines if a key or key combination has been pressed.
        /// </summary>
        /// <param name="keys">The key and an modifiers that are to be checked.</param>
        /// A list of keys of which one must be pressed at the same time as <paramref name="key"/>, or null if no modifier is needed. 
        /// </param>
        /// <returns>A value indicating whether the key or key combination was pressed.</returns>
        internal static bool IsKeyCombinationPressed(KeyCombination keys)
        {
            return IsKeyCombinationPressed(keys.Key, keys.Modifiers);
        }
    }
}
