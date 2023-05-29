using Godot;

public partial class FixFocusHighlight : VBoxContainer
{
	public override void _Ready()
	{
		// Loop through children
		foreach (Node node in GetChildren()) {
			// Get UI element and skip if not a button
			Button button = node as Button;
			if (button == null) continue;

			// Connect focus entered
			button.FocusEntered += delegate() {
				Color focusColor = button.GetThemeColor("font_focus_color", "Button");
				if (button.HasThemeColorOverride("font_pressed_color")) {
					Color pressedColor = button.GetThemeColor("font_focus_color", "Button");
					button.SetMeta("font_pressed_color_to_restore", pressedColor);
				}
				button.AddThemeColorOverride("font_pressed_color", focusColor);
			};

			// Connect focus exited
			button.FocusExited += delegate() {
				if (button.HasMeta("font_pressed_color_to_restore")) {
					Color previouslyPressedColorOverride = (Color)button.GetMeta("font_pressed_color_to_restore");
					button.AddThemeColorOverride("font_pressed_color", previouslyPressedColorOverride);
					button.RemoveMeta("font_pressed_color_to_restore");
				} else {
					button.RemoveThemeColorOverride("font_pressed_color");
				}
			};
		}
	}
}
