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
	public const float CursorRadius = 5.0f;

	// On start
	public override void _Ready() {
		// Set first tool tip
		currentToolTip = defaultToolTip;
	}

	// Every frame
	public override void _Process(double delta)
	{
		// Add to trail
		if (clothEditor.IsCutting) destructionTrail.AddPoint(Simulation.MousePosition);
		
		// Remove from trail
		int pointCount = destructionTrail.GetPointCount();
		if (
			pointCount > 0
			&& (pointCount > TrailLength
			|| !clothEditor.IsCutting)
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
			Vector2 positionToDrawTo = clothEditor.jointGrabbed == null ? clothEditor.jointUnderMouse.position : clothEditor.jointGrabbed.position;
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
				// Draw start connection indicator
				if (clothEditor.jointUnderMouse != null && clothEditor.connectionSelected == null) {
					DrawCircle(
						clothEditor.jointUnderMouse.position,
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
							clothEditor.connectionSelected.firstJoint.position,
							clothEditor.connectionSelected.secondJoint.position,
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
			} else {
				// Draw inserting new connection
				DrawLine(
					clothEditor.connectionInserting.firstJoint.position,
					Simulation.MousePosition,
					new Color(Colors.Green, ToolTransparency),
					Connection.DrawThickness
				);

				// Draw possible placements
				if (
					clothEditor.jointUnderMouse != null
					&& clothEditor.jointUnderMouse != clothEditor.connectionInserting.firstJoint
				) {
					DrawCircle(
						clothEditor.jointUnderMouse.position,
						clothEditor.jointUnderMouse.parent.jointRadius * 1.5f,
						new Color(Colors.Green, ToolTransparency)
					);
				}
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
				if (clothEditor.connectionSelected != null) {
					cursorAlpha = 1.0f;
					SetToolTip(createAdjustToolTip);
				}
				else if (clothEditor.connectionInserting != null) {
					cursorAlpha = 1.0f;
					SetToolTip(createDragToolTip);
				}
				else {
					cursorAlpha = ToolTransparency;
					SetToolTip(createToolTip);
				};
				break;

			case EditMode.Destroy:
				toolColor = Colors.Red;
				if (!clothEditor.IsCutting) {
					cursorAlpha = ToolTransparency;
					SetToolTip(destroyToolTip);
				}
				else {
					cursorAlpha = 1.0f;
					SetToolTip(destroyDragToolTip);
					InitializeTrail();
				}
				break;

			default:
				toolColor = Colors.Blue;
				if (clothEditor.jointGrabbed == null) {
					cursorAlpha = ToolTransparency;
					SetToolTip(defaultToolTip);
				}
				else {
					cursorAlpha = 1.0f;
					SetToolTip(defaultDragToolTip);
				};
				break;
		}
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
