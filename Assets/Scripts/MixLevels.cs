using UnityEngine;
using System.Collections;
using UnityEngine.Audio;

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
}
