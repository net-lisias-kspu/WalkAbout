﻿/*
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
    /// <summary>Represents a request to create a location.</summary>
    internal class LocationRequest
    {
        /// <summary>The name of the KSP facility that the location is associated with.</summary>
        public string AssociatedFacility { get; set; }

        /// <summary>The textual identifier of the location.</summary>
        public string Name { get; set; }
    }
}