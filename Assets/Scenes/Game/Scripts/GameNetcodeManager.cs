using Niantic.Lightship.AR.NavigationMesh;
using UnityEngine;
using Niantic.Lightship.SharedAR.Colocalization;
using Unity.Netcode;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

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

	private string roomName = null;

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

	public void StartNewRoom()
	{
		int code = Random.Range(0, 10000);
		string name = code.ToString("D4");
		SetupRoom(name);

		// start as host
		_startAsHost = true;
	}

	public void Join(string argRoomName)
	{
		SetupRoom(argRoomName);

		// start as client
		_startAsHost = false;
	}

	private void SetupRoom(string roomName)
	{
		var imageTrackingOptions = ISharedSpaceTrackingOptions.CreateImageTrackingOptions(_targetImage, _targetImageSize);

		//set room name from text box
		var roomOptions = ISharedSpaceRoomOptions.CreateLightshipRoomOptions(roomName, 32, "FairyFinder");

		_sharedSpaceManager.StartSharedSpace(imageTrackingOptions, roomOptions);
	}

	private void OnColocalizationTrackingStateChanged(SharedSpaceManager.SharedSpaceManagerStateChangeEventArgs args)
	{
		if (args.Tracking)
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
		TurnOffLocalClientAvatar();
	}

	private void OnClientConnectedCallback(ulong clientId)
	{
		Debug.Log($"Client connected: {clientId}");

		if (_startAsHost)
		{
		}
		else
		{
			TurnOffLocalClientAvatar();
			connected = true;
		}
	}

	private void TurnOffLocalClientAvatar()
	{
		if (NetworkManager.Singleton == null)
		{
			return;
		}

		SharedARTestPlayerController playerController = NetworkManager.Singleton.LocalClient.PlayerObject.GetComponent<SharedARTestPlayerController>();
		playerController.SetModelVisibility(false);
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
}