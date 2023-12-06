using UnityEngine;

namespace FriendCastleFX
{
    public class VFXLightFade : MonoBehaviour
    {
        public enum OnEnd { Stay, Disable, Destroy }

        [Header("Seconds to dim the light")]
        public float life = 0.2f;
        public OnEnd onEnd = OnEnd.Destroy;

        private Light light;
        private float initIntensity;

        // Use this for initialization
        private void Start()
        {
            light = GetComponent<Light>();
            if (light != null)
            {
                initIntensity = light.intensity;
            }
        }

        // Update is called once per frame
        private void Update()
        {
            if (light != null)
            {
                light.intensity -= initIntensity * (Time.deltaTime / life);
                if (light.intensity <= 0f)
                {
                    switch (onEnd)
                    {
                        case OnEnd.Stay:
                            // Do nothing
                            break;
                        case OnEnd.Disable:
                            light.enabled = false;
                            break;
                        case OnEnd.Destroy:
                            Destroy(light);
                            break;
                    }
                }
            }
        }
    }
}