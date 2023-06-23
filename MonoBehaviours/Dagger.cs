using System.Collections;
using GlobalEnums;
using HutongGames.PlayMaker.Actions;
using UnityEngine;
using Modding.Utils;
using HutongGames.PlayMaker;
using Vasi;
using System;

namespace PVCosplay
{
    public class Dagger : MonoBehaviour
    {

        float lerpDuration = 2;
        float damagenumber = 40;
        bool Left = false;
        GameObject clone;
        FsmState fsm;
        DamageEnemies dmg;

        private IEnumerator Start()
        {
            fsm = HeroController.instance.spellControl.GetState("Fireball 2");
            fsm.RemoveAction(3);
            fsm.InsertMethod(3, () =>
            {
                Left = PVCosplay.isFacingLeft;
                for (int i = 1; i < 6; i++)
                {
                    StartCoroutine(CreateDagger(i, Left));
                }
            });
            yield return true;
        }

        private IEnumerator CreateDagger(int i, bool facingLeft)
        {
            yield return new WaitForSeconds(0.08f * i);
            clone = Instantiate(PVCosplay.dagger);
            Destroy(clone.GetComponent<DamageHero>());
            Destroy(clone.GetComponent<Rigidbody2D>());

            Destroy(clone.GetComponent<FaceAngleSimple>());
            clone.layer = (int)PhysLayers.HERO_ATTACK;

            dmg = clone.GetOrAddComponent<DamageEnemies>();
            dmg.damageDealt = 40;
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

            if (HeroController.instance.playerData.equippedCharm_11)
            {
                multiplier *= 0.25f;
                dmg.ignoreInvuln = true;
                HitCounter hitcount = clone.AddComponent<HitCounter>();
                hitcount.ResetHits();

            }

            dmg.damageDealt = (int)Math.Ceiling(damagenumber * multiplier);


            var value = facingLeft ? 110 : 250;
            float modifier = facingLeft ? -1 : 1;
            clone.transform.rotation = Quaternion.Euler(0, 0, value + 10 * i * modifier);
            clone.transform.position = new Vector3(HeroController.instance.transform.position.x, HeroController.instance.transform.position.y, 0);
            clone.SetActive(true);

            StartCoroutine(Velocity(clone, i, modifier));
        }

        private IEnumerator Velocity(GameObject cloneobject, int i, float modifier)
        {
            //velocity

            float timeElapsed = 0;
            Vector3 initialPosition = HeroController.instance.transform.position;


            float[] xvals = { 67, 75, 67, 60, 52 };
            float[] yvals = { -15, 0, 15, 30, 47 };

            Vector3 endPosition = initialPosition + new Vector3(xvals[i - 1] * modifier, yvals[i - 1]);

            while (cloneobject)
            {
                //movement
                if (timeElapsed < lerpDuration)
                {
                    cloneobject.transform.position = Vector3.Lerp(initialPosition, endPosition, timeElapsed / lerpDuration);
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