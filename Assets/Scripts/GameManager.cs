using System;
using System.Collections;
using System.Collections.Generic;
using Niantic.Lightship.AR.NavigationMesh;
using Unity.XR.CoreUtils;
using UnityEngine;
using UnityEngine.XR.ARFoundation;

public class GameManager : MonoBehaviour
{
	public static GameManager instance { get; private set; }

	[SerializeField]
	private LightshipNavMeshManager navMeshManager = null;

	[SerializeField]
	private XROrigin xrOrigin = null;

	[SerializeField]
	private ARSession arSession = null;

	[SerializeField]
	private GameObject HousePrefab;
	
	public string roomName { get; private set; }

	public const int MIN_PLAYER_COUNT = 2;
	public const int MAX_PLAYER_COUNT = 4;

	private void Awake()
	{
		instance = this;
	}
	public void CreateRoom(int argPlayerCount)
	{
		roomName = GameNetcodeManager.instance.StartNewRoom(argPlayerCount);
		GameUIManager.instance.UpdateRoomCode();
		SetArEnabled(true);
	}

	public void JoinRoom(string argRoomName)
	{
		roomName = argRoomName;
		GameNetcodeManager.instance.Join(roomName);
		GameUIManager.instance.UpdateRoomCode();
		SetArEnabled(true);
	}

	private void SetArEnabled(bool argArEnabled)
	{
		xrOrigin.gameObject.SetActive(argArEnabled);
		arSession.gameObject.SetActive(argArEnabled);
		navMeshManager.gameObject.SetActive(argArEnabled);
	}

	public void SpawnHouse()
	{
		SpawnHouse(new Vector3(0, 0, 0));
	}

	public void SpawnHouse(Vector3 location)
	{
		Instantiate(HousePrefab, location, Quaternion.identity);
	}

	public void OnConnectionStarted()
	{
		GameUIManager.instance.TransitionToGamePanel();
	}
}
