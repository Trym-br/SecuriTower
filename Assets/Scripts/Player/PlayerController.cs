using UnityEngine;
using System.Collections.Generic;
using System.Linq;

using static AugustBase.All;

[RequireComponent(typeof(Rigidbody2D), typeof(InputActions))]
public class PlayerController : MonoBehaviour {
	public static PlayerController instance;

	public float speed = 2.35f;

	[Range(0.0f, 1.0f)]
	public float friction = 0.425f;

	public float objectPushingDelay = 0.25f;

	Rigidbody2D  playerRB;
	InputActions input;
	Camera       playerCamera;

	void Awake() {
		if (instance != null) {
			Debug.LogError("There are two players!");
			return;
		}

		instance = this;

		playerRB = GetComponent<Rigidbody2D>();
		playerRB.gravityScale           = 0.0f;
		playerRB.constraints            = RigidbodyConstraints2D.FreezeRotation;
		playerRB.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
		playerRB.bodyType               = RigidbodyType2D.Dynamic;
		playerRB.interpolation          = RigidbodyInterpolation2D.Interpolate;

		input = GetComponent<InputActions>();

#if UNITY_EDITOR
		if (SceneController.instance.currentLevel == 0) {
			transform.position = Vector3.zero;
		}
#else
		transform.position = Vector3.zero;
#endif

		playerCamera = Camera.main;
	}

	void Start() {
		FindStairsInCurrentLevel();
		if (SceneController.instance.currentLevel != 0) {
			transform.position = FindClosestStairPosition(transform.position, false);
		}
	}

	const string parentLevelObjectTag = "Levels Parent Object";
	const string stairsTag = "Stairs";

	StairsController[] stairsInCurrentLevel = new StairsController[0];

	void FindStairsInCurrentLevel() {
		SetLength(ref stairsInCurrentLevel, 0);
		var levelObjectTransform = SceneController.instance.levels[SceneController.instance.currentLevel].transform;

		for (int i = 0; i < levelObjectTransform.childCount; ++i) {
			var child = levelObjectTransform.GetChild(i);

			if (child.CompareTag(stairsTag)) {
				if (child.TryGetComponent<StairsController>(out var stairs)) {
					Append(ref stairsInCurrentLevel, stairs);
				}
			}
		}

		print($"FOUND {stairsInCurrentLevel.Length} STAIRS");
		for (int i = 0; i < stairsInCurrentLevel.Length; ++i) {
			print($"{stairsInCurrentLevel[i]}, position: {stairsInCurrentLevel[i].transform.position}");
		}
	}

	Vector3 FindClosestStairPosition(Vector3 closestTo, bool stairsShouldGoUp) {
		float closestDistanceSquared = Mathf.Infinity;
		StairsController closestStair = null;

		for (int i = 0; i < stairsInCurrentLevel.Length; ++i) {
			if (stairsInCurrentLevel[i].stairsGoUpwards != stairsShouldGoUp) continue;

			var delta = stairsInCurrentLevel[i].transform.position - closestTo;
			float distSquared = Mathf.Abs(delta.x * delta.x) + Mathf.Abs(delta.y * delta.y);

			if (distSquared < closestDistanceSquared) {
				closestDistanceSquared = distSquared;
				closestStair = stairsInCurrentLevel[i];
			}
		}

		if (closestStair != null) {
			return closestStair.transform.position;
		}

		// @Hack
		return closestTo;
	}

	void Update() {
		currentMovementInput = input.movement;
		if (input.interactBegin)
		{
			InteractWithNearest();	
		}
		// if (input.interactBegin) {
		// 	playerWantsToInteract = true;
		// }
	}

	void FixedUpdate() {
		playerRB.linearVelocity += currentMovementInput.normalized * speed;
		playerRB.linearVelocity *= (1.0f - Mathf.Clamp01(friction));

		previousMoveDirection = moveDirection;
		moveDirection = GetHeaviestDirectionOfFour(currentMovementInput);

		MaybeMoveBoxes();
	}
	
	Vector2 GetHeaviestDirectionOfFour(Vector2 v) {
		var result = Vector2.zero;

		if (Mathf.Abs(v.x) > Mathf.Abs(v.y)) {
			if      (v.x < 0.0f) result.x = -1.0f;
			else if (v.x > 0.0f) result.x =  1.0f;
		} else {
			if      (v.y < 0.0f) result.y = -1.0f;
			else if (v.y > 0.0f) result.y =  1.0f;
		}

		return result;
	}

	public Vector2 boxCheckerSize = new Vector2(0.1f, 0.5f);
	public float boxCheckerOffset = 0.65f;

	Vector2 currentMovementInput;
	Vector2 moveDirection = new Vector2(1.0f, 0.0f);

	bool playerWantsToInteract;

	Vector2 GetBoxCheckerSizeWithDirectionAdjustment() {
		var result = boxCheckerSize;

		if (Mathf.Abs(moveDirection.x) < Mathf.Abs(moveDirection.y)) {
			var temp = result.x;
			result.x = result.y;
			result.y = temp;
		}

		return result;
	}

