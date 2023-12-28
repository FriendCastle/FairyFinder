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
		if (playerToFollow != null && Vector3.Distance(transform.position, playerToFollow.transform.position) > .1f)
		{
			Vector3 targetPosition = new Vector3(playerToFollow.transform.position.x, transform.position.y, playerToFollow.transform.position.z);
			transform.position = Vector3.MoveTowards(transform.position, targetPosition, .5f * Time.deltaTime);

			Quaternion targetRotation = Quaternion.LookRotation(targetPosition - transform.position);
			targetRotation.x = 0f;
			targetRotation.z = 0f;
			transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, .5f * Time.deltaTime);

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
}
