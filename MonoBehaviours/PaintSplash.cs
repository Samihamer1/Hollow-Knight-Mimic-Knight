using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Vasi;

namespace PVCosplay
{
    public class PaintSplash: MonoBehaviour
    {
        int damagenumber = 25;
        public void OnTriggerStay2D(Collider2D collider)
        {
            if (collider.gameObject.GetComponent<HealthManager>() != null || collider.gameObject.GetComponentInChildren<HealthManager>() != null || collider.GetComponentInParent<HealthManager>() != null)
            {
                HitInstance hitInstance = new HitInstance();
                hitInstance.AttackType = AttackTypes.Spell;
                hitInstance.IgnoreInvulnerable = false;
                hitInstance.MagnitudeMultiplier = 1;
                hitInstance.MoveDirection = true;
                float multiplier = 1f;
                if (HeroController.instance.playerData.equippedCharm_19)
                {
                    multiplier *= 1.2f;
                }

                hitInstance.DamageDealt = (int)Math.Ceiling(damagenumber * multiplier);
                HitTaker.Hit(collider.gameObject, hitInstance);
            }
        }
    }
}
