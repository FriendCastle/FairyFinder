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
	}
}
