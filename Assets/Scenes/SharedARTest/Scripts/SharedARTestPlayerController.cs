using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class SharedARTestPlayerController : MonoBehaviour
{
	[SerializeField]
	private PlayerAvatar playerAvatar;

	[SerializeField]
	private NetworkObject networkObject;

	[SerializeField]
	private GameObject model;

	public void SetModelVisibility(bool argVisible)
	{
		model.SetActive(argVisible);
	}
}
