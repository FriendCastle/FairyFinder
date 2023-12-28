using Niantic.Lightship.AR.NavigationMesh;
using Unity.Netcode;
using UnityEngine;

public class GameFairyController : NetworkBehaviour
{
	[SerializeField]
	private NetworkObject _networkObject;
	public NetworkObject networkObject => _networkObject;

	[SerializeField]
	private LightshipNavMeshAgent navMeshAgent;

	[SerializeField]
	private LightshipNavMeshAgentPathRenderer pathRenderer;

	[SerializeField]
	private FairyController model;

	private NetworkObject playerToFollow;

	public void SetModelVisibility(bool argVisible)
	{
		model.gameObject.SetActive(argVisible);
	}

	private void Update()
	{
		if (GameNetcodeManager.instance.IsServer && playerToFollow != null)
		{
			if (Vector3.Distance(transform.position, playerToFollow.transform.position) > .5f)
			{
				Vector3 targetPosition = new Vector3(playerToFollow.transform.position.x, transform.position.y, playerToFollow.transform.position.z);

				Quaternion targetRotation = Quaternion.LookRotation(targetPosition - transform.position);
				targetRotation.x = 0f;
				targetRotation.z = 0f;

				transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, 5f * Time.deltaTime);

				transform.position = Vector3.MoveTowards(transform.position, targetPosition, .5f * Time.deltaTime);

				model.SetSpeed(1f);
				SetSpeedClientRpc(1f);
			}
			else
			{
				model.SetSpeed(0f);
				SetSpeedClientRpc(0f);
			}
		}
	}

	public void SetFollow(int argPlayerToFollow)
	{
		Debug.Log("Set fairy to follow player " + argPlayerToFollow);

		playerToFollow = GameNetcodeManager.instance.GetClientObjectThatMatchesPlayerId(argPlayerToFollow);
	}

	[ClientRpc]

	public void EnableModelClientRpc()
	{
		SetModelVisibility(true);
	}

	[ClientRpc]

	public void SetSpeedClientRpc(float argSpeed)
	{
		model.SetSpeed(argSpeed);
	}
}
