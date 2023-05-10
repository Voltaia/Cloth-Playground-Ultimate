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

	// On start
	public override void _Ready() {
		currentToolTip = defaultToolTip;
	}

	// Set tool tip mode
	public void UpdateToolTip(ClothEditor.EditMode editMode, bool isDragging) {
		switch (editMode) {
			case ClothEditor.EditMode.Create:
				if (!isDragging) SetToolTip(createToolTip);
				else SetToolTip(createDragToolTip);
				break;

			case ClothEditor.EditMode.Destroy:
				SetToolTip(destroyToolTip);
				break;

			default:
				SetToolTip(defaultToolTip);
				break;
		}
	}

	// Set tool tip
	private void SetToolTip(Control toolTipToActivate) {
		currentToolTip.Visible = false;
		currentToolTip = toolTipToActivate;
		toolTipToActivate.Visible = true;
	}
}
