using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(InputActions))]
public class PlayerController : MonoBehaviour {
	public float speed = 2.35f;

	[Range(0.0f, 1.0f)]
	public float friction = 0.425f;

	public float objectPushingDelay = 0.25f;

	Rigidbody2D  playerRB;
	InputActions input;

	void Awake() {
		playerRB = GetComponent<Rigidbody2D>();
		playerRB.gravityScale           = 0.0f;
		playerRB.constraints            = RigidbodyConstraints2D.FreezeRotation;
		playerRB.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
		playerRB.bodyType               = RigidbodyType2D.Dynamic;
		playerRB.interpolation          = RigidbodyInterpolation2D.Interpolate;

		input = GetComponent<InputActions>();
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

	void Update() {
		currentMovementInput = input.movement;
	}

	public Vector2 boxCheckerSize = new Vector2(0.1f, 0.5f);
	public float boxCheckerOffset = 0.65f;

	Vector2 currentMovementInput;
	Vector2 moveDirection = new Vector2(1.0f, 0.0f);

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

#if false
				objectBeingPushedAgainstID = dings
				if (objectBeingPushedAgainstID == dings && objectBeingPushedAgainstID != 0
				    && previousMoveDirection == moveDirection
				    && !(moveDirection.x == 0.0f && moveDirection.y == 0.0f)) {
					print($"id is {objectBeingPushedAgainstID} at {Time.time}");
				} else {
					print("not holdering");
				}
#endif
			}
		}
	}

	Vector2 previousMoveDirection;
	void FixedUpdate() {
		playerRB.linearVelocity += currentMovementInput.normalized * speed;
		playerRB.linearVelocity *= (1.0f - Mathf.Clamp01(friction));

		previousMoveDirection = moveDirection;
		moveDirection = GetHeaviestDirectionOfFour(currentMovementInput);

		MaybeMoveBoxes();
	}

#if UNITY_EDITOR
	void OnDrawGizmos() {
		var boxCheckerPosition = moveDirection * boxCheckerOffset;
		boxCheckerPosition.x += transform.position.x;
		boxCheckerPosition.y += transform.position.y;

		Gizmos.color = Color.blue;
		Gizmos.DrawWireCube(boxCheckerPosition, GetBoxCheckerSizeWithDirectionAdjustment());
	}
#endif

#if false
	void OnCollisionEnter2D(Collision2D collision) {
		if (collision.gameObject.TryGetComponent<MakeMoveable>(out var moveable)) {
			var delta = new Vector2(collision.transform.position.x,
			                        collision.transform.position.y)
			            - playerRB.position;

			if (Mathf.Abs(delta.x) > Mathf.Abs(delta.y) && delta.x != 0.0f) {
				Vector2 moveBy = delta;
				moveBy.x /= Mathf.Abs(moveBy.x);
				moveBy.y = 0.0f;
				moveable.TryMoveInDirection(moveBy);
			} else if (Mathf.Abs(delta.x) < Mathf.Abs(delta.y) && delta.y != 0.0f) {
				Vector2 moveBy = delta;
				moveBy.x = 0.0f;
				moveBy.y /= Mathf.Abs(moveBy.y);
				moveable.TryMoveInDirection(moveBy);
			} else {
				// Do nothing!
			}
		}
	}
#endif
}
