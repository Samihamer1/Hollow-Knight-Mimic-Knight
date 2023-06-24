using System.Collections;
using GlobalEnums;
using HutongGames.PlayMaker.Actions;
using UnityEngine;
using Modding.Utils;
using HutongGames.PlayMaker;
using Vasi;
using System;
using UnityEngine.Rendering;

namespace PVCosplay
{
    public class Adidas : MonoBehaviour
    {
        float damagenumber = 20;
        GameObject clone;
        FsmState fsm;
        float lastinput;
        DamageEnemies dmg;

        private void Start()
        {   
            //Adidas state. A copy of the (original) Fireball2 state.
            FsmState adidasstate = HeroController.instance.spellControl.CreateState("Adidas");
            FsmState originalfireball2 = PVCosplay.fireballstatetemplate["Fireball 2"];
            for (int j = 0; j < originalfireball2.Actions.Length; j++)
            {
                adidasstate.InsertAction(j, originalfireball2.Actions[j]);
            }

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
            neutralstate.AddTransition("ALTERNATE", "Adidas");
            neutralstate.AddTransition("REGULAR", "Fireball 2");
            neutralstate.AddAction(newaction);

            //Hijack Level Check.
            fsm = HeroController.instance.spellControl.GetState("Level Check");
            fsm.ChangeTransition("LEVEL 2", "Neutral Check");

            //Hijack the Antic
            fsm = HeroController.instance.spellControl.GetState("Fireball Antic");
            fsm.InsertMethod(1, () =>
            {
                lastinput = HeroController.instance.move_input;
                FsmState neutralstate = HeroController.instance.spellControl.GetState("Neutral Check");
                neutralstate.GetAction<IntCompare>().integer1 = (FsmInt)lastinput;
            });

            //Adidas State linking
            fsm = HeroController.instance.spellControl.GetState("Adidas");
            fsm.RemoveAction(3);
            fsm.InsertMethod(3,() =>
            {
                //Attempt to add the neutral spell.
                CreateAdidas();
            });
            fsm.GetAction<Tk2dPlayAnimationWithEvents>().clipName = "Collect Magical 2";
            AudioPlaySimple audioaction = new AudioPlaySimple();
            audioaction.gameObject = fsm.GetAction<Tk2dPlayAnimationWithEvents>().gameObject;
            audioaction.volume = 1f;
            audioaction.oneShotClip = PVCosplay.hornetfsm.GetAction<AudioPlaySimple>("Sphere", 5).oneShotClip;
            SetVelocity2d velocityaction = new SetVelocity2d();
            velocityaction.gameObject = fsm.GetAction<Tk2dPlayAnimationWithEvents>().gameObject;
            velocityaction.vector = new Vector2(0, 0);
            velocityaction.y = 0f;
            velocityaction.x = 0f;
            velocityaction.everyFrame = true;
            Wait waitaction = new Wait();
            waitaction.time = 0.75f;
            waitaction.realTime = false;
            SendEventByName eventaction = new SendEventByName();
            eventaction.eventTarget = fsm.GetAction<SendEventByName>().eventTarget;
            eventaction.sendEvent = new FsmString("FINISHED");
            eventaction.delay = 0f;
            eventaction.everyFrame = false;
            fsm.GetAction<SendEventByName>(4).delay = 0.75f;
            fsm.InsertAction(4,velocityaction);
            fsm.InsertAction(1, audioaction);
            fsm.RemoveTransition("FINISHED");
            fsm.AddTransition("FINISHED", "Spell End");
        }

        private void CreateAdidas()
        {
            clone = Instantiate(PVCosplay.adidas);
            Destroy(clone.GetComponent<DamageHero>());

            clone.layer = (int)PhysLayers.HERO_ATTACK;

            dmg = clone.GetOrAddComponent<DamageEnemies>();
            dmg.damageDealt = 15;
            dmg.attackType = AttackTypes.Spell;
            dmg.ignoreInvuln = false;
            dmg.magnitudeMult = 0f;
            dmg.moveDirection = false;
            dmg.circleDirection = false;
            dmg.direction = 0;
            dmg.enabled = true;
            dmg.specialType = SpecialTypes.None;

            float multiplier = 1f;

            if (HeroController.instance.playerData.equippedCharm_19)
            {
                clone.transform.localScale += new Vector3((float)0.3, (float)0.3);
                multiplier *= 1.25f;
            }

            dmg.damageDealt = (int)Math.Ceiling(damagenumber * multiplier);


            clone.transform.position = new Vector3(HeroController.instance.transform.position.x, HeroController.instance.transform.position.y, 0);

            StartCoroutine(Util.DelayedActivateForSecondsD(clone,0.25f, 0.5f));
        }      


    }
}