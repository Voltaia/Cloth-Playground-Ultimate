// Dependencies
using Godot;
using System;

// Overlay for information
public partial class Overlay : Control
{
	// Inspector variables
	[Export] public Control defaultToolTip;
	[Export] public Control createToolTip;
	[Export] public Control destroyToolTip;
	[Export] public Control defaultDragToolTip;
	[Export] public Control createDragToolTip;
	[Export] public Control destroyDragToolTip;
	[Export] public Line2D destructionTrail;
	[Export] public ClothEditor clothEditor;

	// General
	private Control currentToolTip;
	private Color toolColor = Colors.Blue;
	private float cursorAlpha = ToolTransparency;

	// Settings
	private const float ToolTransparency = 0.25f;

	// On start
	public override void _Ready() {
		currentToolTip = defaultToolTip;
	}

	// Every frame
	public override void _Process(double delta)
	{
		// Update trail
		if (clothEditor.editMode == ClothEditor.EditMode.Destroy) {

		}

		// Draw stuff this frame
		QueueRedraw();
	}

	// Draw
	public override void _Draw()
	{
		// Draw inserting new connection
		if (clothEditor.connectionInserting != null) {
			DrawLine(
				clothEditor.connectionInserting.firstJoint.Position,
				Simulation.MousePosition,
				new Color(Colors.Green, ToolTransparency),
				Connection.DrawThickness
			);
		}

		// Draw path to joint under mouse
		if (clothEditor.jointUnderMouse != null || clothEditor.jointGrabbed != null) {
			Vector2 positionToDrawTo = clothEditor.jointGrabbed == null ? clothEditor.jointUnderMouse.Position : clothEditor.jointGrabbed.Position;
			DrawLine(
				Simulation.MousePosition,
				positionToDrawTo,
				new Color(toolColor, ToolTransparency),
				2.5f
			);
		}
		
		// Draw cursor
		DrawCircle(Simulation.MousePosition, 3.5f, new Color(toolColor, cursorAlpha));
	}

	// Set tool tip mode
	public void Update() {
		// Change tool tips
		switch (clothEditor.editMode) {
			case ClothEditor.EditMode.Create:
				toolColor = Colors.Green;
				if (!clothEditor.isDraggingPrimary) SetToolTip(createToolTip);
				else SetToolTip(createDragToolTip);
				break;

			case ClothEditor.EditMode.Destroy:
				toolColor = Colors.Red;
				if (!clothEditor.isDraggingPrimary) SetToolTip(destroyToolTip);
				else SetToolTip(destroyDragToolTip);
				break;

			default:
				toolColor = Colors.Blue;
				if (!clothEditor.isDraggingPrimary) SetToolTip(defaultToolTip);
				else SetToolTip(defaultDragToolTip);
				break;
		}

		// Change cursor alpha
		cursorAlpha = clothEditor.isDraggingPrimary ? 1.0f : ToolTransparency;
	}

	// Set tool tip
	private void SetToolTip(Control toolTipToActivate) {
		currentToolTip.Visible = false;
		currentToolTip = toolTipToActivate;
		toolTipToActivate.Visible = true;
	}
}