	int   objectBeingPushedAgainstID;
	float objectBeingPushedAgainstStartedAt = -Mathf.Infinity;
	Vector2 objectBeingPushedAgainstPushDirection;
	void MaybeMoveBoxes() {
		if (moveDirection != objectBeingPushedAgainstPushDirection) {
			objectBeingPushedAgainstID = 0;
			objectBeingPushedAgainstPushDirection = Vector2.zero;
		}

		var boxCheckerPosition = moveDirection * boxCheckerOffset;
		boxCheckerPosition.x += transform.position.x;
		boxCheckerPosition.y += transform.position.y;

		Collider2D[] collidersAtTarget = Physics2D.OverlapBoxAll(boxCheckerPosition, GetBoxCheckerSizeWithDirectionAdjustment(), 0.0f);

		if (collidersAtTarget.Length == 0) {
			objectBeingPushedAgainstID = 0;
			objectBeingPushedAgainstPushDirection = Vector2.zero;
		}

		for (int i = 0; i < collidersAtTarget.Length; ++i) {
			if (collidersAtTarget[i].TryGetComponent<MakeMoveable>(out var moveableAtTarget)) {
				var id = collidersAtTarget[i].gameObject.GetInstanceID();
				if (objectBeingPushedAgainstID == 0) {
					objectBeingPushedAgainstID = id;
					objectBeingPushedAgainstStartedAt = Time.time;
				}

				objectBeingPushedAgainstPushDirection = moveDirection;

				if (objectBeingPushedAgainstStartedAt + objectPushingDelay < Time.time) {
					moveableAtTarget.TryMoveInDirection(moveDirection);
				}
			}

			// TODO: Should we prioritize NPCs instead of going with the first one it finds?
			if (playerWantsToInteract && collidersAtTarget[i].TryGetComponent<IInteractable>(out var interactable)) {
				interactable.Interact();
				playerWantsToInteract = false;
			}
		}

		playerWantsToInteract = false;
	}

	Vector2 previousMoveDirection;

	[SerializeField] private float InteractRange = 0.5f;
	// ("Do the dings" - August)
	private void InteractWithNearest()
	{
		//		Find all objects in range
		Collider2D[] hitColliders = Physics2D.OverlapCircleAll(transform.position, InteractRange);
		// Collider2D[] hitColliders = Physics2D.OverlapBoxAll(transform.position, new Vector2(InteractRange, InteractRange), 0);
		//		Narrow down to objects with the Interactable Interface
		List<GameObject> gameObjects = hitColliders
			.Select(col => col.gameObject)                   // Get GameObject from Collider
			.Where(go => go.GetComponentInChildren<IInteractable>() != null)    // Check if it has the interface
			.ToList();      
		
		// print("hitColliders: " + hitColliders.Length + " / gameObjects: " + gameObjects.Count);
		//		Finds the closest one out of the list
		GameObject bestTarget = null;
		float closestDistanceSqr = Mathf.Infinity;
		Vector3 currentPosition = transform.position;
		foreach(GameObject potentialTarget in gameObjects)
		{
			// print("Checking: " + potentialTarget.name);
			Vector3 directionToTarget = potentialTarget.transform.position - currentPosition;
			float dSqrToTarget = directionToTarget.sqrMagnitude;
			if(dSqrToTarget < closestDistanceSqr)
			{
				closestDistanceSqr = dSqrToTarget;
				bestTarget = potentialTarget;
			}
		}
		//		Interacts with the found object, if any found
		if (bestTarget)
		{
			bestTarget.GetComponentInChildren<IInteractable>().Interact();
		}
	}

	void GoUpOrDownStairs(Vector3 stairPosition, bool stairsGoUp) {
		if (stairsGoUp) SceneController.instance.LoadNextLevel();
		else            SceneController.instance.LoadPreviousLevel();

		FindStairsInCurrentLevel();

		var closest = FindClosestStairPosition(stairPosition, !stairsGoUp);

		// Make sure the camera position relative to the player stays the same.
		var deltaToCamera = transform.position - playerCamera.transform.position;
		var newCameraPosition = closest;
		newCameraPosition.x -= deltaToCamera.x;
		newCameraPosition.y -= deltaToCamera.y;
		// TODO: Check if cinemachine hates this.
		playerCamera.transform.position = newCameraPosition;

		transform.position = closest;

		Physics2D.SyncTransforms();
	}

	public void GoUpStairs(Vector3 stairPosition)   => GoUpOrDownStairs(stairPosition, true);
	public void GoDownStairs(Vector3 stairPosition) => GoUpOrDownStairs(stairPosition, false);

#if UNITY_EDITOR
	void OnDrawGizmos() {
		var boxCheckerPosition = moveDirection * boxCheckerOffset;
		boxCheckerPosition.x += transform.position.x;
		boxCheckerPosition.y += transform.position.y;

		Gizmos.color = Color.blue;
		Gizmos.DrawWireCube(boxCheckerPosition, GetBoxCheckerSizeWithDirectionAdjustment());
		Gizmos.DrawWireSphere(transform.position, InteractRange);
	}
#endif
}
