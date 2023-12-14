using System;
using System.Collections;
using System.Collections.Generic;
using Niantic.Lightship.AR.NavigationMesh;
using Unity.Netcode;
using Unity.VisualScripting;
using Unity.XR.CoreUtils;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using Random = UnityEngine.Random;

public class GameManager : NetworkBehaviour
{
	public static GameManager instance { get; private set; }

	[SerializeField]
	private LightshipNavMeshManager navMeshManager = null;

	[SerializeField]
	private XROrigin xrOrigin = null;

	[SerializeField]
	private ARSession arSession = null;

	[SerializeField]
	private HouseController _housePrefab;

	public string roomName { get; private set; }

	public const int MIN_PLAYER_COUNT = 2;
	public const int MAX_PLAYER_COUNT = 4;

	private int currentPlayerCount = 0;
	private int currentMaxPlayerCount = 0;

	private enum GameState
	{
		None,
		Lobby,
		Game,
		GameEnd
	}

	private NetworkVariable<GameState> currentGameState = new NetworkVariable<GameState>(GameState.None);
	private NetworkVariable<int> playerTurn = new NetworkVariable<int>(0);
	private HouseController[] houseControllers = new HouseController[3];
	private Vector3 startingPosition;
	private bool startingPositionChosen = false;

	private bool inputEnabled = false;
	private int playerId = -1;
	private Camera arCamera => Camera.main;

	private NetworkVariable<int> fairyIndex = new NetworkVariable<int>(-1);

	private const int MAX_ROUND_COUNT = 3;
	private int currentRoundCount = 0;


	private void Awake()
	{
		instance = this;
	}

	private void Update()
	{
		switch (currentGameState.Value)
		{
			case GameState.None:
				break;
			case GameState.Lobby:
				break;
			case GameState.Game:
				UpdateGameState();
				break;
			case GameState.GameEnd:
				break;
		}
	}

	private void UpdateGameState()
	{
		if (GameNetcodeManager.instance.IsServer)
		{
			if (startingPositionChosen == false && Input.GetMouseButtonDown(0))
			{
				if (navMeshManager.LightshipNavMesh != null)
				{
					Ray ray = new Ray(arCamera.transform.position, arCamera.transform.forward);
					RaycastHit hit;

					// TODO: add reticle to indicate initial house placement

					if (Physics.Raycast(ray, out hit, 10f) && navMeshManager.LightshipNavMesh.IsOnNavMesh(hit.point, 0.2f))
					{
						startingPosition = hit.point;
						startingPositionChosen = true;

						RandomizeFairyHouseIndexServerRpc();

						var leftHouse = Instantiate(_housePrefab, hit.point - arCamera.transform.right, Quaternion.identity);
						leftHouse.AssignHouseIndex(0);
						leftHouse.NetworkObject.Spawn();
						houseControllers[0] = leftHouse;

						var middleHouse = Instantiate(_housePrefab, hit.point, Quaternion.identity);
						middleHouse.AssignHouseIndex(1);
						middleHouse.NetworkObject.Spawn();
						houseControllers[1] = middleHouse;

						var rightHouse = Instantiate(_housePrefab, hit.point + arCamera.transform.right, Quaternion.identity);
						rightHouse.AssignHouseIndex(2);
						rightHouse.NetworkObject.Spawn();
						houseControllers[2] = rightHouse;

						houseControllers[fairyIndex.Value].SetFairyEnabled(true);
					}
				}
			}
		}

		if (inputEnabled && Input.GetMouseButtonDown(0))
		{
			Ray ray = new Ray(arCamera.transform.position, arCamera.transform.forward);
			RaycastHit hit;

			if (Physics.Raycast(ray, out hit, 20f))
			{
				var houseController = hit.collider.transform.GetComponentInParent<HouseController>();
				if (houseController != null)
				{
					Debug.Log("hit a house!");
					houseController.Interact();
					houseController.TriggerHouseInteractionClientRpc();
					StartCoroutine(WaitForInteractionAnim());
				}
			}

		}
	}

