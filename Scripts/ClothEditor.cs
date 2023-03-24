// Dependencies
using Godot;
using System;
using System.Collections.Generic;

// Awesome note:
// Make it so when you're creating a new joint-connection-joint that:
// Holding LMB creates one with unfixed joints
// Holding RMB creates one with fixed joints

// Controls cloth!
public partial class ClothEditor : Node2D
{
	// Variables
	public Cloth cloth;
	private bool connectionEditMode = false;
	private bool jointEditMode = false;
	private EditMode editMode = EditMode.Default;
	private Connection connectionBeingInserted = null;
	private Vector2 mousePosition;

	// Edit modes
	private enum EditMode {
		Default,
		Cut,
		Insert
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		// Fill mouse position
		mousePosition = GetViewport().GetMousePosition();

		// Cutting logic
		if (editMode == EditMode.Cut && Input.IsActionPressed("Apply Operation")) {
			AttemptConnectionCut();
			AttemptJointCut();
		}

		// Queue redraw
		QueueRedraw();
	}

	// Draw stuff
	public override void _Draw() {
		// Draw inserting new connection
		if (connectionBeingInserted != null) {
			DrawLine(
				connectionBeingInserted.firstJoint.Position,
				mousePosition,
				Colors.Green,
				Connection.DrawThickness
			);
		}

		// Draw edit mode
		Color editModeColor = Colors.Blue;
		if (editMode == EditMode.Insert) editModeColor = Colors.Green;
		else if (editMode == EditMode.Cut) editModeColor = Colors.Red;
		DrawCircle(mousePosition, 3.5f, editModeColor);
	}

	// Input
	public override void _Input(InputEvent @event)
	{
		// Enable/disable modes
		if (@event.IsActionPressed("Cut")) editMode = EditMode.Cut;
		else if (@event.IsActionPressed("Insert")) editMode = EditMode.Insert;
		else if (
			@event.IsActionReleased("Cut")
			|| @event.IsActionReleased("Insert")
		) editMode = EditMode.Default;

		// Inserting logic
		if (editMode == EditMode.Insert) {
			if (@event.IsActionPressed("Apply Operation")) AttemptInsertStart();
			else if (@event.IsActionReleased("Apply Operation")) AttemptInsertEnd();
		}

		// Modify joint
		if (editMode == EditMode.Insert && @event.IsActionPressed("Modify Joint")) AttemptFlipJoint();

		// Pause simulation
		if (@event.IsActionPressed("Pause Simulation")) cloth.simulationPaused = !cloth.simulationPaused;
	}

	// Insert start
	private void AttemptInsertStart() {
		// Check if there is a joint at mouse position
		Joint jointFound = null;
		foreach (Joint joint in cloth.joints) {
			// If there is a joint
			if (joint.CollidesWithPoint(mousePosition)) jointFound = joint;
		}

		// Create a connection with either a new or old joint attached
		if (jointFound != null) connectionBeingInserted = new Connection(cloth, jointFound, null);
		else connectionBeingInserted = new Connection(cloth, cloth.AddJoint(mousePosition, true), null);
	}

	// Insert end
	private void AttemptInsertEnd() {
		// Check if there is a connection being created
		if (connectionBeingInserted == null) return;

		// Check if there is a joint at mouse position
		Joint jointFound = null;
		foreach (Joint joint in cloth.joints) {
			// If there is a joint
			if (joint.CollidesWithPoint(mousePosition)) jointFound = joint;
		}

		// End connection with either an old or new joint
		if (jointFound != null) connectionBeingInserted.secondJoint = jointFound;
		else connectionBeingInserted.secondJoint = cloth.AddJoint(mousePosition, true);
		connectionBeingInserted.ReadjustLength();
		cloth.AddConnection(connectionBeingInserted);

		// Wipe connection
		connectionBeingInserted = null;
	}

	// Attempt to cut a connection
	private void AttemptConnectionCut() {
		for (int index = cloth.connections.Count - 1; index >= 0; index--) {
			Connection connection = cloth.connections[index];
			if (connection.CollidesWithPoint(mousePosition)) {
				cloth.RemoveConnection(connection);
			}
		}
	}

	// Attempt to cut a joint
	private void AttemptJointCut() {
		for (int index = cloth.joints.Count - 1; index >= 0; index--) {
			Joint joint = cloth.joints[index];
			if (joint.CollidesWithPoint(mousePosition)) {
				cloth.RemoveJoint(joint);
			}
		}
	}

	// Attempt to flip a joint
	private void AttemptFlipJoint() {
		foreach (Joint joint in cloth.joints) {
			if (joint.CollidesWithPoint(mousePosition)) {
				joint.isFixed = !joint.isFixed;
				return;
			}
		}
	}
}
