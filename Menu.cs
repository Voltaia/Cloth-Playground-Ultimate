// Dependencies
using Godot;
using System;

// The menu system
public partial class Menu : Control
{
	// Variables
	private bool isPaused = false;

	// Input
	public override void _UnhandledInput(InputEvent @event)
	{
		// Check for pause
		if (@event.IsActionPressed("Pause")) {
			isPaused = !isPaused;
			GetTree().Paused = isPaused;
			Visible = isPaused;
		}
	}

	// Resume
	public void Resume() {
		isPaused = false;
		Visible = false;
		GetTree().Paused = false;
	}

	// Quit
	public void Quit() {
		GetTree().Quit();
	}
}