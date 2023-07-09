using System.Collections;
using GlobalEnums;
using HutongGames.PlayMaker.Actions;
using UnityEngine;
using Modding.Utils;
using HutongGames.PlayMaker;
using Vasi;
using System;
using UnityEngine.Rendering;
using Modding;

namespace PVCosplay
{
    public class Adidas : MonoBehaviour
    {
        float damagenumber = 20;
        GameObject clone;
        FsmState fsm;
        float lastinput;
        DamageEnemies dmg;
        bool healtheffect;
        float lastadidas = Time.time;

        private void Start()
        {   
            //Adidas State linking
            fsm = HeroController.instance.spellControl.GetState("Fireball 2N");
            FsmOwnerDefault owner = fsm.GetAction<Tk2dPlayAnimationWithEvents>().gameObject;
            fsm.RemoveAction(4);
            fsm.RemoveAction(3);
            fsm.RemoveAction(0);
            fsm.AddMethod(() =>
            {
                //Activate the health effect (and its off button)
                healtheffect = true;
                lastadidas = Time.time;
                StartCoroutine(disableEffect(lastadidas));
                //Create slash.
                CreateAdidas();
            });
            //fsm.GetAction<Tk2dPlayAnimationWithEvents>().clipName = "Collect Magical 2";

            Tk2dPlayAnimationV2 animationaction = new Tk2dPlayAnimationV2
            {
                clipName = "Collect Magical 2",
                gameObject = owner
            };

            AudioPlaySimple audioaction = new AudioPlaySimple();
            audioaction.gameObject = owner;
            audioaction.volume = 1f;
            audioaction.oneShotClip = PVCosplay.hornetfsm.GetAction<AudioPlaySimple>("Sphere", 5).oneShotClip;

            SetVelocity2d velocityaction = new SetVelocity2d();
            velocityaction.gameObject = owner;
            velocityaction.vector = new Vector2(0, 0);
            velocityaction.y = 0f;
            velocityaction.x = 0f;
            velocityaction.everyFrame = true;

            SendEventByNameV2 eventaction = new SendEventByNameV2();
            eventaction.delay = 0.75f;
            eventaction.sendEvent = "FINISHEDN";
            eventaction.everyFrame = false;
            eventaction.eventTarget = HeroController.instance.spellControl.Fsm.EventTarget;

            Wait waitaction = new Wait
            {
                time = 0.75f,
                finishEvent = new FsmEvent("FINISHEDN"),
                realTime = false
            };

            fsm.AddAction(velocityaction);
            fsm.AddAction(audioaction);
            fsm.AddAction(animationaction);
            fsm.AddAction(waitaction);
            fsm.AddAction(eventaction);

            fsm.RemoveTransition("FINISHED");
            fsm.AddTransition("FINISHEDN", "Spell End");


            //While in state, -1 damage.
            ModHooks.TakeHealthHook += DamageEvents;

        }

        private int DamageEvents(int damage)
        {        
            if (healtheffect)
            {
                damage -= 1;
                healtheffect = false;
            }
            return damage;
        }

        private IEnumerator disableEffect(float currentadidas)
        {
            yield return new WaitForSeconds(0.75f);
            if (lastadidas == currentadidas)
            {
                healtheffect = false;
            }
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
                multiplier *= 1.5f;
            }

            dmg.damageDealt = (int)Math.Ceiling(damagenumber * multiplier);


            clone.transform.position = new Vector3(HeroController.instance.transform.position.x, HeroController.instance.transform.position.y, 0);

            StartCoroutine(Util.WhileInState("Fireball 2N",clone));
        }      

        private void OnDisable()
        {
            ModHooks.TakeHealthHook -= DamageEvents;
        }

    }
}