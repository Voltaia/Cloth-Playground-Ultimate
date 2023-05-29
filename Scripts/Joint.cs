// Dependencies
using Godot;
using System;
using System.Collections.Generic;

// Cloth joint
public partial class Joint
{
	// Variables
	public Vector2 position;
	public Vector2 previousPosition;
	public Cloth parent;
	public bool hasBeenRemoved = false;
	private bool _isFixed;
	public bool isFixed {
		get { return _isFixed; }
		set {
			_isFixed = value;
			previousPosition = position;
		}
	}

	// Constructor
	public Joint(Cloth parent, Vector2 position, bool isFixed) {
		// Set up
		this.parent = parent;
		this.position = position;
		previousPosition = position;
		this._isFixed = isFixed;
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public void Simulate(double delta)
	{
		// Don't simulate if fixed or paused
		if (isFixed || parent.simulationPaused) return;

		// Movement
		Vector2 velocity = position - previousPosition;
		previousPosition = position;
		position = position + velocity;
		position = position + Vector2.Down * PlaygroundController.gravity;
	}

	// Collides with point
	public bool CollidesWithPoint(Vector2 pointPosition) {
		float distance = pointPosition.DistanceTo(position);
		return distance < parent.jointRadius;
	}

	// Collides with circle
	public bool CollidesWithCircle(Vector2 circlePosition, float circleRadius) {
		float distance = circlePosition.DistanceTo(position);
		return distance < parent.jointRadius + circleRadius;
	}
}
