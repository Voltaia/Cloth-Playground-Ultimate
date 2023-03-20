// Dependencies
using Godot;
using System;
using System.Collections.Generic;

// Controls cloth!
public partial class ClothController : Node2D
{
	// Variables
	private Cloth cloth = new Cloth();
	private bool connectionEditMode = false;
	private bool jointEditMode = false;
	private EditMode editMode = EditMode.Default;
	private Connection connectionBeingInserted = null;

	// Edit modes
	private enum EditMode {
		Default,
		Cut,
		Insert
	}

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		AddChild(cloth);
		cloth.GeneratePlainCloth(GetViewportRect().Size);
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
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
		// Draw framerate
		DrawString(new Label().GetThemeDefaultFont(), new Vector2(5, 20), Engine.GetFramesPerSecond().ToString());

		// Draw inserting new connection
		if (connectionBeingInserted != null) {
			Vector2 mousePosition = GetViewport().GetMousePosition();
			DrawLine(
				connectionBeingInserted.firstJoint.Position,
				mousePosition,
				Colors.Green,
				Connection.DrawThickness
			);
		}
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
	}

	// Insert start
	private void AttemptInsertStart() {
		// Get mouse position
		Vector2 mousePosition = GetViewport().GetMousePosition();

		// Check if there is a joint at mouse position
		Joint jointFound = null;
		foreach (Joint joint in cloth.joints) {
			// If there is a joint
			if (joint.CollidesWithPoint(mousePosition)) jointFound = joint;
		}

		// Create a connection with either a new or old joint attached
		if (jointFound != null) connectionBeingInserted = new Connection(jointFound, null);
		else connectionBeingInserted = new Connection(cloth.AddJoint(mousePosition, true), null);
	}

	// Insert end
	private void AttemptInsertEnd() {
		// Check if there is a connection being created
		if (connectionBeingInserted == null) return;

		// Get mouse position
		Vector2 mousePosition = GetViewport().GetMousePosition();

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
		Vector2 mousePosition = GetViewport().GetMousePosition();
		for (int index = cloth.connections.Count - 1; index >= 0; index--) {
			Connection connection = cloth.connections[index];
			if (connection.CollidesWithPoint(mousePosition)) {
				cloth.RemoveConnection(connection);
			}
		}
	}

	// Attempt to cut a joint
	private void AttemptJointCut() {
		Vector2 mousePosition = GetViewport().GetMousePosition();
		for (int index = cloth.joints.Count - 1; index >= 0; index--) {
			Joint joint = cloth.joints[index];
			if (joint.CollidesWithPoint(mousePosition)) {
				cloth.RemoveJoint(joint);
			}
		}
	}

	// Attempt to flip a joint
	private void AttemptFlipJoint() {
		Vector2 mousePosition = GetViewport().GetMousePosition();
		foreach (Joint joint in cloth.joints) {
			if (joint.CollidesWithPoint(mousePosition)) {
				joint.isFixed = !joint.isFixed;
				return;
			}
		}
	}
}
