// Dependencies
using Godot;
using System;

// Generates and holds values for the cloth
public partial class PlaygroundController : Node2D
{
	// Inspector
	[Export] public ClothEditor clothEditor;
	[Export] public Menu menu;

	// Variables
	public Cloth cloth;
	public bool isFullscreen = false;
	public int width = 750;
	public int height = 750;
	private Cloth.GenerationSettings generationSettings = new Cloth.GenerationSettings();

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
		DrawString(new Label().GetThemeDefaultFont(), new Vector2(5, 20), "FPS: " + Engine.GetFramesPerSecond().ToString());
	}

	// Set fullscreen
	public void SetFullscreen(bool isFullscreen) {
		this.isFullscreen = isFullscreen;
		menu.SetWindowedOptions(!isFullscreen);
	}

	// Set width
	public void SetWidth(int width) {
		this.width = width;
	}

	// Set height
	public void SetHeight(int height) {
		this.height = height;
	}

	// Set rigid
	public void SetRigid(bool isRigid) {
		generationSettings.rigid = isRigid;
	}

	// Set joint separation
	public void SetJointSeparation(int settingIndex) {
		generationSettings.jointSeparation
			= (Cloth.GenerationSettings.JointSeparation)settingIndex;
	}

	// Visualize stress
	public void VisualizeStress(bool isEnabled) {
		cloth.visualizeStress = isEnabled;
		cloth.RedrawConnections();
	}

	// Create new playground
	public void NewPlayground() {
		// Window settings
		if (isFullscreen) {
			DisplayServer.WindowSetMode(DisplayServer.WindowMode.Fullscreen);
		}
		else {
			DisplayServer.WindowSetMode(DisplayServer.WindowMode.Windowed);
			DisplayServer.WindowSetSize(new Vector2I(width, height));
		}

		// Generate new cloth
		GenerateNewCloth();
	}

	// Generate new cloth
	public void GenerateNewCloth() {
		// Free up old cloth
		if (cloth != null) cloth.QueueFree();

		// Create new cloth
		cloth = new Cloth(generationSettings);
		AddChild(cloth);

		// Attach cloth editor
		clothEditor.cloth = cloth;
	}
}
