// Dependencies
using Godot;
using System;
using System.Collections.Generic;

// Controls cloth!
public partial class ClothController : Node2D
{
	// Variables
	private List<Joint> joints = new List<Joint>();
	private List<Connection> connections = new List<Connection>();
	private bool connectionEditMode = false;
	private bool jointEditMode = false;
	private EditMode editMode = EditMode.Default;
	private Connection connectionBeingInserted = null;

	// Settings
	private const int Separation = 50;
	private const int Padding = 100;

	// Edit modes
	private enum EditMode {
		Default,
		Cut,
		Insert
	}

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		GeneratePlainCloth();
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
		if (@event.IsActionPressed("Modify Joint")) AttemptFlipJoint();
	}

	// Insert start
	private void AttemptInsertStart() {
		// Get mouse position
		Vector2 mousePosition = GetViewport().GetMousePosition();

		// Check if there is a joint at mouse position
		Joint jointFound = null;
		foreach (Joint joint in joints) {
			// If there is a joint
			if (joint.CollidesWithPoint(mousePosition)) jointFound = joint;
		}

		// Create a connection with either a new or old joint attached
		if (jointFound != null) connectionBeingInserted = new Connection(jointFound, null);
		else connectionBeingInserted = new Connection(AddJoint(mousePosition, true), null);
	}

	// Insert end
	private void AttemptInsertEnd() {
		// Check if there is a connection being created
		if (connectionBeingInserted == null) return;

		// Get mouse position
		Vector2 mousePosition = GetViewport().GetMousePosition();

		// Check if there is a joint at mouse position
		Joint jointFound = null;
		foreach (Joint joint in joints) {
			// If there is a joint
			if (joint.CollidesWithPoint(mousePosition)) jointFound = joint;
		}

		// End connection with either an old or new joint
		if (jointFound != null) connectionBeingInserted.secondJoint = jointFound;
		else connectionBeingInserted.secondJoint = AddJoint(mousePosition, true);
		connectionBeingInserted.ReadjustLength();
		AddConnection(connectionBeingInserted);

		// Wipe connection
		connectionBeingInserted = null;
	}

	// Attempt to cut a connection
	private void AttemptConnectionCut() {
		Vector2 mousePosition = GetViewport().GetMousePosition();
		for (int index = connections.Count - 1; index >= 0; index--) {
			Connection connection = connections[index];
			if (connection.CollidesWithPoint(mousePosition)) {
				RemoveConnection(connection);
			}
		}
	}

	// Attempt to cut a joint
	private void AttemptJointCut() {
		Vector2 mousePosition = GetViewport().GetMousePosition();
		for (int index = joints.Count - 1; index >= 0; index--) {
			Joint joint = joints[index];
			if (joint.CollidesWithPoint(mousePosition)) {
				RemoveJoint(joint);
			}
		}
	}

	// Attempt to flip a joint
	private void AttemptFlipJoint() {
		Vector2 mousePosition = GetViewport().GetMousePosition();
		foreach (Joint joint in joints) {
			if (joint.CollidesWithPoint(mousePosition)) {
				joint.isFixed = !joint.isFixed;
				return;
			}
		}
	}

	// Add a connection
	private Connection AddConnection(Joint firstJoint, Joint secondJoint) {
		Connection newConnection = new Connection(firstJoint, secondJoint);
		return AddConnection(newConnection);
	}

	// Add a connection
	private Connection AddConnection(Connection connection) {
		AddChild(connection);
		connections.Add(connection);
		return connection;
	}

	// Add a joint
	private Joint AddJoint(Vector2 position, bool isFixed) {
		Joint newJoint = new Joint(position, isFixed);
		joints.Add(newJoint);
		AddChild(newJoint);
		return newJoint;
	}

	// Remove connection
	private void RemoveConnection(Connection connection) {
		connection.QueueFree();
		connections.Remove(connection);
	}

	// Remove joint
	private void RemoveJoint(Joint joint) {
		for (int index = connections.Count - 1; index >= 0; index--) {
			Connection connection = connections[index];
			if (joint == connection.firstJoint) RemoveConnection(connection);
			else if (joint == connection.secondJoint) RemoveConnection(connection);
		}
		joint.QueueFree();
		joints.Remove(joint);
	}

	// Generate cloth
	private void GeneratePlainCloth()
	{
		// Calculate joints to create
		Vector2 screenSize = GetViewportRect().Size;
		int horizontalCount = ((int)screenSize.X - Padding) / Separation;
		int verticalCount = ((int)screenSize.Y - Padding) / Separation;
		Vector2 clothSize = new Vector2((horizontalCount - 1) * Separation, (verticalCount - 1) * Separation);
		Vector2 startPosition = screenSize / 2 - clothSize / 2;
		Joint[,] jointArray = new Joint[horizontalCount, verticalCount];

		// Create joints and connections one by one
		for (int xIndex = 0; xIndex < horizontalCount; xIndex++)
		{
			for (int yIndex = 0; yIndex < verticalCount; yIndex++)
			{
				// Create joint
				Vector2 newJointPosition = new Vector2(startPosition.X + xIndex * Separation, startPosition.Y + yIndex * Separation);
				bool isFixed = yIndex == 0;
				jointArray[xIndex, yIndex] = AddJoint(newJointPosition, isFixed);

				// Create connection west
				if (xIndex > 0)
				{
					AddConnection(
						jointArray[xIndex - 1, yIndex],
						jointArray[xIndex, yIndex]
					);
				}

				// Connect connection north
				if (yIndex > 0)
				{
					AddConnection(
						jointArray[xIndex, yIndex - 1],
						jointArray[xIndex, yIndex]
					);
				}
			}
		}
	}
}
