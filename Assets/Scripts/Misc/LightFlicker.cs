using UnityEngine;
using UnityEngine.Rendering.Universal;
using System.Collections.Generic;
using static AugustBase.All;

public class LightFlicker : MonoBehaviour
{    
    public float intensityMultiplier = 0.1f;
    public float flickeringSpeed = 3.0f;
    public float flickeringSpeedVariance = 0.5f;

    Light2D light;

    float initialIntensity;
    float flickeringOffset;
    float currentFlickeringSpeedVariance;

    void Start()
    {
        light = gameObject.GetComponentOrStop<Light2D>();
        initialIntensity = light.intensity;
        flickeringOffset = Random.Range(0.0f, 1.0f);
        currentFlickeringSpeedVariance = Random.Range(-1.0f * flickeringSpeedVariance, flickeringSpeedVariance);
    }

    void Update()
    {
        light.intensity = initialIntensity 
                          + (Mathf.Sin((Time.time + flickeringOffset)
                                       * (flickeringSpeed + currentFlickeringSpeedVariance))
                             * intensityMultiplier);
    }
}