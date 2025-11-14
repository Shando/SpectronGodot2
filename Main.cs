using Casper;
using Casper.Forms;
using Godot;
using static MyMenuBar;

public partial class Main : Node
{
    Window MainWindow;
    TextureRect outer;
    TextureRect screen;
    StatusBar statusBar;
    Label titleBar;
    AboutView aboutView;
    KeyboardView keyboardView;
    FileDialog tapeDialog;

    Spectrum spectrum;
    Sound sound;
    Image bitmapOuter;
    Image bitmapScreen;
    int joystick;

    private string _currentFileName;

    // TODO: Need code for these :
    public bool _IsAyEnabled = false;
    public int _StereoMode = 0;

    public bool _IsPaused = false;
    public bool _IsAudioMuted = false;
    public bool _IsFullScreen = false;
    private bool _IsPhysFinished = true;

    private int _ROMType = 0;

    private ImageTexture texOuter;
    private ImageTexture texScreen;

    private Label lblTitle;
    private bool bReady = false;

    public override void _Ready()
    {
        var mb = GetNode<MyMenuBar>("VBoxContainer/PanelContainer/MenuBar");
        mb.MenuFileIdPressed += OnMenuFileIdPressed;
        mb.MenuEmulatorIdPressed += OnMenuEmulatorIdPressed;
        mb.MenuControlIdPressed += OnMenuControlIdPressed;
        mb.MenuHelpIdPressed += OnMenuHelpIdPressed;

        outer = GetNode<TextureRect>("VBoxContainer/PanelContainer2/MarginContainer/outer");
        screen = GetNode<TextureRect>("VBoxContainer/PanelContainer2/MarginContainer/outer/screen");

        titleBar = GetNode<Label>("VBoxContainer/TitleBar/lblTitle");

        statusBar = GetNode<StatusBar>("VBoxContainer/statusBar");
        aboutView = GetNode<AboutView>("AboutView");
        keyboardView = GetNode<KeyboardView>("KeyboardView");
        tapeDialog = GetNode<FileDialog>("TapeDialog");

        _currentFileName = "";

        GetTree().AutoAcceptQuit = false;

        spectrum = new Spectrum();
        spectrum.Screen.RenderPixel += OnRenderPixel;
        spectrum.Border.RenderBorder += OnRenderBorder;
        
        sound = new Sound();
        AddChild(sound);
        spectrum.Speaker.Activate += sound.ActivateSpeaker;

        tapeDialog.FileSelected += LoadSelectedFile;

        bitmapOuter = Image.CreateEmpty(270, 216, false, Image.Format.Rgba8);
        bitmapOuter.Fill(Colors.Red);
        bitmapScreen = Image.CreateEmpty(256, 192, false, Image.Format.Rgba8);
 
        texOuter = ImageTexture.CreateFromImage(bitmapOuter);
        texScreen = ImageTexture.CreateFromImage(bitmapScreen);

        lblTitle = GetNode<Label>("VBoxContainer/TitleBar/lblTitle");
        lblTitle.Text = "Casper in Godot";

        InitializeController();

        string path = "res://Assets/Roms/48.rom";
        byte[] data = LoadFileAsBytes(path);

        if (data != null)
            spectrum.LoadROM(data);

        path = "res://Assets/Games/ManicMiner.tap";
        data = LoadFileAsBytes(path);
        _currentFileName = "ManicMiner.tap";

        if (data != null)
        {
            spectrum.Load(new Casper.FileFormats.Tap(data));
            UpdateWindowTitle();
        }

        bReady = true;
    }

    public void OnMenuFileIdPressed(long id)
    {
        switch (id)
        {
            case 0:
                LoadFile();
                break;
            case 2:
                ExitApplication();
                break;
            default:
                break;
        }
    }

    public void OnMenuEmulatorIdPressed(long id)
    {
        ChangeRomType((int)id);
    }

    public void OnMenuControlIdPressed(long id)
    {
        switch (id)
        {
            case 0:
                ToggleFullScreen();
                break;
            case 2:
                TogglePause();
                break;
            case 4:
                ToggleMute();
                break;
            case 6:
                Reset();
                break;
            case 7:
                HardReset();
                break;
            default:
                break;
        }
    }

