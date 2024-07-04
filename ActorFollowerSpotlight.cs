using Godot;
using System;

public partial class ActorFollowerSpotlight : SpotLight3D
{
	const float HalfLife = 0.1F;

	[Export] public MeshInstance3D Actor { get; set; }

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
        Vector3 pivot = Vector3.Forward;

		Vector3 target = Actor.Position + new Vector3(0, ((CapsuleMesh)Actor.Mesh).Height, 0);

		Vector3 direction = (target - Position).Normalized();
        Vector3 axis = pivot.Cross(direction).Normalized();
		float angle = Mathf.Acos(direction.Dot(pivot));

		Quaternion goal = new(axis, angle);

		// slerp smoothing
		// might be broken, don't know
		Quaternion = Quaternion.Slerp(goal, Mathf.Clamp(16F * (float)delta, 0, 1));
	}
}
