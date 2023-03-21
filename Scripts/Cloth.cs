// Dependencies
using Godot;
using System;
using System.Collections.Generic;

// Cloth data structure
public partial class Cloth : Node2D
{
	// Functional variables
	public List<Joint> joints = new List<Joint>();
	public List<Connection> connections = new List<Connection>();
	public bool simulationPaused = false;
	public bool visualizeStress = false;
	public Random rng = new Random();

	// Generation settings
	public bool generateRigid = false;
	public Vector2 generationSize = Vector2.One * 250;
	public int generationSeparation = 50;
	private int generationSidesPadding = 100;

	// Constructor
	public Cloth() {
		Name = "Cloth";
	}

	// Add a connection
	public Connection AddConnection(Joint firstJoint, Joint secondJoint) {
		Connection newConnection = new Connection(this, firstJoint, secondJoint);
		return AddConnection(newConnection);
	}

	// Add a connection
	public Connection AddConnection(Connection connection) {
		// Add to list
		connections.Add(connection);

		// Put into tree
		AddChild(connection);
		int treePosition = rng.Next(joints.Count, joints.Count + connections.Count);
		MoveChild(connection, treePosition);

		// Return connection
		return connection;
	}

	// Add a joint
	public Joint AddJoint(Vector2 position, bool isFixed) {
		// Create joint and add to list
		Joint newJoint = new Joint(this, position, isFixed);
		joints.Add(newJoint);

		// Put into tree
		AddChild(newJoint);
		int treePosition = rng.Next(0, joints.Count);
		MoveChild(newJoint, treePosition);

		// Return joint
		return newJoint;
	}

	// Remove connection
	public void RemoveConnection(Connection connection) {
		connection.QueueFree();
		connections.Remove(connection);
	}

	// Remove joint
	public void RemoveJoint(Joint joint) {
		for (int index = connections.Count - 1; index >= 0; index--) {
			Connection connection = connections[index];
			if (joint == connection.firstJoint) RemoveConnection(connection);
			else if (joint == connection.secondJoint) RemoveConnection(connection);
		}
		joint.QueueFree();
		joints.Remove(joint);
	}

	// Generate cloth
	public void Generate(Vector2 size)
	{
		// Calculate joints to create
		int horizontalCount = ((int)size.X - generationSidesPadding) / generationSeparation;
		int verticalCount = ((int)size.Y - generationSidesPadding) / generationSeparation;
		Vector2 clothSize = new Vector2((horizontalCount - 1) * generationSeparation, (verticalCount - 1) * generationSeparation);
		Vector2 startPosition = size / 2 - clothSize / 2;
		Joint[,] jointArray = new Joint[horizontalCount, verticalCount];

		// Create joints and connections one by one
		for (int xIndex = 0; xIndex < horizontalCount; xIndex++)
		{
			for (int yIndex = 0; yIndex < verticalCount; yIndex++)
			{
				// Create joint
				Vector2 newJointPosition = new Vector2(startPosition.X + xIndex * generationSeparation, startPosition.Y + yIndex * generationSeparation);
				jointArray[xIndex, yIndex] = AddJoint(newJointPosition, false);

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

				// Rigid connections
				if (generateRigid && xIndex > 0 && yIndex > 0) {
					AddConnection(
						jointArray[xIndex, yIndex],
						jointArray[xIndex - 1, yIndex - 1]
					);
				}
			}
		}

		// Anchor some joints
		if (horizontalCount % 2 == 0) {
			jointArray[0, 0].isFixed = true;
			jointArray[Mathf.FloorToInt(horizontalCount * 0.2f), 0].isFixed = true;
			jointArray[Mathf.FloorToInt(horizontalCount * 0.4f), 0].isFixed = true;
			jointArray[Mathf.FloorToInt(horizontalCount * 0.6f), 0].isFixed = true;
			jointArray[Mathf.FloorToInt(horizontalCount * 0.8f), 0].isFixed = true;
			jointArray[horizontalCount - 1, 0].isFixed = true;
		} else {
			jointArray[0, 0].isFixed = true;
			jointArray[Mathf.FloorToInt(horizontalCount * 0.25f), 0].isFixed = true;
			jointArray[horizontalCount / 2, 0].isFixed = true;
			jointArray[Mathf.FloorToInt(horizontalCount * 0.75f), 0].isFixed = true;
			jointArray[horizontalCount - 1, 0].isFixed = true;
		}
	}
}