    public void OnMenuHelpIdPressed(long id)
    {
        switch (id)
        {
            case 0:
                ShowKeyboardHelpView();
                break;
            case 1:
                ShowAboutView();
                break;
            default:
                break;
        }
    }

    public byte[] LoadFileAsBytes(string path)
    {
        // Check if the file exists before attempting to open
        if (!FileAccess.FileExists(path))
        {
            GD.PrintErr($"File not found: {path}");
            return null;
        }

        using var file = FileAccess.Open(path, FileAccess.ModeFlags.Read);
        if (file == null)
        {
            GD.PrintErr($"Failed to open file: {path}");
            return null;
        }

        // Get the entire file as a byte array
        byte[] data = file.GetBuffer((long)file.GetLength());

        return data;
    }

    public override void _Notification(int what)
    {
        if (what == NotificationWMCloseRequest)
        {
            GetTree().Quit(); // default behavior
        }
    }

    // File
    public void LoadFile()
    {
        tapeDialog.SetFilters(["*.sna ; ZX Spectrum Snapshot File", "*.tap ; ZX Spectrum Tap File", "*.z80 ; ZX Spectrum Z80 File"]);
        tapeDialog.Show();
    }

    public void LoadSelectedFile(string path)
    {
        if (path != null && path != "")
        {
            Reset();
            
            _currentFileName = path.GetFile();
            UpdateWindowTitle();

            byte[] data = LoadFileAsBytes(path);

            if (data != null)
            {
                if (path.GetExtension() == "tap")
                    spectrum.Load(new Casper.FileFormats.Tap(data));
                else
                {
                    string oldFileName = _currentFileName;
                    Reset();
                    _currentFileName = oldFileName;
                    UpdateWindowTitle();
                    spectrum.LoadSnapshot(data);
                }
            }
        }
    }

    public void ExitApplication() => GetTree().Quit();

    public void Reset()
    {
        spectrum.Reset();
        _IsPaused = false;
        _currentFileName = "";

        UpdateWindowTitle();
    }

    public void HardReset()
    {
        string path = "res://Assets/Roms/";

        switch (_ROMType)
        {
            case 0:
                path += "48.rom";
                break;
            case 1:
                path += "gw03.rom";
                break;
            case 2:
                path += "bs140.rom";
                break;
            case 3:
                path += "JGH077.rom";
                break;
            case 4:
                path += "HTR.rom";
                break;
            case 5:
                path += "pretty.rom";
                break;
        }

        byte[] data = LoadFileAsBytes(path);

        if (data != null)
            spectrum.LoadROM(data);

        spectrum.Reset();
    }

    // Emulator
    public void ChangeRomType(int id)
    {
        _ROMType = id;
        HardReset();
    }

    // View
    public void ToggleFullScreen()
    {
        _IsFullScreen = !_IsFullScreen;

        if (_IsFullScreen)
        {
            DisplayServer.WindowSetMode(DisplayServer.WindowMode.ExclusiveFullscreen);
        }
        else
        {
            DisplayServer.WindowSetMode(DisplayServer.WindowMode.Windowed);
        }

    }

    public void ToggleMute()
    {
        _IsAudioMuted = !_IsAudioMuted;

        var bus_idx = AudioServer.GetBusIndex("Master");

        if (_IsAudioMuted)
        {
            AudioServer.SetBusMute(bus_idx, true);
        }
        else
        {
            AudioServer.SetBusMute(bus_idx, false);
        }
    }

    // Help
    public void ShowAboutView()
    {
        aboutView.Show();
    }

    public void ShowKeyboardHelpView()
    {
        keyboardView.Show();
    }

    private void RefreshStatusBar()
    {
        if (spectrum == null)
        {
            return;
        }

        statusBar.setAY(_IsAyEnabled);
        statusBar.setStereo(_StereoMode);
    }

    public void TogglePause()
    {
        _IsPaused = !_IsPaused;
    }

