using UnityEngine;
using UnityEngine.InputSystem;

public class InputActions : MonoBehaviour {
	InputSystem_Actions actions;

	void Awake()     => actions = new InputSystem_Actions();
	void OnEnable()  => actions.Enable();
	void OnDisable() => actions.Disable();

	[HideInInspector] public Vector2 movement { get; private set; }
	[HideInInspector] public Vector2 looking  { get; private set; }

	[HideInInspector] public bool jumpBegin { get; private set; }
	[HideInInspector] public bool jumpHeld  { get; private set; }
	[HideInInspector] public bool jumpEnd   { get; private set; }

	[HideInInspector] public bool interactBegin { get; private set; }
	[HideInInspector] public bool interactHeld  { get; private set; }
	[HideInInspector] public bool interactEnd   { get; private set; }

	[HideInInspector] public bool parasolBegin { get; private set; }
	[HideInInspector] public bool parasolHeld  { get; private set; }
	[HideInInspector] public bool parasolEnd   { get; private set; }

	void Update() {
		if (actions == null) actions = new InputSystem_Actions();

		movement = actions.Player.Move.ReadValue<Vector2>();
		looking  = actions.Player.Look.ReadValue<Vector2>();

		jumpBegin = actions.Player.Jump.WasPressedThisFrame();
		jumpHeld  = actions.Player.Jump.IsPressed();
		jumpEnd   = actions.Player.Jump.WasReleasedThisFrame();

		interactBegin = actions.Player.Interact.WasPressedThisFrame();
		interactHeld  = actions.Player.Interact.IsPressed();
		interactEnd   = actions.Player.Interact.WasReleasedThisFrame();

		parasolBegin = actions.Player.Parasol.WasPressedThisFrame();
		parasolHeld  = actions.Player.Parasol.IsPressed();
		parasolEnd   = actions.Player.Parasol.WasReleasedThisFrame();
	}
}
