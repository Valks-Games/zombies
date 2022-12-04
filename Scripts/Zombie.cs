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

	public override void _PhysicsProcess(double delta)
	{
		if (!ready)
			return;

		var nextLocation = NavigationAgent3D.GetNextLocation();
		var newVelocity = (nextLocation - Position).Normalized();

		Geometry.CreateSphere(nextLocation, 0.2f, 2);

		var targetPos = new Vector3(nextLocation.x,
			Position.y, // lock zombie y rotation
			nextLocation.z);

		if (Position != targetPos)
			// not sure why this works for inverting LookAt but it just does
			LookAt(2 * Position - targetPos, Vector3.Up);

		Velocity = newVelocity;
		MoveAndSlide();
	}
}
