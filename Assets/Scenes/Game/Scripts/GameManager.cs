using System;
using System.Collections;
using System.Collections.Generic;
using Niantic.Lightship.AR.NavigationMesh;
using Unity.Netcode;
using Unity.XR.CoreUtils;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using Random = UnityEngine.Random;

// Game Manager Class to control the game state inlcuding rounds, points, and network objects
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
	private Vector3 startingRightDirection;
	private bool startingPositionChosen = false;

	private bool inputEnabled = false;
	private int playerId = -1;
	private Camera arCamera => Camera.main;

	private const int MAX_ROUND_COUNT = 3;
	private int currentRoundCount = 0;


	// Player score information to be synced for all clients
	private Dictionary<int, int> playerPointDict = new Dictionary<int, int>();
	private NetworkVariable<int> playerWithMostPoints = new NetworkVariable<int>();
	private NetworkVariable<int> mostPointCount = new NetworkVariable<int>(-1);

	// Fairy index used to track which house the fairy is hiding in for the current round to be synced for all clients
	private NetworkVariable<int> fairyIndex = new NetworkVariable<int>(-1);
	public int FairyIndex => fairyIndex.Value;

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
		if (GameNetcodeManager.instance.IsServer && startingPositionChosen == false)
		{
			GameUIManager.instance.UpdateGameInstructionText("Look Around!");

			if (navMeshManager.LightshipNavMesh != null)
			{
				Ray ray = new Ray(arCamera.transform.position, arCamera.transform.forward);
				RaycastHit hit;
				bool hitSomething = Physics.Raycast(ray, out hit, 10f) && navMeshManager.LightshipNavMesh.IsOnNavMesh(hit.point, 0.2f);

				// TODO: add reticle to indicate initial house placement

				if (hitSomething)
				{
					GameUIManager.instance.UpdateGameInstructionText("Tap To Start!");

					if (Input.GetMouseButtonDown(0) && hitSomething)
					{
						startingPosition = hit.point;
						startingRightDirection = arCamera.transform.right;
						startingPositionChosen = true;

						SpawnHouses();

						SetPlayerTurnServerRpc(1);
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
					Debug.LogFormat("hit house {0}, fairy was in: {1}!", houseController.houseIndex.Value, fairyIndex.Value);
					TriggerHouseInteractionServerRpc(houseController.houseIndex.Value);
					inputEnabled = false;
				}
			}

		}
	}

	[ServerRpc(RequireOwnership = false)]
	private void TriggerHouseInteractionServerRpc(int argHouseIndex)
	{
		if (GameNetcodeManager.instance.IsServer)
		{
			houseControllers[argHouseIndex].Interact();

			if (argHouseIndex == fairyIndex.Value)
			{
				AddPointsForPlayerServerRpc(playerTurn.Value);
			}

			houseControllers[argHouseIndex].TriggerHouseInteractionClientRpc();
			StartCoroutine(WaitForInteractionAnim(argHouseIndex));
		}
	}

	private void SpawnHouses()
	{
		foreach (var house in houseControllers)
		{
			if (house != null)
			{
				house.NetworkObject.Despawn();
			}
		}

		houseControllers = new HouseController[3];

		RandomizeFairyHouseIndexServerRpc();

		var leftHouse = Instantiate(_housePrefab, startingPosition - startingRightDirection, Quaternion.identity);
		leftHouse.NetworkObject.Spawn();
		leftHouse.AssignHouseIndexServerRpc(0);
		houseControllers[0] = leftHouse;

		var middleHouse = Instantiate(_housePrefab, startingPosition, Quaternion.identity);
		middleHouse.NetworkObject.Spawn();
		middleHouse.AssignHouseIndexServerRpc(1);
		houseControllers[1] = middleHouse;

		var rightHouse = Instantiate(_housePrefab, startingPosition + startingRightDirection, Quaternion.identity);
		rightHouse.NetworkObject.Spawn();
		rightHouse.AssignHouseIndexServerRpc(2);
		houseControllers[2] = rightHouse;

		Debug.Log("Spawning fairy in house " + fairyIndex.Value);
	}

	private IEnumerator WaitForInteractionAnim(int argHouseIndex)
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
					GameUIManager.instance.TransitionToGamePanel();
					break;
				case GameState.GameEnd:
					inputEnabled = false;
					GameUIManager.instance.UpdateGameEndText(string.Format("Game Over!\nPlayer {0} won with {1} points!", playerWithMostPoints.Value, mostPointCount.Value));
					GameUIManager.instance.TransitionToGameEndPanel();
					break;
			}
		};

		playerTurn.OnValueChanged += delegate
		{
			inputEnabled = playerTurn.Value == playerId;
			Debug.LogFormat("Player input enabled:{0}, Turn: {1}", inputEnabled, playerTurn.Value);
			GameUIManager.instance.UpdateGameInstructionText(string.Format("Player {0}'s Turn", playerTurn.Value));

			if (GameNetcodeManager.instance.IsServer && startingPositionChosen)
			{
				SpawnHouses();
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
		if (argNewTurn > currentMaxPlayerCount)
		{
			currentRoundCount += 1;
			Debug.Log("Current Round: " + currentRoundCount);

			if (currentRoundCount == MAX_ROUND_COUNT)
			{
				SetGameStateServerRpc(GameState.GameEnd);
			}
			else
			{
				playerTurn.Value = 1;
			}
		}
		else
		{
			playerTurn.Value = argNewTurn;
		}
	}

	[ServerRpc(RequireOwnership = false)]
	private void AddPointsForPlayerServerRpc(int argPlayer)
	{
		playerPointDict[argPlayer]++;
		Debug.LogFormat("player {0} got a point! now: {1}", argPlayer, playerPointDict[argPlayer]);

		int maxPoints = int.MinValue;
		KeyValuePair<int, int> winningPlayer = default;

		foreach (var player in playerPointDict)
		{
			if (player.Value > maxPoints)
			{
				maxPoints = player.Value;
				winningPlayer = player;
			}
		}

		playerWithMostPoints.Value = Convert.ToInt32(winningPlayer.Key);
		mostPointCount.Value = winningPlayer.Value;

		GameUIManager.instance.UpdatePlayerScoreText(string.Format("Score: {0}", playerPointDict[playerId]));

		foreach (var player in playerPointDict)
		{
			SetPlayerScoreClientRpc(player.Key, player.Value);
		}
	}

	[ClientRpc]
	public void SetPlayerScoreClientRpc(int argPlayerId, int argPoints)
	{
		if (playerId == argPlayerId)
		{
			GameUIManager.instance.UpdatePlayerScoreText(string.Format("Score: {0}", argPoints));
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

			if (GameNetcodeManager.instance.IsServer)
			{
				playerPointDict.Clear();

				foreach (var client in GameNetcodeManager.instance.ClientContainerDict)
				{
					playerPointDict[Convert.ToInt32(client.Value.playerName)] = 0;
				}
			}
		}
	}

	public void OnConnectionStarted()
	{
		GameUIManager.instance.TransitionToLobbyPanel();
	}
}
