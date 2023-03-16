// Dependencies
using Godot;
using System;
using System.Collections.Generic;

// Data structure for menu stuffs
public class Menu
{
	// Variables
	public string name;
	public List<MenuItem> menuItems = new List<MenuItem>();

	// Constructor
	public Menu(string name) {
		this.name = name;
	}
}

// Data structure for a menu item
public interface MenuItem {
	// Menu button
	public class Button : MenuItem {
		// Variables
		private string name;

		// Constructor
		public Button(string name) {
			this.name = name;
		}
	}
}