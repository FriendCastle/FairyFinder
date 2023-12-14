using System;
using System.Collections;
using System.Collections.Generic;
using Niantic.Lightship.AR.NavigationMesh;
using Unity.Netcode;
using Unity.XR.CoreUtils;
using UnityEngine;
using UnityEngine.XR.ARFoundation;

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
	private GameObject HousePrefab;

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

	private bool inputEnabled = false;
	private int currentPlayerNumber = -1;

	private void Awake()
	{
		instance = this;
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
					break;
				case GameState.GameEnd:
					break;
			}
		};

		playerTurn.OnValueChanged += delegate
		{
			if (playerTurn.Value == currentPlayerNumber)
			{

			}
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
		playerTurn.Value = argNewTurn;
		SetPlayerTurnClientRpc(argNewTurn);
	}

	[ClientRpc]
	private void SetPlayerTurnClientRpc(int argNewTurn)
	{
		playerTurn.Value = argNewTurn;
		Debug.Log($"Game state updated to: {argNewTurn}");
	}

	[ClientRpc]
	public void UpdateCurrentPlayerNumberClientRpc(int argPlayerNumber)
	{
		currentPlayerNumber = argPlayerNumber;
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