	private IEnumerator WaitForInteractionAnim()
	{
		yield return new WaitForSeconds(HouseController.MAX_REVEAL_ANIM_TIME);
		SetPlayerTurnServerRpc(playerTurn.Value + 1);
		
	}

	[ServerRpc(RequireOwnership = false)]
	private void RandomizeFairyHouseIndexServerRpc()
	{
		fairyIndex.Value = Random.Range(0, 3);
	}

	public void CreateRoom(int argPlayerCount)
	{
		currentMaxPlayerCount = argPlayerCount;
		roomName = GameNetcodeManager.instance.StartNewRoom(argPlayerCount);
		GameUIManager.instance.UpdateRoomCode();
		SetArEnabled(true);
		SetGameState(GameState.Lobby);
		AssignSyncedVariableListeners();
		GameNetcodeManager.instance.AddOnPlayerConnectedListener(OnClientConnected);
	}

	public void JoinRoom(string argRoomName)
	{
		roomName = argRoomName;
		GameNetcodeManager.instance.Join(roomName);
		GameUIManager.instance.UpdateRoomCode();
		SetArEnabled(true);
		AssignSyncedVariableListeners();
	}

	private void AssignSyncedVariableListeners()
	{
		Debug.Log("assigning synced variable listeners");
		currentGameState.OnValueChanged += delegate
		{
			Debug.Log($"Game state updated to: {currentGameState.Value}");
			switch (currentGameState.Value)
			{
				case GameState.Lobby:
					break;
				case GameState.Game:
					if (GameNetcodeManager.instance.IsServer)
					{
						SetPlayerTurnServerRpc(1);
					}
					break;
				case GameState.GameEnd:
					break;
			}
		};

		playerTurn.OnValueChanged += delegate
		{
			inputEnabled = playerTurn.Value == playerId;
			Debug.LogFormat("Player input enabled:{0}, Turn: {1}", inputEnabled, playerTurn.Value);
		};
	}

	private void SetArEnabled(bool argArEnabled)
	{
		xrOrigin.gameObject.SetActive(argArEnabled);
		arSession.gameObject.SetActive(argArEnabled);
		navMeshManager.gameObject.SetActive(argArEnabled);
	}

	[ServerRpc(RequireOwnership = false)]
	private void SetGameStateServerRpc(GameState newState)
	{
		currentGameState.Value = newState;
	}

	[ServerRpc(RequireOwnership = false)]
	private void SetPlayerTurnServerRpc(int argNewTurn)
	{
		if (argNewTurn > currentMaxPlayerCount)
		{
			playerTurn.Value = 1;
			currentRoundCount += 1;
			Debug.Log("Current Round: " + currentRoundCount);
		}
		else
		{
			playerTurn.Value = argNewTurn;
		}
	}

	[ClientRpc]
	public void SetPlayerIdClientRpc(ulong argClientId, int argPlayerNumber)
	{
		if (argClientId == NetworkManager.Singleton.LocalClientId)
		{
			Debug.Log("setting player id to " + argPlayerNumber);
			playerId = argPlayerNumber;
			GameUIManager.instance.UpdatePlayerNumberText("Player " + argPlayerNumber.ToString());
		}
	}

	private void SetGameState(GameState argGameState)
	{
		if (GameNetcodeManager.instance.IsServer == false)
		{
			return;
		}

		SetGameStateServerRpc(argGameState);
	}

	private void OnClientConnected(ulong argClientId, int argCurrentPlayerCount)
	{
		currentPlayerCount = argCurrentPlayerCount;

		Debug.Log("Player count now: " + argCurrentPlayerCount);
		if (currentPlayerCount == currentMaxPlayerCount)
		{
			Debug.Log("Reached max player count");
			SetGameState(GameState.Game);
		}
	}

	public void OnConnectionStarted()
	{
		GameUIManager.instance.TransitionToGamePanel();
	}
}
