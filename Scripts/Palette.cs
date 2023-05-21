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
		Marine,
		Crimson,
		Grayscale
	}

	// Set palette
	public static void Set(Theme theme) {
		switch (theme) {
			default:
				backgroundColor = new Color("060608");
				jointColor = new Color("285cc4");
				fixedJointColor = new Color("143464");
				connectionColor = new Color("6d758d");
				break;
			
			case Theme.Crimson:
				backgroundColor = new Color("090A0E");
				jointColor = new Color("880C25");
				fixedJointColor = new Color("2f040d");
				connectionColor = new Color("F05C79");
				break;

			case Theme.Grayscale:
				backgroundColor = new Color("000000");
				jointColor = new Color("666666");
				fixedJointColor = new Color("222222");
				connectionColor = new Color("cccccc");
				break;
		}
	}
}