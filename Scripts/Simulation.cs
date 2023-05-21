// Dependencies
using Godot;
using System;

// Stuffs
public partial class Simulation : Node
{
	// Variables
	public static Vector2 MousePosition;

	// Constructor
	public Simulation() {
		RandomNumberGenerator rng = new RandomNumberGenerator();
		Palette.Set(Palette.Theme.Marine);
	}

	// Process
	public override void _Process(double delta)
	{
		MousePosition = GetViewport().GetMousePosition();
	}
}
