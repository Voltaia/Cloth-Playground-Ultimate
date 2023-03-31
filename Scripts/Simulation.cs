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
		Palette.Set((Palette.Theme)rng.RandiRange(0, Enum.GetNames(typeof(Palette.Theme)).Length));
		Palette.Set(Palette.Theme.Dark);
	}

	// Process
	public override void _Process(double delta)
	{
		MousePosition = GetViewport().GetMousePosition();
	}
}
