using UnityEngine;
using System.Collections;
using UnityEngine.Audio;
using UnityEngine.UI;

public class MixLevels : MonoBehaviour {

    public AudioMixer Mixer;

    public void SetMasterLevel(float masterLevel)
    {
        Mixer.SetFloat("MasterVolume", masterLevel);
    }

    public void SetKeyLevel (float keyLevel)
    {
        Mixer.SetFloat("KeyVolume", keyLevel);
    }

    public void SetBackLevel(float backLevel)
    {
        Mixer.SetFloat("BackVolume", backLevel);
    }

    public void SetPitch(float pitchLevel)
    {
        if(pitchLevel > 0.95f && pitchLevel < 1.05f)
        {
            GameObject.Find("PitchSlider").GetComponent<Slider>().value = 1.0f;
            pitchLevel = 1.0f;
        }

        Mixer.SetFloat("MasterPitch", pitchLevel);
        Time.timeScale = pitchLevel;
    }
}
