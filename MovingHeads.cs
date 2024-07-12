using Godot;
using Stage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

public partial class MovingHeads : Node3D
{
    private delegate Quaternion IndividualHeadRotation(int headCount, int index, Quaternion rotation);

    private readonly record struct Configuration(Quaternion CenterRotation, IndividualHeadRotation IndividualHeadRotation)
    {
        public Quaternion GetIndividualHeadRotation(int headCount, int index)
        {
            return IndividualHeadRotation(headCount, index, CenterRotation);
        }
    }

	private const float Speed = 0.25F;

    private MovingHeads()
    {
        Configs = new()
        {
            [Key.Y] = new Configuration(new Quaternion(Vector3.Right, Mathf.Pi / 180 * 160), AlignHeadOutwards),
            [Key.X] = new Configuration(new Quaternion(Vector3.Right, Mathf.Pi / 180 * 20), AlignHeadOutwards),
            [Key.C] = new Configuration(new Quaternion(Vector3.Right, Mathf.Pi / 180 * 160), AlignHeadInwards),
            [Key.V] = new Configuration(new Quaternion(Vector3.Right, Mathf.Pi / 180 * 20), AlignHeadInwards),
            [Key.B] = new Configuration(new Quaternion(Vector3.Right, Mathf.Pi / 180 * 160), CrossHeadUp),
            [Key.N] = new Configuration(new Quaternion(Vector3.Right, Mathf.Pi / 180 * 20), CrossHeadDown),
            [Key.M] = new Configuration(Quaternion.Identity, InitialRotation),
            [Key.Comma] = new Configuration(Quaternion.Identity, AlignToCenterOfStage),
        };
    }

	private bool running;
    private bool rotating;
	private Configuration currentConfiguration;
	private Configuration nextConfiguration;
	private float progress;

    private Quaternion[] headRotations;

