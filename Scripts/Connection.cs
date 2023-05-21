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

	// Settings
	public const float DrawThickness = 5.0f;
	private const int SimulationIterations = 10;
	private const float StressVisualRange = 20.0f;

	// Properties
	public float actualLength {
		get {return firstJoint.Position.DistanceTo(secondJoint.Position);}
	}

	// Constructor
	public Connection(Cloth parent, Joint firstJoint, Joint secondJoint)
	{
		this.parent = parent;
		this.firstJoint = firstJoint;
		this.secondJoint = secondJoint;
		if (firstJoint != null && secondJoint != null) ReadjustLength();
		Name = "Connection";
		ZIndex = -2;
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public void Simulate(double delta)
	{
		// Redraw
		QueueRedraw();

		// Do not simulate if parent is paused (must go after queue redraw)
		if (parent.simulationPaused) return;

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

	// Draw
	public override void _Draw() {
		// Color to draw
		Color color = Palette.connectionColor;
		if (parent.visualizeStress) {
			float stress = (actualLength - desiredLength) / StressVisualRange;
			stress = Mathf.Clamp(stress, 0, 1);
			float inverseStress = 1.0f - stress;
			color = new Color(stress, inverseStress, 0.0f);
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

	// Re-adjust length
	public void ReadjustLength() {
		desiredLength = (firstJoint.Position - secondJoint.Position).Length();
	}

	// Get center position
	public Vector2 GetCenterPosition() {
		return (firstJoint.Position + secondJoint.Position) / 2;
	}
}
