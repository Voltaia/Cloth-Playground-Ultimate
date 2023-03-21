// Dependencies
using Godot;
using System;

// The menu system
public partial class Menu : Control
{
	// Variables
	[Export] public Control pauseMenu;
	[Export] public Control pauseMenuFocus;
	[Export] public Control newClothMenu;
	[Export] public Control newClothMenuFocus;
	private Control currentMenu;
	private bool isPaused;

	// When started
	public override void _Ready()
	{
		if (Visible) pauseMenuFocus.GrabFocus();
		isPaused = Visible;
		currentMenu = pauseMenu;
	}

	// Input
	public override void _UnhandledInput(InputEvent @event)
	{
		// Check for pause
		if (@event.IsActionPressed("Pause")) {
			if (currentMenu == pauseMenu) {
				isPaused = !isPaused;
				GetTree().Paused = isPaused;
				Visible = isPaused;
				if (Visible) pauseMenuFocus.GrabFocus();
			}
			else if (currentMenu == newClothMenu) {
				LeaveNewClothMenu();
			}
		}
	}

	// Create new cloth
	public void EnterNewClothMenu() {
		currentMenu = newClothMenu;
		pauseMenu.Visible = false;
		newClothMenu.Visible = true;
	}

	// Leave new cloth menu
	public void LeaveNewClothMenu() {
		newClothMenu.Visible = false;
		pauseMenu.Visible = true;
		currentMenu = pauseMenu;
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