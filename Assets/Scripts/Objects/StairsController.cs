using System;
using System.Linq;
using UnityEngine;

public class StairsController : MonoBehaviour {
	public bool stairsGoUpwards;
	[SerializeField] private GameObject LinkedStairs;

	public void PerformStairing() {
		if (SceneController.instance != null)
		{
			if (stairsGoUpwards) { SceneController.instance.LoadNextLevel(); }
			else { SceneController.instance.LoadPreviousLevel(); }
		}
		GameObject camera = GameObject.FindGameObjectWithTag("MainCamera");
		GameObject player = PlayerController.instance.gameObject;
		
		GameObject closestStairs = (LinkedStairs) ? LinkedStairs : FindClosestStairs(!stairsGoUpwards);

		// Vector3 StairPositon = closestStairs.GetComponent<CircleCollider2D>().bounds.center;
		CircleCollider2D circle = closestStairs.GetComponent<CircleCollider2D>();
		Vector3 StairPositon = circle.bounds.center + new Vector3(circle.offset.x, circle.offset.y, 0);
		// Vector3 StairPositon = transform.TransformPoint(closestStairs.GetComponent<CircleCollider2D>().offset);
		print("StairPosition: " + StairPositon);
		
		var delta = camera.transform.position - player.transform.position;
		camera.transform.position = new Vector3(StairPositon.x + delta.x,
		                                        StairPositon.y + delta.y,
		                                        camera.transform.position.z);
		
		//TODO change TO A COLLIDER HITPOINT???
		player.transform.position = StairPositon;

		Physics2D.SyncTransforms();

		// Should be called after position updates!
		PlayerController.instance.StairingWasPerformed(stairsGoUpwards);
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
