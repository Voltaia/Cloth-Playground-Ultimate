// Dependencies
using Godot;
using System;
using System.Collections.Generic;

// Controls cloth!
public partial class ClothEditor : Node2D
{
	// Inspector
	[Export] private Overlay overlay;

	// Variables
	public Cloth cloth;
	public EditMode editMode = EditMode.Default;
	public bool isDraggingPrimary = false;
	private Connection connectionInserting = null;
	private Joint jointGrabbed = null;
	private Vector2 jointGrabbedOffset = Vector2.Zero;
	private Joint jointUnderMouse;
	private Connection connectionUnderMouse;
	private float jointDistanceTolerance = DefaultJointDistanceTolerance;

	// Settings
	private const float DefaultJointDistanceTolerance = 5.0f;

	// Edit modes
	public enum EditMode {
		Default,
		Destroy,
		Create
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		// Fill in joint under mouse
		Joint closestJoint = null;
		float closestJointDistance = Mathf.Inf;
		foreach (Joint joint in cloth.joints) {
			float distanceToJoint = Simulation.MousePosition.DistanceTo(joint.Position);
			if (distanceToJoint < closestJointDistance) {
				closestJoint = joint;
				closestJointDistance = distanceToJoint;
			}
		}
		if (closestJoint != null) {
			if (closestJointDistance < closestJoint.parent.jointRadius * jointDistanceTolerance) jointUnderMouse = closestJoint;
			else jointUnderMouse = null;
		}

		// Fill in connection under mouse
		connectionUnderMouse = null;
		foreach (Connection connection in cloth.connections) {
			if (connection.CollidesWithPoint(Simulation.MousePosition)) connectionUnderMouse = connection;
		}

		// Grabbing logic
		if (jointGrabbed != null) jointGrabbed.Position = Simulation.MousePosition + jointGrabbedOffset;

		// Queue redraw
		QueueRedraw();
	}

	// Draw stuff
	public override void _Draw() {
		// Draw inserting new connection
		if (connectionInserting != null) {
			DrawLine(
				connectionInserting.firstJoint.Position,
				Simulation.MousePosition,
				Colors.Green,
				Connection.DrawThickness
			);
		}

		// Draw path to joint under mouse
		if (editMode == EditMode.Default && jointUnderMouse != null) {
			Vector2 positionToDrawTo = jointGrabbed == null ? jointUnderMouse.Position : jointGrabbed.Position;
			DrawLine(
				Simulation.MousePosition,
				positionToDrawTo,
				new Color(Colors.Blue, 0.25f),
				2.5f
			);
		}
	}

	// Input
	public override void _Input(InputEvent @event)
	{
		// Enable/disable modes
		if (@event.IsActionPressed("Destroy")) {
			editMode = EditMode.Destroy;
			connectionInserting = null;
			jointGrabbed = null;
			jointDistanceTolerance = 1.0f;
			overlay.Update(this);
		}
		else if (@event.IsActionPressed("Create")) {
			editMode = EditMode.Create;
			jointGrabbed = null;
			jointDistanceTolerance = 1.0f;
			overlay.Update(this);
		}
		else if (
			(
				editMode == EditMode.Destroy
				&& @event.IsActionReleased("Destroy")
			)
			|| (
				editMode == EditMode.Create
				&& @event.IsActionReleased("Create")
			)
		) {
			editMode = EditMode.Default;
			connectionInserting = null;
			jointDistanceTolerance = DefaultJointDistanceTolerance;
			overlay.Update(this);
		}

		// Handle primary mouse input
		if (@event.IsActionPressed("Primary Edit")) {
			// Update state
			isDraggingPrimary = true;
			overlay.Update(this);

			// Mode actions
			if (editMode == EditMode.Default) AttemptGrabJoint();
			else if (editMode == EditMode.Create) AttemptInsertStart();
		}
		else if (@event.IsActionReleased("Primary Edit"))
		{
			// Update state
			isDraggingPrimary = false;
			overlay.Update(this);

			// Edit mode actions
			if (editMode == EditMode.Default) AttemptReleaseJoint();
			else if (editMode == EditMode.Create) AttemptInsertEnd();
		}

		// Handle dragging
		if (isDraggingPrimary && editMode == EditMode.Destroy) {
			AttemptConnectionCut();
			AttemptJointCut();
		}

		// Handle secondary mouse input
		if (@event.IsActionPressed("Secondary Edit")) {
			// Edit mode actions
			if (editMode == EditMode.Default) AttemptJointFlip();
			else if (editMode == EditMode.Create) if (connectionInserting != null) AttemptInsertMiddle();
		}

		// Handle tertiary mouse input
		if (editMode == EditMode.Create) {
			if (@event.IsActionPressed("Wheel Up")) AttemptConnectionExtend();
			else if (@event.IsActionPressed("Wheel Down")) AttemptConnectionShrink();
		}

		// Pause simulation
		if (@event.IsActionPressed("Pause Simulation")) cloth.simulationPaused = !cloth.simulationPaused;
	}

