using static Godot.DisplayServer;
using System;
using MouseMode = Godot.Input.MouseModeEnum;
using ENet;

namespace Zombies;

public partial class Player : CharacterBody3D
{
	[Export] public float MouseSensitivity { get; set; } = 0.5f;
	[Export] public float MoveSpeed { get; set; } = 10;
	[Export] public float MoveDampening { get; set; } = 20; // the higher the value, the less the player will slide

	private Camera3D Camera { get; set; }

	public override void _Ready()
	{
		Camera = GetNode<Camera3D>("Camera3D");

		Input.MouseMode = MouseMode.Captured;
	}

	public override void _PhysicsProcess(double d)
	{
		var delta = (float)d;

		if (Input.IsActionJustPressed("ui_cancel"))
			Input.MouseMode = MouseMode.Visible;

		var h_rot = GlobalTransform.basis.GetEuler().y;

		var f_input = Input.GetActionStrength("move_backward") - Input.GetActionStrength("move_forward");
		var h_input = Input.GetActionStrength("move_right") - Input.GetActionStrength("move_left");

		// normalized to prevent "fast strafing movement" by holding down 2 movement keys at the same time
		// rotated to horizontal rotation to always move in the correct direction
		var dir = new Vector3(h_input, 0, f_input).Normalized().Rotated(Vector3.Up, h_rot);

		Velocity = Velocity.Lerp(dir * MoveSpeed, MoveDampening * delta);

		MoveAndSlide();
	}

	public override void _Input(InputEvent @event)
	{
		InputEventMouseMotion(@event);
		InputEventMouseButton(@event);
	}

	private void InputEventMouseMotion(InputEvent @event)
	{
		if (@event is not InputEventMouseMotion motion)
			return;

		if (Input.MouseMode != MouseMode.Captured)
			return;

		// rotate camera horizontally
		RotateY((-motion.Relative.x * MouseSensitivity).ToRadians());

		// rotate camera vertically
		Camera.RotateX((-motion.Relative.y * MouseSensitivity).ToRadians());
	}

	private void InputEventMouseButton(InputEvent @event)
	{
		if (@event is not InputEventMouseButton button)
			return;

		if (button.ButtonIndex == MouseButton.Left && Input.MouseMode == MouseMode.Visible)
			Input.MouseMode = MouseMode.Captured;
	}
}
