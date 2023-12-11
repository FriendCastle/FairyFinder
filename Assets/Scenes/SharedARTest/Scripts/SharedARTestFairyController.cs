using System.Collections;
using System.Collections.Generic;
using Niantic.Lightship.SharedAR.Netcode;
using Unity.Netcode;
using UnityEngine;

public class SharedARTestFairyController : MonoBehaviour
{
	[SerializeField]
	private NetworkObject networkObject;
	public NetworkObject NetworkObject => networkObject;


	[SerializeField]
	private LightshipNetworkObject lightshipNetworkObject;
	public LightshipNetworkObject LightshipNetworkObject => lightshipNetworkObject;

	[SerializeField]
	private GameObject model;

	public void SetModelVisibility(bool argVisible)
	{
		model.SetActive(argVisible);
	}
}
