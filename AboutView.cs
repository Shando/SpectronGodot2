using Godot;

public partial class AboutView : Control
{
    Button btnClose;

    public override void _Ready()
    {
        btnClose = GetNode<Button>("PanelContainer/VBoxContainer/HBoxContainer/btnClose");
        btnClose.Pressed += OnBtnClosePressed;
    }

    public void OnBtnClosePressed()
    {
        Visible = false;
    }

    public override void _Input(InputEvent @event)
    {
        if (@event.IsActionPressed("ui_cancel"))
        {
            Visible = false;
        }
    }
}
