using HutongGames.PlayMaker;
using System.Collections;
using UnityEngine;
using Vasi;

namespace PVCosplay
{
    public static class Util
    {
        public static IEnumerator DelayedDestroy(GameObject obj, float time)
        {
            yield return new WaitForSeconds(time);
            UnityEngine.Object.Destroy(obj);
        }

        public static IEnumerator ActivateForSecondsD(GameObject obj, float time)
        {
            obj.SetActive(true);
            yield return new WaitForSeconds(time);
            UnityEngine.Object.Destroy(obj);
        }

        public static IEnumerator DelayedActivateForSecondsD(GameObject obj, float delaytime, float time)
        {
            yield return new WaitForSeconds(delaytime);
            obj.SetActive(true);
            yield return new WaitForSeconds(time);
            UnityEngine.Object.Destroy(obj);
        }

        public static IEnumerator WhileInState(string state, GameObject obj)
        {
            obj.SetActive(true);
            yield return new WaitWhile(() => state == HeroController.instance.spellControl.ActiveStateName);
            UnityEngine.Object.Destroy(obj);
        }
    }
}
