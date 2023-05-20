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

	// General
	private Control currentToolTip;
	private Color cursorColor = Colors.Blue;
	private float cursorAlpha = 0.5f;
	private ClothEditor clothEditor = null;

	// On start
	public override void _Ready() {
		currentToolTip = defaultToolTip;
	}

	// Every frame
	public override void _Process(double delta)
	{
		// Update trail
		if (clothEditor != null && clothEditor.editMode == ClothEditor.EditMode.Destroy) {

		}

		// Draw stuff this frame
		QueueRedraw();
	}

	// Draw
	public override void _Draw()
	{
		DrawCircle(Simulation.MousePosition, 3.5f, new Color(cursorColor, cursorAlpha));
	}

	// Set tool tip mode
	public void Update(ClothEditor clothEditor) {
		// Update status
		this.clothEditor = clothEditor;

		// Change tool tips
		switch (clothEditor.editMode) {
			case ClothEditor.EditMode.Create:
				cursorColor = Colors.Green;
				if (!clothEditor.isDraggingPrimary) SetToolTip(createToolTip);
				else SetToolTip(createDragToolTip);
				break;

			case ClothEditor.EditMode.Destroy:
				cursorColor = Colors.Red;
				if (!clothEditor.isDraggingPrimary) SetToolTip(destroyToolTip);
				else SetToolTip(destroyDragToolTip);
				break;

			default:
				cursorColor = Colors.Blue;
				if (!clothEditor.isDraggingPrimary) SetToolTip(defaultToolTip);
				else SetToolTip(defaultDragToolTip);
				break;
		}

		// Change cursor alpha
		cursorAlpha = clothEditor.isDraggingPrimary ? 1.0f : 0.25f;
	}

	// Set tool tip
	private void SetToolTip(Control toolTipToActivate) {
		currentToolTip.Visible = false;
		currentToolTip = toolTipToActivate;
		toolTipToActivate.Visible = true;
	}
}
