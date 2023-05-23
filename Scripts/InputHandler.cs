// Dependencies
using Godot;
using System;

// Handles input
public partial class InputHandler : Node
{
	// Inspector
	[Export] ClothEditor clothEditor;

	// General
	private bool isDraggingPrimary = false;

	// Input
	public override void _Input(InputEvent @event)
	{
		// Switch between edit modes
		if (@event.IsActionPressed("Destroy")) clothEditor.SetEditMode(EditMode.Destroy);
		else if (@event.IsActionPressed("Create")) clothEditor.SetEditMode(EditMode.Create);
		else if (
			(
				clothEditor.editMode == EditMode.Destroy
				&& @event.IsActionReleased("Destroy")
			)
			|| (
				clothEditor.editMode == EditMode.Create
				&& @event.IsActionReleased("Create")
			)
		) clothEditor.SetEditMode(EditMode.Default);

		// Handle primary mouse input
		if (@event.IsActionPressed("Primary Edit")) {
			// Mode actions
			if (clothEditor.editMode == EditMode.Default) clothEditor.AttemptGrabJoint();
			else if (clothEditor.editMode == EditMode.Create) clothEditor.AttemptInsertStart();
			else clothEditor.isCutting = true;

			// Update state
			isDraggingPrimary = true;
			clothEditor.overlay.Update();
		}
		else if (@event.IsActionReleased("Primary Edit"))
		{
			// Edit mode actions
			if (clothEditor.editMode == EditMode.Default) clothEditor.AttemptReleaseJoint();
			else if (clothEditor.editMode == EditMode.Create) clothEditor.AttemptInsertEnd();
			else clothEditor.isCutting = false;

			// Update state
			isDraggingPrimary = false;
			clothEditor.overlay.Update();
		}

		// Handle secondary mouse input
		if (@event.IsActionPressed("Secondary Edit")) {
			// Edit mode actions
			if (clothEditor.editMode == EditMode.Default) clothEditor.AttemptJointFlip();
			else if (clothEditor.editMode == EditMode.Create) {
				if (clothEditor.connectionInserting != null) clothEditor.AttemptInsertMiddle();
				else if (clothEditor.connectionSelected == null) clothEditor.AttemptSelectConnection();
			}
		}
		else if (@event.IsActionReleased("Secondary Edit"))
		{
			clothEditor.connectionSelected = null;
			clothEditor.overlay.Update();
		}

		// Handle tertiary mouse input
		if (clothEditor.editMode == EditMode.Create) {
			if (@event.IsActionPressed("Wheel Up")) clothEditor.AttemptConnectionExtend();
			else if (@event.IsActionPressed("Wheel Down")) clothEditor.AttemptConnectionShrink();
		}

		// Pause simulation
		if (@event.IsActionPressed("Pause Simulation")) clothEditor.ToggleSimulationPause();
	}
}
