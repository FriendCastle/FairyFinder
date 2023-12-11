using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR;

public class DemoSceneInterface : MonoBehaviour {
    public Button SpawnButton;
    public Button FairyButton;
    public Button InteractButton;

    [SerializeField] private GameManager gm;
    [SerializeField] private HouseController house;
    [SerializeField] private FairyController fairy;

    private void Awake() {
        gm = FindFirstObjectByType<GameManager>();
        SpawnButton.interactable = true;
        FairyButton.interactable = false;
        InteractButton.interactable = false;
        //TODO:: How many players? Save them to a list
    }

    //TODO:: Add functionality for spawning the "right" amount of houses in the space (with and without fairies)

    //TODO:: When does the game end? When all fairies are discovered? Will we display how many are possible to discover? 

    //TODO:: Add functionality for players taking turns, earning points, etc -- have the fairies follow the player who found them (and display the number somewhere)

    public void SpawnHouse() {
        gm.SpawnHouse(); 
        SpawnButton.interactable = false;
        FairyButton.interactable = true;
        InteractButton.interactable = true;
        house = FindFirstObjectByType<HouseController>();

        if (fairy != null) {
            Destroy(fairy.gameObject); 
        }
    }

    public void AddFairy() {
        house.AddFairy();
        FairyButton.interactable = false;
    }

    public void Interact() {
        house.Interact();
        SpawnButton.interactable = true;
        FairyButton.interactable = false;
        InteractButton.interactable = false;
        fairy = FindFirstObjectByType<FairyController>();
    }
}

public class GameManager : MonoBehaviour
{
	public void SpawnHouse()
	{
		
	}
}