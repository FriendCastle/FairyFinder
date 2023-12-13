using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
	[SerializeField] private GameObject HousePrefab;
	public static GameManager instance { get; private set; }

	public string roomName { get; private set; }

	private void Awake()
	{
		instance = this;
	}
	public void CreateRoom()
	{
		roomName = GameNetcodeManager.instance.StartNewRoom();
		GameUIManager.instance.UpdateRoomCode();
	}

	public void JoinRoom(string argRoomName)
	{
		roomName = argRoomName;
		GameNetcodeManager.instance.Join(roomName);
		GameUIManager.instance.UpdateRoomCode();
	}

	public void SpawnHouse()
	{
		SpawnHouse(new Vector3(0, 0, 0));
	}

	public void SpawnHouse(Vector3 location)
	{
		Instantiate(HousePrefab, location, Quaternion.identity);
	}
}
