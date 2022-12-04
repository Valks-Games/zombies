namespace Zombies;

public partial class Test : Node3D
{
	public override void _PhysicsProcess(double delta)
	{
		LookAt(Player.Instance.Position, Vector3.Up);
	}
}
