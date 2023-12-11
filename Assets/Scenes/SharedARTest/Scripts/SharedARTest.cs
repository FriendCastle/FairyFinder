using Niantic.Lightship.AR.NavigationMesh;
using UnityEngine;
using Niantic.Lightship.SharedAR.Colocalization;
using Unity.Netcode;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
public class SharedARTest : MonoBehaviour
{
	[SerializeField]
	private Camera arCamera = null;

	[SerializeField]
	private LightshipNavMeshManager navMeshManager = null;

	[SerializeField]
	private SharedSpaceManager _sharedSpaceManager = null;

	[SerializeField]
	private Texture2D _targetImage = null;

	[SerializeField]
	private float _targetImageSize = 9;

	[SerializeField]
	private Button hostButton = null;

	[SerializeField]
	private Button joinButton = null;

	[SerializeField]
	private TMP_Text connectionStatusText = null;

	[SerializeField]
	private TMP_Text trackingStatusText = null;

	[SerializeField]
	private TMP_Text clientsConnectedText = null;

	[SerializeField]
	private SharedARTestFairyController fairyControllerPrefab;

	private bool _startAsHost = false;

	private const int MAX_SPORES_TO_SPAWN = 5;
	private const float SPAWN_INTERVAL = 5f;
	private float currentSpawnTime = 0f;
	private List<SharedARTestFairyController> fairyAvatars = new List<SharedARTestFairyController>();

	void Awake()
	{
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
		if (fairyAvatars.Count < MAX_SPORES_TO_SPAWN)
		{
			if (navMeshManager.LightshipNavMesh != null)
			{
				if (currentSpawnTime >= SPAWN_INTERVAL)
				{
					bool foundPosition = navMeshManager.LightshipNavMesh.FindRandomPosition(out Vector3 randomPos);
					if (foundPosition)
					{
						currentSpawnTime = 0f;
						SharedARTestFairyController fairy = Instantiate(fairyControllerPrefab, randomPos, Quaternion.identity);
						fairy.Init(navMeshManager);
						fairyAvatars.Add(fairy);
						currentSpawnTime = 0f;
						Debug.Log(string.Format("Spawned a fairy at {0}", randomPos));
					}
				}
				else
				{
					currentSpawnTime += Time.deltaTime;
				}
			}
		}
	}

	public void StartNewRoom()
	{
		SetupRoom("FriendCastle");

		// start as host
		_startAsHost = true;
	}

	public void Join()
	{
		SetupRoom("FriendCastle");

		// start as client
		_startAsHost = false;
	}

	private void SetupRoom(string roomName)
	{
		var imageTrackingOptions = ISharedSpaceTrackingOptions.CreateImageTrackingOptions(_targetImage, _targetImageSize);

		//set room name from text box
		var roomOptions = ISharedSpaceRoomOptions.CreateLightshipRoomOptions(roomName, 32, "FairyFinder");

		_sharedSpaceManager.StartSharedSpace(imageTrackingOptions, roomOptions);

		hostButton.gameObject.SetActive(false);
		joinButton.gameObject.SetActive(false);

		trackingStatusText.text = string.Format("TRACKING STATUS: Not Tracking");
	}

	private void OnColocalizationTrackingStateChanged(SharedSpaceManager.SharedSpaceManagerStateChangeEventArgs args)
	{
		if (args.Tracking)
		{
			Debug.Log("Colocalized.");
			trackingStatusText.text = string.Format("TRACKING STATUS: Colocalized");

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
			trackingStatusText.text = string.Format("TRACKING STATUS: Not Tracking");
			Debug.Log($"Image tracking not tracking?");
		}
	}

	private void OnServerStarted()
	{
		Debug.Log("Netcode server ready");
		clientsConnectedText.gameObject.SetActive(true);
		connectionStatusText.text = string.Format("CONNECTION STATUS:\nSERVER STARTED");
		clientsConnectedText.text = string.Format("CLIENTS CONNECTED: {0}", NetworkManager.Singleton.ConnectedClients.Count);

		TurnOffLocalClientAvatar();
	}

	private void OnClientConnectedCallback(ulong clientId)
	{
		Debug.Log($"Client connected: {clientId}");

		if (_startAsHost)
		{
			clientsConnectedText.gameObject.SetActive(true);
			clientsConnectedText.text = string.Format("CLIENTS CONNECTED: {0}", NetworkManager.Singleton.ConnectedClients.Count);
		}
		else
		{
			connectionStatusText.text = string.Format("CONNECTION STATUS:\nCONNECTED");
			TurnOffLocalClientAvatar();
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
		connectionStatusText.text = string.Format("CONNECTION STATUS:\nDISCONNECTED");
		hostButton.gameObject.SetActive(true);
		joinButton.gameObject.SetActive(true);
	}
}