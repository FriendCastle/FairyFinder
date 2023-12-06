using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HouseController : MonoBehaviour {
    public bool ContainsFairy { get => containsFairy; }

    [SerializeField] private List<GameObject> PossibleHouses;
    [SerializeField] private List<GameObject> PossibleFairies;

    [SerializeField] private List<GameObject> OrderedSparkleArea;
    [SerializeField] private List<GameObject> OrderedSparkleExplosion;

    [SerializeField] private Vector3 SparkleOffset;

    private bool containsFairy;
    private GameObject houseObject;
    private GameObject fairyObject;
    private Animator animator;
    private int sparkleIndex;

    private void Awake() {
        sparkleIndex = UnityEngine.Random.Range(0, OrderedSparkleArea.Count - 1);
        Debug.Log(sparkleIndex);
    }

    void Start() {
        int random = UnityEngine.Random.Range(0, PossibleHouses.Count - 1);
        houseObject = Instantiate(PossibleHouses[random], transform.position, Quaternion.identity, transform);
        Instantiate(OrderedSparkleArea[sparkleIndex], transform.position + SparkleOffset, Quaternion.identity, transform);
        animator = houseObject.GetComponent<Animator>();
    }

    public void Interact() {
        Instantiate(OrderedSparkleExplosion[sparkleIndex], transform.position + SparkleOffset, Quaternion.identity, transform);
        animator.SetTrigger("Disappear");
        StartCoroutine(RevealFairy());
    }

    public void AddFairy() {
        int random = UnityEngine.Random.Range(0, PossibleFairies.Count - 1);
        fairyObject = Instantiate(PossibleFairies[random], transform.position, Quaternion.identity, transform);
        fairyObject.SetActive(false);
    }

    IEnumerator RevealFairy() {
        if (fairyObject != null) {
            //TODO:: Reveal the fairy and make sure it's not a child of this object
        }

        yield return new WaitForSeconds(3);
        Destroy(this.gameObject);

    }
}
