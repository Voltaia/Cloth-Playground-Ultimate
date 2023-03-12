// Dependencies
using Godot;
using System;

// Cloth joint
public partial class Joint : Node2D
{
	// Variables
	public Vector2 previousPosition;
	public bool isFixed;

	// Settings
	public const float radius = 10.0f;
	private const float gravity = 0.25f;

	// Constructor
	public Joint(Vector2 position, bool isFixed) {
		Position = position;
		previousPosition = position;
		this.isFixed = isFixed;
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		// If the joint is fixed, don't simulate
		if (isFixed) return;

		// Movement
		Vector2 velocity = Position - previousPosition;
		previousPosition = new Vector2(Position.X, Position.Y);
		Position = Position + velocity;
		Position = Position + Vector2.Down * gravity;
	}

	// Draw
	public override void _Draw()
	{
		if (isFixed) DrawCircle(Vector2.Zero, radius, Colors.DarkBlue);
		else DrawCircle(Vector2.Zero, radius, Colors.Blue);
	}
}
