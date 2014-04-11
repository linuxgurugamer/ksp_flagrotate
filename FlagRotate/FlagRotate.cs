/******************************************************************************
 *                    FlagRotate for Kerbal Space Program                     *
 *                                                                            *
 * Version 1.0                                                                *
 * Created: 4/11/2014 by xEvilReeperx                                         *
 * ************************************************************************** *
 
This is free and unencumbered software released into the public domain.

Anyone is free to copy, modify, publish, use, compile, sell, or
distribute this software, either in source code form or as a compiled
binary, for any purpose, commercial or non-commercial, and by any
means.

In jurisdictions that recognize copyright laws, the author or authors
of this software dedicate any and all copyright interest in the
software to the public domain. We make this dedication for the benefit
of the public at large and to the detriment of our heirs and
successors. We intend this dedication to be an overt act of
relinquishment in perpetuity of all present and future rights to this
software under copyright law.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT.
IN NO EVENT SHALL THE AUTHORS BE LIABLE FOR ANY CLAIM, DAMAGES OR
OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE,
ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR
OTHER DEALINGS IN THE SOFTWARE.
 * ***************************************************************************/
using System.Linq;
using System;
using UnityEngine;

namespace FlagRotate
{
    /// <summary>
    /// KSPAddon with equality checking using an additional type parameter. Fixes the issue where AddonLoader prevents multiple start-once addons with the same start scene.
    /// </summary>
    public class KSPAddonFixed : KSPAddon, IEquatable<KSPAddonFixed>
    {
        private readonly Type type;

        public KSPAddonFixed(KSPAddon.Startup startup, bool once, Type type)
            : base(startup, once)
        {
            this.type = type;
        }

        public override bool Equals(object obj)
        {
            if (obj.GetType() != this.GetType()) { return false; }
            return Equals((KSPAddonFixed)obj);
        }

        public bool Equals(KSPAddonFixed other)
        {
            if (this.once != other.once) { return false; }
            if (this.startup != other.startup) { return false; }
            if (this.type != other.type) { return false; }
            return true;
        }

        public override int GetHashCode()
        {
            return this.startup.GetHashCode() ^ this.once.GetHashCode() ^ this.type.GetHashCode();
        }
    }

    [KSPAddonFixed(KSPAddon.Startup.Flight, true, typeof(FlagPrefabEditor))]
    internal class FlagPrefabEditor : MonoBehaviour
    {
        public void Awake() {
            // all right, you're not going to like this next part
            //   what is it doing, why is it doing it this way, etc?
            //
            // Since the flag isn't a normal part, a simple config
            // or MM edit won't do. The most obvious solution is to
            // intercept OnVesselCreate events, determine if the new
            // vessel is a flag and then add the appropriate module.
            // This turned out to have serious non-obvious problems.
            //
            //  1) Either flags are handled in a special way or adding
            //     PartModules at runtime has a bug. Simply adding the
            //     PartModule directly to the flag on vessel creation
            //     works initially, but upon reloading the game an
            //     error related to a null PartModule appears and the
            //     module isn't loaded.
            //
            //  2) As a consequence of 1), I can't store persistent data
            //     within a flag. That turned out to be a problem because
            //     flags have a special transform they use for positioning
            //
            //  3) I tried many variations on this method without success.
            //     Using reflection, fake configs, even resigning myself to
            //     only allowing the flag to be rotated in the game and scene
            //     it was placed and removing it afterwards turned out to be
            //     problematic.
            //
            // Luckily, I came up with Plan B. I wondered what would happen if I
            // added the PartModule directly to the prefab. Could I bypass the
            // PartModule load error if the PartModule was already part
            // of the "stock" flag prefab? It turns out that YES, this
            // does work with one flaw: attempting to even access the prefab's
            // modules will throw a nullref exception. AddModule does this, but
            // whatever it does behind the scenes is enough to get our PartModule
            // in despite the nullref which we ignore. This method has the added
            // bonus of retroactively applying to all ingame flags, plus I can
            // store persistent data correctly!
            //
            // Then there's only one other issue: without being able to access
            // the prefab's modules, we can't know whether we added the rotate
            // PartModule or not. Easily solved by running the plugin only once,
            // but due to a bug in KSPAddon this requires Majiir's addon fix.
            try
            {
                PartLoader.LoadedPartsList.Find(ap => string.Equals(ap.name, "flag")).partPrefab.AddModule("ModuleFlagRotate");
            }
            catch (Exception) { 
                // still throws an exception, but it's in a good enough
                // state to work
            }
        }
    }

    
    /// <summary>
    /// Adds rotation options to flags, provided an eva'ing kerbal is nearby.
    /// </summary>
    public class ModuleFlagRotate : PartModule
    {
        // Constants
        private const float AngleDelta = 5f;        // Angle adjustments are in this many degrees

        // Persistent values
        [KSPField(isPersistant = true)]
        private Quaternion rotation = Quaternion.identity; // ground pivot rotation

        [KSPField(isPersistant = true)]
        private bool ready = false;                 // whether we should use the stored rotation or use its current value


    //---------------------------------------------------------------------
    //  Implementation
    //---------------------------------------------------------------------
        [KSPEvent(guiActive = true, guiName = "Counterclockwise", active = true, guiActiveUnfocused = true, unfocusedRange = 2f, externalToEVAOnly = true)]
        public void RotateCounterclockwise()
        {
            Rotate(-AngleDelta);
        }

        [KSPEvent(guiActive = true, guiName = "Clockwise", active = true, guiActiveUnfocused = true, unfocusedRange = 2f, externalToEVAOnly = true)]
        public void RotateClockwise()
        {
            Rotate(AngleDelta);
        }


        private void Rotate(float angle)
        {
            FlagSite flag = part.Modules.OfType<FlagSite>().SingleOrDefault();
            if (flag != null)
                flag.groundPivot.rotation = Quaternion.AngleAxis(angle, flag.groundPivot.transform.up) * flag.groundPivot.rotation;
            rotation = flag.groundPivot.rotation;
        }


        public void Start()
        {
            var flag = part.Modules.OfType<FlagSite>().Single();

            if (ready)
            {
                flag.groundPivot.rotation = rotation;
            }
            else
            {
                ready = true;
                rotation = flag.groundPivot.rotation;
            }
        }
    }
}
