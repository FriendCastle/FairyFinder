using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class HouseController : NetworkBehaviour
{

    [SerializeField] private List<GameObject> PossibleHouses;

    [SerializeField] private List<GameObject> OrderedSparkleArea;
    [SerializeField] private List<GameObject> OrderedSparkleExplosion;

    [SerializeField] private AudioClip SoundAppear; 
    [SerializeField] private List<AudioClip> SoundDisappearList;

    [SerializeField] private Vector3 SparkleOffset;

    private GameObject houseObject;

	[SerializeField]
    private GameObject fairyObject;
    private AudioSource audioSource;
    private Animator animator;
    private int orderIndex;

	public NetworkVariable<int> houseIndex = new NetworkVariable<int>(-1);

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

	[ServerRpc(RequireOwnership = false)]
	public void AssignHouseIndexServerRpc(int argHouseIndex)
	{
		houseIndex.Value = argHouseIndex;
	}


    public void Interact() {
        Instantiate(OrderedSparkleExplosion[orderIndex], transform.position + SparkleOffset, Quaternion.identity, transform);
        animator.SetTrigger("Disappear");
		if (GameManager.instance.FairyIndex == houseIndex.Value)
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
    }

    IEnumerator RevealFairy() {
        if (fairyObject != null) {
            fairyObject.SetActive(true);
            Instantiate(OrderedSparkleArea[orderIndex], transform.position, Quaternion.identity, transform);
        }

        yield return new WaitForSeconds(MAX_REVEAL_ANIM_TIME);
        gameObject.SetActive(false);

    }

	private IEnumerator DelayedDestroy()
	{
		yield return new WaitForSeconds(MAX_REVEAL_ANIM_TIME);
        gameObject.SetActive(false);
	}

	[ClientRpc]
	public void TriggerHouseInteractionClientRpc()
	{
		Interact();
	}
}
