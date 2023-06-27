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

using KspWalkAbout.Values;
using UnityEngine;

namespace KspWalkAbout.Extensions
{
    /// <summary>Presents utility methods and members for use in logging.</summary>
    internal static class DebugExtensions
    {
        /// <summary>Indicates whether <see cref="Debug(string)"/> operations should write to the log.</summary>
        public static bool DebugIsOn = false;

        /// <summary>Creates an initializes a new instance of the DebugExtensions class.</summary>
        static DebugExtensions()
        {
            DebugIsOn = System.IO.File.Exists($"{WalkAbout.GetModDirectory()}/debug.flg");
        }

        /// <summary>Writes the message to the log.</summary>
        /// <param name="message">Text to be written.</param>
        public static void Log(this string message)
        {
            MonoBehaviour.print($"{Constants.ModName}: {message}");
        }

        /// <summary>Write the message to the log if the DebugOn flag is set.</summary>
        /// <param name="message">Text to be written.</param>
        /// <param name="at">An additional identifier added to the prefix of the message (default is calling method name).</param>
        public static void Debug(
            this string message,
            [System.Runtime.CompilerServices.CallerMemberName] string at = "")
        {
            if (DebugIsOn) MonoBehaviour.print($"{Constants.ModName} ({at}): {message}");
        }

        /// <summary>
        /// Changes whether future <see cref="Debug(string)"/> operations should write to the log.
        /// </summary>
        /// <param name="debugState">Whether or not debug logging should be active.</param>
        public static void SetDebug(bool debugState = true)
        {
            DebugIsOn = debugState;
        }
    }
}
