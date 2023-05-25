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
		Monochrome,
		Forest,
		Candy,
		Sunset
	}

	// Set palette
	public static void Set(Theme theme) {
		switch (theme) {
			default:
				backgroundColor = new Color("060608"); // Darkest
				fixedJointColor = new Color("143464"); // Dark
				jointColor = new Color("285cc4"); // Light
				connectionColor = new Color("6d758d"); // Lightest
				break;
			
			case Theme.Crimson:
				backgroundColor = new Color("090A0E");
				fixedJointColor = new Color("2f040d");
				jointColor = new Color("880C25");
				connectionColor = new Color("F05C79");
				break;

			case Theme.Monochrome:
				backgroundColor = new Color("000000");
				fixedJointColor = new Color("222222");
				jointColor = new Color("666666");
				connectionColor = new Color("cccccc");
				break;

			case Theme.Forest:
				backgroundColor = new Color("193829");
				fixedJointColor = new Color("1a7a3e");
				jointColor = new Color("14a02e");
				connectionColor = new Color("bb7547");
				break;
			
			case Theme.Candy:
				backgroundColor = new Color("242234");
				fixedJointColor = new Color("793a80");
				jointColor = new Color("e86a73");
				connectionColor = new Color("fad6b8");
				break;

			case Theme.Sunset:
				backgroundColor = new Color("3e2731");
				fixedJointColor = new Color("b86f50");
				jointColor = new Color("e4a672");
				connectionColor = new Color("ead4aa");
				break;
		}
	}

	// Set random
	public static int SetRandom() {
		RandomNumberGenerator rng = new RandomNumberGenerator();
		int themeIndex = rng.RandiRange(0, Enum.GetNames(typeof(Theme)).Length - 1);
		Set((Theme)themeIndex);
		return themeIndex;
	}
}