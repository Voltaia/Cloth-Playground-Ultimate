// Dependencies
using Godot;
using System;

// Controls the menu system, because I hate Godot's built in one
public partial class MenuGUI : Node2D
{
	// Variables
	private Menu mainMenu;
	private Menu currentMenu;
	private bool isActivated = false;

	// Settings
	private const float Width = 250.0f;
	private const int FontSize = 100;
	private const float ItemHeight = 100.0f;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		// Prepare menu contents
		mainMenu = new Menu("Pause Menu");
		//mainMenu.menuItems.Add(new MenuItem.Button("Resume"));

		// Set starting point
		currentMenu = mainMenu;
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{

	}

	// Draw
	public override void _Draw()
	{
		// Check to ensure menu is activated
		if (!isActivated) return;

		// Get menu height
		float height = ItemHeight + mainMenu.menuItems.Count * ItemHeight;

		// Get font stuffs
		Font font = new Label().GetThemeDefaultFont();
		float fontOffset = FontSize / 2.0f;

		// Draw background
		Vector2 screenSize = GetViewportRect().Size;
		Vector2 mainMenuPosition = new Vector2(screenSize.X / 2 - Width / 2, screenSize.Y / 2 - height / 2);
		Rect2 menuRect = new Rect2(mainMenuPosition, Width, height);
		DrawRect(menuRect, new Color(0.0f, 0.0f, 0.0f, 0.5f));

		// Draw title
		DrawString(
			font,
			mainMenuPosition + Vector2.Down * (ItemHeight / 2.0f) + Vector2.Down * fontOffset,
			currentMenu.name,
			HorizontalAlignment.Center,
			Width,
			FontSize,
			Colors.White
		);
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
