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

		// Draw path to connection under mouse
		if (clothEditor.editMode == EditMode.Create) {
			if (clothEditor.jointUnderMouse != null && !clothEditor.isDraggingPrimary) {
				DrawCircle(
					clothEditor.jointUnderMouse.Position,
					clothEditor.jointUnderMouse.parent.jointRadius * 1.5f,
					new Color(Colors.Green, ToolTransparency)
				);
			}
			else if (clothEditor.connectionUnderMouse != null)
			{
				DrawLine(
					Simulation.MousePosition,
					clothEditor.connectionUnderMouse.GetCenterPosition(),
					new Color(toolColor, ToolTransparency),
					2.5f
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
				if (!clothEditor.isDraggingPrimary) SetToolTip(createToolTip);
				else SetToolTip(createDragToolTip);
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

	// Initialize trail
	private void InitializeTrail() {
		for (int count = 1; count <= TrailLength; count++) destructionTrail.AddPoint(Simulation.MousePosition);
	}
}
