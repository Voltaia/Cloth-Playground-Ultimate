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
	private bool isCutting = false;
	private Connection connectionBeingInserted = null;

	// Settings
	private const int Separation = 50;
	private const int Padding = 100;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		GeneratePlainCloth();
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		// Switching modes
		if (Input.IsActionPressed("Edit Connections")) {
			connectionEditMode = true;
			jointEditMode = false;
		} else if (Input.IsActionPressed("Edit Joints")) {
			jointEditMode = true;
			connectionEditMode = false;
		} else {
			connectionEditMode = false;
			jointEditMode = false;
		}

		// Checking modes
		if (connectionEditMode) {
			if (isCutting) {
				AttemptConnectionCut();
			}
		} else if (jointEditMode) {
			if (isCutting) {
				AttemptJointCut();
			}
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
		// Exit app
		//if (@event.IsActionPressed("Exit")) GetTree().Quit();

		// Enable/disable cutting
		if (@event.IsActionPressed("Cut Item")) isCutting = true;
		else if (@event.IsActionReleased("Cut Item")) isCutting = false;

		// Insert pressed
		if (@event.IsActionPressed("Insert Item")) {
			if (jointEditMode) AttemptJointInsert();
			else if (connectionEditMode) AttemptConnectionInsertStart();
		}

		// Insert released
		if (@event.IsActionReleased("Insert Item") && connectionBeingInserted != null) {
			if (connectionEditMode) AttemptConnectionInsertEnd();
			connectionBeingInserted = null;
		}
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

	// Begin inserting a connection
	private void AttemptConnectionInsertStart() {
		Vector2 mousePosition = GetViewport().GetMousePosition();
		foreach (Joint joint in joints) {
			if (joint.CollidesWithPoint(mousePosition)) {
				connectionBeingInserted = new Connection(joint, null);
				return;
			}
		}
	}

	// Finish inserting a connection
	private void AttemptConnectionInsertEnd() {
		Vector2 mousePosition = GetViewport().GetMousePosition();
		foreach (Joint joint in joints) {
			if (joint.CollidesWithPoint(mousePosition)) {
				connectionBeingInserted.secondJoint = joint;
				connectionBeingInserted.ReadjustLength();
				AddConnection(connectionBeingInserted);
				return;
			}
		}
	}

	// Insert a joint
	private void AttemptJointInsert() {
		Vector2 mousePosition = GetViewport().GetMousePosition();
		foreach (Joint joint in joints) {
			if (joint.CollidesWithPoint(mousePosition)) {
				joint.isFixed = !joint.isFixed;
				return;
			}
		}
		AddJoint(mousePosition, true);
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
