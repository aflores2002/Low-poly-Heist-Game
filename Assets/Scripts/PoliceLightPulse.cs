using UnityEngine;

public class PoliceLightPulse : MonoBehaviour
{
    public Light policeLight;
    public float pulseSpeed = 5f;
    public float minIntensity = 0f;
    public float maxIntensity = 6f;

    void Update()
    {
        float pulse = Mathf.PingPong(Time.time * pulseSpeed, 1);
        policeLight.intensity = Mathf.Lerp(minIntensity, maxIntensity, pulse);
    }
}

