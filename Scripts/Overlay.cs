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
	[Export] public Control createDragToolTip;

	// General
	private Control currentToolTip;
	private Color cursorColor = Colors.Blue;
	private float cursorAlpha = 0.5f;

	// On start
	public override void _Ready() {
		currentToolTip = defaultToolTip;
	}

	// Every frame
	public override void _Process(double delta)
	{
		QueueRedraw();
	}

	// Draw
	public override void _Draw()
	{
		DrawCircle(Simulation.MousePosition, 3.5f, new Color(cursorColor, cursorAlpha));
	}

	// Set tool tip mode
	public void Update(ClothEditor.EditMode editMode, bool isDragging) {
		// Change tool tips
		switch (editMode) {
			case ClothEditor.EditMode.Create:
				cursorColor = Colors.Green;
				if (!isDragging) SetToolTip(createToolTip);
				else SetToolTip(createDragToolTip);
				break;

			case ClothEditor.EditMode.Destroy:
				cursorColor = Colors.Red;
				SetToolTip(destroyToolTip);
				break;

			default:
				cursorColor = Colors.Blue;
				SetToolTip(defaultToolTip);
				break;
		}

		// Change cursor alpha
		cursorAlpha = isDragging ? 1.0f : 0.25f;
	}

	// Set tool tip
	private void SetToolTip(Control toolTipToActivate) {
		currentToolTip.Visible = false;
		currentToolTip = toolTipToActivate;
		toolTipToActivate.Visible = true;
	}
}
