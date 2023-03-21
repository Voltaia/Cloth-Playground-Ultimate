// Dependencies
using Godot;
using System;

// Generates and holds values for the cloth
public partial class ClothController : Node2D
{
	// Inspector
	[Export] public ClothEditor clothEditor;

	// Variables
	public Cloth cloth;

	// Cloth sizes
	public enum ClothSize {
		Small,
		Medium,
		Large,
		Fullscreen
	}

	// On start
	public override void _Ready()
	{
		GenerateNewCloth();
	}

	// Every frame
	public override void _Process(double delta)
	{
		QueueRedraw();
	}

	// Draw
	public override void _Draw() {
		// Draw framerate
		DrawString(new Label().GetThemeDefaultFont(), new Vector2(5, 20), Engine.GetFramesPerSecond().ToString());
	}

	// Generate new
	public void GenerateNewCloth() {
		// Free up old cloth
		if (cloth != null) cloth.QueueFree();

		// Create new cloth
		cloth = new Cloth();
		AddChild(cloth);

		// Attach cloth editor
		clothEditor.cloth = cloth;
	}
}
