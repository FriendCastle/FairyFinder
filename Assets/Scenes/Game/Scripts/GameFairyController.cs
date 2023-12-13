using System.Collections;
using System.Collections.Generic;
using Niantic.Lightship.AR.NavigationMesh;
using Niantic.Lightship.SharedAR.Netcode;
using Unity.Netcode;
using UnityEngine;

public class GameFairyController : MonoBehaviour
{
	[SerializeField]
	private LightshipNavMeshAgent navMeshAgent;

	[SerializeField]
	private LightshipNavMeshAgentPathRenderer pathRenderer;

	[SerializeField]
	private GameObject model;

	private LightshipNavMeshManager navMeshManager;

	private const float MIN_TIME_BETWEEN_MOVEMENT_ATTEMPT = 5f;
	private float currentTimeBetweenMovement = 0f;

	public void Init(LightshipNavMeshManager argLightshipNavMeshManager)
	{
		navMeshManager = argLightshipNavMeshManager;
	}

	public void SetModelVisibility(bool argVisible)
	{
		model.SetActive(argVisible);
		pathRenderer.enabled = argVisible;
	}

	private void Update()
	{
		if (currentTimeBetweenMovement >= MIN_TIME_BETWEEN_MOVEMENT_ATTEMPT)
		{
			currentTimeBetweenMovement = 0f;
			bool foundPosition = navMeshManager.LightshipNavMesh.FindRandomPosition(out Vector3 randomPos);
			if (foundPosition)
			{
				navMeshAgent.SetDestination(randomPos);
			}
		}
		else
		{
			currentTimeBetweenMovement += Time.deltaTime;
		}
	}
}
