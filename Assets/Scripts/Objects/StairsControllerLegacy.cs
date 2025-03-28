using System;
using System.Linq;
using UnityEngine;

public class StairsControllerLegacy : MonoBehaviour, IInteractable {
	public bool stairsGoUpwards;
	[SerializeField] private GameObject LinkedStairs;

	public void Interact() {
		if (SceneController.instance != null)
		{
			print("Going up stairs" + this.name);
			if (stairsGoUpwards) { SceneController.instance.LoadNextLevel(); }
			else { SceneController.instance.LoadPreviousLevel(); }
		}
		GameObject camera = GameObject.FindGameObjectWithTag("MainCamera");
		GameObject player = GameObject.FindGameObjectWithTag("Player");
		
		Vector3 closestStairs = (LinkedStairs) ? LinkedStairs.transform.position : FindClosestStairs(!stairsGoUpwards);

		//TODO change TO A COLLIDER HITPOINT???
		player.transform.position = closestStairs;
		camera.transform.position = new Vector3(player.transform.position.x, player.transform.position.y, camera.transform.position.z);
		
		Physics2D.SyncTransforms();
	}

	private Vector3 FindClosestStairs(bool dir = false)
	{
		var levelObjectTransform = SceneController.instance.levels[SceneController.instance.currentLevel].transform;
		
		var childrenWithTag = levelObjectTransform.Cast<Transform>()
			.Where(t => t.CompareTag("Stairs") && t.TryGetComponent<StairsControllerLegacy>(out StairsControllerLegacy Stair) && Stair.stairsGoUpwards == dir)
			.Select(t => t.gameObject)
			.ToList();
		
		GameObject bestTarget = null;
		Vector3 currentPosition = transform.position;
		float closestDistanceSqr = Mathf.Infinity;
		foreach(GameObject potentialTarget in childrenWithTag)
		{
			Vector3 directionToTarget = potentialTarget.transform.position - currentPosition;
			float dSqrToTarget = directionToTarget.sqrMagnitude;
			if(dSqrToTarget < closestDistanceSqr)
			{
				closestDistanceSqr = dSqrToTarget;
				bestTarget = potentialTarget;
			}
		}
		return bestTarget.transform.position;
	}
}
