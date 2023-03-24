// Dependencies
using Godot;
using System;
using System.Collections.Generic;

// Controls cloth!
public partial class ClothEditor : Node2D
{
	// Variables
	public Cloth cloth;
	private bool connectionEditMode = false;
	private bool jointEditMode = false;
	private EditMode editMode = EditMode.Default;
	private Connection connectionInserting = null;

	// Edit modes
	private enum EditMode {
		Default,
		Destroy,
		Create
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
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
		// Draw edit mode
		Color editModeColor = Colors.Blue;
		if (editMode == EditMode.Create) editModeColor = Colors.Green;
		else if (editMode == EditMode.Destroy) editModeColor = Colors.Red;
		DrawCircle(Game.MousePosition, 3.5f, editModeColor);
	}

	// Input
	public override void _Input(InputEvent @event)
	{
		// Enable/disable modes
		if (@event.IsActionPressed("Destroy")) {
			editMode = EditMode.Destroy;
			connectionInserting = null;
		}
		else if (@event.IsActionPressed("Create")) editMode = EditMode.Create;
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

		// Inserting logic
		if (editMode == EditMode.Create) {
			if (@event.IsActionPressed("Primary Edit")) AttemptInsertStart();
			else if (@event.IsActionReleased("Primary Edit")) AttemptInsertEnd();
			else if (@event.IsActionPressed("Secondary Edit")) {
				if (connectionInserting != null) AttemptInsertMiddle();
				else AttemptFlipJoint();
			}
		}

		// Pause simulation
		if (@event.IsActionPressed("Pause Simulation")) cloth.simulationPaused = !cloth.simulationPaused;
	}

	// Insert start
	private void AttemptInsertStart() {
		// Check if there is a joint at mouse position
		Joint jointFound = JointUnderMouse();

		// Create a connection with either a new or old joint attached
		if (jointFound != null) connectionInserting = new Connection(cloth, jointFound, null);
		else connectionInserting = new Connection(cloth, cloth.AddJoint(Game.MousePosition, true), null);
	}

	// Insert middle
	private void AttemptInsertMiddle() {
		// Check if there is a joint at mouse position
		Joint jointToConnect = JointUnderMouse();
		
		// Create new joint if there is none
		if (jointToConnect == null) jointToConnect = cloth.AddJoint(Game.MousePosition, false);

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
		Joint jointFound = JointUnderMouse();

		// End connection with either an old or new joint
		if (jointFound != null) connectionInserting.secondJoint = jointFound;
		else connectionInserting.secondJoint = cloth.AddJoint(Game.MousePosition, true);
		connectionInserting.ReadjustLength();
		cloth.AddConnection(connectionInserting);

		// Wipe connection
		connectionInserting = null;
	}

	// Attempt to cut a connection
	private void AttemptConnectionCut() {
		for (int index = cloth.connections.Count - 1; index >= 0; index--) {
			Connection connection = cloth.connections[index];
			if (connection.CollidesWithPoint(Game.MousePosition)) {
				cloth.RemoveConnection(connection);
			}
		}
	}

	// Attempt to cut a joint
	private void AttemptJointCut() {
		for (int index = cloth.joints.Count - 1; index >= 0; index--) {
			Joint joint = cloth.joints[index];
			if (joint.CollidesWithPoint(Game.MousePosition)) {
				cloth.RemoveJoint(joint);
			}
		}
	}

	// Attempt to flip a joint
	private void AttemptFlipJoint() {
		foreach (Joint joint in cloth.joints) {
			if (joint.CollidesWithPoint(Game.MousePosition)) {
				joint.isFixed = !joint.isFixed;
				return;
			}
		}
	}

	// Check if joint collides with mouse position
	private Joint JointUnderMouse() {
		// Check if there is a joint at mouse position
		foreach (Joint joint in cloth.joints) {
			// If there is a joint
			if (joint.CollidesWithPoint(Game.MousePosition)) return joint;
		}
		return null;
	}
}
