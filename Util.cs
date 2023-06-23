using System.Collections;
using UnityEngine;


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
    }
}
