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
using KspWalkAbout.Entities;
using KspWalkAbout.Extensions;
using UnityEngine;
using static KspAccess.CommonKspAccess;

namespace KspWalkAbout
{
    /// <summary>
    /// Module to determine if a kerbal on EVA has been assigned inventory items and to 
    /// add the items to the kerbal's inventory.
    /// </summary>
    [KSPAddon(KSPAddon.Startup.Flight, false)]
    class WalkAboutEva : MonoBehaviour
    {
        /// <summary>Detects if the scene is for a kerbal on EVA and adds any outstanding inventory items.</summary>
        public void Start()
        {
            if (!(FlightGlobals.ActiveVessel?.isEVA ?? false))
            {
                "WalkAboutEva deactivated: not an EVA".Debug();
                return;
            }
            var crew = FlightGlobals.ActiveVessel.GetVesselCrew();
            if ((crew?.Count ?? 0) != 1)
            {
                "WalkAboutEva deactivated: invalid crew count".Debug();
                return;
            }

            var kerbal = crew[0];

            $"Flight scene started for {kerbal.name}".Debug();

            System.Reflection.Assembly KIS = null;
            var foundKis = false;
            if (IsModInstalled("KIS"))
            {
                KIS = GetMod("KIS");
                foundKis = true;
                $"obtained KIS mod assembly [{KIS}]".Debug();
            }
            else
            {
                $"KIS not installed".Debug();
            }


            if (foundKis && WalkAboutPersistent.InventoryItems.ContainsKey(kerbal.name))
            {
                var ModuleKISInventoryType = KIS.GetType("KIS.ModuleKISInventory");
                "found KIS inventory type".Debug();
                var inventory = FlightGlobals.ActiveVessel.GetComponent(ModuleKISInventoryType);
                "obtained modules for the active vessel".Debug();

                if (inventory != null)
                {
                    var items = WalkAboutPersistent.InventoryItems[kerbal.name];

                    foreach (var itemName in items)
                    {
                        $"{kerbal.name} has a {itemName} to be added".Debug();

                        var part = PartLoader.getPartInfoByName(itemName)?.partPrefab;
                        if (part != null)
                        {
                            $"invoking AddItem member using (part [{part.GetType()}])".Debug();
                            var item = ModuleKISInventoryType.InvokeMember("AddItem", System.Reflection.BindingFlags.InvokeMethod, null, inventory, new object[] { part, 1f, -1 });
                            $"{itemName} is in the inventory as {item}".Debug();
                        }
                        else
                        {
                            "Cannot add item to inventory".Debug();
                        }
                    }
                }

                WalkAboutPersistent.InventoryItems.Remove(kerbal.name);
            }
        }
    }
}