    public void UpdateWindowTitle()
    {
        if (_currentFileName == string.Empty)
        {
            lblTitle.Text = "Casper in Godot";
            return;
        }

        lblTitle.Text = $"Casper in Godot [{_currentFileName}]";
    }

    public override void _PhysicsProcess(double _delta)
    {
        if (bReady)
        {
            if (_IsPhysFinished)
            {
                _IsPhysFinished = false;

                if (!_IsPaused)
                {
                    double frate = 1f / _delta;
                    int iRate = (int)frate;

                    statusBar.setFPS(iRate);

                    UpdateController();
                    spectrum.Step();
                    sound.FlushBuffer();
                    OnPaint();
                    outer.QueueRedraw();
                    screen.QueueRedraw();
                }
                else
                    statusBar.setFPS(0);

                _IsPhysFinished = true;
            }
        }
    }

    void InitializeController()
    {
        var joypads = Input.GetConnectedJoypads();

        if (joypads.Count > 0)
        {
            joystick = joypads[0]; // Use first connected device
            GD.Print($"Using Joypad ID: {joystick}");
        }
        else
        {
            GD.Print("No gamepad found.");
        }
    }

    void UpdateController()
    {
        if (joystick == -1)
            return;

        // Button 1 → SPACE
        if (Input.IsJoyButtonPressed(joystick, (JoyButton)1))
            spectrum.Keyboard.OnPhysicalKey(true, Casper.Key.SPACE);
        else
            spectrum.Keyboard.OnPhysicalKey(false, Casper.Key.SPACE);

        // Axis X → Q / W
        var axisX = Input.GetJoyAxis(joystick, (int)JoyAxis.LeftX); // Usually axis 0
        spectrum.Keyboard.OnPhysicalKey(axisX < -0.9, Casper.Key.Q);
        spectrum.Keyboard.OnPhysicalKey(axisX > +0.9, Casper.Key.SPACE);
    }

    static float NormalizeAxis(int axis)
    {
        axis += short.MinValue;
        return (axis < 0) ? -((float)axis / short.MinValue) : ((float)axis / short.MaxValue);
    }

    void OnRenderPixel(int x, int y, ColorIndex16 colorIndex)
    {
        bitmapScreen.SetPixel(x, y, Colors16.Palette16[(int)colorIndex]);
    }

    void OnRenderBorder(ColorIndex16 col)
    {
        Color borderCol = Colors16.FromColorIndex16(col);
        bitmapOuter.Fill(borderCol);
    }

    private void OnPaint()
    {
        texOuter = ImageTexture.CreateFromImage(bitmapOuter);
        texScreen = ImageTexture.CreateFromImage(bitmapScreen);

        outer.Texture = texOuter;
        screen.Texture = texScreen;
    }

    public override void _UnhandledKeyInput(InputEvent @event)
    {
        // This is for PHYSICAL keyboard interactions
        if (@event is InputEventKey)
        {
            if (@event.IsPressed() && !@event.IsEcho())
                OnPhysicalKey((InputEventKey)@event, true);
            else if (@event.IsReleased() && !@event.IsEcho())
                OnPhysicalKey((InputEventKey)@event, false);
        }
    }

    public void OnKeyPress(InputEventKey e)
    {
        // This is for SOFTWARE based keyboard interactions
        OnLogicalKey(e);
    }

    public bool UseLogicalKeyboardLayout { get; set; }

    void OnPhysicalKey(InputEventKey args, bool down)
    {
        // Emulator control keys
        switch (args.Keycode)
        {
            case Godot.Key.Pause: if (down) { _IsPaused = !_IsPaused; }; return;
            case Godot.Key.Escape: if (down) { UseLogicalKeyboardLayout = !UseLogicalKeyboardLayout; }; return;
        }

        // KeyCode is the PHYSICAL key pressed so Keys.Q would be the first letter on the first row of letters.
        // For an AZERTY keyboard when the "A" key is pressed the KeyCode value is Keys.Q.
        if (KeyCodeMap.TryGetValue((args.Keycode, args.Location), out var keyCode))
        {
            spectrum.Keyboard.OnPhysicalKey(down, keyCode);

            // Windows is unable to detect when a shift key is released if the other shift key is still pressed
            // As a workaround if one is released, release them both
            if (!down && (keyCode == KeyCode.ShiftLeft || keyCode == KeyCode.ShiftRight))
                spectrum.Keyboard.OnPhysicalKeys(down: false, Casper.Key.CAPS, Casper.Key.SYMB);
        }
    }

