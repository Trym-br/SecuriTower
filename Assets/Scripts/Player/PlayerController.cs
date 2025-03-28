using UnityEngine;
using System.Collections.Generic;
using System.Linq;

using static AugustBase.All;

[RequireComponent(typeof(Rigidbody2D), typeof(InputActions))]
public class PlayerController : MonoBehaviour, IResetable {
	public static PlayerController instance;

	// Movement
	public float speed = 2.35f;
	[Range(0.0f, 1.0f)]
	public float friction = 0.425f;

	// Block Pushing
	public float objectPushingDelay = 0.25f;
	
	// Animation
	public Vector3 lastDir = Vector3.zero;

	// Components
	Rigidbody2D  playerRB;
	InputActions input;
	Camera       playerCamera;
	Animator animator;
	[SerializeField] Collider2D collider;
	
	// ???
	public bool inMenu = false;

	void Awake() {
		// Singleton stuff
		if (instance != null) {
			Debug.LogError("There are two players!");
			return;
		}
		instance = this;

		// Player physics
		playerRB = GetComponent<Rigidbody2D>();
		playerRB.gravityScale           = 0.0f;
		playerRB.constraints            = RigidbodyConstraints2D.FreezeRotation;
		playerRB.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
		playerRB.bodyType               = RigidbodyType2D.Dynamic;
		playerRB.interpolation          = RigidbodyInterpolation2D.Interpolate;

		// Player Components
		input = GetComponent<InputActions>();
		animator = GetComponent<Animator>();
		collider = GetComponent<Collider2D>();
		BoxPushRange = collider.bounds.extents.x;

#if UNITY_EDITOR
		if (SceneController.instance != null && SceneController.instance.currentLevel == 0) {
			transform.position = Vector3.zero;
		}
#else
		transform.position = Vector3.zero;
#endif

		playerCamera = Camera.main;
	}

	void Start() {
		// FindStairsInCurrentLevel();
		if (SceneController.instance != null && SceneController.instance.currentLevel != 0) {
			transform.position = FindClosestStairs(false);
		}
	}

	const string parentLevelObjectTag = "Levels Parent Object";

	void Update()
	{
		if (inMenu)
		{
			playerRB.linearVelocity = Vector3.zero;
			return;
		}
		currentMovementInput = input.movement;
		if (input.interactBegin)
		{
			InteractWithNearest();	
		}
		if (input.resetBegin) {
			print("Resetting stage: " + LevelResetController.instance);
			if (LevelResetController.instance != null) {
				LevelResetController.instance.ResetLevel();
			}
		}
		// if (input.interactBegin) {
		// 	playerWantsToInteract = true;
		// }
	}

	void FixedUpdate()
	{
		if (inMenu) { return; }
		
		playerRB.linearVelocity += currentMovementInput.normalized * speed;
		playerRB.linearVelocity *= (1.0f - Mathf.Clamp01(friction));

		// animator.SetFloat("x", playerRB.linearVelocityX);
		// animator.SetFloat("y", playerRB.linearVelocityY);
		if (currentMovementInput != Vector2.zero)
		{
			animator.SetFloat("x", currentMovementInput.normalized.x);
			animator.SetFloat("y", currentMovementInput.normalized.y);
			lastDir = currentMovementInput.normalized/2;
		}
		else
		{
			animator.SetFloat("x", lastDir.x);
			animator.SetFloat("y", lastDir.y);
		}
		
		previousMoveDirection = moveDirection;
		moveDirection = GetHeaviestDirectionOfFour(currentMovementInput);

		if (currentMovementInput != Vector2.zero)
		{
			MaybeMoveBoxes();
		}
	}

	void FootstepsSound() {
		FMODController.PlayFootstepSound(FMODController.FootstepSoundType.Stein); // .Teppe
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
		var result = collider.bounds.size;

		if (Mathf.Abs(moveDirection.x) > Mathf.Abs(moveDirection.y)) {
			(result.x, result.y) = (result.y, result.x);
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

		// var boxCheckerPosition = moveDirection * boxCheckerOffset;
		// var boxCheckerPosition = moveDirection * collider.bounds.extents;
		var boxCheckerPosition = moveDirection * (collider.bounds.extents + new Vector3(0.2f, 0.2f));
		boxCheckerPosition.x += collider.bounds.center.x;
		boxCheckerPosition.y += collider.bounds.center.y;

		// Collider2D[] collidersAtTarget = Physics2D.OverlapBoxAll(boxCheckerPosition, GetBoxCheckerSizeWithDirectionAdjustment(), 0.0f);
		Collider2D[] collidersAtTarget = Physics2D.OverlapBoxAll(boxCheckerPosition, collider.bounds.size - new Vector3(0.05f, 0.05f), 0.0f);
		// Collider2D[] collidersAtTarget = Physics2D.OverlapBoxAll(boxCheckerPosition, collider.bounds.size, 0.0f);

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
					objectBeingPushedAgainstID = 0;
					objectBeingPushedAgainstStartedAt = -Mathf.Infinity;
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
	[SerializeField] private float BoxPushRange = 0.5f;
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
			print("Player interacts with: " + bestTarget.name);
			bestTarget.GetComponentInChildren<IInteractable>().Interact();
		}
	}
	
	private Vector3 FindClosestStairs(bool dir = false)
	{
		var levelObjectTransform = SceneController.instance.levels[SceneController.instance.currentLevel].transform;
		
		var childrenWithTag = levelObjectTransform.Cast<Transform>()
			.Where(t => t.CompareTag("Stairs") && t.GetComponent<StairsController>().stairsGoUpwards == dir)
			.Select(t => t.gameObject)
			.ToList();
		
		GameObject bestTarget = null;
		float closestDistanceSqr = Mathf.Infinity;
		Vector3 currentPosition = transform.position;
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

	void IResetable.Reset()
	{
		if (SceneController.instance != null && SceneController.instance.currentLevel != 0) {
			transform.position = FindClosestStairs(false);
		}
	}

#if UNITY_EDITOR
	void OnDrawGizmos() {
		// var boxCheckerPosition = moveDirection * boxCheckerOffset;
		var boxCheckerPosition = moveDirection * new Vector3(boxCheckerOffset, boxCheckerOffset);
		// boxCheckerPosition.x += transform.position.x;
		// boxCheckerPosition.y += transform.position.y;
		boxCheckerPosition.x += collider.bounds.center.x;
		boxCheckerPosition.y += collider.bounds.center.y;

		// Gizmos.color = Color.blue;
		// Gizmos.DrawWireCube(boxCheckerPosition, GetBoxCheckerSizeWithDirectionAdjustment());
		// Gizmos.DrawWireSphere(transform.position, InteractRange);
		
		Gizmos.color = Color.blue;
		Gizmos.DrawWireCube(boxCheckerPosition, collider.bounds.size);
		Gizmos.DrawWireSphere(collider.bounds.center, InteractRange);
	}
#endif
}
