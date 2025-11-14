using Godot;

public partial class StatusBar : Node
{
    Main main;
    public TextureRect texMicro;
    public Label lblComputer;
    public TextureRect texAY;
    public VSeparator sepAY;
    public Label lblFPS1;

    public override void _Ready()
    {
        texAY = GetNode<TextureRect>("PanelContainer/HBoxContainer/texAY");
        sepAY = GetNode<VSeparator>("PanelContainer/HBoxContainer/sepAY");
        lblFPS1 = GetNode<Label>("PanelContainer/HBoxContainer/lblFPS1");

        lblFPS1.Text = "50";
    }

    public void setAY(bool bOn)
    {
        texAY.Visible = bOn;
        sepAY.Visible = bOn;
    }

    public void setFPS(int iSpeed)
    {
        lblFPS1.Text = iSpeed.ToString();
    }

    public void setStereo(int stereoMode)
    {
        switch(stereoMode)
        {
            case 0: // StereoMode.Mono:
                texAY.TooltipText = "AY Mono";
                break;
            case 1: // StereoMode.StereoABC:
                texAY.TooltipText = "AY Stereo ABC";
                break;
            case 2: // StereoMode.StereoACB:
                texAY.TooltipText = "AY Stereo ACB";
                break;
            default:
                break;
        };
    }
}
