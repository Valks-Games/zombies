global using Godot;
global using GodotUtils;
global using System;
global using System.Linq;
global using System.Threading;

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
