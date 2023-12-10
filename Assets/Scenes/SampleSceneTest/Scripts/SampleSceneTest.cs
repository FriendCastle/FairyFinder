using Niantic.Lightship.AR.NavigationMesh;
using UnityEngine;
using Niantic.Lightship.SharedAR.Colocalization;
using Unity.Netcode;
using UnityEngine.UI;
using TMPro;
public class SampleSceneTest : MonoBehaviour
{
    [SerializeField]
    private Camera arCamera = null;

    [SerializeField]
    private LightshipNavMeshManager navMeshManager = null;

    [SerializeField]
    private LightshipNavMeshRenderer navMeshRenderer = null;

    [SerializeField]
    private GameObject reticlePrefab = null;

    [SerializeField]
    private float reticleDistance = 2f;

    [SerializeField]
    private float reticleHeightAboveGround = .1f;

    [SerializeField]
    private GameObject objectToPlace = null;

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

    private GameObject reticleInstance;

    private float currentReticleHeight = 0;
	private bool _startAsHost = false;

    void Awake()
    {
        reticleInstance = Instantiate(reticlePrefab);
        reticleInstance.SetActive(false);
    }

	private void Start()
	{
			_sharedSpaceManager.sharedSpaceManagerStateChanged += OnColocalizationTrackingStateChanged;
            NetworkManager.Singleton.OnServerStarted += OnServerStarted;
            NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnectedCallback;
            NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnectedCallback;
	}


    void Update()
    {
        if (navMeshManager != null)
        {
            LightshipNavMesh navMesh = navMeshManager.LightshipNavMesh;

            Ray ray = new Ray(arCamera.transform.position, arCamera.transform.forward);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, reticleDistance) && navMesh.IsOnNavMesh(hit.point, 0.2f))
            {
                if (reticleInstance != null && reticleInstance.activeSelf == false)
                {
                    reticleInstance.SetActive(true);
                }

                currentReticleHeight = hit.point.y;
            }
        }

        UpdateReticlePosition();

        if (Input.GetMouseButtonDown(0) && reticleInstance != null && reticleInstance.activeSelf)
        {
            Instantiate(objectToPlace, reticleInstance.transform.position, Quaternion.identity);
        }
    }

    void UpdateReticlePosition()
    {
        if (reticleInstance != null)
        {
            Vector3 newPosition = arCamera.transform.position + arCamera.transform.forward * reticleDistance;
            newPosition.y = currentReticleHeight + reticleHeightAboveGround;
            reticleInstance.transform.position = newPosition;
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
		var imageTrackingOptions = ISharedSpaceTrackingOptions.CreateImageTrackingOptions(
		_targetImage, _targetImageSize);

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
		connectionStatusText.text = string.Format("CONNECTION STATUS:\nSERVER STARTED");
		clientsConnectedText.text = string.Format("CLIENTS CONNECTED: {0}", NetworkManager.Singleton.ConnectedClients.Count);
	}

	private void OnClientConnectedCallback(ulong clientId)
	{
		Debug.Log($"Client connected: {clientId}");
		clientsConnectedText.text = string.Format("CLIENTS CONNECTED: {0}", NetworkManager.Singleton.ConnectedClients.Count);
	}

	// Handle network disconnect
	private void OnClientDisconnectedCallback(ulong clientId)
	{
		var selfId = NetworkManager.Singleton.LocalClientId;
		if (NetworkManager.Singleton)
		{
			if (NetworkManager.Singleton.IsHost && clientId != NetworkManager.ServerClientId)
			{
				// ignore other clients' disconnect event
				return;
			}
		}

		Debug.Log($"Client disconnected");
		connectionStatusText.text = string.Format("CONNECTION STATUS:\nDISCONNECTED");
		hostButton.gameObject.SetActive(true);
		joinButton.gameObject.SetActive(true);
		clientsConnectedText.text = string.Format("CLIENTS CONNECTED: {0}", NetworkManager.Singleton.ConnectedClients.Count);
	}
}