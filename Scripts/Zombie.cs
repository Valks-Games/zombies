namespace Zombies;

public partial class Zombie : Node3D
{
	private AnimationPlayer AnimationPlayer { get; set; }

	public override void _Ready()
	{
		AnimationPlayer = GetNode<AnimationPlayer>("AnimationPlayer");
		AnimationPlayer.Play("walk");
	}

	public override void _PhysicsProcess(double delta)
	{
		if (Player.Instance != null)
			LookAt(Position - Player.Instance.Position, Vector3.Up);
	}
}
