using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ligthAnimator : MonoBehaviour
{
    Light light;

    public AnimationCurve curve;

    float baseIntensity = 0;
    float baseRange = 0;
    public float animationSpeed;
    // Start is called before the first frame update
    void Start()
    {
        light =GetComponent<Light>();
        baseIntensity = light.intensity;
        baseRange= light.range;
    }

    // Update is called once per frame
    void Update()
    {
        light.intensity = baseIntensity* curve.Evaluate(Time.time * animationSpeed);
        light.range = baseRange * curve.Evaluate(Time.time* animationSpeed);
    }
}
