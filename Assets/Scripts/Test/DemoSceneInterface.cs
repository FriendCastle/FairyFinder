using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR;

public class DemoSceneInterface : MonoBehaviour {
    public Button SpawnButton;
    public Button FairyButton;
    public Button InteractButton;

    private Spawner spawner;
    private HouseController house;
    private FairyController fairy;

    private void Awake() {
        spawner = FindFirstObjectByType<Spawner>();
        SpawnButton.interactable = true;
        FairyButton.interactable = false;
        InteractButton.interactable = false;
    }

    public void SpawnHouse() {
        spawner.SpawnHouse(); 
        SpawnButton.interactable = false;
        FairyButton.interactable = true;
        InteractButton.interactable = true;
        house = FindFirstObjectByType<HouseController>();
    }

    public void AddFairy() {
        house.AddFairy();
        FairyButton.interactable = false;
        fairy = FindFirstObjectByType<FairyController>();
    }

    public void Interact() {
        house.Interact();
        SpawnButton.interactable = true;
        FairyButton.interactable = false;
        InteractButton.interactable = false;
    }
}
