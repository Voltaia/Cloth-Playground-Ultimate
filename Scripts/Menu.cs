// Dependencies
using Godot;
using System;

// The menu system
public partial class Menu : Control
{
	// Variables
	[Export] public Control pauseMenu;
	[Export] public Control pauseMenuFocus;
	[Export] public Control newPlaygroundMenu;
	[Export] public Control newPlaygroundMenuFocus;
	[Export] public Slider widthSlider;
	[Export] public Slider heightSlider;
	private Control currentMenu;
	private bool isPaused = false;

	// When started
	public override void _Ready()
	{
		Visible = false;
		pauseMenu.Visible = true;
		newPlaygroundMenu.Visible = false;
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
				if (isPaused) pauseMenuFocus.GrabFocus();
			}
			else if (currentMenu == newPlaygroundMenu) {
				LeaveNewClothMenu();
			}
		}
	}

	// Create new cloth
	public void EnterNewClothMenu() {
		currentMenu = newPlaygroundMenu;
		pauseMenu.Visible = false;
		newPlaygroundMenu.Visible = true;
		newPlaygroundMenuFocus.GrabFocus();
	}

	// Leave new cloth menu
	public void LeaveNewClothMenu() {
		newPlaygroundMenu.Visible = false;
		currentMenu = pauseMenu;
		pauseMenu.Visible = true;
		pauseMenuFocus.GrabFocus();
	}

	// Set window size options
	public void SetWindowedOptions(bool enabled) {
		widthSlider.Editable = enabled;
		heightSlider.Editable = enabled;
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