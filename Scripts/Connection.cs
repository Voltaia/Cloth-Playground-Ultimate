// Dependencies
using Godot;

// A cloth connection
public partial class Connection : Node2D
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
		get {return firstJoint.Position.DistanceTo(secondJoint.Position);}
	}

	// Constructor
	public Connection(Cloth parent, Joint firstJoint, Joint secondJoint)
	{
		// Set fields
		this.parent = parent;
		this.firstJoint = firstJoint;
		this.secondJoint = secondJoint;
		
		// Set node properties
		Name = "Connection";
		ZIndex = -2;

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

		// Redraw
		QueueRedraw();

		// Do not simulate if parent is paused (must go after queue redraw)
		if (!parent.simulationPaused) {
			for (int iteration = 0; iteration < SimulationIterations; iteration++) {
				// Get connection values
				Vector2 center = GetCenterPosition();
				Vector2 direction = (firstJoint.Position - secondJoint.Position).Normalized();

				// Simulate first node
				if (!firstJoint.isFixed)
					firstJoint.Position = center + direction * desiredLength / 2;

				// Simulate second node
				if (!secondJoint.isFixed)
					secondJoint.Position = center - direction * desiredLength / 2;
			}
		}
	}

	// Draw
	public override void _Draw() {
		// Color to draw
		Color color = Palette.connectionColor;
		if (parent.visualizeStress) {
			float stressClamped = Mathf.Clamp(stress, 0, 1);
			float inverseStressClamped = 1.0f - stress;
			color = new Color(stressClamped, inverseStressClamped, 0.0f);
		}
		
		// Draw self
		DrawLine(
			firstJoint.Position,
			secondJoint.Position,
			color,
			DrawThickness
		);
	}

	// Checks if a point collides with it
	public bool CollidesWithPoint(Vector2 pointPosition, float tolerance) {
		// Get distances from mouse to joints
		float distanceFromFirstJoint = pointPosition.DistanceTo(firstJoint.Position);
		float distanceFromSecondJoint = pointPosition.DistanceTo(secondJoint.Position);

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

		float denominator = ((secondJoint.Position.X - firstJoint.Position.X) * (lineEnd.Y - lineStart.Y)) - ((secondJoint.Position.Y - firstJoint.Position.Y) * (lineEnd.X - lineStart.X));
		float firstNumerator = ((firstJoint.Position.Y - lineStart.Y) * (lineEnd.X - lineStart.X)) - ((firstJoint.Position.X - lineStart.X) * (lineEnd.Y - lineStart.Y));
		float secondNumerator = ((firstJoint.Position.Y - lineStart.Y) * (secondJoint.Position.X - firstJoint.Position.X)) - ((firstJoint.Position.X - lineStart.X) * (secondJoint.Position.Y - firstJoint.Position.Y));
		
		// Like what do r and s stand for?
		// Dude, label your variables

		float r = firstNumerator / denominator;
		float s = secondNumerator / denominator;
		
		return (r >= 0 && r <= 1) && (s >= 0 && s <= 1);
	}

	// Re-adjust length
	public void ReadjustLength() {
		desiredLength = (firstJoint.Position - secondJoint.Position).Length();
	}

	// Get center position
	public Vector2 GetCenterPosition() {
		return (firstJoint.Position + secondJoint.Position) / 2;
	}
}
