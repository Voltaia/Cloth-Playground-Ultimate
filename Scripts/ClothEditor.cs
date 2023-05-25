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
	[Export] private Overlay overlay;

	// General
	public Cloth cloth;
	public EditMode editMode = EditMode.Default;
	public Connection connectionInserting = null;
	public Joint jointGrabbed = null;
	private Vector2 jointGrabbedOffset = Vector2.Zero;
	public Joint jointUnderMouse;
	public Connection connectionUnderMouse;
	public Connection connectionSelected;
	private Vector2 lastMousePosition;
	private float jointDistanceTolerance = JointDistanceToleranceDefault;

	// Properties
	private bool isCutting;
	public bool IsCutting {
		get { return isCutting; }
		set { isCutting = value; overlay.Update(); }
	}

	// Settings
	private const float JointDistanceToleranceDefault = 5.0f;
	private const float JointDistanceToleranceCreate = 1.5f;
	private const float ConnectionDistanceTolerance = 45.0f;
	private const float ConnectionDestroyTolerance = 2.0f;
	private const float CutCollisionCheckDivisor = 15.0f;

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		// Fill in elements under mouse
		jointUnderMouse = cloth.GetClosestJoint(Simulation.MousePosition, jointDistanceTolerance);
		connectionUnderMouse = cloth.GetClosestConnection(Simulation.MousePosition, ConnectionDistanceTolerance);

		// Grabbing logic
		if (jointGrabbed != null) jointGrabbed.Position = Simulation.MousePosition + jointGrabbedOffset;

		// Cutting logic
		if (isCutting) {
			AttemptConnectionCut(Simulation.MousePosition);
			AttemptConnectionCut(lastMousePosition, Simulation.MousePosition);
			AttemptJointCut(Simulation.MousePosition);
		}

		// Queue redraw
		QueueRedraw();

		// Fill in last mouse position
		lastMousePosition = Simulation.MousePosition;
	}

	// Set edit mode
	public void SetEditMode(EditMode editMode) {
		// Change edit mode
		this.editMode = editMode;

		// Reset some stuff
		switch (editMode) {
			default:
				jointDistanceTolerance = JointDistanceToleranceDefault;
				break;

			case EditMode.Create:
				jointDistanceTolerance = JointDistanceToleranceCreate;
				break;

			case EditMode.Destroy:
				jointDistanceTolerance = 1.0f;
				break;
		}
		
		// Clean up
		ResetUsing();
		overlay.Update();
	}

	// Reset stuff
	public void ResetUsing() {
		connectionInserting = null;
		connectionSelected = null;
		connectionUnderMouse = null;
		jointGrabbed = null;
		jointUnderMouse = null;
		isCutting = false;
	}

	// Pause simulation
	public void ToggleSimulationPause() {
		cloth.simulationPaused = !cloth.simulationPaused;
	}

	// Release connection
	public void ReleaseConnection() {
		connectionSelected = null;
		overlay.Update();
	}

	// Attempt grab joint
	public void AttemptGrabJoint() {
		if (jointUnderMouse == null) return;
		jointGrabbed = jointUnderMouse;
		jointGrabbedOffset = jointGrabbed.Position - Simulation.MousePosition;
		overlay.Update();
	}
	
	// Attempt to release joint
	public void AttemptReleaseJoint() {
		jointGrabbed = null;
		overlay.Update();
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
		overlay.Update();
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
		overlay.Update();
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
		overlay.Update();
	}

	// Attempt to cut a connection
	public void AttemptConnectionCut(Vector2 cutPosition) {
		for (int index = cloth.connections.Count - 1; index >= 0; index--) {
			Connection connection = cloth.connections[index];
			if (connection.CollidesWithPoint(cutPosition, ConnectionDestroyTolerance)) {
				cloth.RemoveConnection(connection);
			}
		}
	}

	// An override
	public void AttemptConnectionCut(Vector2 startCut, Vector2 endCut) {
		for (int index = cloth.connections.Count - 1; index >= 0; index--) {
			Connection connection = cloth.connections[index];
			if (connection.CollidesWithLine(startCut, endCut)) {
				cloth.RemoveConnection(connection);
			}
		}
	}

	// Attempt to cut a joint
	public void AttemptJointCut(Vector2 cutPosition) {
		for (int index = cloth.joints.Count - 1; index >= 0; index--) {
			Joint joint = cloth.joints[index];
			if (joint.CollidesWithCircle(cutPosition, Overlay.CursorRadius)) {
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
