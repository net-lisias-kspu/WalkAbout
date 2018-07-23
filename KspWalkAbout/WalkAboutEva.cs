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
using KspWalkAbout.Values;
using UnityEngine;
using static KspAccess.CommonKspAccess;
using static KspWalkAbout.Entities.WalkAboutPersistent;

namespace KspWalkAbout
{
    /// <summary>
    /// Module to determine if a kerbal on EVA has been assigned inventory items and to 
    /// add the items to the kerbal's inventory.
    /// </summary>
    [KSPAddon(KSPAddon.Startup.Flight, false)]
    class WalkAboutEva : MonoBehaviour
    {
        private MotionState _motion = MotionState.normal;
        private bool _inRunMode = false;

        /// <summary>Detects if the scene is for a kerbal on EVA and adds any outstanding inventory items.</summary>
        public void Start()
        {
            var kerbalEva = GetKerbalEva();

            if (kerbalEva == null)
            {
                "WalkAboutEva deactivated: not a valid Kerbal EVA".Debug();
                return;
            }

            var kerbalPcm = FlightGlobals.ActiveVessel.GetVesselCrew()[0];
            $"Flight scene started for {kerbalPcm.name}".Debug();

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

            if (foundKis && WalkAboutPersistent.AllocatedItems.ContainsKey(kerbalPcm.name))
            {
                $"{kerbalPcm.name} has {WalkAboutPersistent.AllocatedItems[kerbalPcm.name].Count} items to be assigned".Debug();
                var ModuleKISInventoryType = KIS.GetType("KIS.ModuleKISInventory");
                "found KIS inventory type".Debug();
                var inventory = FlightGlobals.ActiveVessel.GetComponent(ModuleKISInventoryType);
                "obtained modules for the active vessel".Debug();

                if (inventory != null)
                {
                    var items = WalkAboutPersistent.AllocatedItems[kerbalPcm.name];

                    foreach (var itemName in items)
                    {
                        $"{kerbalPcm.name} has a {itemName} to be added".Debug();

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

                WalkAboutPersistent.AllocatedItems.Remove(kerbalPcm.name);
            }
            else
            {
                $"{kerbalPcm.name} has no items to add to inventory".Debug();
            }
        }

        public void FixedUpdate()
        {
            var kerbalEva = GetKerbalEva();
            if ((kerbalEva == null) // not a kerbal on EVA
                || (!FlightGlobals.ActiveVessel.LandedOrSplashed)
                || (kerbalEva.isRagdoll)
                || (FlightGlobals.currentMainBody?.GeeASL == null)) // avoids null reference on exit
            {
                return;
            }

            if (CheckForKeyCombo(GetModConfig().PmActivationHotKey, GetModConfig().PmActivationHotKeyModifiers)) 
            {
                _motion = (_motion == MotionState.perpetual) ? MotionState.stopping : MotionState.perpetual;
                $"Set motion state to {_motion}".Debug();
                if (_motion == MotionState.perpetual)
                {
                    TimeWarp.fetch.Mode = TimeWarp.Modes.LOW;
                }
            }

            if (GameSettings.EVA_Run.GetKeyDown())
            {
                _inRunMode = !_inRunMode;
            }


            if (_motion == MotionState.normal) { return; }

            if ((TimeWarp.WarpMode == TimeWarp.Modes.HIGH) && (TimeWarp.CurrentRate != 1))
            {
                var rate = Mathf.Min(4, TimeWarp.CurrentRateIndex);
                $"Forcing TimeWarp from mode:HIGH rate:{TimeWarp.CurrentRateIndex} to mode:LOW, rate {rate}".Debug();
                TimeWarp.fetch.Mode = TimeWarp.Modes.LOW;
                TimeWarp.SetRate(rate, true, true);
            }

            // Determine speed and animation
            float speed;
            string animation;
            SetSpeedAndAnimation(kerbalEva, out speed, out animation);

            // move the kerbal in the direction it is facing
            MoveKerbal(kerbalEva, speed, animation);
        }

        private static void AddItemToInventory(KerbalEVA kerbal, System.Type ModuleKISInventoryType, Component inventory, string itemName)
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

        private static void MoveKerbal(KerbalEVA kerbalEva, float speed, string animation)
        {
            Animation currentAnimation = null;
            kerbalEva.GetComponentCached<Animation>(ref currentAnimation);

            Rigidbody rigidbody = null;
            kerbalEva.GetComponentCached<Rigidbody>(ref rigidbody);

            if ((currentAnimation != null) && (rigidbody != null))
            {
                var orientation = kerbalEva.part.vessel.transform.rotation;
                var deltaPosition = orientation * Vector3.forward.normalized * (TimeWarp.deltaTime * speed);

                currentAnimation.CrossFade(animation);
                rigidbody.interpolation = RigidbodyInterpolation.Extrapolate;
                rigidbody.MovePosition(rigidbody.position + deltaPosition);
            }
        }

        private void SetSpeedAndAnimation(KerbalEVA kerbal, out float speed, out string animation)
        {
            var gforce = FlightGlobals.currentMainBody.GeeASL;
            speed = kerbal.walkSpeed;
            animation = "wkC_forward";
            if (_motion == MotionState.stopping)
            {
                speed = 0;
                animation = (kerbal.part.WaterContact) ? "swim_idle" : "idle";
                _motion = MotionState.normal;
            }
            else if (kerbal.part.WaterContact)
            {
                speed = kerbal.swimSpeed;
                animation = "swim_forward";
            }
            else if (_inRunMode && (gforce >= kerbal.minRunningGee))
            {
                speed = kerbal.runSpeed;
                animation = "wkC_run";
            }
            else if (gforce < kerbal.minWalkingGee)
            {
                speed = kerbal.boundSpeed;
                animation = "wkC_loG_forward";
            }
        }

        private KerbalEVA GetKerbalEva()
        {
            if (!(FlightGlobals.ActiveVessel?.isEVA ?? false))
            {
                return null;
            }
            var crew = FlightGlobals.ActiveVessel.GetVesselCrew();
            if ((crew?.Count ?? 0) != 1)
            {
                return null;
            }

            return FlightGlobals.ActiveVessel.GetComponent<KerbalEVA>();
        }
    }
}
