using System;
using System.Collections.Generic;
using HutongGames.PlayMaker;
using HutongGames.PlayMaker.Actions;
using Modding;
using Modding.Utils;
using UnityEngine;
using UnityEngine.EventSystems;
using Vasi;

namespace PVCosplay
{
    public class PVCosplay : Mod
    {
        new public string GetName() => "Mimic Knight";
        public override string GetVersion() => "v1.7";

        //Original spell behavior (shoutout to pronoespro)
        public Dictionary<string, FsmState> divestatetemplate;
        public string[] divestatenames = new string[] { "Quake Antic", "Level Check 2", "Q2 Effect", "Quake2 Down", "Q2 Land", "Q2 Pillar", "Quake Finish", "Q1 Effect", "Quake1 Down", "Quake1 Land"};
        public static Dictionary<string, FsmState> fireballstatetemplate;
        public string[] fireballstatenames = new string[] { "Fireball Antic", "Level Check", "Fireball 2", "Fireball Recoil", "Fireball 1" };
        public Dictionary<string, FsmState> wraithstatetemplate;
        public string[] wraithstatenames = new string[] { "Level Check 3", "Scream Antic2", "Scream Burst 2", "End Roar 2", "Scream End 2" };


        public static bool init;
        public static bool isFacingLeft;
        float lastinput;
        private static PlayMakerFSM hkfsm;
        public static GameObject dagger;
        public static GameObject paintshotR;
        public static GameObject ptslashred;
        public static GameObject ptstomppillar;
        public static GameObject ptstompsplash;
        public static GameObject adidas;
        public static GameObject xeroblade;
        public static PlayMakerFSM originalfsm;
        public static PlayMakerFSM sheofsm;
        public static PlayMakerFSM hornetfsm;
        public static PlayMakerFSM xerofsm;
        public static Dictionary<string, GameObject> preloadedGO = new Dictionary<string, GameObject>();

        public override List<(string, string)> GetPreloadNames()
        {
            return new List<(string, string)>
            {
                ("GG_Hollow_Knight","Battle Scene/HK Prime"),
                ("GG_Hollow_Knight","Battle Scene/Focus Blasts"),
                ("GG_Painter","Battle Scene/Sheo Boss"),
                ("GG_Hornet_2","Boss Holder/Hornet Boss 2"),
                ("GG_Ghost_Xero","Warrior/Ghost Warrior Xero")
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

            GameObject xero = preloadedObjects["GG_Ghost_Xero"]["Warrior/Ghost Warrior Xero"];
            UnityEngine.Object.DontDestroyOnLoad(xero);
            xerofsm = xero.LocateMyFSM("Attacking");
            xeroblade = xero.Child("Sword 3");

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

        private void ResetSpell(Dictionary<string, FsmState> spelltemplate, string[] spelltemplatenames, string addon)
        {
            //statetemplates have [string, state] eg. string = "Wraith Antic", state = antic state
            originalfsm = HeroController.instance.spellControl;
            for (int i = 0; i < spelltemplate.Count; i++)
            {
                FsmState currentstate;
                if (!init)
                {
                    currentstate = originalfsm.CreateState(spelltemplatenames[i] + addon);
                }
                currentstate = originalfsm.GetState(spelltemplatenames[i] + addon);

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

            for (int i = 0; i < spelltemplate.Count; i++)
            {
                FsmState currentstate;
                if (!init)
                {
                    currentstate = originalfsm.GetState(spelltemplatenames[i] + addon);
                    FsmTransition[] transitions = spelltemplate[spelltemplatenames[i]].Transitions;
                    for (int x = 0; x < transitions.Length; x++)
                    {
                        if (transitions[x].ToState == "" || transitions[x].ToState == "Spell End")
                        {
                            currentstate.AddTransition(transitions[x].EventName, transitions[x].ToState);
                        }
                        else
                        {
                            currentstate.AddTransition(transitions[x].EventName, transitions[x].ToState + addon);
                        }
                    }
                }

            };

        }

        private void ResetSpellControl()
        {
            ResetSpell(fireballstatetemplate, fireballstatenames,"C");
            ResetSpell(fireballstatetemplate, fireballstatenames, "N");
            ResetSpell(wraithstatetemplate, wraithstatenames,"C");
            ResetSpell(divestatetemplate, divestatenames,"C");
            if (!init)
            {
                CreateNeutralSpell();
            }
            init = true;
        }

        private void CreateNeutralSpell()
        {
            //Neutral Check state.
            FsmState neutralstate = HeroController.instance.spellControl.CreateState("Neutral Check");
            FsmEvent event1 = new FsmEvent("ALTERNATE");
            FsmEvent event2 = new FsmEvent("REGULAR");
            IntCompare newaction = new IntCompare();
            newaction.integer1 = (FsmInt)lastinput;
            newaction.integer2 = 0;
            newaction.equal = event1;
            newaction.lessThan = event2;
            newaction.greaterThan = event2;
            newaction.everyFrame = false;
            neutralstate.AddTransition("ALTERNATE", "Fireball AnticN");
            neutralstate.AddTransition("REGULAR", "Fireball Antic");
            neutralstate.AddAction(newaction);

            HeroController.instance.spellControl.ChangeTransition("Wallside?", "FINISHED", "Neutral Check");

            //setup the lastinput val
            FsmState fsm = HeroController.instance.spellControl.GetState("Wallside?");
            fsm.InsertMethod(1, () =>
            {
                lastinput = HeroController.instance.move_input;
                FsmState neutralstate = HeroController.instance.spellControl.GetState("Neutral Check");
                neutralstate.GetAction<IntCompare>().integer1 = (FsmInt)lastinput;
            });
        }
        private void DirectionalCustomToggle(bool val)
        {
            if (val)
            {
                HeroController.instance.spellControl.ChangeTransition("Neutral Check", "REGULAR", "Fireball AnticC");
            }
            else
            {
                HeroController.instance.spellControl.ChangeTransition("Neutral Check", "REGULAR", "Fireball Antic");
            }
        }

        private void UpCustomToggle(bool val)
        {
            if (val)
            {
                HeroController.instance.spellControl.ChangeTransition("Scream Get?", "FINISHED", "Level Check 3C");
            }
            else
            {
                HeroController.instance.spellControl.ChangeTransition("Scream Get?", "FINISHED", "Level Check 3");
            }
        }

        private void DownCustomToggle(bool val)
        {
            if (val)
            {
                HeroController.instance.spellControl.ChangeTransition("Q On Ground", "FINISHED", "Quake AnticC");
                HeroController.instance.spellControl.ChangeTransition("Q Off Ground", "FINISHED", "Quake AnticC");
            }
            else
            {
                HeroController.instance.spellControl.ChangeTransition("Q On Ground", "FINISHED", "Quake Antic");
                HeroController.instance.spellControl.ChangeTransition("Q Off Ground", "FINISHED", "Quake Antic");
            }
        }
        private void NeutralCustomToggle(bool val)
        {
            if (val)
            {
                FsmState neutralstate = HeroController.instance.spellControl.GetState("Neutral Check");
                
            } else
            {

            }
        }

        private void OnHeroControllerAwake(On.HeroController.orig_Awake orig, HeroController self)
        {
            orig.Invoke(self);
            TemplateInit();
            ResetSpellControl();
            self.gameObject.GetOrAddComponent<Dagger>();
            self.gameObject.GetOrAddComponent<SheoDive>();
            self.gameObject.GetOrAddComponent<XeroBlades>();
            self.gameObject.GetOrAddComponent<Adidas>();
            //test
            DirectionalCustomToggle(true);
            UpCustomToggle(true);
            
            
        }
    }
}


