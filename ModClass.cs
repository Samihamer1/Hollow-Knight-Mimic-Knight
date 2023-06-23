using System;
using System.Collections.Generic;
using HutongGames.PlayMaker;
using HutongGames.PlayMaker.Actions;
using Modding;
using Modding.Utils;
using UnityEngine;
using Vasi;

namespace PVCosplay
{
    public class PVCosplay : Mod
    {
        new public string GetName() => "PureVesselCosplay";
        public override string GetVersion() => "v1.4";

        public static bool isFacingLeft;
        private static PlayMakerFSM hkfsm;
        public static GameObject dagger;
        public static GameObject paintshotR;
        public static GameObject ptslashred;
        public static GameObject ptstomppillar;
        public static GameObject ptstompsplash;
        public static PlayMakerFSM sheofsm;
        public static Dictionary<string, GameObject> preloadedGO = new Dictionary<string, GameObject>();

        public override List<(string, string)> GetPreloadNames()
        {
            return new List<(string, string)>
            {
                ("GG_Hollow_Knight","Battle Scene/HK Prime"),
                ("GG_Hollow_Knight","Battle Scene/Focus Blasts"),
                ("GG_Painter","Battle Scene/Sheo Boss")
            };
        }

        public override void Initialize(Dictionary<string, Dictionary<string, GameObject>> preloadedObjects)
        {
            On.HeroController.Awake += new On.HeroController.hook_Awake(this.OnHeroControllerAwake);
            On.HeroController.Move += FindDirection;
            GameObject hkprime = preloadedObjects["GG_Hollow_Knight"]["Battle Scene/HK Prime"];
            UnityEngine.Object.DontDestroyOnLoad(hkprime);
            hkfsm = hkprime.LocateMyFSM("Control");

            GameObject sheo = preloadedObjects["GG_Painter"]["Battle Scene/Sheo Boss"];
            UnityEngine.Object.DontDestroyOnLoad(sheo);
            sheofsm = sheo.LocateMyFSM("nailmaster_sheo");

            dagger = hkfsm.GetAction<HutongGames.PlayMaker.Actions.FlingObjectsFromGlobalPoolTime>("SmallShot LowHigh", 2).gameObject.Value;
            paintshotR = sheofsm.GetAction<SpawnObjectFromGlobalPool>("RedShot L", 2).gameObject.Value;
            ptslashred = sheo.Child("Pt SlashRed");
            ptstomppillar = sheo.Child("Pt Stomp Pillar");
            ptstompsplash = sheo.Child("Pt Stomp Splash");
            

        }
        
        private void FindDirection(On.HeroController.orig_Move orig, HeroController self, float move_direction)
        {
            if (Mathf.Abs(move_direction) > 0)
            {
                isFacingLeft = move_direction < 0;
            }
            orig(self, move_direction);
        }
        private void OnHeroControllerAwake(On.HeroController.orig_Awake orig, HeroController self)
        {
            orig.Invoke(self);
            self.gameObject.GetOrAddComponent<Dagger>();
            self.gameObject.GetOrAddComponent<SheoDive>();
        }
    }
}


