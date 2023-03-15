// Dependencies
using Godot;
using System;

// Controls the menu system, because I hate Godot's built in one
public partial class MainMenu : Node2D
{
	// Variables
	private bool isActivated = false;
	private float currentHeight = 250.0f;

	// Settings
	private const float WIDTH = 250.0f;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{

	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{

	}

	// Draw
	public override void _Draw()
	{
		if (isActivated) {
			Vector2 screenSize = GetViewportRect().Size;
			Vector2 mainMenuPosition = new Vector2(screenSize.X / 2 - WIDTH / 2, screenSize.Y / 2 - currentHeight / 2);
			Rect2 menuRect = new Rect2(mainMenuPosition, WIDTH, currentHeight);
			DrawRect(menuRect, new Color(0.0f, 0.0f, 0.0f, 0.5f));
		}
	}

	// Input
	public override void _Input(InputEvent @event)
	{
		// Check for toggle
		if (@event.IsActionReleased("Toggle Menu")) {
			isActivated = !isActivated;
			QueueRedraw();
		}
	}
}
