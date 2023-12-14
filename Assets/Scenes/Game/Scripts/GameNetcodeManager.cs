using Niantic.Lightship.AR.NavigationMesh;
using UnityEngine;
using Niantic.Lightship.SharedAR.Colocalization;
using Unity.Netcode;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System;
using Random = UnityEngine.Random;

public class GameNetcodeManager : MonoBehaviour
{
	public static GameNetcodeManager instance { get; private set; }

	[SerializeField]
	private Camera arCamera = null;

	[SerializeField]
	private LightshipNavMeshManager navMeshManager = null;

	[SerializeField]
	private SharedSpaceManager _sharedSpaceManager = null;

	[SerializeField]
	private Texture2D _targetImage = null;

	[SerializeField]
	private float _targetImageSize = 0.09f;

	private bool _startAsHost = false;

	private bool connected = false;

	private Dictionary<ulong, ClientContainer> clientContainerDict = new Dictionary<ulong, ClientContainer>();
	private ulong localClientId;
	private class ClientContainer
	{
		public ulong clientId;
		public string playerName;
		public GamePlayerController gamePlayerController;

		public ClientContainer(ulong clientId, string playerName, GamePlayerController gamePlayerController)
		{
			this.clientId = clientId;
			this.playerName = playerName;
			this.gamePlayerController = gamePlayerController;
		}

		public override string ToString()
		{
			return $"ClientContainer - ClientId: {clientId}, PlayerName: {playerName}, GamePlayerController: {gamePlayerController}";
		}
	}

	public bool IsServer => NetworkManager.Singleton != null && NetworkManager.Singleton.IsServer;
	private Action<ulong, int> OnClientConnected;

	void Awake()
	{
		instance = this;
	}

	private void Start()
	{
		_sharedSpaceManager.sharedSpaceManagerStateChanged += OnColocalizationTrackingStateChanged;
		NetworkManager.Singleton.OnServerStarted += OnServerStarted;
		NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnectedCallback;
		NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnectedCallback;
	}

	private void Update()
	{
		if (connected == false)
		{
			return;
		}
	}

	public string StartNewRoom(int argPlayerCount)
	{
		int code = Random.Range(0, 10000);
		string name = code.ToString("D4");
		SetupRoom(name, argPlayerCount);

		// start as host
		_startAsHost = true;

		return name;
	}

	public void Join(string argRoomName)
	{
		SetupRoom(argRoomName);

		// start as client
		_startAsHost = false;
	}

	private void SetupRoom(string roomName, int argPlayerCount = 10)
	{
		var imageTrackingOptions = ISharedSpaceTrackingOptions.CreateImageTrackingOptions(_targetImage, _targetImageSize);

		//set room name from text box
		var roomOptions = ISharedSpaceRoomOptions.CreateLightshipRoomOptions(roomName, argPlayerCount, "FairyFinder");

		_sharedSpaceManager.StartSharedSpace(imageTrackingOptions, roomOptions);
	}

	private void OnColocalizationTrackingStateChanged(SharedSpaceManager.SharedSpaceManagerStateChangeEventArgs args)
	{
		if (args.Tracking && connected == false)
		{
			Debug.Log("Colocalized.");

			// Start networking
			if (_startAsHost)
			{
				NetworkManager.Singleton.StartHost();
			}
			else
			{
				NetworkManager.Singleton.StartClient();
			}

			GameManager.instance.OnConnectionStarted();
		}
		else
		{
			Debug.Log($"Image tracking not tracking?");
		}
	}

	private void OnServerStarted()
	{
		Debug.Log("Netcode server ready");
		connected = true;
		clientContainerDict.Clear();

		localClientId = NetworkManager.Singleton.LocalClientId;
		clientContainerDict[localClientId] = new ClientContainer(localClientId, "1", NetworkManager.Singleton.LocalClient.PlayerObject.GetComponent<GamePlayerController>());

		UpdateAllPlayerNames();
		TurnOffLocalClientAvatar();
	}

	private void UpdateAllPlayerNames()
	{
		foreach (var client in clientContainerDict)
		{
			Debug.LogFormat("Logging out client data: {0}", client.ToString());
		}

		foreach (ulong clientId in NetworkManager.Singleton.ConnectedClientsIds)
		{
			Debug.LogFormat("Updated name for client {0}", clientId);
			clientContainerDict[clientId].gamePlayerController.UpdatePlayerTextClientRpc(clientContainerDict[clientId].playerName);
		}
	}

	private void OnClientConnectedCallback(ulong argClientId)
	{
		Debug.Log($"Client connected: {argClientId}");

		if (_startAsHost == false)
		{
			localClientId = NetworkManager.Singleton.LocalClientId;
			TurnOffLocalClientAvatar();
			connected = true;
		}
		else
		{
			int clientCount = NetworkManager.Singleton.ConnectedClientsIds.Count;
			clientContainerDict[argClientId] = new ClientContainer(argClientId, clientCount.ToString(),
			NetworkManager.Singleton.ConnectedClients[argClientId].PlayerObject.GetComponent<GamePlayerController>());
			UpdateAllPlayerNames();

			if (OnClientConnected != null)
			{
				OnClientConnected(argClientId, clientCount);
			}
		}
	}

	private void TurnOffLocalClientAvatar()
	{
		if (NetworkManager.Singleton == null)
		{
			return;
		}

		NetworkManager.Singleton.LocalClient.PlayerObject.GetComponent<GamePlayerController>().SetModelVisibility(false);
	}

	// Handle network disconnect
	private void OnClientDisconnectedCallback(ulong clientId)
	{
		if (NetworkManager.Singleton)
		{
			if (NetworkManager.Singleton.IsHost && clientId != NetworkManager.ServerClientId)
			{
				return;
			}
		}

		Debug.Log($"Client disconnected");
		connected = false;
	}

	public void AddOnPlayerConnectedListener(System.Action<ulong, int> argOnPlayerConnected)
	{
		OnClientConnected -= argOnPlayerConnected;
		OnClientConnected += argOnPlayerConnected;
	}

	public void RemoveOnPlayerConnectedListener(System.Action<ulong, int> argOnPlayerConnected)
	{
		OnClientConnected -= argOnPlayerConnected;
	}
}