    void OnLogicalKey(InputEventKey args)
    {
        if (UseLogicalKeyboardLayout)
            spectrum.Keyboard.OnLogicalKeys(args.KeyLabel.ToString());
    }

    static readonly System.Collections.Generic.Dictionary<(Godot.Key, KeyLocation loc), KeyCode> KeyCodeMap = new() {

        // https://www.w3.org/TR/uievents-code/#key-alphanumeric-writing-system
            { (Godot.Key.Asciitilde, KeyLocation.Unspecified), KeyCode.Backquote },
            { (Godot.Key.Backslash, KeyLocation.Unspecified), KeyCode.Backslash },
            { (Godot.Key.Backspace, KeyLocation.Unspecified), KeyCode.Backspace },
            { (Godot.Key.Bracketleft, KeyLocation.Unspecified), KeyCode.BracketLeft },
            { (Godot.Key.Bracketright, KeyLocation.Unspecified), KeyCode.BracketRight },
            { (Godot.Key.Comma, KeyLocation.Unspecified), KeyCode.Comma },

            { (Godot.Key.Key0, KeyLocation.Unspecified), KeyCode.Digit0 },
            { (Godot.Key.Key1, KeyLocation.Unspecified), KeyCode.Digit1 },
            { (Godot.Key.Key2, KeyLocation.Unspecified), KeyCode.Digit2 },
            { (Godot.Key.Key3, KeyLocation.Unspecified), KeyCode.Digit3 },
            { (Godot.Key.Key4, KeyLocation.Unspecified), KeyCode.Digit4 },
            { (Godot.Key.Key5, KeyLocation.Unspecified), KeyCode.Digit5 },
            { (Godot.Key.Key6, KeyLocation.Unspecified), KeyCode.Digit6 },
            { (Godot.Key.Key7, KeyLocation.Unspecified), KeyCode.Digit7 },
            { (Godot.Key.Key8, KeyLocation.Unspecified), KeyCode.Digit8 },
            { (Godot.Key.Key9, KeyLocation.Unspecified), KeyCode.Digit9 },

        //Equal,
        //IntlBackslash,
        //IntlRo,
        //IntlYen,

            { (Godot.Key.A, KeyLocation.Unspecified), KeyCode.KeyA },
            { (Godot.Key.B, KeyLocation.Unspecified), KeyCode.KeyB },
            { (Godot.Key.C, KeyLocation.Unspecified), KeyCode.KeyC },
            { (Godot.Key.D, KeyLocation.Unspecified), KeyCode.KeyD },
            { (Godot.Key.E, KeyLocation.Unspecified), KeyCode.KeyE },
            { (Godot.Key.F, KeyLocation.Unspecified), KeyCode.KeyF },
            { (Godot.Key.G, KeyLocation.Unspecified), KeyCode.KeyG },
            { (Godot.Key.H, KeyLocation.Unspecified), KeyCode.KeyH },
            { (Godot.Key.I, KeyLocation.Unspecified), KeyCode.KeyI },
            { (Godot.Key.J, KeyLocation.Unspecified), KeyCode.KeyJ },
            { (Godot.Key.K, KeyLocation.Unspecified), KeyCode.KeyK },
            { (Godot.Key.L, KeyLocation.Unspecified), KeyCode.KeyL },
            { (Godot.Key.M, KeyLocation.Unspecified), KeyCode.KeyM },
            { (Godot.Key.N, KeyLocation.Unspecified), KeyCode.KeyN },
            { (Godot.Key.O, KeyLocation.Unspecified), KeyCode.KeyO },
            { (Godot.Key.P, KeyLocation.Unspecified), KeyCode.KeyP },
            { (Godot.Key.Q, KeyLocation.Unspecified), KeyCode.KeyQ },
            { (Godot.Key.R, KeyLocation.Unspecified), KeyCode.KeyR },
            { (Godot.Key.T, KeyLocation.Unspecified), KeyCode.KeyT },
            { (Godot.Key.U, KeyLocation.Unspecified), KeyCode.KeyU },
            { (Godot.Key.V, KeyLocation.Unspecified), KeyCode.KeyV },
            { (Godot.Key.W, KeyLocation.Unspecified), KeyCode.KeyW },
            { (Godot.Key.X, KeyLocation.Unspecified), KeyCode.KeyX },
            { (Godot.Key.Y, KeyLocation.Unspecified), KeyCode.KeyY },
            { (Godot.Key.Z, KeyLocation.Unspecified), KeyCode.KeyZ },

            //{ Keys.OemMinus, KeyLocation.Unspecified), KeyCode.Equal },
            //{ Keys.OemPeriod, KeyLocation.Unspecified), KeyCode.Period },
            //{ Keys.OemQuotes, KeyLocation.Unspecified), KeyCode.Quote },
            //{ Keys.OemSemicolon, KeyLocation.Unspecified), KeyCode.Semicolon },

        //Slash,

        // https://www.w3.org/TR/uievents-code/#key-alphanumeric-functional
            //{ Keys.LMenu, KeyLocation.Unspecified), KeyCode.AltLeft },
            //{ Keys.Menu, KeyLocation.Unspecified), KeyCode.AltRight },
            //{ Keys.CapsLock, KeyLocation.Unspecified), KeyCode.CapsLock },

        // ContextMenu,

            { (Godot.Key.Ctrl, KeyLocation.Left), KeyCode.ControlLeft },
            { (Godot.Key.Ctrl, KeyLocation.Right), KeyCode.ControlRight },
            { (Godot.Key.Enter, KeyLocation.Unspecified), KeyCode.Enter },
            //{ Keys.LWin, KeyLocation.Unspecified), KeyCode.MetaLeft },
            //{ Keys.RWin, KeyLocation.Unspecified), KeyCode.MetaRight },
            { (Godot.Key.Shift, KeyLocation.Left), KeyCode.ShiftLeft },
            { (Godot.Key.Shift, KeyLocation.Right), KeyCode.ShiftRight },
            { (Godot.Key.Space, KeyLocation.Unspecified), KeyCode.Space },
            //{ Keys.Tab, KeyCode.Tab },

        // https://www.w3.org/TR/uievents-code/#key-controlpad-section
            //{ Keys.Delete, KeyCode.Delete },
            //{ Keys.End, KeyCode.End },
            //{ Keys.Help, KeyCode.Help },
            //{ Keys.Home, KeyCode.Home },
            //{ Keys.Insert, KeyCode.Insert },
            //{ Keys.PageDown, KeyCode.PageDown },
            //{ Keys.PageUp, KeyCode.PageUp },

        // https://www.w3.org/TR/uievents-code/#key-arrowpad-section
            { (Godot.Key.Down, KeyLocation.Unspecified), KeyCode.ArrowDown },
            { (Godot.Key.Left, KeyLocation.Unspecified), KeyCode.ArrowLeft },
            { (Godot.Key.Right, KeyLocation.Unspecified), KeyCode.ArrowRight },
            { (Godot.Key.Up, KeyLocation.Unspecified), KeyCode.ArrowUp },

        // https://www.w3.org/TR/uievents-code/#key-numpad-section
            //{ (Godot.Key.NumLock, KeyLocation.Unspecified), KeyCode.NumLock },
            { (Godot.Key.Kp0, KeyLocation.Unspecified), KeyCode.Numpad0 },
            { (Godot.Key.Kp1, KeyLocation.Unspecified), KeyCode.Numpad1 },
            { (Godot.Key.Kp2, KeyLocation.Unspecified), KeyCode.Numpad2 },
            { (Godot.Key.Kp3, KeyLocation.Unspecified), KeyCode.Numpad3 },
            { (Godot.Key.Kp4, KeyLocation.Unspecified), KeyCode.Numpad4 },
            { (Godot.Key.Kp5, KeyLocation.Unspecified), KeyCode.Numpad5 },
            { (Godot.Key.Kp6, KeyLocation.Unspecified), KeyCode.Numpad6 },
            { (Godot.Key.Kp7, KeyLocation.Unspecified), KeyCode.Numpad7 },
            { (Godot.Key.Kp8, KeyLocation.Unspecified), KeyCode.Numpad8 },
            { (Godot.Key.Kp9, KeyLocation.Unspecified), KeyCode.Numpad9 },

            //{ Keys.Oemplus, KeyCode.NumpadAdd },

        //NumpadAdd,
        //NumpadBackspace,
        //NumpadClear,
        //NumpadClearEntry,
        //NumpadComma,
        //NumpadDecimal,
        //NumpadDivide,
        //NumpadEnter,
        //NumpadEqual,
        //NumpadHash,
        //NumpadMemoryAdd,
        //NumpadMemoryClear,
        //NumpadMemoryRecall,
        //NumpadMemoryStore,
        //NumpadMemorySubtract,
        //NumpadMultiply,
        //NumpadParenLeft,
        //NumpadParenRight,
        //NumpadStar,
        //NumpadSubtract,

        // https://www.w3.org/TR/uievents-code/#key-function-section
            //{ Keys.Escape, KeyCode.Escape },
            //{ Keys.F1, KeyCode.F1 },
            //{ Keys.F2, KeyCode.F2 },
            //{ Keys.F3, KeyCode.F3 },
            //{ Keys.F4, KeyCode.F4 },
            //{ Keys.F5, KeyCode.F5 },
            //{ Keys.F6, KeyCode.F6 },
            //{ Keys.F7, KeyCode.F7 },
            //{ Keys.F8, KeyCode.F8 },
            //{ Keys.F9, KeyCode.F9 },
            //{ Keys.F10, KeyCode.F10 },
            //{ Keys.F11, KeyCode.F11 },
            //{ Keys.F12, KeyCode.F12 },
        //Fn,
        //FnLock,
            //{ Keys.PrintScreen, KeyCode.PrintScreen },
            //{ Keys.Scroll, KeyCode.ScrollLock },
            //{ Keys.Pause, KeyCode.Pause },

        // https://www.w3.org/TR/uievents-code/#key-media
            //{ Keys.BrowserBack, KeyCode.BrowserBack },
            //{ Keys.BrowserFavorites, KeyCode.BrowserFavorites },
            //{ Keys.BrowserForward, KeyCode.BrowserForward },
            //{ Keys.BrowserHome, KeyCode.BrowserHome },
            //{ Keys.BrowserRefresh, KeyCode.BrowserRefresh },
            //{ Keys.BrowserSearch, KeyCode.BrowserSearch },
            //{ Keys.BrowserStop, KeyCode.BrowserStop },
        //Eject,
            //{ Keys.LaunchApplication1, KeyCode.LaunchApp1 },
            //{ Keys.LaunchApplication2, KeyCode.LaunchApp2 },
            //{ Keys.LaunchMail, KeyCode.LaunchMail },
            //{ Keys.MediaPlayPause, KeyCode.MediaPlayPause },
            //{ Keys.SelectMedia, KeyCode.MediaSelect },
            //{ Keys.MediaStop, KeyCode.MediaStop },
            //{ Keys.MediaNextTrack, KeyCode.MediaTrackNext },
            //{ Keys.MediaPreviousTrack, KeyCode.MediaTrackPrevious },
        //Power,
            //{ Keys.Sleep, KeyCode.Sleep },
            //{ Keys.VolumeDown, KeyCode.AudioVolumeDown },
            //{ Keys.VolumeMute, KeyCode.AudioVolumeMute },
            //{ Keys.VolumeUp, KeyCode.AudioVolumeUp },
        //WakeUp,

        // https://www.w3.org/TR/uievents-code/#key-legacy
        //Hyper,
        //Super,
        //Turbo,
        //Abort,
        //Resume,
        //Suspend,
        //Again,
        //Copy,
        //Cut,
        //Find,
        //Open,
        //Paste,
        //Props,
        //Select,
        //Undo,
        //Hiragana,
        //Katakana,
        //Unidentified,
        };
}
