using UnityEngine;
using System.Collections.Generic;
using System.Linq;

using static AugustBase.All;

[RequireComponent(typeof(Rigidbody2D), typeof(InputActions))]
public class PlayerController : MonoBehaviour, IResetable {
	public static PlayerController instance;

	// Movement
	public float speed = 2.35f;
	const float sprintSpeedMultiplier = 1.45f;
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
	[SerializeField] Collider2D playerCollider;
	
	// ???
	public bool inMenu = false;
	private bool isResetting = false;

	float currentSpeed;
	bool isSprinting;

	// Yeah I stole your epic name.
	bool awoken = false;
	void Awake() {
		if (awoken) return;
		awoken = true;

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
		playerCollider = GetComponent<Collider2D>();
		BoxPushRange = playerCollider.bounds.extents.x;

		playerCamera = Camera.main;
	}

	void Start() {
#if UNITY_EDITOR
		if (SceneController.instance != null && SceneController.instance.currentLevel == 0) {
			transform.position = Vector3.zero;
		}
#else
		transform.position = Vector3.zero;
#endif

		// FindStairsInCurrentLevel();
		if (SceneController.instance != null && SceneController.instance.currentLevel != 0) {
			Vector3 StairPositon = FindClosestStairs(false).GetComponent<CircleCollider2D>().bounds.center;
			transform.position = StairPositon;
		}

		int levelsCount = 1;
		if (SceneController.instance != null) {
			levelsCount = SceneController.instance.levels.Length;
		}

		levelToResetPosition = new Vector3[levelsCount];
		for (int i = 0; i < levelToResetPosition.Length; ++i) {
			levelToResetPosition[i] = Vector3.zero;
		}

		if (SceneController.instance != null) {
			levelToResetPosition[SceneController.instance.currentLevel] = transform.position;
		}

		currentSpeed = speed;
	}

	public float resetHoldTime = 1.0f;
	float resetTimer = 0.0f;
	bool didResetAndResetIsStillHeld;

	// A value from 0.0f to 1.0f, where 1.0f means we are about to reset the level.
	[HideInInspector] public float resetTimerProgress;

	[HideInInspector] public Vector3[] levelToResetPosition;

	public void StairingWasPerformed(bool stairWasUpwards) {
		resetTimer = 0.0f;
		didResetAndResetIsStillHeld = true;

		if (SceneController.instance != null && stairWasUpwards) {
			levelToResetPosition[SceneController.instance.currentLevel] = transform.position;
		}
	}

	void DisableAllNearbyOutlines() {
		var overlapped = Physics2D.OverlapCircleAll(playerCollider.bounds.center, 100.0f);
		for (int i = 0; i < overlapped.Length; ++i) {
			var outline = overlapped[i].gameObject.GetComponentInChildren<InteractableOutline>();
			if (outline == null) return;
			outline.DisableOutline();
		}
	}

