// Dependencies
using Godot;
using System;
using System.Collections.Generic;

// Controls cloth!
public partial class ClothEditor : Node2D
{
	// Variables
	public Cloth cloth;
	private EditMode editMode = EditMode.Default;
	private Connection connectionInserting = null;
	private Joint jointGrabbed = null;
	private Joint jointUnderMouse;
	private Connection connectionUnderMouse;

	// Edit modes
	private enum EditMode {
		Default,
		Destroy,
		Create
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		// Fill in joint under mouse
		jointUnderMouse = null;
		foreach (Joint joint in cloth.joints) {
			if (joint.CollidesWithPoint(Simulation.MousePosition)) jointUnderMouse = joint;
		}

		// Fill in connection under mouse
		connectionUnderMouse = null;
		foreach (Connection connection in cloth.connections) {
			if (connection.CollidesWithPoint(Simulation.MousePosition)) connectionUnderMouse = connection;
		}

		// Grabbing logic
		if (jointGrabbed != null) jointGrabbed.Position = Simulation.MousePosition;

		// Cutting logic
		if (editMode == EditMode.Destroy && Input.IsActionPressed("Primary Edit")) {
			AttemptConnectionCut();
			AttemptJointCut();
		}

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

		// Draw edit mode
		Color editModeColor = Colors.Blue;
		if (editMode == EditMode.Create) editModeColor = Colors.Green;
		else if (editMode == EditMode.Destroy) editModeColor = Colors.Red;
		DrawCircle(Simulation.MousePosition, 3.5f, editModeColor);
	}

	// Input
	public override void _Input(InputEvent @event)
	{
		// Enable/disable modes
		if (@event.IsActionPressed("Destroy")) {
			editMode = EditMode.Destroy;
			connectionInserting = null;
			jointGrabbed = null;
		}
		else if (@event.IsActionPressed("Create")) {
			editMode = EditMode.Create;
			jointGrabbed = null;
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
		) editMode = EditMode.Default;

		// Different modes
		if (editMode == EditMode.Default) {
			// Grab
			if (@event.IsActionPressed("Primary Edit")) AttemptGrabJoint();
			else if (@event.IsActionReleased("Primary Edit")) AttemptReleaseJoint();
		} else if (editMode == EditMode.Create) {
			// Insertion
			if (@event.IsActionPressed("Primary Edit")) AttemptInsertStart();
			else if (@event.IsActionReleased("Primary Edit")) AttemptInsertEnd();
			else if (@event.IsActionPressed("Secondary Edit")) {
				if (connectionInserting != null) AttemptInsertMiddle();
				else AttemptJointFlip();
			}

			// Connection size edit
			if (@event.IsActionPressed("Wheel Up")) AttemptConnectionExtend();
			else if (@event.IsActionPressed("Wheel Down")) AttemptConnectionShrink();
		}

		// Pause simulation
		if (@event.IsActionPressed("Pause Simulation")) cloth.simulationPaused = !cloth.simulationPaused;
	}

	// Attempt grab joint
	private void AttemptGrabJoint() {
		jointGrabbed = jointUnderMouse;
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
		foreach (Joint joint in cloth.joints) {
			if (joint.CollidesWithPoint(Simulation.MousePosition)) {
				joint.isFixed = !joint.isFixed;
				return;
			}
		}
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
