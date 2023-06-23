using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace PVCosplay
{
    public class HitCounter : MonoBehaviour
    {
        public int maxhits = 2;
        int totalhits;
        public void OnTriggerStay2D(Collider2D collider)
        {
            if (collider.gameObject.GetComponent<HealthManager>() != null || collider.gameObject.GetComponentInChildren<HealthManager>() != null || collider.GetComponentInParent<HealthManager>() != null)
            {
                totalhits++;
                if (totalhits >= maxhits)
                {
                    transform.gameObject.SetActive(false);
                }
            }
        }
        public void ResetHits()
        {
            totalhits = 0;
        }
    }
}
