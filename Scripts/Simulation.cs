// Dependencies
using Godot;
using System;

// Stuffs
public partial class Simulation : Node
{
	// Variables
	public static Vector2 MousePosition;
	public static Palette Palette;

	// Constructor
	public Simulation() {
		Palette = new DefaultPalette();
	}

	// Process
	public override void _Process(double delta)
	{
		MousePosition = GetViewport().GetMousePosition();
	}
}
