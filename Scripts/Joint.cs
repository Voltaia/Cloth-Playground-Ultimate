// Dependencies
using Godot;
using System;

// Cloth joint
public partial class Joint : Node2D
{
	// Variables
	public Vector2 previousPosition;
	public Cloth parent;
	private bool _isFixed;
	public bool isFixed {
		get { return _isFixed; }
		set {
			_isFixed = value;
			previousPosition = Position;
			QueueRedraw();
		}
	}

	// Settings
	public const float Radius = 10.0f;
	private const float Gravity = 0.25f;

	// Constructor
	public Joint(Cloth parent, Vector2 position, bool isFixed) {
		// Set up
		this.parent = parent;
		Position = position;
		previousPosition = position;
		this._isFixed = isFixed;
		Name = "Joint";
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		// Don't simulate if fixed or paused
		if (isFixed || parent.simulationPaused) return;

		// Movement
		Vector2 velocity = Position - previousPosition;
		previousPosition = new Vector2(Position.X, Position.Y);
		Position = Position + velocity;
		Position = Position + Vector2.Down * Gravity;
	}

	// Draw
	public override void _Draw()
	{
		if (isFixed) DrawCircle(Vector2.Zero, Radius, Colors.DarkBlue);
		else DrawCircle(Vector2.Zero, Radius, Colors.Blue);
	}

	// Colides with point
	public bool CollidesWithPoint(Vector2 pointPosition) {
		float distance = pointPosition.DistanceTo(Position);
		return distance < Radius;
	}
}
