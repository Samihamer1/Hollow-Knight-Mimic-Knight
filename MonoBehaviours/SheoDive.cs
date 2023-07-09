using GlobalEnums;
using HutongGames.PlayMaker;
using HutongGames.PlayMaker.Actions;
using Modding;
using Modding.Utils;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using Vasi;


namespace PVCosplay
{
    public class SheoDive : MonoBehaviour
    {
        FsmState fsm;
        float lerpDuration = 2;
        GameObject clone;
        DamageEnemies dmg;
        PaintBullet paint;
        FsmState newstate;
        int damagenumber = 20;

        private IEnumerator Start()
        {
            fsm = HeroController.instance.spellControl.GetState("Level Check 2");

            newstate = HeroController.instance.spellControl.CreateState("PaintSlash");

            fsm.ChangeTransition("LEVEL 2", "PaintSlash");
            newstate.AddTransition("PAINTDOWN", "Quake2 Down");
            //fsm.AddTransition("FINISHED", "PaintSlash");

            //Remove the Quake Antic animation
            FsmState anticstate = HeroController.instance.spellControl.GetState("Quake Antic");
            anticstate.RemoveAction(7);
            anticstate.AddMethod(() =>
            {
                HeroController.instance.spellControl.SendEvent("ANIM END");
            });

            AudioPlaySimple audioaction = new AudioPlaySimple();
            audioaction.gameObject = anticstate.GetAction<AudioPlay>().gameObject;
            audioaction.volume = 1f;
            audioaction.oneShotClip = PVCosplay.sheofsm.GetAction<AudioPlayerOneShotSingle>("JumpSlash1", 3).audioClip;

            newstate.AddAction(audioaction);
            newstate.AddMethod(() =>
            {
                tk2dSpriteAnimator animator = HeroController.instance.GetComponent<tk2dSpriteAnimator>();
                animator.Stop();
                animator.Play("DownSlash");
                clone = Instantiate(PVCosplay.ptslashred);
                clone.transform.position = new Vector3(HeroController.instance.transform.position.x, HeroController.instance.transform.position.y, 0);
                Util.ActivateForSecondsD(clone, 1f);
                clone.GetComponent<ParticleSystem>().Play();
                for (int i = 1; i < 7; i++)
                {
                    CreatePaintR(i);
                }

                animator.PlayAnimWait("DownSlash");

                Invoke("TransitionControl", 0.4f);
            });

            //Remove Trail animation
            FsmState downstate = HeroController.instance.spellControl.GetState("Quake2 Down");
            downstate.GetAction<Tk2dPlayAnimationWithEvents>().clipName = "Quake Fall";
            downstate.RemoveAction(2);

            //Change the slam effect to paint
            FsmState pillarstate = HeroController.instance.spellControl.GetState("Q2 Pillar");
            pillarstate.RemoveAction(3);
            pillarstate.RemoveAction(2);
            pillarstate.RemoveAction(1);

            //Change slam animation to the regular landing (no shade) and also land state changes
            FsmState landstate = HeroController.instance.spellControl.GetState("Q2 Land");
            landstate.GetAction<Tk2dPlayAnimationWithEvents>().clipName = "Quake Land";
            FsmOwnerDefault owner = landstate.GetAction<AudioPlay>().gameObject;
            landstate.RemoveAction(11);
            landstate.RemoveAction(8);
            landstate.RemoveAction(3);
            landstate.RemoveAction(1);
            landstate.InsertMethod(8, () =>
            {
                //PLEASE PLEASE PLEASE MAKE THIS MORE EFFICIENT LATER
                clone = Instantiate(PVCosplay.ptstomppillar);
                clone.layer = (int)PhysLayers.HERO_ATTACK;
                clone.transform.position = new Vector3(HeroController.instance.transform.position.x, HeroController.instance.transform.position.y, 0);
                clone.GetComponent<ParticleSystem>().Play();

                BoxCollider2D collider = clone.AddComponent<BoxCollider2D>();
                collider.size = new Vector2(6, 10);
                collider.offset = new Vector2(0, 0);
                collider.isTrigger = true;
                collider.enabled = true;

                PaintSplash dmg = clone.GetOrAddComponent<PaintSplash>();

                if (HeroController.instance.playerData.equippedCharm_19)
                {
                    clone.transform.localScale += new Vector3((float)0.3, (float)0.3);
                }

                StartCoroutine(Util.ActivateForSecondsD(clone, 0.5f));

                clone = Instantiate(PVCosplay.ptstompsplash);
                clone.layer = (int)PhysLayers.HERO_ATTACK;
                clone.transform.position = new Vector3(HeroController.instance.transform.position.x, HeroController.instance.transform.position.y, 0);
                clone.GetComponent<ParticleSystem>().Play();

                collider = clone.AddComponent<BoxCollider2D>();
                collider.size = new Vector2(8, 5);
                collider.offset = new Vector2(0, 0);
                collider.isTrigger = true;
                collider.enabled = true;

                if (HeroController.instance.playerData.equippedCharm_19)
                {
                    clone.transform.localScale += new Vector3((float)0.3, (float)0.3);
                }

                dmg = clone.GetOrAddComponent<PaintSplash>();

                StartCoroutine(Util.DelayedActivateForSecondsD(clone, 0.75f, 0.4f));
            });

            FsmObject clip = PVCosplay.sheofsm.GetAction<AudioPlayerOneShotSingle>("Dstab Land", 7).audioClip;

            FsmStateAction action = new AudioPlay(){
                gameObject = owner,
                volume = 1f,
                oneShotClip = clip
            };
            
            landstate.InsertAction(2,action);
           yield return true;

            
        }


