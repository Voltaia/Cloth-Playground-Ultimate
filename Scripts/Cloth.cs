// Dependencies
using Godot;
using System;
using System.Collections.Generic;

// Cloth data structure
public partial class Cloth : Node2D
{
	// Variables
	public List<Joint> joints = new List<Joint>();
	public List<Connection> connections = new List<Connection>();
	public bool simulationPaused = false;
	public bool visualizeStress = false;
	public Random rng = new Random();

	// Settings
	private const int Separation = 50;
	private const int SidesPadding = 100;

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
	public void GeneratePlainCloth(Vector2 size)
	{
		// Calculate joints to create
		int horizontalCount = ((int)size.X - SidesPadding) / Separation;
		int verticalCount = ((int)size.Y - SidesPadding) / Separation;
		Vector2 clothSize = new Vector2((horizontalCount - 1) * Separation, (verticalCount - 1) * Separation);
		Vector2 startPosition = size / 2 - clothSize / 2;
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
