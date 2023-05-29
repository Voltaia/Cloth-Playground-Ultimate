// Dependencies
using Godot;

// A cloth connection
public partial class Connection
{
	// Variables
	public Joint firstJoint;
	public Joint secondJoint;
	public float desiredLength;
	public Cloth parent;
	public float stress;

	// Settings
	public const float DrawThickness = 5.0f;
	private const int SimulationIterations = 5;
	private const float StressRange = 25.0f;

	// Properties
	public float actualLength {
		get {return firstJoint.position.DistanceTo(secondJoint.position);}
	}

	// Constructor
	public Connection(Cloth parent, Joint firstJoint, Joint secondJoint)
	{
		// Set fields
		this.parent = parent;
		this.firstJoint = firstJoint;
		this.secondJoint = secondJoint;

		// Ensure stable connections
		bool firstJointExists = firstJoint != null;
		bool secondJointExists = secondJoint != null;
		if (firstJointExists && secondJointExists) ReadjustLength();
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public void Simulate(double delta)
	{
		// Calculate stress
		stress = (actualLength - desiredLength) / StressRange;

		// Do not simulate if parent is paused (must go after queue redraw)
		if (!parent.simulationPaused) {
			for (int iteration = 0; iteration < SimulationIterations; iteration++) {
				// Get connection values
				Vector2 center = GetCenterPosition();
				Vector2 direction = (firstJoint.position - secondJoint.position).Normalized();

				// Simulate first node
				if (!firstJoint.isFixed)
					firstJoint.position = center + direction * desiredLength / 2;

				// Simulate second node
				if (!secondJoint.isFixed)
					secondJoint.position = center - direction * desiredLength / 2;
			}
		}
	}

	// Checks if a point collides with it
	public bool CollidesWithPoint(Vector2 pointPosition, float tolerance) {
		// Get distances from mouse to joints
		float distanceFromFirstJoint = pointPosition.DistanceTo(firstJoint.position);
		float distanceFromSecondJoint = pointPosition.DistanceTo(secondJoint.position);

		// Calculate some distances
		float distanceBetweenJoints = actualLength;
		float distanceFromConnection = distanceFromFirstJoint + distanceFromSecondJoint;

		// Check collision
		bool collided =
			distanceFromConnection >= distanceBetweenJoints - tolerance
			&& distanceFromConnection <= distanceBetweenJoints + tolerance;

		// Return collision
		return collided;
	}

	// Checks if a line collides with it
	public bool CollidesWithLine(Vector2 lineStart, Vector2 lineEnd) {
		// I don't really know what any of this does
		// I got it from the internet

		float denominator = ((secondJoint.position.X - firstJoint.position.X) * (lineEnd.Y - lineStart.Y)) - ((secondJoint.position.Y - firstJoint.position.Y) * (lineEnd.X - lineStart.X));
		float firstNumerator = ((firstJoint.position.Y - lineStart.Y) * (lineEnd.X - lineStart.X)) - ((firstJoint.position.X - lineStart.X) * (lineEnd.Y - lineStart.Y));
		float secondNumerator = ((firstJoint.position.Y - lineStart.Y) * (secondJoint.position.X - firstJoint.position.X)) - ((firstJoint.position.X - lineStart.X) * (secondJoint.position.Y - firstJoint.position.Y));
		
		// Like what do r and s stand for?
		// Dude, label your variables

		float r = firstNumerator / denominator;
		float s = secondNumerator / denominator;
		
		return (r >= 0 && r <= 1) && (s >= 0 && s <= 1);
	}

	// Re-adjust length
	public void ReadjustLength() {
		desiredLength = (firstJoint.position - secondJoint.position).Length();
	}

	// Get center position
	public Vector2 GetCenterPosition() {
		return (firstJoint.position + secondJoint.position) / 2;
	}
}
