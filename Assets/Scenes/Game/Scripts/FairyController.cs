using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FairyController : MonoBehaviour
{
	[SerializeField] private List<AudioClip> SoundRubberDuckyList;
	[SerializeField] private List<AudioClip> SoundVoiceList;

	private Animator animator;
	private AudioSource audioSource;

	void Awake()
	{
		animator = gameObject.GetComponent<Animator>();
		audioSource = GetComponent<AudioSource>();
	}

	void OnEnable()
	{
		StartCoroutine(PlayStartSounds());
		animator.SetTrigger("Jump");
	}

	private IEnumerator PlayStartSounds()
	{
		int random = 0;
		random = UnityEngine.Random.Range(0, SoundRubberDuckyList.Count - 1);
		audioSource.clip = SoundRubberDuckyList[random];
		audioSource.Play();
		yield return new WaitForSeconds(SoundRubberDuckyList[random].length);
		random = UnityEngine.Random.Range(0, SoundVoiceList.Count - 1);
		audioSource.clip = SoundVoiceList[random];
		audioSource.Play();
	}
}
