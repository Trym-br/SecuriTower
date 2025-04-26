using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class MakeMoveable : MonoBehaviour, IResetable {
	const string crystalTag = "Crystal";

	public LayerMask getStoppedBy = ~0;
	ContactFilter2D filter = new ContactFilter2D();
	// [SerializeField] private AnimationCurve moveCurve = AnimationCurve.EaseInOut(0,0,1,1);
	[SerializeField] private AnimationCurve moveCurve;
	[SerializeField] private float moveDuration = 1f;
	private Rigidbody2D rb;
	private Vector3 targetPosition;
	private Vector3 startPosition;
	private bool isMoving = false;
	public UnityEvent OnMoveComplete;
	public UnityEvent OnMoveStart;

	private bool FinishedMoving = false;
	//[SerializeField] private float snapAmount = 32f;

	public bool canBeMovedInConjunction = true;

	Vector3 originalPosition;

	bool alreadyStarted;
	void Start() {
		if (alreadyStarted) return;
		alreadyStarted = true;

		originalPosition = transform.position;
		filter.useTriggers = false;
		rb = GetComponent<Rigidbody2D>();
	}

	public void Reset() {
		Start();

		transform.position = originalPosition;
		OnMoveComplete.Invoke();
	}

	public bool TryMoveInDirection(Vector2 direction)
	{
		if (isMoving) { return false; }
		startPosition = transform.position;
		var moveTo = direction;
		moveTo.x += transform.position.x;
		moveTo.y += transform.position.y;
		// Collider2D[] collidersAtTarget = Physics2D.OverlapCircleAll(moveTo, 0.1f);
		// Collider2D[] collidersAtTarget; 
		List<Collider2D> collidersAtTarget = new List<Collider2D>();
		Physics2D.OverlapCircle(moveTo, 0.1f, filter, collidersAtTarget);

		// foreach (var collider in collidersAtTarget)
		// {
		// 	print(this.name + " is being stopped from moving by: "  + collider.gameObject.name);
		// }

		bool canMove = collidersAtTarget.Count == 0;
		if (canBeMovedInConjunction) {
			for (int i = 0; i < collidersAtTarget.Count; ++i) {
				if (collidersAtTarget[i].TryGetComponent<MakeMoveable>(out var moveableAtTarget)) {
					if (moveableAtTarget.TryMoveInDirection(direction)) {
						canMove = true;
					}
				}
			}
		}
		// print(this.name + " is trying to be moved: " + canMove);
		if (!canMove) return false;

		var newPosition = transform.position;
		newPosition.x = moveTo.x;
		newPosition.y = moveTo.y;
		targetPosition = newPosition;

		//StartCoroutine("MoveAnimation");
		isMoving = true;
		elapsedMovingTime = 0.0f;

		// transform.position = newPosition;
		// Physics.SyncTransforms();
		// Vector3 velocity = (newPosition - transform.position) / 0.5f;
		// print("Moving: " + this.name + " with " + velocity + " speed");
		// rb.linearVelocity = velocity;
		// rb.MovePosition(newPosition);

		// @Hardcoded
		if (CompareTag(crystalTag)) {
			FMODController.PlaySound(FMODController.Sound.SFX_CrystalPush);
		} else {
			FMODController.PlaySound(FMODController.Sound.SFX_BoxPush);
		}

		return true;
	}

#if true
	// TODO: Rename
	float elapsedMovingTime = Mathf.Infinity;
	void Update() {
		if (elapsedMovingTime < moveDuration) {
			if (FinishedMoving)
			{
				OnMoveStart.Invoke();
			}
			elapsedMovingTime += Time.deltaTime;

			var move = Vector3.Lerp(startPosition, targetPosition,
			                        moveCurve.Evaluate(elapsedMovingTime / moveDuration));

			/* Snapping to the nearest pixel aint it.
			if (snapAmount != 0.0f) {
				move = SnapToStep(move, 1.0f / snapAmount);
			}
			*/

			transform.position = move;
			Physics2D.SyncTransforms();
			FinishedMoving = false;
		} else {
			isMoving = false;
			if (!FinishedMoving)
			{
				OnMoveComplete.Invoke();
				FinishedMoving = true;
			}
		}
	}
#else
	private IEnumerator MoveAnimation()
	{
		// rb.MovePosition(moveCurve.Evaluate(Time.time));
		// Track the elapsed time
		float elapsedTime = 0f;

		// Loop until the specified duration is reached
		while (elapsedTime < moveDuration)
		{
			// print($"{this.name}: {elapsedTime}/{moveDuration}: {Time.time}");
			// Increment the elapsed time
			elapsedTime += Time.deltaTime;
			float t = elapsedTime / moveDuration;

			// Use the curve to get the scale factor (from 1 to 0 based on curve)
			// float scaleFactor = moveCurve.Evaluate(t);
			// rb.MovePosition(moveCurve.Evaluate(t));
			Vector3 move = Vector3.Lerp(startPosition, targetPosition, moveCurve.Evaluate(t));
			// print($"{this.name}: Here/there/where: {startPosition} / {targetPosition} / {move}");
			// rb.MovePosition(move);
			if (snapAmount != 0)
			{
				move = SnapToStep(move, 1f/snapAmount);
			}
			// print($"{this.name}: m/s: {move}/{snapped}");
			rb.MovePosition(move);
			yield return null;
		}
		isMoving = false;
	}
#endif

	Vector3 SnapToStep(Vector3 value, float step)
	{
		return new Vector3(
			Mathf.Round(value.x / step) * step,
			Mathf.Round(value.y / step) * step,
			Mathf.Round(value.z / step) * step
		);
	}
}
