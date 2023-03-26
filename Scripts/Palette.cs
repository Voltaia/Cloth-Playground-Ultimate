// Dependencies
using Godot;
using System;

// Color palette
public class Palette
{
	public Color backgroundColor;
	public Color jointColor;
	public Color fixedJointColor;
	public Color connectionColor;
}

// Default
public class DefaultPalette : Palette
{
	public DefaultPalette() {
		backgroundColor = Colors.DimGray;
		jointColor = Colors.Blue;
		fixedJointColor = Colors.DarkBlue;
		connectionColor = Colors.LightBlue;
	}
}