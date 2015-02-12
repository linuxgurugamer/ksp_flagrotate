using System;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace FlagRotate
{
    [KSPAddon(KSPAddon.Startup.MainMenu, true)]
    class FlagPrefabEditor : MonoBehaviour
    {
        private void Awake()
        {
            try
            {
                var part = PartLoader.LoadedPartsList.Find(ap => string.Equals(ap.name, "flag"));
                var cfg = GameDatabase.Instance.GetConfigNodes("MODULE_FLAG_ROTATOR");

                if (cfg.Length == 0)
                {
                    Debug.LogError("FlagRotate: Failed to find MODULE_FLAG_ROTATOR config. Did you delete FlagRotate config files?");
                }
                else
                {
                    if (part != null)
                    {
                        // AddModule with ConfigNode will lead to an exception because the prefab is inactive and so
                        // PartModule.Awake won't be called; that method does important setup for PartModule
                        var pm = part.partPrefab.gameObject.AddComponent<ModuleFlagRotator>();

                        typeof (PartModule).GetMethod("Awake",
                            BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public)
                            .Invoke(pm, null);

                        pm.Load(cfg.Single());
                    }
                    else Debug.LogError("FlagRotate: Failed to find flag prefab");
                }
            }
            catch (Exception e)
            {
                // still throws an exception, but it's in a good enough
                // state to work
                Debug.LogError("FlagRotate: Unexpected exception while modifying prefab!");
                Debug.LogError(e);
            }

            Destroy(this);
        }
    }
}
