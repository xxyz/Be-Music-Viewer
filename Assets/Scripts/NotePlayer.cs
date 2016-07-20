using UnityEngine;
using System.Collections;
using BMSParser_new;
using System.Collections.Generic;

public class NotePlayer : MonoBehaviour {

    public List<Note> notes;
    public ulong pulse;

    private int noteIndex = 0;
    private AudioSource audioSource;

	// Use this for initialization
	void Start () {
        audioSource = GetComponent<AudioSource>();
        audioSource.Play();
	}
	
	// Update is called once per frame
	void Update () {
        if (notes != null)
        {
            if (noteIndex < notes.Count && pulse > notes[noteIndex].y)
            {
                if(audioSource.clip.isReadyToPlay)
                    audioSource.Play();

                noteIndex++;
            }
        }
    }
}
