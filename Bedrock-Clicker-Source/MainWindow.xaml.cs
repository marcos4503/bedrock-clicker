using Linearstar.Windows.RawInput;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Media;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Bedrock_Clicker
{
    /// <summary>
    /// Interação lógica para MainWindow.xam
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            OnStart();
        }

        //Window variables
        private NotifyIcon notifyIcon = new NotifyIcon();
        private WindowOverlay overlay = new WindowOverlay();
        private KeyboardHotkey_Interceptor toggleHotkey = null;
        private KeyboardKeys_Watcher keysWatcher = null;
        private MouseWheelKeys_Watcher mouseWatcher = null;

        //Auto click variables
        private string currentWindow = "Minecraft";
        private bool isEnabled = false;
        private int clicksPerSecond = -1;
        private Timer autoClickTimer = null;
        private SoundPlayer clickSound = null;
        private int clickCount = 0;
        private bool canIncreaseClickCount = false;
        private Timer clickCountTimer = null;

        //Window methods

        public void OnStart()
        {
            //Try to load all preferences of user
            ApplicationPreferences applicationPreferences = new ApplicationPreferences();
            applicationPreferences.LoadPreferences();

            //Apply preferences loaded to interface
            pref_cps.SelectedIndex = applicationPreferences.clicksPerSecond;
            pref_hotkey.SelectedIndex = applicationPreferences.toggleHotkey;
            pref_autoOff.SelectedIndex = applicationPreferences.autoToggleOff;
            pref_onlyInsideMc.SelectedIndex = applicationPreferences.worksOnlyInMinecraft;
            pref_clickSound.SelectedIndex = applicationPreferences.playSound;

            //Alow minimizing and restore last window position
            this.ResizeMode = ResizeMode.CanMinimize;
            this.Left = applicationPreferences.windowPositionX;
            this.Top = applicationPreferences.windowPositionY;
            this.Show();

            //Prepare the notify icon (Requires System.Drawing and System.Windows.Forms references on project)
            this.notifyIcon.Visible = true;
            this.notifyIcon.Text = "Bedrock Clicker";
            this.notifyIcon.MouseClick += NotifyIcon_Click;
            this.notifyIcon.ContextMenuStrip = new ContextMenuStrip();
            this.notifyIcon.ContextMenuStrip.Items.Add("See PVP Tips!", null, this.NotifyIcon_PvpTips);
            this.notifyIcon.ContextMenuStrip.Items.Add("About", null, this.NotifyIcon_About);
            this.notifyIcon.ContextMenuStrip.Items.Add("Quit", null, this.NotifyIcon_Quit);
            this.notifyIcon.Icon = new Icon(@"../../resources/icon-tray.ico");

            //Prepare and open the overlay window and hide to use later
            overlay = new WindowOverlay();
            float oldOverlayWidth = (float)overlay.Width;
            float oldOverlayHeight = (float)overlay.Height;
            overlay.Top = -100;
            overlay.Left = -100;
            overlay.Width = 1;
            overlay.Height = 1;
            overlay.Show();
            overlay.Owner = this;
            overlay.Visibility = Visibility.Collapsed;
            overlay.Width = oldOverlayWidth;
            overlay.Height = oldOverlayHeight;
            overlay.Left = (Screen.PrimaryScreen.Bounds.Width / 2) - (oldOverlayWidth / 2);
            overlay.Top = 80;

            //Get the desired clicks per second
            if (applicationPreferences.clicksPerSecond == 0)
                clicksPerSecond = 1;
            if (applicationPreferences.clicksPerSecond == 1)
                clicksPerSecond = 5;
            if (applicationPreferences.clicksPerSecond == 2)
                clicksPerSecond = 8;
            if (applicationPreferences.clicksPerSecond == 3)
                clicksPerSecond = 10;
            if (applicationPreferences.clicksPerSecond == 4)
                clicksPerSecond = 12;
            if (applicationPreferences.clicksPerSecond == 5)
                clicksPerSecond = 15;
            if (applicationPreferences.clicksPerSecond == 6)
                clicksPerSecond = 18;
            if (applicationPreferences.clicksPerSecond == 7)
                clicksPerSecond = 20;
            if (applicationPreferences.clicksPerSecond == 8)
                clicksPerSecond = 25;
            if (applicationPreferences.clicksPerSecond == 9)
                clicksPerSecond = 30;
            if (applicationPreferences.clicksPerSecond == 10)
                clicksPerSecond = 40;
            if (applicationPreferences.clicksPerSecond == 11)
                clicksPerSecond = 50;

            //Register the hotkey for toggle auto click
            RegisterToggleHotkey(applicationPreferences.toggleHotkey);
            //If is desired auto disable auto click on change weapon
            if (applicationPreferences.autoToggleOff == 1)
                RegisterKeysWatcher();
            //If is desired to work only when is in minecraft, enable the program monitor
            if (applicationPreferences.worksOnlyInMinecraft == 1)
                StartCurrentWindowMonitor();

            //Initialize the click sound
            if (applicationPreferences.playSound == 1)
                clickSound = new SoundPlayer(@"../../resources/click-low.wav");
            if (applicationPreferences.playSound == 2)
                clickSound = new SoundPlayer(@"../../resources/click-high.wav");

            //Prepare the help button
            help.Click += (sender, eventt) => 
            {
                //Show help
                System.Windows.MessageBox.Show("The Bedrock Clicker is an auto-clicker program for you that play Minecraft! To use the program is very simple!\n\n" +
                                               "In \"Toggle Hotkey\", define a hotkey to Enable and Disable the auto-clicker. Once that's done, leave Bedrock Clicker " +
                                               "open while you play Minecraft. Whenever you need to hit, press the defined key to start or stop hitting.\n\n" +
                                               "Enjoy and take a look at the other preferences too! There may be something that will be of use to you!", 
                                               "How To Use Bedrock Clicker");
            };
            //Prepare the pvp tips button
            pvpTips.Click += (sender, eventt) =>
            {
                //Open the PVP tips
                WindowPvpTips pvpTips = new WindowPvpTips();
                pvpTips.Show();
            };
            //Prepare the donate button
            donate.Click += (sender, eventt) =>
            {
                //Open the donation page
                Process.Start("https://www.paypal.com/donate/?hosted_button_id=MVDJY3AXLL8T2");
            };

            //Show warning if clicks per second rate is much high
            pref_cps.SelectionChanged += (sender, eventt) =>
            {
                if(pref_cps.SelectedIndex >= 6)
                    System.Windows.MessageBox.Show("Note that some servers are able to detect very high Clicks Per Second rate. Also, some servers may enforce account kicking, suspension or banning for using auto-clicker with very high CPS rate.\n\nStay tuned! :)", "Warning!");
            };
        }

        //Auto clicker methods

        private void RegisterToggleHotkey(int hotkeySelected)
        {
            //Prepare the desired hotkey
            VirtualKeyCodes keycode = VirtualKeyCodes.A;
            if (hotkeySelected == 0)
                keycode = VirtualKeyCodes.TAB;
            if (hotkeySelected == 1)
                keycode = VirtualKeyCodes.CAPS_LOCK;
            if (hotkeySelected == 2)
                keycode = VirtualKeyCodes.R;
            if (hotkeySelected == 3)
                keycode = VirtualKeyCodes.F;

            //Create the toggle hotkey interceptor
            toggleHotkey = new KeyboardHotkey_Interceptor(this, ModifierKeyCodes.None, keycode);
            toggleHotkey.OnPressHotkey += () => 
            {
                //If the autoclick is disabled, enable it
                if(isEnabled == false)
                {
                    if(currentWindow.Equals("Minecraft") == true) //<- Only starts the auto click, if the Minecraft is on foreground
                        EnableAutoClick();
                    return;
                }
                //If the autoclick is enabled, disable it
                if (isEnabled == true)
                {
                    DisableAutoClick();
                    return;
                }
            };
        }

        private void RegisterKeysWatcher()
        {
            //Create the keys watcher object
            keysWatcher = new KeyboardKeys_Watcher();
            keysWatcher.OnPressKeys += (Keys key) =>
            {
                //If is a key of hotbar, auto disable the auto click
                if (key == Keys.D1 || key == Keys.D2 || key == Keys.D3 || key == Keys.D4 || key == Keys.D5 || key == Keys.D6 || key == Keys.D7 || key == Keys.D8 || key == Keys.D9)
                    DisableAutoClick();
            };

            //Crate the mouse watcher object
            mouseWatcher = new MouseWheelKeys_Watcher(this);
            mouseWatcher.OnWheelScroll += () =>
            {
                //If is scrolling, auto disable the auto click
                DisableAutoClick();
            };
        }

        private void StartCurrentWindowMonitor()
        {
            //Start a timer that each 100ms check current program on foreground and store the title
            Timer timer = new Timer { Interval = 100 };
            timer.Enabled = true;
            timer.Tick += new EventHandler((object sender, EventArgs e) => {
                //Get the title of current program in foreground
                string currentProgramOnForeground = GetActiveWindowTitle();

                //Store the program name on variable
                if (currentProgramOnForeground != null)
                    currentWindow = currentProgramOnForeground;
                if (currentProgramOnForeground == null)
                    currentWindow = "";
            });
        }

        private void EnableAutoClick()
        {
            //Change the interface of program
            autoclick_cps.Content = clicksPerSecond.ToString();
            autoclick_off.Visibility = Visibility.Hidden;
            autoclick_on.Visibility = Visibility.Visible;
            autoclick_bg.Fill = new SolidColorBrush(System.Windows.Media.Color.FromRgb(247, 196, 196));
            overlay.clicking0.Visibility = Visibility.Collapsed;
            overlay.clicking1.Visibility = Visibility.Collapsed;
            overlay.clicking2.Visibility = Visibility.Collapsed;
            overlay.clicking3.Visibility = Visibility.Collapsed;
            overlay.clicking4.Visibility = Visibility.Collapsed;
            int random = new Random().Next(0, 5);
            if (random == 0)
                overlay.clicking0.Visibility = Visibility.Visible;
            if (random == 1)
                overlay.clicking1.Visibility = Visibility.Visible;
            if (random == 2)
                overlay.clicking2.Visibility = Visibility.Visible;
            if (random == 3)
                overlay.clicking3.Visibility = Visibility.Visible;
            if (random == 4)
                overlay.clicking4.Visibility = Visibility.Visible;
            overlay.Visibility = Visibility.Visible;

            //Do the first click, and allow click counter increase, before interval conclusion
            canIncreaseClickCount = true;
            Click();
            //If the timer is null, create the timer to do the clicks with delay between each click
            if (autoClickTimer == null)
            {
                autoClickTimer = new Timer { Interval = ((int)(1000.0f / (float)clicksPerSecond)) };
                autoClickTimer.Enabled = true;
                autoClickTimer.Tick += new EventHandler((object sender, EventArgs e) => { Click(); });

                clickCountTimer = new Timer { Interval = 1000 };
                clickCountTimer.Enabled = true;
                clickCountTimer.Tick += new EventHandler((object sender, EventArgs e) => { canIncreaseClickCount = false; clickCountTimer.Stop(); });
            }
            //If the timer is not null, resume the timer
            if (autoClickTimer != null)
                autoClickTimer.Start();
            if (clickCountTimer != null)
                clickCountTimer.Start();

            //Inform that auto click is running
            isEnabled = true;

            //Enable The Object That Reads Raw Input Of Mouse If Have (If Auto Disable Auto-Click is Enabled)
            if (mouseWatcher != null)
                mouseWatcher.StartWatch();
        }

        private void Click()
        {
            //Play click sound, if have
            if(clickSound != null)
                clickSound.Play();

            //Do the simulated click
            DoLeftMouseClick();

            //Increase click counter
            if(canIncreaseClickCount == true)
            {
                clickCount += 1;
                overlay.cps.Content = clickCount.ToString();
            }
        }

        private void DisableAutoClick()
        {
            //Change the interface of program
            autoclick_cps.Content = "0";
            autoclick_off.Visibility = Visibility.Visible;
            autoclick_on.Visibility = Visibility.Hidden;
            autoclick_bg.Fill = new SolidColorBrush(System.Windows.Media.Color.FromRgb(196, 225, 247));
            overlay.Visibility = Visibility.Collapsed;

            //Stop the timer of the clicks
            autoClickTimer.Stop();
            clickCountTimer.Stop();

            //Reset click counter
            clickCount = 0;
            canIncreaseClickCount = false;
            overlay.cps.Content = "0";

            //Inform that auto click is not running
            isEnabled = false;

            //Disable The Object That Reads Raw Input Of Mouse If Have (If Auto Disable Auto-Click is Enabled)
            if (mouseWatcher != null)
                mouseWatcher.StopWatch();
        }

        //Notify icon methods

        private void NotifyIcon_Click(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            //Bring this window to front
            this.WindowState = WindowState.Normal;
            this.Topmost = true;
            this.Topmost = false;
        }

        private void NotifyIcon_PvpTips(object sender, EventArgs e)
        {
            //Open pvp tips window
            WindowPvpTips pvpTips = new WindowPvpTips();
            pvpTips.Show();
        }

        private void NotifyIcon_About(object sender, EventArgs e)
        {
            //Show warning about
            System.Windows.MessageBox.Show("Bedrock Clicker was created with the intent of making PVP more easier for everyone who plays Minecraft Bedrock Edition!\n\nBedrock Clicker was created by Marcos Tomaz in 2023.\n\nPlease consider a donation if you like this software! :)", "About Bedrock Clicker");
        }

        private void NotifyIcon_Quit(object sender, EventArgs e)
        {
            //Close this window and childrens, to kill process completely
            System.Windows.Application.Current.Shutdown();
        }

        //Window events

        private void pref_save_Click(object sender, RoutedEventArgs e)
        {
            //Load all preferences of user
            ApplicationPreferences applicationPreferences = new ApplicationPreferences();
            applicationPreferences.LoadPreferences();

            //Put the new preferences
            applicationPreferences.clicksPerSecond = pref_cps.SelectedIndex;
            applicationPreferences.toggleHotkey = pref_hotkey.SelectedIndex;
            applicationPreferences.autoToggleOff = pref_autoOff.SelectedIndex;
            applicationPreferences.worksOnlyInMinecraft = pref_onlyInsideMc.SelectedIndex;
            applicationPreferences.playSound = pref_clickSound.SelectedIndex;

            //Save the new preferences
            applicationPreferences.ApplyPreferences();

            //Show a alert dialog
            System.Windows.MessageBox.Show("Preferences have been applied! It is necessary to restart the program for the changes to take effect!", "Warning!");
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            //Load all preferences of user
            ApplicationPreferences applicationPreferences = new ApplicationPreferences();
            applicationPreferences.LoadPreferences();

            //Put the new preferences
            applicationPreferences.windowPositionX = (int)this.Left;
            applicationPreferences.windowPositionY = (int)this.Top;

            //Save the new preferences
            applicationPreferences.ApplyPreferences();

            //Clear the hotkeys registered
            if (toggleHotkey != null)
                toggleHotkey.Dispose();
            if (keysWatcher != null)
                keysWatcher.Dispose();
            if (mouseWatcher != null)
                mouseWatcher.Dispose();

            //Completely shutdown the application
            System.Windows.Application.Current.Shutdown();
        }


        #region KeyboardAndMouseWatcherHookUtils

        //Modifier Key Codes

        [Flags]
        public enum ModifierKeyCodes : uint
        {
            None = 0,
            Alt = 1,
            Control = 2,
            Shift = 4,
            Windows = 8
        }

        //Key Codes

        public enum VirtualKeyCodes : uint
        {
            TAB = 9,
            CAPS_LOCK = 20,
            A = 65,
            B = 66,
            C = 67,
            D = 68,
            E = 69,
            F = 70,
            G = 71,
            H = 72,
            I = 73,
            J = 74,
            K = 75,
            L = 76,
            M = 77,
            N = 78,
            O = 79,
            P = 80,
            Q = 81,
            R = 82,
            S = 83,
            T = 84,
            U = 85,
            V = 86,
            W = 87,
            X = 88,
            Y = 89,
            Z = 90
        }

        //Classes

        private class KeyboardHotkey_Interceptor : IDisposable
        {
            //Private variables
            private WindowInteropHelper host;
            private int identifier;
            private bool isDisposed = false;

            //Public variables
            public Window window;
            public ModifierKeyCodes modifier;
            public VirtualKeyCodes key;

            //Public callbacks
            public event Action OnPressHotkey;

            //Import methods

            [DllImport("user32.dll")]
            public static extern bool UnregisterHotKey(IntPtr hWnd, int id);

            [DllImport("user32.dll")]
            public static extern bool RegisterHotKey(IntPtr hWnd, int id, ModifierKeyCodes fdModifiers, VirtualKeyCodes vk);

            //Core methods

            public KeyboardHotkey_Interceptor(Window window, ModifierKeyCodes modifierCode, VirtualKeyCodes keyCode)
            {
                //Store the information
                this.window = window;
                this.modifier = modifierCode;
                this.key = keyCode;

                //Prepare the host
                host = new WindowInteropHelper(this.window);
                identifier = this.window.GetHashCode();

                //Register the hotkey
                RegisterHotKey(host.Handle, identifier, this.modifier, this.key);

                //Register the callback with a pre-process logic
                ComponentDispatcher.ThreadPreprocessMessage += ProcessMessage;
            }

            void ProcessMessage(ref MSG msg, ref bool handled)
            {
                //Validate the response
                if ((msg.message == 786) && (msg.wParam.ToInt32() == identifier) && (OnPressHotkey != null))
                    OnPressHotkey();
            }

            public void Dispose()
            {
                //If is not disposed, dispose of this object
                if (isDisposed == false)
                {
                    //Unregister callback pre-process logic
                    ComponentDispatcher.ThreadPreprocessMessage -= ProcessMessage;

                    //Unregister the hotkey
                    UnregisterHotKey(host.Handle, identifier);
                    this.window = null;
                    host = null;
                }
                isDisposed = true;
            }
        }

        private class KeyboardKeys_Watcher
        {
            //Private constants
            private const int WH_KEYBOARD_LL = 13;
            private const int WM_KEYDOWN = 0x0100;

            //Private variables
            private LowLevelKeyboardProc procedure = null;
            private IntPtr hookId = IntPtr.Zero;
            private bool isDisposed = false;

            //Public callbacks

            public event Action<Keys> OnPressKeys;

            //Import methods

            private delegate IntPtr LowLevelKeyboardProc(int nCode, IntPtr wParam, IntPtr lParam);

            [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
            private static extern IntPtr SetWindowsHookEx(int idHook, LowLevelKeyboardProc lpfn, IntPtr hMod, uint dwThreadId);

            [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
            [return: MarshalAs(UnmanagedType.Bool)]
            private static extern bool UnhookWindowsHookEx(IntPtr hhk);

            [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
            private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

            [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
            private static extern IntPtr GetModuleHandle(string lpModuleName);

            //Core methods

            public KeyboardKeys_Watcher()
            {
                //Store the callback into a strong reference
                procedure = HookCallback;

                //Register the callback to the low level keys, of windows hook
                using (Process curProcess = Process.GetCurrentProcess())
                using (ProcessModule curModule = curProcess.MainModule)
                {
                    hookId = SetWindowsHookEx(WH_KEYBOARD_LL, procedure, GetModuleHandle(curModule.ModuleName), 0);
                }
            }

            private IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
            {
                //If the key code is major than zero, continues to process
                if (nCode >= 0)
                    if(wParam == (IntPtr)WM_KEYDOWN)   //<- (for more performance, if is a keyboard key pressed down event, send callback)
                        if (OnPressKeys != null)
                            OnPressKeys((Keys)Marshal.ReadInt32(lParam));

                return CallNextHookEx(hookId, nCode, wParam, lParam);
            }
        
            public void Dispose()
            {
                //If is not disposed, dispose of this object
                if (isDisposed == false)
                {
                    //Remove all low level hooks
                    UnhookWindowsHookEx(hookId);

                    //Clean variables
                    procedure = null;
                    hookId = IntPtr.Zero;
                }
                isDisposed = true;
            }
        }

        private class MouseWheelKeys_Watcher
        {
            //Private constants
            private const int WM_INPUT = 0x00FF;

            //Private variables
            private IntPtr windowHandler = IntPtr.Zero;
            private HwndSource windowSource = null;

            //Public variables
            public RawInputDevice[] inputDevicesList = null;

            //Public callbacks

            public event Action OnWheelScroll;

            //Core methods

            public MouseWheelKeys_Watcher(Window window)
            {
                //Get the window handler
                WindowInteropHelper windowInteropHelper = new WindowInteropHelper(window);
                windowHandler = windowInteropHelper.Handle;

                // Get the devices that can be handled with Raw Input.
                inputDevicesList = RawInputDevice.GetDevices();
            }

            public void StartWatch()
            {
                //Register the mouse to be watched
                RawInputDevice.RegisterDevice(HidUsageAndPage.Mouse, RawInputDeviceFlags.ExInputSink | RawInputDeviceFlags.NoLegacy, windowHandler);

                //Register the hook
                windowSource = HwndSource.FromHwnd(windowHandler);
                windowSource.AddHook(Hook);
            }

            private IntPtr Hook(IntPtr hwnd, int msg, IntPtr wparam, IntPtr lparam, ref bool handled)
            {
                //Read inputs by processing the WM_INPUT message
                if (msg == WM_INPUT)
                {
                    // Create an RawInputData from the handle stored in lParam.
                    var data = RawInputData.FromHandle(lparam);

                    // They data contain the raw input data in their properties.
                    switch (data)
                    {
                        case RawInputMouseData mouse:
                            if (mouse.Mouse.ButtonData == 120 || mouse.Mouse.ButtonData == -120)  //<- If is mouse scroll up or down
                                if (OnWheelScroll != null)
                                    OnWheelScroll();
                            break;
                    }
                }

                return IntPtr.Zero;
            }
        
            public void StopWatch()
            {
                //Unregister the mouse watched
                RawInputDevice.UnregisterDevice(HidUsageAndPage.Mouse);

                //Unregister the hook
                windowSource.RemoveHook(Hook);
            }
        
            public void Dispose()
            {
                //Remove all hooks and registrations
                StopWatch();

                //Reset object
                windowHandler = IntPtr.Zero;
                windowSource = null;
                inputDevicesList = null;
            }
        }

        #endregion

        #region CurrentForegroundProgramTitle

        [DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll")]
        static extern int GetWindowText(IntPtr hWnd, StringBuilder text, int count);

        //Detection of current app in foreground

        private string GetActiveWindowTitle()
        {
            const int nChars = 256;
            StringBuilder Buff = new StringBuilder(nChars);
            IntPtr handle = GetForegroundWindow();

            if (GetWindowText(handle, Buff, nChars) > 0)
            {
                return Buff.ToString();
            }
            return null;
        }

        #endregion

        #region MouseClicksSimulationInterface

        public const int MOUSEEVENTF_LEFTDOWN = 0x02;
        public const int MOUSEEVENTF_LEFTUP = 0x04;

        public struct POINT
        {
            public int X;
            public int Y;

            public POINT(int x, int y)
            {
                this.X = x;
                this.Y = y;
            }
        }

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        public static extern void mouse_event(int dwFlags, int dx, int dy, int cButtons, int dwExtraInfo);
        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool GetCursorPos(out POINT pPoint);


        public static void DoLeftMouseClick()
        {
            POINT pnt;
            GetCursorPos(out pnt);

            mouse_event(MOUSEEVENTF_LEFTDOWN, pnt.X, pnt.Y, 0, 0);
            mouse_event(MOUSEEVENTF_LEFTUP, pnt.X, pnt.Y, 0, 0);
        }

        #endregion
    }
}
