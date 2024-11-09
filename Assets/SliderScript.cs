using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;

public class SliderScript : MonoBehaviour
{
    public AudioMixer audioMixer;
    public Slider slider;


    void Update()
    {
        float level;
        audioMixer.GetFloat("Volume", out level);
        slider.value = level;
    }
}
