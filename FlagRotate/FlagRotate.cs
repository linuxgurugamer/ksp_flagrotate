/******************************************************************************
 *                    FlagRotate for Kerbal Space Program                     *
 *                                                                            *
 * Version 1.0                                                                *
 * Created: 5/11/2014 by xEvilReeperx                                         *
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
using UnityEngine;

namespace FlagRotate
{
    [KSPAddon(KSPAddon.Startup.Flight, false)]
    internal class FlagWatcher : MonoBehaviour
    {
        public void Start()
        {
            GameEvents.onVesselCreate.Add(OnVesselCreated);
        }

        public void OnDestroy()
        {
            GameEvents.onVesselCreate.Remove(OnVesselCreated);
        }

        public void OnVesselCreated(Vessel vessel)
        {
            if (vessel != null)
                if (vessel.rootPart != null)
                    if (vessel.rootPart.Modules.OfType<FlagSite>().ToList().Count > 0)
                        vessel.rootPart.AddModule("FlagRotate");
        }
    }


    internal class FlagRotate : PartModule
    {
        private const float AngleDelta = 5f;
    
        #region Events

            [KSPEvent(guiActive = true, guiName = "Counterclockwise", active = true, guiActiveUnfocused = true, unfocusedRange = 500)]
            public void RotateCounterclockwise()
            {
                Rotate(-AngleDelta);
            }

            [KSPEvent(guiActive = true, guiName = "Clockwise", active = true, guiActiveUnfocused = true, unfocusedRange = 500)]
            public void RotateClockwise()
            {
                Rotate(AngleDelta);
            }


            private void Rotate(float angle)
            {
                FlagSite flag = part.Modules.OfType<FlagSite>().SingleOrDefault();
                if (flag != null)
                    flag.groundPivot.rotation = Quaternion.AngleAxis(angle, flag.groundPivot.transform.up) * flag.groundPivot.rotation;
            }

        #endregion 
    }
}
