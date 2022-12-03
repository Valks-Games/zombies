global using Godot;
global using System;
global using System.Collections.Generic;
global using System.Collections.Concurrent;
global using System.Diagnostics;
global using System.Runtime.CompilerServices;
global using System.Threading;
global using System.Text.RegularExpressions;
global using System.Threading.Tasks;
global using System.Linq;

namespace Zombies;

public partial class Root : Node
{
	private bool Fullscreen { get; set; }

	public override void _Ready()
	{
		Input.MouseMode = Input.MouseModeEnum.Captured;
	}

	public override void _PhysicsProcess(double delta)
	{
		if (Input.IsActionJustPressed("ui_cancel"))
			Input.MouseMode = Input.MouseModeEnum.Visible;
	}

	public override void _Input(InputEvent @event)
	{
		if (Input.IsActionJustPressed("fullscreen"))
		{
			Fullscreen = !Fullscreen;

			if (Fullscreen)
				DisplayServer.WindowSetMode(DisplayServer.WindowMode.Fullscreen);
			else
				DisplayServer.WindowSetMode(DisplayServer.WindowMode.Windowed);
		}

		InputEventMouseButton(@event);
	}

	private void InputEventMouseButton(InputEvent @event) 
	{
		if (@event is not InputEventMouseButton button)
			return;

		if (button.ButtonIndex == MouseButton.Left && Input.MouseMode == Input.MouseModeEnum.Visible)
			Input.MouseMode = Input.MouseModeEnum.Captured;
	}
}
