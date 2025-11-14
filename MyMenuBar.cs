using Godot;

public partial class MyMenuBar : MenuBar
{
    public PopupMenu mnuFile;
    public PopupMenu mnuEmulator;
    public PopupMenu mnuControl;
    public PopupMenu mnuHelp;

    [Signal]
    public delegate void MenuFileIdPressedEventHandler(long id);
    [Signal]
    public delegate void MenuEmulatorIdPressedEventHandler(long id);
    [Signal]
    public delegate void MenuControlIdPressedEventHandler(long id);
    [Signal]
    public delegate void MenuHelpIdPressedEventHandler(long id);

    public override void _Ready()
    {
        mnuFile = GetNode<PopupMenu>("mnuFile");
        mnuEmulator = GetNode<PopupMenu>("mnuEmulator");
        mnuControl = GetNode<PopupMenu>("mnuControl");
        mnuHelp = GetNode<PopupMenu>("mnuHelp");

        AddOpenFileItem();
        mnuFile.AddSeparator();
        AddExitFileItem();

        mnuEmulator.AddRadioCheckItem("Original");
        mnuEmulator.AddRadioCheckItem("Gosh Wonderful");
        mnuEmulator.AddRadioCheckItem("Busy Soft v1.40");
        mnuEmulator.AddRadioCheckItem("J.G. Harston v0.77");
        mnuEmulator.AddRadioCheckItem("H.T.R. SuperBasic");
        mnuEmulator.AddRadioCheckItem("Pretty BASIC");
        mnuEmulator.SetItemChecked(0, true);

        mnuControl.AddCheckItem("Fullscreen");
        AddFullScreenItem();
        mnuControl.SetItemChecked(0, false);
        mnuControl.AddSeparator();
        mnuControl.AddCheckItem("Pause", 2, Key.F2);
        mnuControl.SetItemChecked(2, false);
        mnuControl.AddSeparator();
        mnuControl.AddCheckItem("Mute");
        mnuControl.SetItemChecked(4, false);
        mnuControl.AddSeparator();
        AddResetFileItem();
        mnuControl.AddItem("Hard Reset");

        mnuHelp.AddItem("Keyboard", 0, Key.F1);
        mnuHelp.AddItem("About");

        mnuFile.IdPressed += OnMenuFileIdPressed;
        mnuEmulator.IdPressed += OnMenuEmulatorIdPressed;
        mnuControl.IdPressed += OnMenuControlIdPressed;
        mnuHelp.IdPressed += OnMenuHelpIdPressed;
    }
    public void AddOpenFileItem()
    {
        var openEvent = new InputEventKey
        {
            Keycode = Key.O,
            CtrlPressed = true,
            CommandOrControlAutoremap = true
        };

        var shortcut = new Shortcut
        {
            ResourceName = "Load",
            Events = (Godot.Collections.Array)new Godot.Collections.Array<InputEvent> { openEvent }
        };

        mnuFile.AddShortcut(shortcut, 0);
    }

    public void AddExitFileItem()
    {
        var openEvent = new InputEventKey
        {
            Keycode = Key.F4,
            CtrlPressed = true,
            CommandOrControlAutoremap = true
        };

        var shortcut = new Shortcut
        {
            ResourceName = "Exit",
            Events = (Godot.Collections.Array)new Godot.Collections.Array<InputEvent> { openEvent }
        };

        mnuFile.AddShortcut(shortcut, 7);
    }

    public void AddResetFileItem()
    {
        var openEvent = new InputEventKey
        {
            Keycode = Key.F5,
            CtrlPressed = true,
            CommandOrControlAutoremap = true
        };

        var shortcut = new Shortcut
        {
            ResourceName = "Reset",
            Events = (Godot.Collections.Array)new Godot.Collections.Array<InputEvent> { openEvent }
        };

        mnuControl.AddShortcut(shortcut, 6);
    }

    public void AddFullScreenItem()
    {
        var openEvent = new InputEventKey
        {
            Keycode = Key.F12,
            CtrlPressed = true,
            CommandOrControlAutoremap = true
        };

        var shortcut = new Shortcut
        {
            ResourceName = "Fullscreen",
            Events = (Godot.Collections.Array)new Godot.Collections.Array<InputEvent> { openEvent }
        };

        mnuControl.SetItemShortcut(0, shortcut);
    }

    public void OnMenuFileIdPressed(long id)
    {
        EmitSignal(SignalName.MenuFileIdPressed, id);
    }

    public void OnMenuEmulatorIdPressed(long id)
    {
        mnuEmulator.SetItemChecked(0, false);
        mnuEmulator.SetItemChecked(1, false);
        mnuEmulator.SetItemChecked(2, false);
        mnuEmulator.SetItemChecked(3, false);
        mnuEmulator.SetItemChecked(4, false);
        mnuEmulator.SetItemChecked(5, false);

        mnuEmulator.SetItemChecked((int)id, true);

        EmitSignal(SignalName.MenuEmulatorIdPressed, id);
    }

    public void OnMenuControlIdPressed(long id)
    {
        if (mnuControl.IsItemChecked((int)id))
            mnuControl.SetItemChecked((int)id, false);
        else
            mnuControl.SetItemChecked((int)id, true);

        EmitSignal(SignalName.MenuControlIdPressed, id);
    }

    public void OnMenuHelpIdPressed(long id)
    {
        EmitSignal(SignalName.MenuHelpIdPressed, id);
    }
}
