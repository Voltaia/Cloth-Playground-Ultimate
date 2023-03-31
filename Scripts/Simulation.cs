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
		Palette.Set(Palette.Theme.Default);
	}

	// Process
	public override void _Process(double delta)
	{
		MousePosition = GetViewport().GetMousePosition();
	}
}
