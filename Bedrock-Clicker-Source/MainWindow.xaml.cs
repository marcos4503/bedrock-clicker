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

        //Window enums
        private enum AimType
        {
            Up,
            Normal,
            Down
        }
        private enum AimPhysic
        {
            Vanilla,
            NetherGames
        }
        private enum ClickType
        {
            Left,
            Right
        }

        //Window variables
        private NotifyIcon notifyIcon = new NotifyIcon();
        private WindowOverlay overlay = new WindowOverlay();
        private WindowMouseClick mouseClicking = new WindowMouseClick();
        private WindowSprint overlaySprint = new WindowSprint();
        private WindowAimType overlayAimType = new WindowAimType();
        private WindowAim overlayAim = new WindowAim();
        private KeyboardHotkey_Interceptor toggleHotkeyLeft = null;
        private KeyboardHotkey_Interceptor toggleHotkeyRight = null;
        private KeyboardHotkey_Interceptor ctrlToggleHotkeyLeft = null;
        private KeyboardHotkey_Interceptor ctrlToggleHotkeyRight = null;
        private KeyboardHotkey_Interceptor shiftToggleHotkeyLeft = null;
        private KeyboardHotkey_Interceptor shiftToggleHotkeyRight = null;
        private KeyboardHotkey_Interceptor sprintHotkey = null;
        private KeyboardHotkey_Interceptor aimPhysicHotkey = null;
        private KeyboardKeys_Watcher keysWatcher = null;
        private MouseWheelKeys_Watcher mouseWatcher = null;

        //Auto click variables
        private string currentWindow = "Minecraft";
        private bool isEnabled = false;
        private int isEnabledClickingWithButton = -1;
        private int clicksPerSecond = -1;
        private Timer autoClickTimer = null;
        private SoundPlayer clickSound = null;
        private int clickCount = 0;
        private bool canIncreaseClickCount = false;
        private Timer clickCountTimer = null;
        //Auto sprint variables
        private bool isAutoRunEnabled = false;
        private SoundPlayer autoRunSound = null;
        private Timer autoRunNotifyTimer = null;
        private Timer releaseCtrlTimer = null;
        //Crosshair complement variables
        private AimType crosshairComplementType = AimType.Normal;
        private AimPhysic crosshairComplementPhysic = AimPhysic.Vanilla;
        private SoundPlayer aimChangeSound = null;
        private Timer crosshairComplementTimer = null;

        //Window methods

        public void OnStart()
        {
            //Try to load all preferences of user
            ApplicationPreferences applicationPreferences = new ApplicationPreferences();
            applicationPreferences.LoadPreferences();

            //Apply preferences loaded to interface
            pref_cps.SelectedIndex = applicationPreferences.clicksPerSecond;
            pref_hotkey_l.SelectedIndex = applicationPreferences.toggleHotkey;
            pref_sprint_hotkey.SelectedIndex = applicationPreferences.sprintHotkey;
            pref_autoOff.SelectedIndex = applicationPreferences.autoToggleOff;
            if (applicationPreferences.autoToggleOff == 0)
            {
                pref_autoSprint.SelectedIndex = 0;
                pref_crosshairComplement.SelectedIndex = 0;
            }
            if (applicationPreferences.autoToggleOff == 1)
            {
                pref_autoSprint.SelectedIndex = 1;
                pref_crosshairComplement.SelectedIndex = 1;
            }
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

            //Initialize the click and sprint sound
            if (applicationPreferences.playSound == 1)
                clickSound = new SoundPlayer(@"../../resources/click-low.wav");
            if (applicationPreferences.playSound == 2)
                clickSound = new SoundPlayer(@"../../resources/click-high.wav");
            autoRunSound = new SoundPlayer(@"../../resources/auto-sprint.wav");
            aimChangeSound = new SoundPlayer(@"../../resources/aim-change.wav");

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
            overlay.Top = 86;
            //Prepare and open the mouse cliking window of auto sprint, and hide to use later
            mouseClicking = new WindowMouseClick();
            float oldOverlayClickingWidth = (float)mouseClicking.Width;
            float oldOverlayClickingHeight = (float)mouseClicking.Height;
            mouseClicking.Top = -100;
            mouseClicking.Left = -100;
            mouseClicking.Width = 1;
            mouseClicking.Height = 1;
            mouseClicking.Show();
            mouseClicking.Owner = this;
            mouseClicking.Visibility = Visibility.Collapsed;
            mouseClicking.Width = oldOverlayClickingWidth;
            mouseClicking.Height = oldOverlayClickingHeight;
            mouseClicking.Left = (Screen.PrimaryScreen.Bounds.Width / 2) - (oldOverlayClickingWidth / 2);
            mouseClicking.Top = (Screen.PrimaryScreen.Bounds.Height / 2) - (oldOverlayClickingHeight / 2) + 32;
            //Prepare and open the overlay window of auto sprint, and hide to use later
            overlaySprint = new WindowSprint();
            float oldOverlaySprintWidth = (float)overlaySprint.Width;
            float oldOverlaySprintHeight = (float)overlaySprint.Height;
            overlaySprint.Top = -100;
            overlaySprint.Left = -100;
            overlaySprint.Width = 1;
            overlaySprint.Height = 1;
            overlaySprint.Show();
            overlaySprint.Owner = this;
            overlaySprint.Visibility = Visibility.Collapsed;
            overlaySprint.Width = oldOverlaySprintWidth;
            overlaySprint.Height = oldOverlaySprintHeight;
            overlaySprint.Left = (Screen.PrimaryScreen.Bounds.Width / 2) - (oldOverlaySprintWidth / 2);
            overlaySprint.Top = 170;
            //Prepare and open the overlay aim type and hide to use later
            overlayAimType = new WindowAimType();
            float oldOverlayAimTypeWidth = (float)overlayAimType.Width;
            float oldOverlayAimTypeHeight = (float)overlayAimType.Height;
            overlayAimType.Top = -100;
            overlayAimType.Left = -100;
            overlayAimType.Width = 1;
            overlayAimType.Height = 1;
            overlayAimType.Show();
            overlayAimType.Owner = this;
            overlayAimType.Visibility = Visibility.Collapsed;
            overlayAimType.Width = oldOverlayAimTypeWidth;
            overlayAimType.Height = oldOverlayAimTypeHeight;
            overlayAimType.Left = 8;
            overlayAimType.Top = (Screen.PrimaryScreen.Bounds.Height / 2) - (oldOverlayAimTypeHeight / 2);
            //Prepare and open the overlay aim window and hide to use later
            overlayAim = new WindowAim();
            float oldOverlayAimWidth = (float)overlayAim.Width;
            float oldOverlayAimHeight = (float)overlayAim.Height;
            overlayAim.Top = -100;
            overlayAim.Left = -100;
            overlayAim.Width = 1;
            overlayAim.Height = 1;
            overlayAim.Show();
            overlayAim.Owner = this;
            overlayAim.Visibility = Visibility.Collapsed;
            overlayAim.Width = oldOverlayAimWidth;
            overlayAim.Height = oldOverlayAimHeight;
            overlayAim.Left = (Screen.PrimaryScreen.Bounds.Width / 2) - (oldOverlayAimWidth / 2);
            overlayAim.Top = (Screen.PrimaryScreen.Bounds.Height / 2) + 6;

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
            //If is desired auto disable auto click on change weapon, register the keys watcher to auto disable clicker and prepare the auto sprint & crosshair complement embeded feature
            if (applicationPreferences.autoToggleOff == 1)
                RegisterKeysWatcher_And_PrepareAutoSprintFeature_And_CrosshairComplementFeature(applicationPreferences.sprintHotkey);
            //If is desired to work only when is in minecraft, enable the program monitor
            if (applicationPreferences.worksOnlyInMinecraft == 1)
                StartCurrentWindowMonitor();

            //Show warning if selected clicks per second rate is much high
            pref_cps.SelectionChanged += (sender, eventt) =>
            {
                if (pref_cps.SelectedIndex >= 6)
                    System.Windows.MessageBox.Show("Note that some servers are able to detect very high Clicks Per Second rate. Also, some servers may enforce account " +
                                                   "kicking, suspension or banning for using auto-clicker with very high CPS rate.\n\nStay tuned! :)", "Warning!");
            };
            //Update the autosprint feature display, on change the auto toggle of when change item
            pref_autoOff.SelectionChanged += (sender, eventt) =>
            {
                if (pref_autoOff.SelectedIndex == 0)
                {
                    pref_autoSprint.SelectedIndex = 0;
                    pref_crosshairComplement.SelectedIndex = 0;
                }
                if (pref_autoOff.SelectedIndex == 1)
                {
                    pref_autoSprint.SelectedIndex = 1;
                    pref_crosshairComplement.SelectedIndex = 1;
                }

                if (pref_autoOff.SelectedIndex == 1)
                    System.Windows.MessageBox.Show("You have just enabled the \"Auto Toggle Off On Change Item\" function. It will cause the auto-clicker to be disabled whenever " +
                                               "you scroll your mouse wheel or press a button from 1 to 9 to switch slots in Minecraft. This function comes with the following features...\n\n" +
                                               "Auto Sprint\n\nYou can now turn Auto Sprint off or on by pressing \"Page Up\" on your keyboard. If Auto Sprint is on, whenever you press " +
                                               "\"W\" or \"SPACE\" in Minecraft Bedrock, you will automatically start sprinting. If you are going to type something, REMEMBER to disable " +
                                               "Auto Sprint to avoid typing interference.\n\n"+
                                               "Crosshair Complement\n\nNow, whenever you press \"Right Button\" of your mouse to shoot a Bow or Crossbow, you will see a Crosshair Complement just below " +
                                               "your crosshair. You can use it to determine how much to raise your crosshair based on the distance to the target. There are 3 types of " +
                                               "complements, one for targets at the same height, one for targets below you and one for targets above you. You can switch between add-ons " +
                                               "using the \"Middle Button\" of your mouse.\n\n" +
                                               "The crosshairs add-on will display silhouettes of different sizes on the side of your crosshair. When shooting with the Bow, compare " +
                                               "the size of the target with the size of the nearest silhouette. If the target is the same size as some silhouette, adjust the aim until the trace " +
                                               "of the silhouette hits the top of the target's head. If the target is not the exact size of some silhouette, use the closest-sized silhouette to the target, " +
                                               "aligning the middle of the silhouette with the center of the target body. Servers may have different physics for arrows, which may affect aiming accuracy. " +
                                               "So use \"Page Down\" if you want to toggle the physics type of the server you're on, for more accurate of aim complement.\n\n" +
                                               "The following elements must be obeyed for this add-on to work perfectly...\n" +
                                               "- The game needs to be Maxed out, but not Full Screen.\n" +
                                               "- The screen resolution (height) must be 1080 pixels.\n" +
                                               "- The OS must be Windows 11, because of the screen elements size.\n\n" +
                                               "That's all! :)",
                                               "Watch out!");
            };

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
        }

        //Auto clicker methods

        private void RegisterToggleHotkey(int hotkeySelected)
        {
            //Prepare the desired hotkey
            VirtualKeyCodes keycode = VirtualKeyCodes.O;
            if (hotkeySelected == 0)
                keycode = VirtualKeyCodes.TAB;
            if (hotkeySelected == 1)
                keycode = VirtualKeyCodes.CAPS_LOCK;
            if (hotkeySelected == 2)
                keycode = VirtualKeyCodes.R;
            if (hotkeySelected == 3)
                keycode = VirtualKeyCodes.F;

            //Create the toggle hotkey interceptor (left)
            toggleHotkeyLeft = new KeyboardHotkey_Interceptor(this, 2, ModifierKeyCodes.None, keycode);
            toggleHotkeyLeft.OnPressHotkey += () =>
            {
            ReInterceptToggleHotkeyLeft:
                //If the autoclick is disabled, enable it
                if (isEnabled == false)
                {
                    DisableAutoClick();
                    if (currentWindow.Equals("Minecraft") == true) //<- Only starts the auto click, if the Minecraft is on foreground
                        EnableAutoClick(ClickType.Left);
                    return;
                }
                //If the autoclick is enabled, disable it
                if (isEnabled == true)
                {
                    if (isEnabledClickingWithButton != 0) //<- If is already clicking with other button, cancel and change clicking with this, before cancel this clicking
                    {
                        isEnabled = false;
                        goto ReInterceptToggleHotkeyLeft;
                    }
                    DisableAutoClick();
                    return;
                }
            };

            //Create the toggle hotkey interceptor (right)
            toggleHotkeyRight = new KeyboardHotkey_Interceptor(this, 3, ModifierKeyCodes.None, VirtualKeyCodes.APOSTROPHE);
            toggleHotkeyRight.OnPressHotkey += () =>
            {
            ReInterceptToggleHotkeyRight:
                //If the autoclick is disabled, enable it
                if (isEnabled == false)
                {
                    DisableAutoClick();
                    if (currentWindow.Equals("Minecraft") == true) //<- Only starts the auto click, if the Minecraft is on foreground
                        EnableAutoClick(ClickType.Right);
                    return;
                }
                //If the autoclick is enabled, disable it
                if (isEnabled == true)
                {
                    if (isEnabledClickingWithButton != 1) //<- If is already clicking with other button, cancel and change clicking with this, before cancel this clicking
                    {
                        isEnabled = false;
                        goto ReInterceptToggleHotkeyRight;
                    }
                    DisableAutoClick();
                    return;
                }
            };

            //Create the alternative toggle hotkey interceptor with CTRL modifier
            ctrlToggleHotkeyLeft = new KeyboardHotkey_Interceptor(this, 4, ModifierKeyCodes.Control, keycode);
            ctrlToggleHotkeyLeft.OnPressHotkey += () => { toggleHotkeyLeft.ForceOnPressHotkeyEvent(); };
            ctrlToggleHotkeyRight = new KeyboardHotkey_Interceptor(this, 5, ModifierKeyCodes.Control, VirtualKeyCodes.APOSTROPHE);
            ctrlToggleHotkeyRight.OnPressHotkey += () => { toggleHotkeyRight.ForceOnPressHotkeyEvent(); };
            //Create the alternative toggle hotkey interceptor with SHIFT modifier
            shiftToggleHotkeyLeft = new KeyboardHotkey_Interceptor(this, 6, ModifierKeyCodes.Shift, keycode);
            shiftToggleHotkeyLeft.OnPressHotkey += () => { toggleHotkeyLeft.ForceOnPressHotkeyEvent(); };
            shiftToggleHotkeyRight = new KeyboardHotkey_Interceptor(this, 7, ModifierKeyCodes.Shift, VirtualKeyCodes.APOSTROPHE);
            shiftToggleHotkeyRight.OnPressHotkey += () => { toggleHotkeyRight.ForceOnPressHotkeyEvent(); };
        }

        private void RegisterKeysWatcher_And_PrepareAutoSprintFeature_And_CrosshairComplementFeature(int sprintHotkeySelected)
        {
            //Fires prepare the mouse watcher object, that will be used in this entire method...
            mouseWatcher = new MouseWheelKeys_Watcher(this);

            //Prepare the autosprint feature to work with PAGE UP anyway! Register the hotkey to toggle autosprint, for auto sprint feature work...
            sprintHotkey = new KeyboardHotkey_Interceptor(this, 8, ModifierKeyCodes.None, VirtualKeyCodes.PAGE_UP);
            sprintHotkey.OnPressHotkey += () =>
            {
                //If auto sprint is disabled, enable it
                if (isAutoRunEnabled == false)
                {
                    if (currentWindow.Equals("Minecraft") == true) //<- Only starts the autosprint, if the Minecraft is on foreground
                        EnableAutoSprint();
                    return;
                }
                //If auto sprint is enabled, disable it
                if (isAutoRunEnabled == true)
                {
                    DisableAutoSprint();
                    return;
                }
            };
            //Now that the hotkey for toggle auto sprint is created, disable the autosprint by default and show the overlay to user know
            DisableAutoSprint();
            //If have selected a hotkey for autosprint in the mouse, register the button to call the event "OnPressHotkey" of "sprintHotkey"
            if (sprintHotkeySelected == 1)
                mouseWatcher.OnButtonForward += () => { sprintHotkey.ForceOnPressHotkeyEvent(); };
            if (sprintHotkeySelected == 2)
                mouseWatcher.OnButtonBackward += () => { sprintHotkey.ForceOnPressHotkeyEvent(); };

            //Create the keys watcher object for auto disable clicker, and for auto sprint feature
            keysWatcher = new KeyboardKeys_Watcher();
            keysWatcher.OnPressKeys += (Keys key) =>
            {
                //If is a key of hotbar, auto disable the auto click
                if (key == Keys.D1 || key == Keys.D2 || key == Keys.D3 || key == Keys.D4 || key == Keys.D5 || key == Keys.D6 || key == Keys.D7 || key == Keys.D8 || key == Keys.D9)
                    if (isEnabled == true)
                        DisableAutoClick();

                //If pressed W or SPACE, send to autosprint process it
                if (key == Keys.W || key == Keys.Space)
                    if (isAutoRunEnabled == true)
                        Sprint();
            };

            //Config the mouse watcher object for auto disable clicker and to make the feature of crosshair complement work
            mouseWatcher.OnWheelScroll += () =>
            {
                //If is scrolling, auto disable the auto click
                if (isEnabled == true)
                    DisableAutoClick();
            };
            mouseWatcher.OnButtonMiddle += () =>
            {
                //Change crosshair complement type for Vanilla Physic
                if (crosshairComplementType == AimType.Down && crosshairComplementPhysic == AimPhysic.Vanilla)
                {
                    overlayAimType.up.Visibility = Visibility.Collapsed;
                    overlayAimType.normal.Visibility = Visibility.Visible;
                    overlayAimType.down.Visibility = Visibility.Collapsed;
                    overlayAim.vanillaAimUp.Visibility = Visibility.Collapsed;
                    overlayAim.vanillaAimNormal.Visibility = Visibility.Visible;
                    overlayAim.vanillaAimDown.Visibility = Visibility.Collapsed;
                    crosshairComplementType = AimType.Normal;
                    aimChangeSound.Play();
                    return;
                }
                if (crosshairComplementType == AimType.Normal && crosshairComplementPhysic == AimPhysic.Vanilla)
                {
                    overlayAimType.up.Visibility = Visibility.Visible;
                    overlayAimType.normal.Visibility = Visibility.Collapsed;
                    overlayAimType.down.Visibility = Visibility.Collapsed;
                    overlayAim.vanillaAimUp.Visibility = Visibility.Visible;
                    overlayAim.vanillaAimNormal.Visibility = Visibility.Collapsed;
                    overlayAim.vanillaAimDown.Visibility = Visibility.Collapsed;
                    crosshairComplementType = AimType.Up;
                    aimChangeSound.Play();
                    return;
                }
                if (crosshairComplementType == AimType.Up && crosshairComplementPhysic == AimPhysic.Vanilla)
                {
                    overlayAimType.up.Visibility = Visibility.Collapsed;
                    overlayAimType.normal.Visibility = Visibility.Collapsed;
                    overlayAimType.down.Visibility = Visibility.Visible;
                    overlayAim.vanillaAimUp.Visibility = Visibility.Collapsed;
                    overlayAim.vanillaAimNormal.Visibility = Visibility.Collapsed;
                    overlayAim.vanillaAimDown.Visibility = Visibility.Visible;
                    crosshairComplementType = AimType.Down;
                    aimChangeSound.Play();
                    return;
                }
                if (crosshairComplementType == AimType.Normal && crosshairComplementPhysic == AimPhysic.NetherGames)
                {
                    overlayAimType.up.Visibility = Visibility.Collapsed;
                    overlayAimType.normal.Visibility = Visibility.Visible;
                    overlayAimType.down.Visibility = Visibility.Collapsed;
                    overlayAim.ngAimNormal.Visibility = Visibility.Visible;
                    crosshairComplementType = AimType.Normal;
                    aimChangeSound.Play();
                    return;
                }
            };
            mouseWatcher.OnButtonRight += (bool isDown) =>
            {
                //If is pressing down, start the timer to show the aim complement
                if (isDown == true && currentWindow.Equals("Minecraft")) //<- only shows the aim complement if Minecraft is on foreground
                {
                    //If the timer is null, create one
                    if(crosshairComplementTimer == null)
                    {
                        crosshairComplementTimer = new Timer { Interval = 1000 };
                        crosshairComplementTimer.Enabled = true;
                        crosshairComplementTimer.Tick += new EventHandler((object sender, EventArgs e) => { overlayAim.Visibility = Visibility.Visible; crosshairComplementTimer.Stop(); });
                    }
                    //Start the timer to show aim complement after 1000ms holding the right button
                    crosshairComplementTimer.Stop();
                    crosshairComplementTimer.Start();
                }
                //If is releasing up, stop the timer and hide the aim complement
                if (isDown == false)
                {
                    //Stop the timer and hide the aim complement
                    if (crosshairComplementTimer != null)
                        crosshairComplementTimer.Stop();
                    overlayAim.Visibility = Visibility.Collapsed;
                }
            };
            mouseWatcher.StartWatch();

            //Show the crosshair complement type on side
            overlayAimType.Visibility = Visibility.Visible;
            //Finnaly, register the hotkey to change the aim complement physic, to more accurate
            aimPhysicHotkey = new KeyboardHotkey_Interceptor(this, 10, ModifierKeyCodes.None, VirtualKeyCodes.PAGE_DOWN);
            aimPhysicHotkey.OnPressHotkey += () =>
            {
                //Change the aim physic type
                if(crosshairComplementPhysic == AimPhysic.Vanilla)
                {
                    overlayAimType.up.Visibility = Visibility.Collapsed;
                    overlayAimType.normal.Visibility = Visibility.Visible;
                    overlayAimType.down.Visibility = Visibility.Collapsed;
                    overlayAimType.physic.Content = "NetherGames Physic";
                    overlayAim.vanillaPhysic.Visibility = Visibility.Collapsed;
                    overlayAim.ngPhysic.Visibility = Visibility.Visible;
                    overlayAim.ngAimNormal.Visibility = Visibility.Visible;
                    crosshairComplementType = AimType.Normal;
                    crosshairComplementPhysic = AimPhysic.NetherGames;
                    aimChangeSound.Play();
                    //Save the defined aim complement physic type
                    ApplicationPreferences tempPrefs = new ApplicationPreferences();
                    tempPrefs.LoadPreferences();
                    tempPrefs.aimComplementPhysic = 1;
                    tempPrefs.ApplyPreferences();
                    return;
                }
                if (crosshairComplementPhysic == AimPhysic.NetherGames)
                {
                    overlayAimType.up.Visibility = Visibility.Collapsed;
                    overlayAimType.normal.Visibility = Visibility.Visible;
                    overlayAimType.down.Visibility = Visibility.Collapsed;
                    overlayAimType.physic.Content = "Vanilla Physic";
                    overlayAim.vanillaPhysic.Visibility = Visibility.Visible;
                    overlayAim.ngPhysic.Visibility = Visibility.Collapsed;
                    overlayAim.vanillaAimUp.Visibility = Visibility.Collapsed;
                    overlayAim.vanillaAimNormal.Visibility = Visibility.Visible;
                    overlayAim.vanillaAimDown.Visibility = Visibility.Collapsed;
                    crosshairComplementType = AimType.Normal;
                    crosshairComplementPhysic = AimPhysic.Vanilla;
                    aimChangeSound.Play();
                    //Save the defined aim complement physic type
                    ApplicationPreferences tempPrefs = new ApplicationPreferences();
                    tempPrefs.LoadPreferences();
                    tempPrefs.aimComplementPhysic = 0;
                    tempPrefs.ApplyPreferences();
                    return;
                }
            };
            //Load the aim complement physic type defined by the user and select it automatically
            ApplicationPreferences applicationPreferences = new ApplicationPreferences();
            applicationPreferences.LoadPreferences();
            if (applicationPreferences.aimComplementPhysic == 0)   //<- If is Vanilla Physic defined
                Debug.Print("Vanilla Physic was defined to Aim Complement feature.");
            if (applicationPreferences.aimComplementPhysic == 1)   //<- If is NetherGames Physic defined
                aimPhysicHotkey.ForceOnPressHotkeyEvent();
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

        private void EnableAutoClick(ClickType clickType)
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
            mouseClicking.clicking_left.Visibility = Visibility.Collapsed;
            mouseClicking.clicking_right.Visibility = Visibility.Collapsed;
            if (clickType == ClickType.Left)
                mouseClicking.clicking_left.Visibility = Visibility.Visible;
            if (clickType == ClickType.Right)
                mouseClicking.clicking_right.Visibility = Visibility.Visible;
            mouseClicking.Visibility = Visibility.Visible;

            //Do the first click, and allow click counter increase, before interval conclusion
            canIncreaseClickCount = true;
            if (clickType == ClickType.Left)
            {
                isEnabledClickingWithButton = 0;
                Click(ClickType.Left);
            }
            if (clickType == ClickType.Right)
            {
                isEnabledClickingWithButton = 1;
                Click(ClickType.Right);
            }
               
            //LEFT
            if (clickType == ClickType.Left)
                if (autoClickTimer == null)   //<--- If the timer is null, create the timer to do the clicks with delay between each click
                {
                    autoClickTimer = new Timer { Interval = ((int)(1000.0f / (float)clicksPerSecond)) };
                    autoClickTimer.Enabled = true;
                    autoClickTimer.Tick += new EventHandler((object sender, EventArgs e) => { Click(ClickType.Left); });

                    clickCountTimer = new Timer { Interval = 1000 };
                    clickCountTimer.Enabled = true;
                    clickCountTimer.Tick += new EventHandler((object sender, EventArgs e) => { canIncreaseClickCount = false; clickCountTimer.Stop(); });
                }
            //RIGHT
            if (clickType == ClickType.Right)
                if (autoClickTimer == null)   //<--- If the timer is null, create the timer to do the clicks with delay between each click
                {
                    autoClickTimer = new Timer { Interval = ((int)(1000.0f / (float)clicksPerSecond)) };
                    autoClickTimer.Enabled = true;
                    autoClickTimer.Tick += new EventHandler((object sender, EventArgs e) => { Click(ClickType.Right); });

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
        }

        private void Click(ClickType clickType)
        {
            //Play click sound, if have
            if (clickSound != null)
                clickSound.Play();

            //Do the simulated click
            if (clickType == ClickType.Left)
                DoLeftMouseClick();
            if (clickType == ClickType.Right)
                DoRightMouseClick();

            //Increase click counter
            if (canIncreaseClickCount == true)
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
            mouseClicking.clicking_left.Visibility = Visibility.Collapsed;
            mouseClicking.clicking_right.Visibility = Visibility.Collapsed;
            mouseClicking.Visibility = Visibility.Collapsed;

            //Stop the timer of the clicks
            if (autoClickTimer != null)
                autoClickTimer.Stop();
            autoClickTimer = null;
            if (clickCountTimer != null)
                clickCountTimer.Stop();
            clickCountTimer = null;

            //Reset click counter
            clickCount = 0;
            canIncreaseClickCount = false;
            overlay.cps.Content = "0";

            //Inform that auto click is not running
            isEnabled = false;
            isEnabledClickingWithButton = -1;
        }

        private void EnableAutoSprint()
        {
            //Show and change the interface of sprint overlay
            overlaySprint.background.Fill = new SolidColorBrush(System.Windows.Media.Color.FromArgb(191, 3, 109, 0));
            overlaySprint.status.Content = "ON";
            overlaySprint.Visibility = Visibility.Visible;

            //If have a timer, start (or restart) the timer that hides the overlay after a time
            if (autoRunNotifyTimer != null)
            {
                autoRunNotifyTimer.Stop();
                autoRunNotifyTimer.Start();
            }

            //Play the sound
            if (autoRunSound != null)
                autoRunSound.Play();
            //Inform that autosprint is running
            isAutoRunEnabled = true;
        }

        private void Sprint()
        {
            //Now that is receiving W or SPACE keys input, simulate the CTRL input together, to start to run ingame
            if (releaseCtrlTimer == null) //<- If don't have a release CTRL timer created, create one to simulate CTRL key release
            {
                releaseCtrlTimer = new Timer { Interval = 50 };
                releaseCtrlTimer.Enabled = true;
                releaseCtrlTimer.Tick += new EventHandler((object sender, EventArgs e) =>
                {
                    DoCtrlKeyboardPress_Up();
                    releaseCtrlTimer.Stop();
                    overlaySprint.Visibility = Visibility.Collapsed; //<- hiding overlay = CTRL is up
                });
            }

            //Simulate CTRL key press down and prepare the timer to release CTRL key
            DoCtrlKeyboardPress_Down();
            releaseCtrlTimer.Stop();
            releaseCtrlTimer.Start();
            //Show the sprint overlay to player knows that the auto sprint is making effect
            autoRunNotifyTimer.Stop();
            overlaySprint.Visibility = Visibility.Visible; //<- showing overlay = CTRL is down
        }

        private void DisableAutoSprint()
        {
            //Show and change the interface of sprint overlay
            overlaySprint.background.Fill = new SolidColorBrush(System.Windows.Media.Color.FromArgb(191, 136, 0, 0));
            overlaySprint.status.Content = "OFF";
            overlaySprint.Visibility = Visibility.Visible;

            //If don't have a timer, create the timer that hides the overlay after a time
            if (autoRunNotifyTimer == null)
            {
                autoRunNotifyTimer = new Timer { Interval = 5000 };
                autoRunNotifyTimer.Enabled = true;
                autoRunNotifyTimer.Tick += new EventHandler((object sender, EventArgs e) => { overlaySprint.Visibility = Visibility.Collapsed; autoRunNotifyTimer.Stop(); });
            }
            //If have a timer, start (or restart) the timer that hides the overlay after a time
            if (autoRunNotifyTimer != null)
            {
                autoRunNotifyTimer.Stop();
                autoRunNotifyTimer.Start();
            }

            //Play the sound
            if (autoRunSound != null)
                autoRunSound.Play();
            //Inform that autosprint is not running
            isAutoRunEnabled = false;
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
            applicationPreferences.toggleHotkey = pref_hotkey_l.SelectedIndex;
            applicationPreferences.sprintHotkey = pref_sprint_hotkey.SelectedIndex;
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
            if (toggleHotkeyLeft != null)
                toggleHotkeyLeft.Dispose();
            if (toggleHotkeyRight != null)
                toggleHotkeyRight.Dispose();
            if (ctrlToggleHotkeyLeft != null)
                ctrlToggleHotkeyLeft.Dispose();
            if (ctrlToggleHotkeyRight != null)
                ctrlToggleHotkeyRight.Dispose();
            if (shiftToggleHotkeyLeft != null)
                shiftToggleHotkeyLeft.Dispose();
            if (shiftToggleHotkeyRight != null)
                shiftToggleHotkeyRight.Dispose();
            if (sprintHotkey != null)
                sprintHotkey.Dispose();
            if (aimPhysicHotkey != null)
                aimPhysicHotkey.Dispose();
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
            PAGE_UP = 33,
            PAGE_DOWN = 34,
            APOSTROPHE = 192,
            MOUSE_6 = 6,
            MOUSE_5 = 5,
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

            public KeyboardHotkey_Interceptor(Window window, int id, ModifierKeyCodes modifierCode, VirtualKeyCodes keyCode)
            {
                //Store the information
                this.window = window;
                this.modifier = modifierCode;
                this.key = keyCode;

                //Prepare the host and identifier of this hotkey registration
                host = new WindowInteropHelper(this.window);
                identifier = (this.window.GetHashCode() + id);

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

            public void ForceOnPressHotkeyEvent()
            {
                //Force the execution of the event "OnPressHotkey"
                if (OnPressHotkey != null)
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
                    if (wParam == (IntPtr)WM_KEYDOWN)   //<- (for more performance, if is a keyboard key pressed down event, send callback)
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
            private const int RI_MOUSE_WHEEL_UP = 120;
            private const int RI_MOUSE_WHEEL_DOWN = -120;
            private const int RI_MOUSE_MIDDLE_DOWN = 16;
            private const int RI_MOUSE_RIGHT_DOWN = 4;
            private const int RI_MOUSE_RIGHT_UP = 8;
            private const int RI_MOUSE_FORWARD_UP = 512;
            private const int RI_MOUSE_BACKWARD_UP = 128;

            //Private variables
            private IntPtr windowHandler = IntPtr.Zero;
            private HwndSource windowSource = null;
            private bool alreadyStartedOnce = false;

            //Public variables
            public RawInputDevice[] inputDevicesList = null;

            //Public callbacks
            public event Action OnWheelScroll;
            public event Action<bool> OnButtonRight;
            public event Action OnButtonMiddle;
            public event Action OnButtonForward;
            public event Action OnButtonBackward;

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
                //RawInputDevice.RegisterDevice(HidUsageAndPage.Mouse, RawInputDeviceFlags.InputSink | RawInputDeviceFlags.NoLegacy, windowHandler);
                RawInputDevice.RegisterDevice(HidUsageAndPage.Mouse, RawInputDeviceFlags.InputSink, windowHandler); //"InputSink" work inside game, "ExInputSink" not work inside game

                //Register the hook
                windowSource = HwndSource.FromHwnd(windowHandler);
                windowSource.AddHook(Hook);

                //Inform that already started
                alreadyStartedOnce = true;
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
                            if (mouse.Mouse.ButtonData == RI_MOUSE_WHEEL_UP || mouse.Mouse.ButtonData == RI_MOUSE_WHEEL_DOWN)  //<- If is mouse scroll up or down
                                if (OnWheelScroll != null)
                                    OnWheelScroll();
                            if (((int)mouse.Mouse.Buttons) == RI_MOUSE_MIDDLE_DOWN)  //<- If is mouse middle button down
                                if (OnButtonMiddle != null)
                                    OnButtonMiddle();
                            if (((int)mouse.Mouse.Buttons) == RI_MOUSE_RIGHT_DOWN)  //<- If is mouse right button down
                                if (OnButtonRight != null)
                                    OnButtonRight(true);
                            if (((int)mouse.Mouse.Buttons) == RI_MOUSE_RIGHT_UP)  //<- If is mouse right button up
                                if (OnButtonRight != null)
                                    OnButtonRight(false);
                            if (((int)mouse.Mouse.Buttons) == RI_MOUSE_FORWARD_UP)  //<- If is mouse forward button up
                                if (OnButtonForward != null)
                                    OnButtonForward();
                            if (((int)mouse.Mouse.Buttons) == RI_MOUSE_BACKWARD_UP)  //<- If is mouse backward button up
                                if (OnButtonBackward != null)
                                    OnButtonBackward();
                            break;
                    }
                }

                return IntPtr.Zero;
            }

            public void StopWatch()
            {
                //If is never started, cancel
                if (alreadyStartedOnce == false)
                    return;

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
                alreadyStartedOnce = false;
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
        public const int MOUSEEVENTF_RIGHTDOWN = 0x08;
        public const int MOUSEEVENTF_RIGHTUP = 0x10;

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

        public static void DoRightMouseClick()
        {
            POINT pnt;
            GetCursorPos(out pnt);

            mouse_event(MOUSEEVENTF_RIGHTDOWN, pnt.X, pnt.Y, 0, 0);
            mouse_event(MOUSEEVENTF_RIGHTUP, pnt.X, pnt.Y, 0, 0);
        }

        #endregion

        #region ControlPressSimulationInterface

        const int VK_CONTROL = 0x11;
        const uint KEYEVENTF_EXTENDEDKEY = 0x0001;
        const uint KEYEVENTF_KEYUP = 0x0002;


        [DllImport("user32.dll")]
        public static extern void keybd_event(byte bVk, byte bScan, uint dwFlags, uint dwExtraInfo);


        public static void DoCtrlKeyboardPress_Down()
        {
            keybd_event((byte)VK_CONTROL, 0x45, KEYEVENTF_EXTENDEDKEY, 0);
        }

        public static void DoCtrlKeyboardPress_Up()
        {
            keybd_event((byte)VK_CONTROL, 0x45, KEYEVENTF_EXTENDEDKEY, 0);
            keybd_event((byte)VK_CONTROL, 0x45, KEYEVENTF_EXTENDEDKEY | KEYEVENTF_KEYUP, 0);
        }

        #endregion
    }
}
