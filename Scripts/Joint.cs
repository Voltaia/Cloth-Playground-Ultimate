// Dependencies
using Godot;
using System;
using System.Collections.Generic;

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

	// Constructor
	public Joint(Cloth parent, Vector2 position, bool isFixed) {
		// Set up
		this.parent = parent;
		Position = position;
		previousPosition = position;
		this._isFixed = isFixed;
		Name = "Joint";
		ZIndex = -1;
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public void Simulate(double delta)
	{
		// Don't simulate if fixed or paused
		if (isFixed || parent.simulationPaused) return;

		// Movement
		Vector2 velocity = Position - previousPosition;
		previousPosition = Position;
		Position = Position + velocity;
		Position = Position + Vector2.Down * PlaygroundController.gravity;
	}

	// Draw
	public override void _Draw()
	{
		if (!parent.drawJoints) return;
		if (isFixed) DrawCircle(Vector2.Zero, parent.jointRadius, Palette.fixedJointColor);
		else DrawCircle(Vector2.Zero, parent.jointRadius, Palette.jointColor);
	}

	// Collides with point
	public bool CollidesWithPoint(Vector2 pointPosition) {
		float distance = pointPosition.DistanceTo(Position);
		return distance < parent.jointRadius;
	}

	// Collides with circle
	public bool CollidesWithCircle(Vector2 circlePosition, float circleRadius) {
		float distance = circlePosition.DistanceTo(Position);
		return distance < parent.jointRadius + circleRadius;
	}
}
