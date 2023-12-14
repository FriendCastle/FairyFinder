using System;
using TMPro;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

public class GamePlayerController : NetworkBehaviour
{
	[SerializeField]
	private GamePlayerAvatar _playerAvatar;

	[SerializeField]
	private NetworkObject _networkObject;

	[SerializeField]
	private GameObject _model;

	[SerializeField]
	private GameObject[] _wands;

	[SerializeField]
	private TMP_Text _playerText;

	private int currentWandIndex;

	private void Awake()
	{
	}

	public override void OnNetworkSpawn()
	{
		base.OnNetworkSpawn();
	}

	public void SetModelVisibility(bool argVisible)
	{
		_model.SetActive(argVisible);
	}


	[ClientRpc]
	public void UpdatePlayerTextClientRpc(FixedString32Bytes argName)
	{
		_playerText.text = "Player " + argName.Value;
		currentWandIndex = Convert.ToInt32(argName.Value) - 1;

		_wands[currentWandIndex].SetActive(true);
	}
}
