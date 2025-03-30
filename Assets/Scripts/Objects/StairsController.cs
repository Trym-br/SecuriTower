using System;
using System.Linq;
using UnityEngine;

public class StairsController : MonoBehaviour {
	public bool stairsGoUpwards;
	[SerializeField] private GameObject LinkedStairs;

	public void PerformStairing() {
		if (SceneController.instance != null)
		{
			print("Going up stairs" + this.name);
			if (stairsGoUpwards) { SceneController.instance.LoadNextLevel(); }
			else { SceneController.instance.LoadPreviousLevel(); }
		}
		GameObject camera = GameObject.FindGameObjectWithTag("MainCamera");
		GameObject player = PlayerController.instance.gameObject;
		
		GameObject closestStairs = (LinkedStairs) ? LinkedStairs : FindClosestStairs(!stairsGoUpwards);

		Vector3 StairPositon = closestStairs.GetComponent<CircleCollider2D>().bounds.center;
		
		//TODO change TO A COLLIDER HITPOINT???
		player.transform.position = StairPositon;
		camera.transform.position = new Vector3(player.transform.position.x, player.transform.position.y, camera.transform.position.z);
		
		PlayerController.instance.StairingWasPerformed();
		Physics2D.SyncTransforms();
	}

	private GameObject FindClosestStairs(bool dir = false)
	{
		var levelObjectTransform = SceneController.instance.levels[SceneController.instance.currentLevel].transform;
		
		var childrenWithTag = levelObjectTransform.Cast<Transform>()
			.Where(t => t.CompareTag("Stairs") && t.TryGetComponent<StairsController>(out StairsController Stair) && Stair.stairsGoUpwards == dir)
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
		return bestTarget;
	}
	
	void OnTriggerEnter2D(Collider2D other) {
		if (other.CompareTag("Player")) {
			PerformStairing();
		}
	}
}
