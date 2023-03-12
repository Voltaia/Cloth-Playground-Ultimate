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

	// Settings
	private const int Separation = 50;
	private const int Padding = 50;

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

		// Checking edit modes
		if (connectionEditMode) {
			if (isCutting) {
				AttemptConnectionCut();
			}
		} else if (jointEditMode) {

		}

		// Queue redraw
		QueueRedraw();
	}

	// Draw stuff
	public override void _Draw() {
		// Draw framerate
		DrawString(new Label().GetThemeDefaultFont(), new Vector2(5, 20), Engine.GetFramesPerSecond().ToString());
	}

	// Input
	public override void _Input(InputEvent @event)
	{
		// Exit app
		if (@event.IsActionPressed("Exit")) GetTree().Quit();

		// Enable/disable cutting
		if (@event.IsActionPressed("Cut Item")) isCutting = true;
		else if (@event.IsActionReleased("Cut Item")) isCutting = false;
	}

	// Attempt connection cut
	private void AttemptConnectionCut() {
		Vector2 mousePosition = GetViewport().GetMousePosition();
		for (int index = connections.Count - 1; index >= 0; index--) {
			Connection connection = connections[index];
			if (CollisionChecker.PointCollidesWithConnection(mousePosition, connection)) {
				connection.QueueFree();
				connections.Remove(connection);
			}
		}
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
				Joint newJoint = new Joint(newJointPosition, isFixed);
				joints.Add(newJoint);
				AddChild(newJoint);
				jointArray[xIndex, yIndex] = newJoint;

				// Create connection west
				if (xIndex > 0)
				{
					Connection westConnection = new Connection(
						jointArray[xIndex - 1, yIndex],
						jointArray[xIndex, yIndex]
					);
					AddChild(westConnection);
					connections.Add(westConnection);
				}

				// Connect connection north
				if (yIndex > 0)
				{
					Connection northConnection = new Connection(
						jointArray[xIndex, yIndex - 1],
						jointArray[xIndex, yIndex]
					);
					AddChild(northConnection);
					connections.Add(northConnection);
				}
			}
		}
	}
}
