using Godot;
using Stage;
using System;
using System.Linq;

public partial class MovingHeads : Node3D
{
	private const float Speed = 0.25F;

	private bool running;
	private float progress;

	private static readonly Quaternion BeginningRotation = new(Vector3.Right, Mathf.Pi / 180 * 160);;
	private static readonly Quaternion EndingRotation = new(Vector3.Right, Mathf.Pi / 180 * 20);

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
	{

	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		if (Input.IsKeyPressed(Key.Q))
		{
			running = true;
			progress = 0;
		}

		if (running)
		{
			progress += (float)delta * Speed;
			float actualProgress = Mathf.Clamp((float)Easing.EaseInOutCubic(progress), 0, 1);

			Quaternion rotation = BeginningRotation.Slerp(EndingRotation, actualProgress);

			foreach (Node3D child in GetChildren().OfType<Node3D>())
			{
				child.Quaternion = rotation;
			}
		}
	}
}