    private readonly Dictionary<Key, Configuration> Configs;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        currentConfiguration = Configs[Key.Y];
        nextConfiguration = currentConfiguration;
        AlignAllHeads(Configs[Key.Y]);
    }

    public override void _Input(InputEvent @event)
    {
        if (@event is InputEventKey { Keycode: Key key, Pressed: true })
		{
			if (!Configs.ContainsKey(key))
			{
				return;
			}

            running = true;
            rotating = false;
            headRotations = GetChildren().OfType<Node3D>().Select(c => c.Quaternion).ToArray();
            currentConfiguration = nextConfiguration;
            nextConfiguration = Configs[key];
            progress = 0;
        }

        //if (@event is InputEventMouseButton { ButtonIndex: (MouseButton.WheelUp or MouseButton.WheelDown) and MouseButton button })
        //{
        //    if (button is MouseButton.WheelUp)
        //    {
        //        Speed += 0.25F;
        //    }
        //    else
        //    {
        //        Speed -= 0.25F;
        //    }
        //}
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
	{
        Quaternion startRotation = new(Vector3.Right, Mathf.Pi / 4);

        if (running && nextConfiguration == Configs[Key.M])
        {
            running = false;
            rotating = true;
        }

        if (rotating)
        {
            progress += (float)delta * Speed;

            int i = 0;
            int headCount = GetChildren().OfType<Node3D>().Count();

            foreach (Node3D child in GetChildren().OfType<Node3D>())
            {
                float offset = Mathf.Tau / headCount * 2 * i;
                child.Quaternion = Rotators.IntegrateOntoRotate(headRotations[i], progress, speed: 2, offset, axis: Vector3.Forward, startRotation);
                i++;
            }
        }

        if (running)
        {
            progress += (float)delta * Speed;
            float actualProgress = Mathf.Clamp((float)Easing.EaseInOutCubic(progress), 0, 1);

            int i = 0;
            int headCount = GetChildren().OfType<Node3D>().Count();

            foreach (Node3D child in GetChildren().OfType<Node3D>())
            {
                child.Quaternion = headRotations[i].Slerp(nextConfiguration.GetIndividualHeadRotation(headCount, i), actualProgress);

                i++;
            }

            if (actualProgress >= 1)
            {
                running = false;
                //progress = 0;
            }
        }
    }

    private Quaternion AlignToCenterOfStage(int headCount, int index, Quaternion rotation)
    {
        Vector3 pivot = Vector3.Up;

        Vector3 target = Vector3.Zero;

        Vector3 direction = (target - ((Node3D)GetChildren()[index]).Position).Normalized();
        Vector3 axis = pivot.Cross(direction).Normalized();
        float angle = Mathf.Acos(direction.Dot(pivot));

        return new Quaternion(axis, angle);
    }

    private static Quaternion InitialRotation(int headCount, int index, Quaternion _)
    {
        Quaternion start = new(Vector3.Right, Mathf.Pi / 4);
        const float progress = 0;
        Quaternion rotation = new(Vector3.Forward, (progress + Mathf.Tau / headCount * 2 * index) /** Mathf.Pow(-1, i)*/);
        return rotation * start;
    }

    private static Quaternion AlignHeadOutwards(int headCount, int index, Quaternion rotation)
    {
        const float OuterTilt = Mathf.Pi / 180 * 10;

        // we calculate the head's own rotation
        // i = 0 is the leftmost head, so it should get a value of -1 * OuterTilt
        // i = headCount - 1 is the rightmost head, so it should get an angle of 1 * OuterTilt
        // i = (headCount - 1) / 2 is the center head (if it exists), so it should get an angle of 0 * OuterTilt
        Quaternion headIntrinsicRotation = new(Vector3.Forward, (index - ((headCount - 1) / 2.0F)) / (headCount - 1) * 2.0F * OuterTilt);

        return rotation * headIntrinsicRotation;
    }

    private static Quaternion AlignHeadInwards(int headCount, int index, Quaternion rotation)
    {
        const float InnerTilt = Mathf.Pi / 180 * -20;

        // we calculate the head's own rotation
        // i = 0 is the leftmost head, so it should get a value of -1 * OuterTilt
        // i = headCount - 1 is the rightmost head, so it should get an angle of 1 * OuterTilt
        // i = (headCount - 1) / 2 is the center head (if it exists), so it should get an angle of 0 * OuterTilt
        Quaternion headIntrinsicRotation = new(Vector3.Forward, (index - ((headCount - 1) / 2.0F)) / (headCount - 1) * 2.0F * InnerTilt);

        return rotation * headIntrinsicRotation;
    }

    private static Quaternion CrossHeadUp(int headCount, int index, Quaternion rotation)
    {
        float newIndex = (index - ((headCount - 1) / 2.0F)) / (headCount - 1) * 2.0F;

        Quaternion headIntrinsicRotation;

        if (newIndex < 0)
        {
            headIntrinsicRotation = new Quaternion(Vector3.Forward, 15);
        }
        else if (newIndex > 0)
        {
            headIntrinsicRotation = new Quaternion(Vector3.Forward, -15);
        }
        else
        {
            headIntrinsicRotation = Quaternion.Identity;
        }

        return rotation * headIntrinsicRotation;
    }

    private static Quaternion CrossHeadDown(int headCount, int index, Quaternion rotation)
    {
        float newIndex = (index - ((headCount - 1) / 2.0F)) / (headCount - 1) * 2.0F;

        Quaternion headIntrinsicRotation;

        if (newIndex < 0)
        {
            headIntrinsicRotation = new Quaternion(Vector3.Forward, 15);
        }
        else if (newIndex > 0)
        {
            headIntrinsicRotation = new Quaternion(Vector3.Forward, -15);
        }
        else
        {
            headIntrinsicRotation = Quaternion.Identity;
        }

        return headIntrinsicRotation * rotation;
    }

    private void AlignAllHeads(Configuration config)
    {
        int i = 0;
        int headCount = GetChildren().OfType<Node3D>().Count();

        foreach (Node3D child in GetChildren().OfType<Node3D>())
        {
            child.Quaternion = config.GetIndividualHeadRotation(headCount, i);
            i++;
        }
    }
}
