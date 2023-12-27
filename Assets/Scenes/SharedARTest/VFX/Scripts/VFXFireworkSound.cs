using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VFXFireworkSound : MonoBehaviour {
    [SerializeField] private List<AudioClip> SoundFireworksList;

    private AudioSource audioSource;

    void Start() {
        audioSource = GetComponent<AudioSource>();
        int random = UnityEngine.Random.Range(0, SoundFireworksList.Count - 1);
        audioSource.clip = SoundFireworksList[random];
        audioSource.Play();
    }

}
