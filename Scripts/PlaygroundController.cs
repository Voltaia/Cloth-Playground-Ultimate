// Dependencies
using Godot;
using System;

// Generates and holds values for the cloth
public partial class PlaygroundController : Node2D
{
	// Inspector
	[Export] public ClothEditor clothEditor;
	[Export] public Menu menu;

	// General
	public Cloth cloth;
	public bool isFullscreen = false;
	public int width = 750;
	public int height = 750;
	public bool startEmpty = false;
	public bool visualizeStress = false;
	public static float gravity = DefaultGravity;
	private Cloth.GenerationSettings generationSettings = new Cloth.GenerationSettings();
	private bool showFPS = true;

	// Settings
	private const float DefaultGravity = 0.25f;

	// On start
	public override void _Ready()
	{
		GenerateNewCloth(false);
		RenderingServer.SetDefaultClearColor(Palette.backgroundColor);
	}

	// Every frame
	public override void _Process(double delta)
	{
		QueueRedraw();
	}

	// Draw
	public override void _Draw() {
		// Draw framerate
		if (showFPS) DrawString(new Label().GetThemeDefaultFont(), new Vector2(5, 20), "FPS: " + Engine.GetFramesPerSecond().ToString());
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

	// Set zero G
	public void SetZeroG(bool isZeroG) {
		if (isZeroG) gravity = 0.0f;
		else gravity = DefaultGravity;
	}

	// Set joint separation
	public void SetJointSeparation(int settingIndex) {
		generationSettings.jointSeparation
			= (Cloth.GenerationSettings.JointSeparation)settingIndex;
	}

	// Visualize stress
	public void VisualizeStress(bool visualizeStress) {
		this.visualizeStress = visualizeStress;
		cloth.visualizeStress = visualizeStress;
		cloth.RedrawConnections();
	}

	// Set start empty
	public void SetStartEmpty(bool startEmpty) {
		this.startEmpty = startEmpty;
	}

	// Toggle fps
	private void ToggleFPS(bool enabled) {
		showFPS = enabled;
	}

	// Set palette
	public void SetPalette(int index) {
		Palette.Set((Palette.Theme)index);
		cloth.RedrawConnections();
		cloth.RedrawJoints();
		RenderingServer.SetDefaultClearColor(Palette.backgroundColor);
	}

	// Create new playground
	public void NewWindow() {
		// Window settings
		if (isFullscreen) {
			DisplayServer.WindowSetMode(DisplayServer.WindowMode.Fullscreen);
		}
		else {
			DisplayServer.WindowSetMode(DisplayServer.WindowMode.Windowed);
			DisplayServer.WindowSetSize(new Vector2I(width, height));
		}

		// Generate new cloth
		GenerateNewCloth(startEmpty);
	}

	// Generate new cloth
	public void GenerateNewCloth(bool isEmpty) {
		// Clear cloth
		if (cloth != null) cloth.QueueFree();

		// Create new cloth
		if (!isEmpty) cloth = new Cloth(generationSettings);
		else cloth = new Cloth();
		cloth.visualizeStress = visualizeStress;
		AddChild(cloth);

		// Attach cloth editor
		clothEditor.cloth = cloth;
	}
}
