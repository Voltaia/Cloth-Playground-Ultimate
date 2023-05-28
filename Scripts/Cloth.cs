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
	public int jointRadius = 10;
	public bool drawJoints = false;
	public bool breakUnderStress = false;
	private GenerationSettings generationSettings;
	private int jointSeparation = 50;

	// Settings
	private const float DisposalDistance = 10000;
	private const float StressBreakMultiplier = 5.0f;

	// Constructor
	public Cloth(GenerationSettings generationSettings) {
		Name = "Cloth";
		this.generationSettings = generationSettings;
		jointSeparation = this.generationSettings.ConvertJointSeparation();
		jointRadius = this.generationSettings.GetJointRadius();
	}

	// Constructor
	public Cloth() {
		Name = "Cloth";
	}

	// On start
	public override void _Ready() {
		if (generationSettings != null) Generate(GetViewportRect().Size);
	}

	// Every frame
	public override void _Process(double delta) {
		// Simulate joints
		for (int index = joints.Count - 1; index >= 0; index--) {
			// Simulate
			joints[index].Simulate(delta);

			// Dispose of
			float distanceFromMouse = joints[index].Position.DistanceTo(Simulation.MousePosition);
			if (distanceFromMouse > DisposalDistance) RemoveJointAt(index);
		}

		// Simulate connections
		for (int index = connections.Count - 1; index >= 0; index--) {
			// Simulate
			connections[index].Simulate(delta);

			// Check stress
			if (breakUnderStress && connections[index].stress >= StressBreakMultiplier) {
				RemoveConnectionAt(index);
				continue;
			}
		}
	}

	// Add a connection
	public Connection AddConnection(Joint firstJoint, Joint secondJoint) {
		Connection newConnection = new Connection(this, firstJoint, secondJoint);
		return AddConnection(newConnection);
	}

	// Add a connection
	public Connection AddConnection(Connection connection) {
		// Add to list
		int listPosition = rng.Next(0, connections.Count);
		connections.Insert(listPosition, connection);

		// Add to simulation
		AddChild(connection);

		// Return connection
		return connection;
	}

	// Add a joint
	public Joint AddJoint(Vector2 position, bool isFixed) {
		// Create joint and add to list
		Joint newJoint = new Joint(this, position, isFixed);
		int listPosition = rng.Next(0, joints.Count);
		joints.Insert(listPosition, newJoint);

		// Put into tree
		AddChild(newJoint);

		// Return joint
		return newJoint;
	}

	// Remove connection at
	public void RemoveConnectionAt(int connectionIndex) {
		connections[connectionIndex].QueueFree();
		connections.RemoveAt(connectionIndex);
	}

	// Remove joint
	public void RemoveJointAt(int jointIndex) {
		// Get reference
		Joint joint = joints[jointIndex];

		// Remove connections
		for (int index = connections.Count - 1; index >= 0; index--) {
			Connection connection = connections[index];
			if (joint == connection.firstJoint) RemoveConnectionAt(index);
			else if (joint == connection.secondJoint) RemoveConnectionAt(index);
		}

		// Remove self
		joint.QueueFree();
		joints.Remove(joint);
	}

	// Redraw connections
	public void RedrawConnections() {
		foreach (Connection connection in connections) {
			connection.QueueRedraw();
		}
	}

	// Get closest joint
	public Joint GetClosestJoint(Vector2 position, float maximumDistance) {
		Joint closestJoint = null;
		float closestJointDistance = Mathf.Inf;
		foreach (Joint joint in joints) {
			float distanceToJoint = position.DistanceTo(joint.Position);
			if (distanceToJoint < closestJointDistance) {
				closestJoint = joint;
				closestJointDistance = distanceToJoint;
			}
		}
		if (
			closestJoint != null
			&& closestJointDistance >= closestJoint.parent.jointRadius * maximumDistance
		) closestJoint = null;
		return closestJoint;
	}

	// Get closest connection
	public Connection GetClosestConnection(Vector2 position, float maximumDistance) {
		Connection closestConnection = null;
		float closestConnectionDistance = Mathf.Inf;
		foreach (Connection connection in connections) {
			float distanceToConnection = position.DistanceTo(connection.GetCenterPosition());
			if (distanceToConnection < closestConnectionDistance) {
				closestConnection = connection;
				closestConnectionDistance = distanceToConnection;
			}
		}
		if (
			closestConnection != null
			&& closestConnectionDistance >= maximumDistance
		) closestConnection = null;
		return closestConnection;
	}

	// Redraw joints
	public void RedrawJoints() {
		foreach (Joint joint in joints) {
			joint.QueueRedraw();
		}
	}

	// Generate cloth
	public void Generate(Vector2 size)
	{
		// Calculate joints to create
		int horizontalCount = ((int)size.X - generationSettings.sidePadding) / jointSeparation;
		int verticalCount = ((int)size.Y - generationSettings.sidePadding) / jointSeparation;
		Vector2 clothSize = new Vector2((horizontalCount - 1) * jointSeparation, (verticalCount - 1) * jointSeparation);
		Vector2 startPosition = new Vector2(size.X / 2 - clothSize.X / 2, generationSettings.sidePadding / 2);
		Joint[,] jointArray = new Joint[horizontalCount, verticalCount];

		// Adjust vertical count to make room for expansion
		verticalCount = (int)(verticalCount * 0.9f);

		// Create joints and connections one by one
		for (int xIndex = 0; xIndex < horizontalCount; xIndex++)
		{
			for (int yIndex = 0; yIndex < verticalCount; yIndex++)
			{
				// Create joint
				Vector2 newJointPosition = new Vector2(
					startPosition.X + xIndex * jointSeparation,
					startPosition.Y + yIndex * jointSeparation
				);
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
				if (generationSettings.rigid && xIndex > 0 && yIndex > 0) {
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

	// Generation settings
	public class GenerationSettings
	{
		// Class variables
		public bool rigid = false;
		public JointSeparation jointSeparation = JointSeparation.Medium;
		public int sidePadding = 100;

		// Joint separation
		public enum JointSeparation {
			Small,
			Medium,
			Large
		}

		// Get joint separation
		public int ConvertJointSeparation() {
			switch (jointSeparation) {
				case JointSeparation.Small:
				return 40;

				default:
				return 50;

				case JointSeparation.Large:
				return 60;
			}
		}

		// Get joint size
		public int GetJointRadius() {
			switch (jointSeparation) {
				case JointSeparation.Small:
				return 8;

				default:
				return 10;

				case JointSeparation.Large:
				return 12;
			}
		}
	}
}
