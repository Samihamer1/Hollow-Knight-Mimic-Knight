using System;
using System.Collections;
using System.Collections.Generic;
using GlobalEnums;
using HutongGames.PlayMaker.Actions;
using IL;
using Modding;
using On;
using On.HutongGames.PlayMaker.Actions;
using UnityEngine;
using UnityEngine.UIElements;
using Vasi;

namespace MyFirstMod
{
    public class myFirstMod : Mod
    {
        new public string GetName() => "A Mod";
        public override string GetVersion() => "v1";

        public static bool isFacingLeft;
        public static GameObject hkprime;
        private static PlayMakerFSM hkfsm;
        public static GameObject dagger;
        public static Dictionary<string, GameObject> preloadedGO = new Dictionary<string, GameObject>();

        public override List<(string, string)> GetPreloadNames()
        {
            return new List<(string, string)>
            {
                ("GG_Hollow_Knight","Battle Scene/HK Prime"),
                ("GG_Hollow_Knight","Battle Scene/Focus Blasts"),
            };
        }

        public override void Initialize(Dictionary<string, Dictionary<string, GameObject>> preloadedObjects)
        {
            On.HeroController.Awake += new On.HeroController.hook_Awake(this.OnHeroControllerAwake);
            On.HeroController.Move += FindDirection;
            hkprime = preloadedObjects["GG_Hollow_Knight"]["Battle Scene/HK Prime"];
            UnityEngine.Object.DontDestroyOnLoad(hkprime);
            //Modding.Logger.Log("hk = " + hkprime);
            hkfsm = hkprime.LocateMyFSM("Control");
           
            dagger = hkfsm.GetAction<HutongGames.PlayMaker.Actions.FlingObjectsFromGlobalPoolTime>("SmallShot LowHigh", 2).gameObject.Value;

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
            self.gameObject.AddComponent<Action>();
        }
    }
}


