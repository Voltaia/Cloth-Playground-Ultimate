// Dependencies
using Godot;
using System;

// The menu system
public partial class Menu : Control
{
	// Variables
	[Export] public BaseButton startFocus;
	private bool isPaused;

	// When started
	public override void _Ready()
	{
		if (Visible) startFocus.GrabFocus();
		isPaused = Visible;
	}

	// Input
	public override void _UnhandledInput(InputEvent @event)
	{
		// Check for pause
		if (@event.IsActionPressed("Pause")) {
			isPaused = !isPaused;
			GetTree().Paused = isPaused;
			Visible = isPaused;
			if (Visible) startFocus.GrabFocus();
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