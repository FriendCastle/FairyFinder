using UnityEngine;

namespace FriendCastleFX
{
    public class VFXLightFade : MonoBehaviour
    {
        public enum OnEnd { Stay, Disable, Destroy }

        [Header("Seconds to dim the light")]
        public float life = 0.2f;
        public OnEnd onEnd = OnEnd.Destroy;

        private Light lit;
        private float initIntensity;

        // Use this for initialization
        private void Start()
        {
            lit = GetComponent<Light>();
            if (lit != null)
            {
                initIntensity = lit.intensity;
            }
        }

        // Update is called once per frame
        private void Update()
        {
            if (lit != null)
            {
                lit.intensity -= initIntensity * (Time.deltaTime / life);
                if (lit.intensity <= 0f)
                {
                    switch (onEnd)
                    {
                        case OnEnd.Stay:
                            // Do nothing
                            break;
                        case OnEnd.Disable:
                            lit.enabled = false;
                            break;
                        case OnEnd.Destroy:
                            Destroy(lit);
                            break;
                    }
                }
            }
        }
    }
}