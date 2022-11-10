using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityExtensions;

public class Options : MonoSingleton<Options>
{
    public GameObject[] vibrations;
    public GameObject[] sounds;
    public bool soundsOn;
    public bool vibrationsOn;
    void Start()
    {
        soundsOn = Convert.ToBoolean(PlayerPrefs.GetInt("Sounds", 0));
        vibrationsOn = Convert.ToBoolean(PlayerPrefs.GetInt("Vibrations", 1));
        ProcessOptions();
    }

    void ProcessOptions()
    {
        vibrations[0].SetActive(vibrationsOn);
        vibrations[1].SetActive(!vibrationsOn);
        sounds[0].SetActive(soundsOn);
        sounds[1].SetActive(!soundsOn);
    }

    public void ChangeSounds()
    {
        soundsOn = !soundsOn;
        PlayerPrefs.SetInt("Sounds",Convert.ToInt32(soundsOn));
        ProcessOptions();
    }
    
    public void ChangeVibrations()
    {
        vibrationsOn = !vibrationsOn;
        PlayerPrefs.SetInt("Vibrations",Convert.ToInt32(vibrationsOn));
        ProcessOptions();
    }
}