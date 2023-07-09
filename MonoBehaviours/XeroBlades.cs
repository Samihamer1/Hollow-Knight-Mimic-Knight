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
    public class XeroBlades : MonoBehaviour
    {

        Vector3[] offsets = {new Vector3(-2,1), new Vector3(-1,2), new Vector3(1,2), new Vector3(2,1)};
        float damagenumber = 15;
        GameObject clone;
        FsmState fsm;
        DamageEnemies dmg;
        bool bladeout;

        private void Start()
        {
            fsm = HeroController.instance.spellControl.GetState("Scream Burst 2C");
            fsm.RemoveAction(8);
            fsm.RemoveAction(3);
            fsm.InsertMethod(3, () =>
            {
                CreateBlades();
            });
            fsm.RemoveAction(1);
            fsm.RemoveAction(0);
        }

        private void CreateBlades()
        {
            if (!bladeout)
            {
                bladeout = true;
                for (int i = 0; i < 4; i++)
                {
                    clone = Instantiate(PVCosplay.xeroblade);
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
                        multiplier *= 1.25f;
                    }

                    dmg.damageDealt = (int)Math.Ceiling(damagenumber * multiplier);


                    clone.transform.position = new Vector3(HeroController.instance.transform.position.x, HeroController.instance.transform.position.y, 0);
                    clone.SetActive(true);

                    //find a target....
                    HealthManager[] obj = FindObjectsOfType<HealthManager>();
                    GameObject target = null;
                    float distance = 9999999f;
                    for (int j = 0; j < obj.Length; j++)
                    {

                        float mag = (obj[j].gameObject.transform.position - clone.transform.position).magnitude;
                        if (mag < distance)
                        {
                            distance = mag;
                            target = obj[j].gameObject;
                        }
                    }

                    if (target == null || distance > 35f)
                    {
                        StartCoroutine(Dissipate(clone));
                        if (i == 3)
                        {
                            bladeout = false;
                        }
                    }
                    else
                    {
                        StartCoroutine(BladeBehaviour(clone, target, i));
                    }
                }
            } else
            {
                if (HeroController.instance.playerData.equippedCharm_33)
                {
                    HeroController.instance.AddMPCharge(24);
                } else
                {
                    HeroController.instance.AddMPCharge(33);
                }
            }
        }

        private IEnumerator BladeBehaviour(GameObject blade, GameObject target, int i)
        {
            PlayMakerFSM fsm = blade.GetComponent<PlayMakerFSM>();
            DistanceFlySmooth homeaction = fsm.GetState("Home").GetAction<DistanceFlySmooth>();
            homeaction.target = HeroController.instance.gameObject;
            homeaction.targetRadius = 0.1f;
            homeaction.offset = offsets[i];

            GetAngleToTarget2D action = fsm.GetState("Antic Point").GetAction<GetAngleToTarget2D>();
            action.target = target;
            action.offsetY = 0;

            GetAngleToTarget2D action2 = fsm.GetState("Antic Spin").GetAction<GetAngleToTarget2D>();
            action2.target = target;
            action2.offsetY = 0;

            fsm.GetState("Shoot").RemoveAction(5);
            yield return new WaitForSeconds(0.5f * i);
            fsm.SendEvent("ATTACK");
            yield return new WaitForSeconds(1.5f);
            Destroy(blade);
            if (i == 3)
            {
                bladeout = false;
            }
        }

        private IEnumerator Dissipate(GameObject blade)
        {
            yield return new WaitForSeconds(0.6f);
            PlayMakerFSM fsm = blade.GetComponent<PlayMakerFSM>();
            fsm.SetState("Dissipate");
            yield return new WaitForSeconds(0.25f);
            Destroy(blade);
        }
    }
}