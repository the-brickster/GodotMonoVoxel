using Godot;
using System;

public class Test : WorldEnvironment
{
    private const string Text = "Hello from Mono!";

    public override void _Ready()
    {
        GD.Print(Text);
        GD.Print();
    }
}
