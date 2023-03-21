// Dependencies
using Godot;
using System;

// Generates and holds values for the cloth
public partial class ClothController : Node
{
	// Inspector
	[Export] public ClothEditor clothEditor;

	// Variables
	public Cloth cloth;

	// On start
	public override void _Ready()
	{
		GenerateNewCloth();
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
