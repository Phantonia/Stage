using Godot;
using Stage;
using System;

public partial class CraneCamera : PathFollow3D
{
	private const float Speed = 0.1F;

	[Export] Camera3D Camera { get; set; }
	[Export] Vector3 CenterOfStage { get; set; }

	private float progress = 0;
	private bool running = false;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{

	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		if (Input.IsKeyPressed(Key.Space))
		{
			running = true;
			progress = 0;
		}

		if (running)
		{
			progress += Speed * (float)delta;
			ProgressRatio = Math.Clamp((float)Easing.EaseInOutCubic(progress), 0.0F, 1.0F);

            if (progress >= 1)
			{
				running = false;
			}
		}

		Vector3 pivot = Vector3.Forward;

		Vector3 direction = (CenterOfStage - Position).Normalized();
		Vector3 axis = pivot.Cross(direction).Normalized();
		float angle = Mathf.Acos(direction.Dot(pivot));

		Quaternion goal = new(axis, angle);
		Camera.Quaternion = goal;
	}
}
