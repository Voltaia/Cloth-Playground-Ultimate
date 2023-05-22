// Dependencies
using Godot;
using System;

// Stuffs
public partial class Simulation : Node
{
	// Variables
	public static Vector2 MousePosition;

	// Process
	public override void _Process(double delta)
	{
		MousePosition = GetViewport().GetMousePosition();
	}
}
