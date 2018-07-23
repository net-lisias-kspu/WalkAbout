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
using KspWalkAbout.Values;
using UnityEngine;

namespace KspWalkAbout.Extensions
{
    internal static class DebugExtensions
    {
        /// <summary>Indicates whether <see cref="Debug(string)"/> operations should write to the log.</summary>
        public static bool DebugOn = false;

        /// <summary>Writes the message to the log.</summary>
        /// <param name="message">Text to be written.</param>
        public static void Log(this string message)
        {
            MonoBehaviour.print($"{Constants.ModName}: {message}");
        }

        /// <summary>Write the message to the log if the DebugOn flag is set.</summary>
        /// <param name="message">Text to be written.</param>
        public static void Debug(this string message)
        {
            if (DebugOn) MonoBehaviour.print($"{Constants.ModName}: {message}");
        }
    }
}
