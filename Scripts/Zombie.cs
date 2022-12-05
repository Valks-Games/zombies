namespace Zombies;

public partial class Zombie : CharacterBody3D
{
	private AnimationPlayer AnimationPlayer { get; set; }
	private AnimationTree AnimationTree { get; set; }
	private NavigationAgent3D NavigationAgent3D { get; set; }

	private Godot.Timer Timer { get; set; }
	private bool ready = false;

	public override void _Ready()
	{
		//AnimationPlayer = GetNode<AnimationPlayer>("GLTF/AnimationPlayer");
		//AnimationPlayer.Play("walk");

		AnimationTree = GetNode<AnimationTree>("GLTF/AnimationTree");

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
	private float animationMotion = 0f;

	public override void _PhysicsProcess(double delta)
	{
		if (!ready)
			return;

		AnimationTree.Set("parameters/move/blend_position", animationMotion);

		var nextLocation = NavigationAgent3D.GetNextLocation();
		var newVelocity = (nextLocation - Position).Normalized();

		Geometry.CreateSphere(nextLocation, 0.2f, 2);

		var dir = nextLocation - Position;

		var angle = Mathf.Atan2(dir.x, dir.z);

		var rot = Rotation;
		rot.y = Mathf.LerpAngle(rot.y, angle, 0.01f);
		Rotation = rot;

		if (Rotation.y < prevRot.y + 0.01f && Rotation.y > prevRot.y - 0.01f)
		{
			animationMotion = Mathf.Lerp(animationMotion, 0.5f, 0.02f);
			Velocity = Velocity.MoveToward(newVelocity, 0.1f);
		}
		else
		{ 
			animationMotion = Mathf.Lerp(animationMotion, 0f, 0.02f);
			Velocity = Vector3.Zero;
		}

		prevRot = Rotation;

		MoveAndSlide();
	}
}
