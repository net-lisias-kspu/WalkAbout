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

namespace KspWalkAbout.Entities
{

    /// <summary>Represents an item that can be added to a kerbal's inventory.</summary>
    public class InventoryItem
    {
        /// <summary>The textual identifier of the item.</summary>
        [Persistent]
        public string Name;

        /// <summary>
        /// An indicator of where this item should appear in a sorted list of items 
        /// (higher numbers appear earlier on the list).
        /// </summary>
        [Persistent]
        internal int Queueing;

        /// <summary>Indicates whether the item can currently be added to an inventory.</summary>
        public bool IsAvailable;

        /// <summary>The user-friendly textual identifier.</summary>
        public string Title { get; internal set; }

        /// <summary>The cost of the item in KSP funds.</summary>
        public float Cost { get; internal set; }

        /// <summary>The volume of the item in litres.</summary>
        public float Volume { get; internal set; }
    }
}