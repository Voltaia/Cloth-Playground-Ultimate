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
		Default
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
		}
	}
}