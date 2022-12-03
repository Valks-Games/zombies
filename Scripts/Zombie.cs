namespace Zombies;

public partial class Zombie : Node
{
	private AnimationPlayer AnimationPlayer { get; set; }

	public override void _Ready()
	{
		AnimationPlayer = GetNode<AnimationPlayer>("AnimationPlayer");
		AnimationPlayer.Play("walk");
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
}
