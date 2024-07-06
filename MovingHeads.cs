using Godot;
using Stage;
using System;
using System.Linq;

public partial class MovingHeads : Node3D
{
	private float Speed = 0.25F;

	private bool running;
	private int currentConfiguration;
	private int nextConfiguration;
	private float progress;


	private static readonly Quaternion[] Configurations =
	{
		new(Vector3.Right, Mathf.Pi / 180 * 160),
		new(Vector3.Right, Mathf.Pi / 180 * 20),
	};

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
		AlignAllHeads(Configurations[0]);
    }

    private void AlignAllHeads(Quaternion rotation)
    {
        int i = 0;
        int headCount = GetChildren().OfType<Node3D>().Count();

        foreach (Node3D child in GetChildren().OfType<Node3D>())
        {
            const float OuterTilt = Mathf.Pi / 180 * 10;

            // we calculate the head's own rotation
            // i = 0 is the leftmost head, so it should get a value of -1 * OuterTilt
            // i = headCount - 1 is the rightmost head, so it should get an angle of 1 * OuterTilt
            // i = (headCount - 1) / 2 is the center head (if it exists), so it should get an angle of 0 * OuterTilt
            Quaternion headIntrinsicRotation = new(Vector3.Forward, (i - ((headCount - 1) / 2.0F)) / (headCount - 1) * 2.0F * OuterTilt);

            child.Quaternion = rotation * headIntrinsicRotation;
            i++;
        }
    }

    public override void _Input(InputEvent @event)
    {
        if (@event is InputEventKey { Keycode: Key key, Pressed: true })
		{
			if (key is not (Key.Y or Key.X))
			{
				return;
			}

            running = true;
            currentConfiguration = nextConfiguration;
            nextConfiguration = key switch
			{
				Key.Y => 0,
				Key.X => 1,
				_ => -1,
			};
            progress = 0;
        }

        if (@event is InputEventMouseButton { ButtonIndex: (MouseButton.WheelUp or MouseButton.WheelDown) and MouseButton button })
        {
            if (button is MouseButton.WheelUp)
            {
                Speed += 0.25F;
            }
            else
            {
                Speed -= 0.25F;
            }
        }
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
	{
		Quaternion start = new(Vector3.Right, Mathf.Pi / 4);

		progress += (float)delta * 2 * Speed;

        int i = 0;
        int headCount = GetChildren().OfType<Node3D>().Count();

        foreach (Node3D child in GetChildren().OfType<Node3D>())
        {
            Quaternion rotation = new(Vector3.Forward, (progress + Mathf.Tau / headCount * 2 * i) /** Mathf.Pow(-1, i)*/);
            child.Quaternion = rotation * start;
            i++;
        }

        //if (running)
        //{
        //	progress += (float)delta * Speed;
        //	float actualProgress = Mathf.Clamp((float)Easing.EaseInOutCubic(progress), 0, 1);

        //	Quaternion rotation = Configurations[currentConfiguration].Slerp(Configurations[nextConfiguration], actualProgress);

        //	AlignAllHeads(rotation);

        //	if (actualProgress >= 1)
        //	{
        //		running = false;
        //	}
        //}
    }
}
