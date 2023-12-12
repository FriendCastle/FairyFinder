using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class SharedARTestHouseController : MonoBehaviour
{
	[SerializeField]
	private NetworkObject networkObject;
	public NetworkObject NetworkObject => networkObject;

	[SerializeField]
	private GameObject model;

	public void SetModelVisibility(bool argVisible)
	{
		model.SetActive(argVisible);
	}
}