	// Attempt grab joint
	private void AttemptGrabJoint() {
		jointGrabbed = jointUnderMouse;
		jointGrabbedOffset = jointGrabbed.Position - Simulation.MousePosition;
	}
	
	// Attempt to release joint
	private void AttemptReleaseJoint() {
		jointGrabbed = null;
	}

	// Insert start
	private void AttemptInsertStart() {
		// Check if there is a joint at mouse position
		Joint jointFound = jointUnderMouse;

		// Create a connection with either a new or old joint attached
		if (jointFound != null) connectionInserting = new Connection(cloth, jointFound, null);
		else connectionInserting = new Connection(cloth, cloth.AddJoint(Simulation.MousePosition, true), null);
	}

	// Insert middle
	private void AttemptInsertMiddle() {
		// Check if there is a joint at mouse position
		Joint jointToConnect = jointUnderMouse;
		
		// Create new joint if there is none
		if (jointToConnect == null) jointToConnect = cloth.AddJoint(Simulation.MousePosition, false);

		// Finish connection
		connectionInserting.secondJoint = jointToConnect;
		connectionInserting.ReadjustLength();
		cloth.AddConnection(connectionInserting);

		// Start new connection
		connectionInserting = new Connection(cloth, jointToConnect, null);
	}

	// Insert end
	private void AttemptInsertEnd() {
		// Check if there is a connection being created
		if (connectionInserting == null) return;

		// Check if there is a joint at mouse position
		Joint jointFound = jointUnderMouse;

		// End connection with either an old or new joint
		if (jointFound != null) connectionInserting.secondJoint = jointFound;
		else connectionInserting.secondJoint = cloth.AddJoint(Simulation.MousePosition, true);
		connectionInserting.ReadjustLength();
		cloth.AddConnection(connectionInserting);

		// Wipe connection
		connectionInserting = null;
	}

	// Attempt to cut a connection
	private void AttemptConnectionCut() {
		for (int index = cloth.connections.Count - 1; index >= 0; index--) {
			Connection connection = cloth.connections[index];
			if (connection.CollidesWithPoint(Simulation.MousePosition)) {
				cloth.RemoveConnection(connection);
			}
		}
	}

	// Attempt to cut a joint
	private void AttemptJointCut() {
		for (int index = cloth.joints.Count - 1; index >= 0; index--) {
			Joint joint = cloth.joints[index];
			if (joint.CollidesWithPoint(Simulation.MousePosition)) {
				cloth.RemoveJoint(joint);
			}
		}
	}

	// Attempt to flip a joint
	private void AttemptJointFlip() {
		if (jointUnderMouse != null) jointUnderMouse.isFixed = !jointUnderMouse.isFixed;
	}

	// Attempt to extend connection
	private void AttemptConnectionExtend() {
		Connection connectionFound = connectionUnderMouse;
		if (connectionFound != null) connectionFound.desiredLength += 5.0f;
	}

	// Attempt to shrink connection
	private void AttemptConnectionShrink() {
		Connection connectionFound = connectionUnderMouse;
		if (
			connectionFound != null
			&& connectionFound.desiredLength > 1.0f
		) connectionFound.desiredLength -= 5.0f;
	}
}
