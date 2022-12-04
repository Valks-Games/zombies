namespace Zombies;

public partial class Zombie : CharacterBody3D
{
	private AnimationPlayer AnimationPlayer { get; set; }
	private NavigationAgent3D NavigationAgent3D { get; set; }

	private Godot.Timer Timer { get; set; }
	private bool ready = false;

	public override void _Ready()
	{
		AnimationPlayer = GetNode<AnimationPlayer>("AnimationPlayer");
		AnimationPlayer.Play("walk");

		NavigationAgent3D = GetNode<NavigationAgent3D>("NavigationAgent3D");

		Timer = new Godot.Timer();
		Timer.WaitTime = 1;
		Timer.Autostart = true;
		Timer.OneShot = false;
		Timer.Timeout += () => 
		{
			ready = true;
			NavigationAgent3D.TargetLocation = Player.Instance.Position;
		};
		AddChild(Timer);
		Timer.Start();
	}

	private Vector3 prevRot;

	public override void _PhysicsProcess(double delta)
	{
		if (!ready)
			return;

		var nextLocation = NavigationAgent3D.GetNextLocation();
		var newVelocity = (nextLocation - Position).Normalized();

		Geometry.CreateSphere(nextLocation, 0.2f, 2);

		var dir = nextLocation - Position;

		var angle = Mathf.Atan2(dir.x, dir.z);

		var rot = Rotation;
		rot.y = Mathf.LerpAngle(rot.y, angle, 0.01f);
		Rotation = rot;

		if (Rotation.y < prevRot.y + 0.005f && Rotation.y > prevRot.y - 0.005f)
		{
			Velocity = newVelocity;
			MoveAndSlide();
		}

		

		prevRot = Rotation;
	}
}
