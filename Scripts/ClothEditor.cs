// Dependencies
using Godot;
using System;
using System.Collections.Generic;

// Edit modes
public enum EditMode {
	Default,
	Destroy,
	Create
}

// Controls cloth!
public partial class ClothEditor : Node2D
{
	// Inspector
	[Export] public Overlay overlay;

	// General
	public Cloth cloth;
	public EditMode editMode = EditMode.Default;
	public Connection connectionInserting = null;
	public Joint jointGrabbed = null;
	private Vector2 jointGrabbedOffset = Vector2.Zero;
	public Joint jointUnderMouse;
	public Connection connectionUnderMouse;
	public Connection connectionSelected;
	public bool isCutting = false;
	private float jointDistanceTolerance = JointDistanceToleranceDefault;

	// Settings
	private const float JointDistanceToleranceDefault = 5.0f;
	private const float JointDistanceToleranceCreate = 1.5f;
	private const float ConnectionDistanceTolerance = 45.0f;
	private const float ConnectionDestroyTolerance = 2.0f;

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
		Connection closestConnection = null;
		float closestConnectionDistance = Mathf.Inf;
		foreach (Connection connection in cloth.connections) {
			float distanceToConnection = Simulation.MousePosition.DistanceTo(connection.GetCenterPosition());
			if (distanceToConnection < closestConnectionDistance) {
				closestConnection = connection;
				closestConnectionDistance = distanceToConnection;
			}
		}
		if (closestConnection != null) {
			if (closestConnectionDistance < ConnectionDistanceTolerance) connectionUnderMouse = closestConnection;
			else connectionUnderMouse = null;
		}

		// Grabbing logic
		if (jointGrabbed != null) jointGrabbed.Position = Simulation.MousePosition + jointGrabbedOffset;

		// Cutting logic
		if (isCutting) {
			AttemptConnectionCut();
			AttemptJointCut();
		}

		// Queue redraw
		QueueRedraw();
	}

	// Set edit mode
	public void SetEditMode(EditMode editMode) {
		// Change edit mode
		this.editMode = editMode;

		// Reset some stuff
		switch (editMode) {
			default:
				connectionInserting = null;
				connectionSelected = null;
				jointDistanceTolerance = JointDistanceToleranceDefault;
				break;

			case EditMode.Create:
				jointGrabbed = null;
				jointDistanceTolerance = JointDistanceToleranceCreate;
				break;

			case EditMode.Destroy:
				connectionInserting = null;
				connectionSelected = null;
				jointGrabbed = null;
				jointDistanceTolerance = 1.0f;
				break;
		}
		
		// Update overlay
		overlay.Update();
	}

	// Attempt grab joint
	public void AttemptGrabJoint() {
		if (jointUnderMouse == null) return;
		jointGrabbed = jointUnderMouse;
		jointGrabbedOffset = jointGrabbed.Position - Simulation.MousePosition;
	}
	
	// Attempt to release joint
	public void AttemptReleaseJoint() {
		jointGrabbed = null;
	}

	// Attempt to select connection
	public void AttemptSelectConnection() {
		if (connectionUnderMouse != null) {
			connectionSelected = connectionUnderMouse;
			overlay.Update();
		}
	}

	// Insert start
	public void AttemptInsertStart() {
		// Return if something is already happening
		if (connectionSelected != null) return;

		// Check if there is a joint at mouse position
		Joint jointFound = jointUnderMouse;

		// Create a connection with either a new or old joint attached
		if (jointFound != null) connectionInserting = new Connection(cloth, jointFound, null);
		else connectionInserting = new Connection(cloth, cloth.AddJoint(Simulation.MousePosition, true), null);
	}

	// Insert middle
	public void AttemptInsertMiddle() {
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
	public void AttemptInsertEnd() {
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
	public void AttemptConnectionCut() {
		for (int index = cloth.connections.Count - 1; index >= 0; index--) {
			Connection connection = cloth.connections[index];
			if (connection.CollidesWithPoint(Simulation.MousePosition, ConnectionDestroyTolerance)) {
				cloth.RemoveConnection(connection);
			}
		}
	}

	// Attempt to cut a joint
	public void AttemptJointCut() {
		for (int index = cloth.joints.Count - 1; index >= 0; index--) {
			Joint joint = cloth.joints[index];
			if (joint.CollidesWithPoint(Simulation.MousePosition)) {
				cloth.RemoveJoint(joint);
			}
		}
	}

	// Attempt to flip a joint
	public void AttemptJointFlip() {
		if (jointGrabbed != null) jointGrabbed.isFixed = !jointGrabbed.isFixed;
		else if (jointUnderMouse != null) jointUnderMouse.isFixed = !jointUnderMouse.isFixed;
	}

	// Attempt to extend connection
	public void AttemptConnectionExtend() {
		if (connectionSelected != null) connectionSelected.desiredLength += 5.0f;
	}

	// Attempt to shrink connection
	public void AttemptConnectionShrink() {
		if (
			connectionSelected != null
			&& connectionSelected.desiredLength > 1.0f
		) connectionSelected.desiredLength -= 5.0f;
	}
}
