using static Godot.DisplayServer;
using System;
using MouseMode = Godot.Input.MouseModeEnum;
using ENet;

namespace Zombies;

public partial class Player : CharacterBody3D
{
	[Export] public float   MouseSensitivity         { get; set; } = 0.5f;
	[Export] public float   GravityForce             { get; set; } = 10;
	[Export] public float   JumpForce                { get; set; } = 150;
	[Export] public float   MoveSpeed                { get; set; } = 10;
	[Export] public float   MoveDampening            { get; set; } = 20; // the higher the value, the less the player will slide
											   
	private Camera3D        Camera                   { get; set; }
	private Camera3D        GunCam                   { get; set; }
	private RayCast3D       RayCast                  { get; set; }
	private Node3D          Gun                      { get; set; }
	private AnimationPlayer GunAnim                  { get; set; }
	private Vector3         GravityVec               { get; set; }

	[Export] public Vector2 Spray { get; set; } // the guns spray offset
	[Export] public float Recoil { get; set; }

	public override void _Ready()
	{
		Camera  = GetNode<Camera3D>       ("Camera3D");
		GunCam  = GetNode<Camera3D>       ("Camera3D/SubViewportContainer/SubViewport/Camera3D");
		Gun     = GetNode<Node3D>         ("Camera3D/Gun");
		GunAnim = GetNode<AnimationPlayer>("Camera3D/Gun/AnimationPlayer");
		RayCast = GetNode<RayCast3D>      ("Camera3D/RayCast");

		Input.MouseMode = MouseMode.Captured;
	}

	public override void _Process(double delta)
	{
		GunCam.GlobalTransform = Camera.GlobalTransform;
	}

	public override void _PhysicsProcess(double d)
	{
		var delta = (float)d;

		if (Input.IsActionJustPressed("ui_cancel"))
			Input.MouseMode = MouseMode.Visible;

		if (Input.IsActionPressed("shoot"))
		{
			RayCast.Rotation = Vector3.Zero;
			RayCast.RotateX(Math.RandomRange(-Spray.x * delta, Spray.x * delta));
			RayCast.RotateY(Math.RandomRange(-Spray.y * delta, Spray.y * delta));

			Camera.RotateX(Recoil * delta);

			if (RayCast.IsColliding())
			{
				// create a temporary sphere at the raycast collision point
				var sphere = new MeshInstance3D();

				sphere.Mesh = new SphereMesh();
				var sphereMesh = (SphereMesh)sphere.Mesh;

				var radius = 0.2f;

				sphereMesh.Radius = radius;
				sphereMesh.Height = sphereMesh.Radius * 2;

				sphere.Position = RayCast.GetCollisionPoint();

				var timer = GetTree().CreateTimer(2.0f);
				timer.Timeout += () => sphere.QueueFree();

				GetTree().Root.AddChild(sphere);
			}

			// play gun animation
			GunAnim.Play("fire");
		}

		var h_rot = GlobalTransform.basis.GetEuler().y;

		var f_input = Input.GetActionStrength("move_backward") - Input.GetActionStrength("move_forward");
		var h_input = Input.GetActionStrength("move_right") - Input.GetActionStrength("move_left");

		// normalized to prevent "fast strafing movement" by holding down 2 movement keys at the same time
		// rotated to horizontal rotation to always move in the correct direction
		var dir = new Vector3(h_input, 0, f_input).Rotated(Vector3.Up, h_rot).Normalized();

		if (IsOnFloor())
		{
			GravityVec = Vector3.Zero;

			if (Input.IsActionJustPressed("jump"))
			{
				GravityVec = Vector3.Up * JumpForce * delta;
			}
		}
		else
		{
			GravityVec += Vector3.Down * GravityForce * delta;
		}

		Velocity = Velocity.Lerp(dir * MoveSpeed, MoveDampening * delta);
		Velocity += GravityVec;

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

		// rotate player horizontally
		RotateY(-motion.Relative.x * MouseSensitivity);

		// rotate camera vertically
		Camera.RotateX(-motion.Relative.y * MouseSensitivity);

		// prevent camera from looking too far up or down
		var rotDeg = Camera.Rotation;
		rotDeg.x = Mathf.Clamp(rotDeg.x, -89f.ToRadians(), 89f.ToRadians());
		Camera.Rotation = rotDeg;
	}

	private void InputEventMouseButton(InputEvent @event)
	{
		if (@event is not InputEventMouseButton button)
			return;

		if (button.ButtonIndex == MouseButton.Left && Input.MouseMode == MouseMode.Visible)
			Input.MouseMode = MouseMode.Captured;
	}
}
