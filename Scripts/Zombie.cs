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
		var player = Player.Instance;

		var targetPos = new Vector3(player.Position.x,
			Position.y, // lock zombie y rotation
			player.Position.z);

		if (Player.Instance != null)
			LookAt(Position - targetPos, Vector3.Up);
	}
}
