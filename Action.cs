using System;
using System.Collections;
using System.Collections.Generic;
using GlobalEnums;
using HutongGames.PlayMaker.Actions;
using IL;
using Modding;
using On;
using UnityEngine;
using UnityEngine.UIElements;
using MyFirstMod;
using System.CodeDom;
using UnityEngine.EventSystems;
using Modding.Utils;
using HutongGames.PlayMaker;
using Vasi;

namespace MyFirstMod
{
    public class Action : MonoBehaviour
    {

        float lerpDuration = 2;
        public GameObject clone;
        FsmState fsm;
        DamageEnemies dmg;
        private void Update()
        {
            fsm = HeroController.instance.spellControl.GetState("Fireball 2");
            fsm.RemoveAction(3);
            fsm.InsertMethod(3, () =>
            {
                for (int i = 1; i < 6; i++)
                {
                    base.StartCoroutine(CreateDagger(i));
                }
            });
        }



        private IEnumerator CreateDagger(int i)
        {
            yield return new WaitForSeconds(0.08f * i);
            clone = Instantiate(myFirstMod.dagger);
            Destroy(clone.GetComponent<DamageHero>());
            Destroy(clone.GetComponent<Rigidbody2D>());
            //Destroy(clone.GetComponent<BoxCollider2D>());
            Destroy(clone.GetComponent<FaceAngleSimple>());
            clone.layer = (int)PhysLayers.HERO_ATTACK;
            
            dmg = clone.GetOrAddComponent<DamageEnemies>();
            dmg.damageDealt = 50;
            dmg.attackType = AttackTypes.Spell;
            dmg.ignoreInvuln = false;
            dmg.magnitudeMult = 0f;
            dmg.moveDirection = false;
            dmg.circleDirection = false;
            dmg.direction = 0;
            dmg.enabled = true;
            dmg.specialType = SpecialTypes.None;

            var value = (myFirstMod.isFacingLeft ? 110 : 250);
            clone.transform.rotation = Quaternion.Euler(0, 0, value + (10 * i * (myFirstMod.isFacingLeft ? -1 : 1)));
            clone.transform.position = new Vector3(HeroController.instance.transform.position.x, HeroController.instance.transform.position.y, 0);
            clone.SetActive(true);

            base.StartCoroutine(Velocity(clone, i));
        }

        private IEnumerator Velocity(GameObject cloneobject, int i)
        {
            //velocity

            float timeElapsed = 0;
            Vector3 initialPosition = HeroController.instance.transform.position;

            float modifier = (myFirstMod.isFacingLeft ? -1 : 1);

            float[] xvals = { 67, 75, 67, 60, 52 };
            float[] yvals = { -15, 0, 15, 30, 45 };

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