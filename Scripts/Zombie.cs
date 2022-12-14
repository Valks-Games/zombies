namespace Zombies;

public partial class Zombie : CharacterBody3D
{
	private AnimationTree AnimationTree { get; set; }
	private NavigationAgent3D NavigationAgent3D { get; set; }
	private Skeleton3D Skeleton3D { get; set; }

	private Godot.Timer Timer { get; set; }
	private bool WeAreReady { get; set; }
	private Vector3 PrevRot { get; set; }
	private float AnimationMotion { get; set; } = 0f;

	public override void _Ready()
	{
		AnimationTree = GetNode<AnimationTree>("AnimationTree");

		NavigationAgent3D = GetNode<NavigationAgent3D>("NavigationAgent3D");

		Skeleton3D = GetNode<Skeleton3D>("Armature/Skeleton3D");

		Timer = new Godot.Timer();
		Timer.WaitTime = 1;
		Timer.Autostart = true;
		Timer.OneShot = false;
		Timer.Timeout += () => 
		{
			WeAreReady = true;
			NavigationAgent3D.TargetLocation = Player.Instance.Position;
		};
		AddChild(Timer);
		Timer.Start();
	}

	public override void _PhysicsProcess(double delta)
	{
		if (!WeAreReady)
			return;

		AnimationTree.Set("parameters/move/blend_position", AnimationMotion);

		var nextLocation = NavigationAgent3D.GetNextLocation();
		var newVelocity = (nextLocation - Position).Normalized();

		//Geometry.CreateSphere(nextLocation, 0.2f, 2);

		var dir = nextLocation - Position;

		var angle = Mathf.Atan2(dir.x, dir.z);

		var rot = Rotation;
		rot.y = Mathf.LerpAngle(rot.y, angle, 0.01f);
		Rotation = rot;

		if (
			!NavigationAgent3D.IsTargetReached() && 
			Rotation.y < PrevRot.y + 0.01f && Rotation.y > PrevRot.y - 0.01f
			)
		{
			AnimationMotion = Mathf.Lerp(AnimationMotion, 0.5f, 0.02f);
			NavigationAgent3D.SetVelocity(newVelocity);
		}
		else
		{ 
			AnimationMotion = Mathf.Lerp(AnimationMotion, 0f, 0.02f);
			NavigationAgent3D.SetVelocity(Vector3.Zero);
		}

		PrevRot = Rotation;
	}

	private void _on_navigation_agent_3d_velocity_computed(Vector3 safeVelocity)
	{
		Velocity = Velocity.MoveToward(safeVelocity, 0.1f);
		MoveAndSlide();
	}

	private void _on_navigation_agent_3d_target_reached()
	{
		
	}
}