        void TransitionControl()
        {
            HeroController.instance.spellControl.SendEvent("PAINTDOWN");
        }


        void CreatePaintR(int i)
        {
            float multiplier = 1f;
            clone = Instantiate(PVCosplay.paintshotR);
            Destroy(clone.GetComponent<DamageHero>());
            Destroy(clone.GetComponent<PaintBullet>());
            BoxCollider2D collider = clone.GetComponent<BoxCollider2D>();
            collider.isTrigger = true;
            clone.layer = (int)PhysLayers.HERO_ATTACK;

            dmg = clone.GetOrAddComponent<DamageEnemies>();
            dmg.damageDealt = damagenumber;
            dmg.attackType = AttackTypes.Spell;
            dmg.ignoreInvuln = false;
            dmg.magnitudeMult = 0f;
            dmg.moveDirection = false;
            dmg.circleDirection = false;
            dmg.direction = 0;
            dmg.enabled = true;
            dmg.specialType = SpecialTypes.None;

            if (HeroController.instance.playerData.equippedCharm_19)
            {
                clone.transform.localScale += new Vector3((float)0.3, (float)0.3);
                multiplier *= 1.2f;
            }

            dmg.damageDealt = (int)Math.Ceiling(damagenumber * multiplier);

            GameObject trailobject = clone.Child("Pt Trail");
            ParticleSystem trail = trailobject.GetComponent<ParticleSystem>();
            paint = clone.GetOrAddComponent<PaintBullet>();
            paint.SetRed();
            paint.trailParticle = trail;
            paint.stretchFactor = 1.2f;
            paint.stretchMaxY = 1.75f;
            paint.stretchMinX = 0.75f;
            paint.scaleMax = 1.45f;
            paint.scaleMin = 1.45f;
            paint.enabled = true;    

            clone.transform.position = new Vector3(HeroController.instance.transform.position.x, HeroController.instance.transform.position.y, 0);
            clone.SetActive(true);

            StartCoroutine(Velocity(clone, i));

        }

        private IEnumerator Velocity(GameObject cloneobject, int i)
        {
            //velocity

            float timeElapsed = 0;
            Vector3 initialPosition = HeroController.instance.transform.position;


            float[] xvals = { -10, -20, -30, 10, 20 , 30};
            float[] lerpdurs = { 10, 12, 14, 10, 12, 14 };

            Vector3 endPosition = initialPosition + new Vector3(xvals[i - 1],-20);

            while (cloneobject)
            {
                //movement
                if (timeElapsed < lerpDuration)
                {
                    cloneobject.transform.position = Vector3.Lerp(initialPosition, endPosition, timeElapsed / (lerpdurs[i-1]/10));
                    timeElapsed += Time.deltaTime;
                    yield return new Wait();
                }
                else
                {
                    yield break;
                }

            }
        }
    }

}
    