	void Update() {
		// Reset
		if (input.resetBegin) {
			FMODController.BeginResetSpell();
		}

		if (input.resetHeld) {
			// Do not merge these conditions.
			if (!didResetAndResetIsStillHeld) {
				isResetting = true;
				animator.SetBool("resetting", true);
				resetTimer += Time.deltaTime;

				if (resetHoldTime < resetTimer) {
					resetTimer = 0.0f;
					didResetAndResetIsStillHeld = true;
					FMODController.PlaySound(FMODController.Sound.SFX_ResetSpell);

					// Time to reset the level!
					if (LevelResetController.instance != null) {
						FMODController.ResetSpellComplete();
						LevelResetController.instance.ResetLevel();
					}
				}
			}
			else
			{
				animator.SetBool("resetting", false);
				isResetting = false;
			}
		} else {
			isResetting = false;
			animator.SetBool("resetting", false);
			didResetAndResetIsStillHeld = false;
			resetTimer = 0.0f;
			FMODController.StopResetSpell();
		}

		if (resetHoldTime != 0.0f) {
			resetTimerProgress = Mathf.Clamp01(resetTimer / resetHoldTime);
		}

		if (previousNearestOutline != null) {
			previousNearestOutline.DisableOutline();
			previousNearestOutline = null;
		}

		if (isResetting) {
			playerRB.linearVelocity = Vector3.zero;
			animator.SetFloat("x", 0);
			animator.SetFloat("y", -0.5f);
			animator.SetBool("pushing", false);
			return;
		}

		if (inMenu) {
			playerRB.linearVelocity = Vector3.zero;
			animator.SetBool("pushing", false);
			animator.SetFloat("x", lastDir.x);
			animator.SetFloat("y", lastDir.y);
			return;
		}

		currentMovementInput = input.movement;
		isSprinting = input.sprintHeld;

		{ // Interaction!
			var nearest = GetNearestInteractable();

			if (nearest != null) {
				var nearestOutline = nearest.GetComponentInChildren<InteractableOutline>();
				if (nearestOutline != null) nearestOutline.EnableOutline();

				previousNearestOutline = nearestOutline;

				if (input.interactBegin) {
					nearest.GetComponentInChildren<IInteractable>().Interact();
				}
			}
		}
	}

	InteractableOutline previousNearestOutline;

