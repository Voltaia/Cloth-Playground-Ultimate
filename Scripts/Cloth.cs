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
	public bool drawJoints = true;
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
			float distanceFromMouse = joints[index].position.DistanceTo(Simulation.MousePosition);
			if (distanceFromMouse > DisposalDistance) RemoveJointAt(index);
		}

		// Simulate connections
		for (int index = connections.Count - 1; index >= 0; index--) {
			// Get reference
			Connection connection = connections[index];

			// Dispose of
			if (
				connection.firstJoint.hasBeenRemoved
				|| connection.secondJoint.hasBeenRemoved
			) {
				RemoveConnectionAt(index);
				continue;
			}

			// Simulate
			connection.Simulate(delta);

			// Check stress
			if (breakUnderStress && connection.stress >= StressBreakMultiplier) {
				RemoveConnectionAt(index);
				continue;
			}
		}

		// Queue redraw
		QueueRedraw();
	}

	// Draw
	public override void _Draw() {
		// Draw connections
		Color conectionColor = Palette.connectionColor;
		foreach (Connection connection in connections) {
			// Get stress
			if (visualizeStress) {
				float stressClamped = Mathf.Clamp(connection.stress, 0, 1);
				float inverseStressClamped = 1.0f - connection.stress;
				conectionColor = new Color(stressClamped, inverseStressClamped, 0.0f);
			}
			
			// Draw self
			DrawLine(
				connection.firstJoint.position,
				connection.secondJoint.position,
				conectionColor,
				Connection.DrawThickness
			);
		}

		// Draw joints
		if (drawJoints) {
			foreach (Joint joint in joints) {
				// Decide to draw fixed or loose colors
				if (joint.isFixed) DrawCircle(joint.position, jointRadius, Palette.fixedJointColor);
				else DrawCircle(joint.position, jointRadius, Palette.jointColor);
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

		// Return connection
		return connection;
	}

	// Add a joint
	public Joint AddJoint(Vector2 position, bool isFixed) {
		// Create joint and add to list
		Joint newJoint = new Joint(this, position, isFixed);
		int listPosition = rng.Next(0, joints.Count);
		joints.Insert(listPosition, newJoint);

		// Return joint
		return newJoint;
	}

	// Remove connection at
	public void RemoveConnectionAt(int connectionIndex) {
		connections.RemoveAt(connectionIndex);
	}

	// Remove joint
	public void RemoveJointAt(int jointIndex) {
		// Get reference
		Joint joint = joints[jointIndex];

		// Remove self
		joint.hasBeenRemoved = true;
		joints.RemoveAt(jointIndex);
	}

	// Get closest joint
	public Joint GetClosestJoint(Vector2 position, float maximumDistance) {
		Joint closestJoint = null;
		float closestJointDistance = Mathf.Inf;
		foreach (Joint joint in joints) {
			float distanceToJoint = position.DistanceTo(joint.position);
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

		// Anchor end joints
		jointArray[0, 0].isFixed = true;
		jointArray[horizontalCount - 1, 0].isFixed = true;

		// Anchor center joints
		int maxIndex = horizontalCount - 1;
		if (horizontalCount % 2 == 0) {
			jointArray[Mathf.RoundToInt(maxIndex * 0.2f), 0].isFixed = true;
			jointArray[Mathf.RoundToInt(maxIndex * 0.4f), 0].isFixed = true;
			jointArray[Mathf.RoundToInt(maxIndex * 0.6f), 0].isFixed = true;
			jointArray[Mathf.RoundToInt(maxIndex * 0.8f), 0].isFixed = true;
			
		} else {
			jointArray[Mathf.RoundToInt(maxIndex * 0.25f), 0].isFixed = true;
			jointArray[Mathf.RoundToInt(maxIndex * 0.5f), 0].isFixed = true;
			jointArray[Mathf.RoundToInt(maxIndex * 0.75f), 0].isFixed = true;
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
