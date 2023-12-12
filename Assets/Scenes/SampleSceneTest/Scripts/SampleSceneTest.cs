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

    private GameObject reticleInstance;

    private float currentReticleHeight = 0;
	private bool _startAsHost = false;

    void Awake()
    {
        reticleInstance = Instantiate(reticlePrefab);
        reticleInstance.SetActive(false);
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
}