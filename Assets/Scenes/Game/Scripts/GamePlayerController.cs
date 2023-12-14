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
	private TMP_Text _playerText;

	private NetworkVariable<FixedString32Bytes> playerName = new NetworkVariable<FixedString32Bytes>(writePerm: NetworkVariableWritePermission.Server);

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

	public void UpdatePlayerName(ulong argClientId, FixedString32Bytes argName)
	{
		if (argClientId == OwnerClientId)
		{
			UpdatePlayerTextClientRpc(argClientId, argName);
		}
	}


	[ClientRpc]
	private void UpdatePlayerTextClientRpc(ulong argClientId, FixedString32Bytes argName)
	{
		playerName.Value = argName;
		_playerText.text = "Player " + argName.Value;
	}
}
