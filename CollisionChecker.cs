// Dependencies
using System;
using Godot;

// Checks for collisions?
public static class CollisionChecker
{
	// Settings
	private const float Tolerance = 2.0f;

	// Checks for a point/connection collision
	public static bool PointCollidesWithConnection(Vector2 pointPosition, Connection connection) {
		// Get distances from mouse to joints
		float distanceFromFirstJoint = pointPosition.DistanceTo(connection.firstJoint.Position);
		float distanceFromSecondJoint = pointPosition.DistanceTo(connection.secondJoint.Position);

		// Calculate some distances
		float distanceBetweenJoints = connection.actualLength;
		float distanceFromConnection = distanceFromFirstJoint + distanceFromSecondJoint;

		// Check collision
		bool collided =
			distanceFromConnection >= distanceBetweenJoints - Tolerance
			&& distanceFromConnection <= distanceBetweenJoints + Tolerance;

		// Return collision
		return collided;
	}
}
