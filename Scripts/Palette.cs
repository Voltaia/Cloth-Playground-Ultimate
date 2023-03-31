// Dependencies
using Godot;
using System;

// Color palette
public static class Palette
{
	// Colors
	public static Color backgroundColor;
	public static Color jointColor;
	public static Color fixedJointColor;
	public static Color connectionColor;

	// Palette variations
	public enum Theme {
		Classic,
		Marine
	}

	// Set palette
	public static void Set(Theme theme) {
		switch (theme) {
			default:
			backgroundColor = Colors.DimGray;
			jointColor = Colors.Blue;
			fixedJointColor = Colors.DarkBlue;
			connectionColor = Colors.LightBlue;
			break;

			case Theme.Marine:
			backgroundColor = new Color("060608");
			jointColor = new Color("285cc4");
			fixedJointColor = new Color("143464");
			connectionColor = new Color("6d758d");
			break;
		}
	}
}