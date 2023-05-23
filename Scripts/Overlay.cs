// Dependencies
using Godot;
using System;

// Overlay for information
public partial class Overlay : Control
{
	// Inspector variables
	[Export] public ClothEditor clothEditor;
	[Export] public Control toolTips;
	[Export] public Control defaultToolTip;
	[Export] public Control createToolTip;
	[Export] public Control destroyToolTip;
	[Export] public Control defaultDragToolTip;
	[Export] public Control createDragToolTip;
	[Export] public Control destroyDragToolTip;
	[Export] public Control createAdjustToolTip;
	[Export] public Line2D destructionTrail;
	[Export] public Label fpsLabel;

	// General
	private Control currentToolTip;
	private Color toolColor = Colors.Blue;
	private float cursorAlpha = ToolTransparency;

	// Settings
	private const float ToolTransparency = 0.5f;
	private const int TrailLength = 15;
	private const float CursorRadius = 5.0f;

	// On start
	public override void _Ready() {
		// Set first tool tip
		currentToolTip = defaultToolTip;
	}

	// Every frame
	public override void _Process(double delta)
	{
		// Add to trail
		if (
			clothEditor.editMode == EditMode.Destroy
			&& clothEditor.isDraggingPrimary
		) destructionTrail.AddPoint(Simulation.MousePosition);
		
		// Remove from trail
		int pointCount = destructionTrail.GetPointCount();
		if (
			pointCount > 0
			&& (pointCount > TrailLength
			|| !clothEditor.isDraggingPrimary)
		) destructionTrail.RemovePoint(0);

		// Update FPS
		fpsLabel.Text = "FPS: " + Engine.GetFramesPerSecond().ToString();

		// Draw stuff this frame
		QueueRedraw();
	}

	// Draw
	public override void _Draw()
	{
		// Draw path to joint under mouse for grabbing
		if (
			clothEditor.editMode == EditMode.Default
			&& (clothEditor.jointUnderMouse != null
			|| clothEditor.jointGrabbed != null)
		) {
			Vector2 positionToDrawTo = clothEditor.jointGrabbed == null ? clothEditor.jointUnderMouse.Position : clothEditor.jointGrabbed.Position;
			DrawLine(
				Simulation.MousePosition,
				positionToDrawTo,
				new Color(toolColor, ToolTransparency),
				2.5f
			);
		}

		// Draw create mode tools
		if (clothEditor.editMode == EditMode.Create) {
			// If a connection is being inserted, we do not want to draw anything
			if (clothEditor.connectionInserting == null) {
				// Draw new connection tool
				if (clothEditor.jointUnderMouse != null && clothEditor.connectionSelected == null) {
					DrawCircle(
						clothEditor.jointUnderMouse.Position,
						clothEditor.jointUnderMouse.parent.jointRadius * 1.5f,
						new Color(Colors.Green, ToolTransparency)
					);
				}
				
				// Draw cloth adjustment tools
				if (clothEditor.connectionUnderMouse != null || clothEditor.connectionSelected != null)
				{
					Vector2 attachedPosition;
					if (clothEditor.connectionSelected != null) {
						attachedPosition = clothEditor.connectionSelected.GetCenterPosition();
						DrawLine(
							clothEditor.connectionSelected.firstJoint.Position,
							clothEditor.connectionSelected.secondJoint.Position,
							new Color(toolColor, ToolTransparency),
							2.5f
						);
					} else {
						attachedPosition = clothEditor.connectionUnderMouse.GetCenterPosition();
					}
					DrawLine(
						Simulation.MousePosition,
						attachedPosition,
						new Color(toolColor, ToolTransparency),
						2.5f
					);
				}
			}

			// Draw inserting new connection
			if (clothEditor.connectionInserting != null) {
				DrawLine(
					clothEditor.connectionInserting.firstJoint.Position,
					Simulation.MousePosition,
					new Color(Colors.Green, ToolTransparency),
					Connection.DrawThickness
				);
			}
		}
		
		// Draw cursor
		DrawCircle(Simulation.MousePosition, CursorRadius, new Color(toolColor, cursorAlpha));
	}

	// Set tool tip mode
	public void Update() {
		// Change tool tips
		switch (clothEditor.editMode) {
			case EditMode.Create:
				toolColor = Colors.Green;
				if (clothEditor.connectionSelected != null) SetToolTip(createAdjustToolTip);
				else if (clothEditor.connectionInserting != null) SetToolTip(createDragToolTip);
				else SetToolTip(createToolTip);
				break;

			case EditMode.Destroy:
				toolColor = Colors.Red;
				if (!clothEditor.isDraggingPrimary) SetToolTip(destroyToolTip);
				else {
					SetToolTip(destroyDragToolTip);
					InitializeTrail();
				}
				break;

			default:
				toolColor = Colors.Blue;
				if (!clothEditor.isDraggingPrimary) SetToolTip(defaultToolTip);
				else if (clothEditor.jointGrabbed != null) SetToolTip(defaultDragToolTip);
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

	// Toggle fps
	private void ToggleFPS(bool enabled) {
		fpsLabel.Visible = enabled;
	}

	// Set tool tips
	private void ToggleToolTips(bool enabled) {
		toolTips.Visible = enabled;
	}

	// Initialize trail
	private void InitializeTrail() {
		for (int count = 1; count <= TrailLength; count++) destructionTrail.AddPoint(Simulation.MousePosition);
	}
}
