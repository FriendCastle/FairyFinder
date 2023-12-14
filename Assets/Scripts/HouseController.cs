using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class HouseController : NetworkBehaviour
{
    public bool ContainsFairy { get => containsFairy; }

    [SerializeField] private List<GameObject> PossibleHouses;
    [SerializeField] private List<GameObject> PossibleFairies;

    [SerializeField] private List<GameObject> OrderedSparkleArea;
    [SerializeField] private List<GameObject> OrderedSparkleExplosion;

    [SerializeField] private AudioClip SoundAppear; 
    [SerializeField] private List<AudioClip> SoundDisappearList;

    [SerializeField] private Vector3 SparkleOffset;

    private bool containsFairy;
    private GameObject houseObject;
    private GameObject fairyObject;
    private AudioSource audioSource;
    private Animator animator;
    private int orderIndex;

	public int houseIndex {get; private set;}

	public const float MAX_REVEAL_ANIM_TIME = 3f;

    private void Awake() {
        orderIndex = UnityEngine.Random.Range(0, OrderedSparkleArea.Count - 1);
    }

    void Start() {
        int random = UnityEngine.Random.Range(0, PossibleHouses.Count - 1);
        houseObject = Instantiate(PossibleHouses[random], transform.position, Quaternion.identity, transform);
        audioSource = houseObject.GetComponent<AudioSource>();
        Instantiate(OrderedSparkleArea[orderIndex], transform.position + SparkleOffset, Quaternion.identity, transform);
        animator = houseObject.GetComponent<Animator>(); 
        audioSource.clip = SoundAppear;
        audioSource.Play();
    }

	public void AssignHouseIndex(int argHouseIndex)
	{
		houseIndex = argHouseIndex;
	}

    public void Interact() {
        Instantiate(OrderedSparkleExplosion[orderIndex], transform.position + SparkleOffset, Quaternion.identity, transform);
        animator.SetTrigger("Disappear");
		if (containsFairy)
		{
			StartCoroutine(RevealFairy());
		}
		else
		{
			int random = UnityEngine.Random.Range(0, SoundDisappearList.Count - 1);
			audioSource.clip = SoundDisappearList[random];
			audioSource.Play();
			StartCoroutine(DelayedDestroy());
		}
    }

    public void AddFairy() {
        int random = UnityEngine.Random.Range(0, PossibleFairies.Count - 1);
        fairyObject = Instantiate(PossibleFairies[random], transform.position, Quaternion.identity, transform);
        fairyObject.SetActive(false);
    }

    IEnumerator RevealFairy() {
        if (fairyObject != null) {
            fairyObject.transform.SetParent(null);
            fairyObject.SetActive(true);
            Instantiate(OrderedSparkleArea[orderIndex], transform.position, Quaternion.identity, transform);
        }

        yield return new WaitForSeconds(MAX_REVEAL_ANIM_TIME);
        Destroy(this.gameObject);

    }

	private IEnumerator DelayedDestroy()
	{
		yield return new WaitForSeconds(MAX_REVEAL_ANIM_TIME);
		Destroy(gameObject);
	}

	public void SetFairyEnabled(bool argEnabled)
	{
		containsFairy = argEnabled;
	}

	[ClientRpc]
	public void TriggerHouseInteractionClientRpc()
	{
		Interact();
	}
}
