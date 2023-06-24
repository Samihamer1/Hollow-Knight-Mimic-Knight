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
        new public string GetName() => "Mimic Knight";
        public override string GetVersion() => "v1.6";

        //Original spell behavior (shoutout to pronoespro)
        public Dictionary<string, FsmState> divestatetemplate;
        public string[] divestatenames = new string[] { "Quake Antic", "Level Check 2", "Q2 Effect", "Quake2 Down", "Q2 Land", "Q2 Pillar", "Quake Finish", "Q1 Effect", "Quake1 Down", "Quake1 Land"};
        public static Dictionary<string, FsmState> fireballstatetemplate;
        public string[] fireballstatenames = new string[] { "Fireball Antic", "Level Check", "Fireball 2", "Fireball Recoil", "Fireball 1" };
        public Dictionary<string, FsmState> wraithstatetemplate;
        public string[] wraithstatenames = new string[] { "Level Check 3", "Scream Antic2", "Scream Burst 2", "End Roar 2", "Scream End 2" };



        public static bool isFacingLeft;
        private static PlayMakerFSM hkfsm;
        public static GameObject dagger;
        public static GameObject paintshotR;
        public static GameObject ptslashred;
        public static GameObject ptstomppillar;
        public static GameObject ptstompsplash;
        public static GameObject adidas;
        public static PlayMakerFSM originalfsm;
        public static PlayMakerFSM sheofsm;
        public static PlayMakerFSM hornetfsm;
        public static Dictionary<string, GameObject> preloadedGO = new Dictionary<string, GameObject>();

        public override List<(string, string)> GetPreloadNames()
        {
            return new List<(string, string)>
            {
                ("GG_Hollow_Knight","Battle Scene/HK Prime"),
                ("GG_Hollow_Knight","Battle Scene/Focus Blasts"),
                ("GG_Painter","Battle Scene/Sheo Boss"),
                ("GG_Hornet_2","Boss Holder/Hornet Boss 2")
            };
        }

        public override void Initialize(Dictionary<string, Dictionary<string, GameObject>> preloadedObjects)
        {
            On.HeroController.Awake += new On.HeroController.hook_Awake(this.OnHeroControllerAwake);
            On.HeroController.Move += FindDirection;

            //Yknow, at some point, I'm really gonna have to make this less manual.
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

            GameObject hornet = preloadedObjects["GG_Hornet_2"]["Boss Holder/Hornet Boss 2"];
            UnityEngine.Object.DontDestroyOnLoad(hornet);
            hornetfsm = hornet.LocateMyFSM("Control");
            adidas = hornet.Child("Sphere Ball");
            

        }
        
        private void FindDirection(On.HeroController.orig_Move orig, HeroController self, float move_direction)
        {
            if (Mathf.Abs(move_direction) > 0)
            {
                isFacingLeft = move_direction < 0;
            }
            orig(self, move_direction);
        }

        private Dictionary<string, FsmState> SetTemplate(Dictionary<string, FsmState> states, string[] statenames)
        {
            if (states == null)
            {
                originalfsm = HeroController.instance.spellControl;

                states = new Dictionary<string, FsmState>();
                for (int i = 0; i < statenames.Length; i++)
                {
                    states.Add(statenames[i], new FsmState(originalfsm.GetState(statenames[i])));
                }

                return states;
            } else
            {
                return states;
            }
        }

        private void TemplateInit()
        {
            divestatetemplate = SetTemplate(divestatetemplate, divestatenames);
            wraithstatetemplate = SetTemplate(wraithstatetemplate, wraithstatenames);
            fireballstatetemplate = SetTemplate(fireballstatetemplate, fireballstatenames);
        }

        private void ResetSpell(Dictionary<string, FsmState> spelltemplate, string[] spelltemplatenames)
        {
            //statetemplates have [string, state] eg. string = "Wraith Antic", state = antic state
            originalfsm = HeroController.instance.spellControl;
            for (int i = 0; i < spelltemplate.Count; i++)
            {
                FsmState currentstate = originalfsm.GetState(spelltemplatenames[i]);
                FsmState templatestate = spelltemplate[spelltemplatenames[i]];
                //Complete obliteration of actions.
                for (int j = 0; j < currentstate.Actions.Length; j++)
                {
                    currentstate.RemoveAction(0);
                }

                //The reinsertion of actions. I could do them in the same loop, but I want to be sure it works.
                for (int j = 0; j < templatestate.Actions.Length; j++)
                {
                    currentstate.InsertAction(j,templatestate.Actions[j]);
                }

            };
        }

        private void ResetSpellControl()
        {
            ResetSpell(fireballstatetemplate, fireballstatenames);
            ResetSpell(wraithstatetemplate, wraithstatenames);
            ResetSpell(divestatetemplate, divestatenames);

        }

        private void OnHeroControllerAwake(On.HeroController.orig_Awake orig, HeroController self)
        {
            orig.Invoke(self);
            TemplateInit();
            self.gameObject.GetOrAddComponent<Dagger>();
            self.gameObject.GetOrAddComponent<SheoDive>();
            self.gameObject.GetOrAddComponent<Adidas>();
        }
    }
}