	void FixedUpdate()
	{
		if (inMenu || isResetting) { return; }

		currentSpeed = speed;
		if (isSprinting) currentSpeed *= sprintSpeedMultiplier;

		animator.speed = 1.0f;
		if (isSprinting) animator.speed *= sprintSpeedMultiplier;

		playerRB.linearVelocity += currentMovementInput.normalized * currentSpeed;
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
		else
		{
			animator.SetBool("pushing", false);
			objectBeingPushedAgainstStartedAt = Time.time;
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


	Vector2 GetBoxCheckerSizeWithDirectionAdjustment() {
		var result = playerCollider.bounds.size;

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
			animator.SetBool("pushing", false);
		}

		var boxCheckerPosition = moveDirection * (playerCollider.bounds.extents + new Vector3(0.2f, 0.2f));
		boxCheckerPosition.x += playerCollider.bounds.center.x;
		boxCheckerPosition.y += playerCollider.bounds.center.y;

		Collider2D[] collidersAtTarget = Physics2D.OverlapBoxAll(boxCheckerPosition, playerCollider.bounds.size - new Vector3(0.05f, 0.05f), 0.0f);

		if (collidersAtTarget.Length == 0) {
			objectBeingPushedAgainstID = 0;
			objectBeingPushedAgainstStartedAt = -Mathf.Infinity;
			objectBeingPushedAgainstPushDirection = Vector2.zero;
		}

		bool thereWasAMoveable = false;
		for (int i = 0; i < collidersAtTarget.Length; ++i) {
			if (collidersAtTarget[i].TryGetComponent<MakeMoveable>(out var moveableAtTarget)) {
				thereWasAMoveable = true;
				animator.SetBool("pushing", true);

				var id = collidersAtTarget[i].gameObject.GetInstanceID();
				if (objectBeingPushedAgainstID == 0) {
					objectBeingPushedAgainstID = id;
					objectBeingPushedAgainstStartedAt = Time.time;
				}

				objectBeingPushedAgainstPushDirection = moveDirection;

				if (objectBeingPushedAgainstStartedAt + objectPushingDelay < Time.time) {
					if (moveableAtTarget.TryMoveInDirection(moveDirection)) {
						FMODController.PlaySound(FMODController.Sound.VO_Wizard_PushGrunt);
					}

					objectBeingPushedAgainstID = 0;
					objectBeingPushedAgainstStartedAt = -Mathf.Infinity;
					objectBeingPushedAgainstPushDirection = Vector2.zero;
				}
			}
		}

		if (!thereWasAMoveable) {
			objectBeingPushedAgainstID = 0;
			objectBeingPushedAgainstStartedAt = -Mathf.Infinity;
			objectBeingPushedAgainstPushDirection = Vector2.zero;
		}
	}

	Vector2 previousMoveDirection;

	[SerializeField] private float InteractRange = 0.5f;
	[SerializeField] private float BoxPushRange = 0.5f;

	// NOTE: May return null!
	GameObject GetNearestInteractable() {
		//		Find all objects in range
		Collider2D[] hitColliders = Physics2D.OverlapCircleAll(playerCollider.bounds.center, InteractRange);
		// Collider2D[] hitColliders = Physics2D.OverlapBoxAll(playerCollider.bounds.center, new Vector2(InteractRange, InteractRange), 0);
		//		Narrow down to objects with the Interactable Interface
		List<GameObject> gameObjects = hitColliders
			.Select(col => col.gameObject)                   // Get GameObject from Collider
			.Where(go => go.GetComponentInChildren<IInteractable>() != null)    // Check if it has the interface
			.ToList();
		
		// print("hitColliders: " + hitColliders.Length + " / gameObjects: " + gameObjects.Count);
		//		Finds the closest one out of the list
		GameObject bestTarget = null;
		float closestDistanceSqr = Mathf.Infinity;
		Vector3 currentPosition = playerCollider.bounds.center;

		foreach(GameObject potentialTarget in gameObjects) {
			// print("Checking: " + potentialTarget.name);
			Vector3 directionToTarget = potentialTarget.transform.position - currentPosition;
			float dSqrToTarget = directionToTarget.sqrMagnitude;

			if(dSqrToTarget < closestDistanceSqr) {
				closestDistanceSqr = dSqrToTarget;
				bestTarget = potentialTarget;
			}
		}

		return bestTarget;
	}

	private GameObject FindClosestStairs(bool dir = false)
	{
		var levelObjectTransform = SceneController.instance.levels[SceneController.instance.currentLevel].transform;
		
		var childrenWithTag1 = levelObjectTransform.Cast<Transform>()
			.Where(t => t.CompareTag("Stairs") && t.TryGetComponent<StairsController>(out StairsController Stair) && Stair.stairsGoUpwards == dir)
			.Select(t => t.gameObject)
			.ToList();
		var childrenWithTag2 = levelObjectTransform.Cast<Transform>()
			.Where(t => t.CompareTag("Stairs") && t.TryGetComponent<StairsControllerLegacy>(out StairsControllerLegacy Stair) && Stair.stairsGoUpwards == dir)
			.Select(t => t.gameObject)
			.ToList();
		List<GameObject> childrenWithTag = childrenWithTag1.Concat(childrenWithTag2).ToList();
			
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
		return bestTarget;
	}

	void IResetable.Reset()
	{
		Awake();

		if (SceneController.instance != null) {
			var currentLevel = SceneController.instance.currentLevel;
			var newPosition = levelToResetPosition[currentLevel];

			if (newPosition == Vector3.zero && currentLevel != 0) {
				newPosition = FindClosestStairs(false).GetComponent<CircleCollider2D>().bounds.center;
			}

			transform.position = newPosition;
		}
	}

	public void GameReset() {
		// Yeah.
		Start();
	}

#if UNITY_EDITOR
	void OnDrawGizmos() {
		// var boxCheckerPosition = moveDirection * boxCheckerOffset;
		var boxCheckerPosition = moveDirection * new Vector3(boxCheckerOffset, boxCheckerOffset);
		// boxCheckerPosition.x += transform.position.x;
		// boxCheckerPosition.y += transform.position.y;
		boxCheckerPosition.x += playerCollider.bounds.center.x;
		boxCheckerPosition.y += playerCollider.bounds.center.y;

		// Gizmos.color = Color.blue;
		// Gizmos.DrawWireCube(boxCheckerPosition, GetBoxCheckerSizeWithDirectionAdjustment());
		// Gizmos.DrawWireSphere(transform.position, InteractRange);
		
		Gizmos.color = Color.blue;
		Gizmos.DrawWireCube(boxCheckerPosition, playerCollider.bounds.size);
		Gizmos.DrawWireSphere(playerCollider.bounds.center, InteractRange);
	}
#endif
}
