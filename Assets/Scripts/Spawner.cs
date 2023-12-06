using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spawner : MonoBehaviour
{
    [SerializeField] private GameObject HousePrefab;

    //TODO:: Rules for where to spawn the houses within the space based on how many players there are

    public void SpawnHouse() {
        SpawnHouse(new Vector3(0,0,0));
    }

    public void SpawnHouse(Vector3 location) {
        Instantiate(HousePrefab, location, Quaternion.identity);
    }

}
