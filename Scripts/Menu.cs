// Dependencies
using Godot;
using System;

// The menu system
public partial class Menu : Control
{
	// Inspector variables
	[Export] public Control pauseMenu;
	[Export] public Control pauseMenuFocus;
	[Export] public Control newPlaygroundMenu;
	[Export] public Control newPlaygroundMenuFocus;
	[Export] public Slider widthSlider;
	[Export] public Slider heightSlider;
	[Export] public Control newClothMenu;
	[Export] public Control newClothMenuFocus;

	// Class variables
	private Control currentMenu;
	private bool isPaused = false;

	// When started
	public override void _Ready()
	{
		Visible = false;
		pauseMenu.Visible = true;
		newPlaygroundMenu.Visible = false;
		newClothMenu.Visible = false;
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
				EnterPauseMenu();
			}
		}
	}

	// Menu navigations
	public void EnterPauseMenu()
	{ EnterMenu(pauseMenu, pauseMenuFocus); }

	public void EnterNewPlaygroundMenu()
	{ EnterMenu(newPlaygroundMenu, newPlaygroundMenuFocus); }

	public void EnterNewClothMenu()
	{ EnterMenu(newClothMenu, newClothMenuFocus); }

	// Enter a menu
	public void EnterMenu(Control menuToEnter, Control focus) {
		currentMenu.Visible = false;
		currentMenu = menuToEnter;
		menuToEnter.Visible = true;
		focus.GrabFocus();
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