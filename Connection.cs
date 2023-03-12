// Dependencies
using Godot;

// A cloth connection
public partial class Connection : Node
{
	// Variables
	public Joint firstJoint;
	public Joint secondJoint;
	public readonly float desiredLength;

	// Settings
	public const float drawThickness = 5.0f;
	private const int simulationIterations = 10;

	// Properties
	public float actualLength {
		get {return firstJoint.Position.DistanceTo(secondJoint.Position);}
	}

	// Constructor
	public Connection(Joint firstJoint, Joint secondJoint)
	{
		this.firstJoint = firstJoint;
		this.secondJoint = secondJoint;
		desiredLength = (firstJoint.Position - secondJoint.Position).Length();
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		for (int iteration = 0; iteration < simulationIterations; iteration++)
		{
			// Get connection values
			Vector2 center = (firstJoint.Position + secondJoint.Position) / 2;
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
