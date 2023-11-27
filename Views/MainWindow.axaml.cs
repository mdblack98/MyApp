using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.Rendering;
using Avalonia.Styling;
using Newtonsoft.Json;
//using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace HamlibGUI
{
    public partial class MainWindow : Window
    {
        private UdpClient? _udpClient;
        private bool _isConnected;
        private Button? _joinButton;
        private readonly IBrush _defaultButtonColor = new SolidColorBrush(Colors.Gray);
        private bool isSelectionEventEnabledVFOAFreqBox = false;
        private bool isSelectionEventEnabledVFOBFreqBox = false;
        private bool isSelectionEventEnabledVFOAModeBox = false;
        private bool isSelectionEventEnabledVFOBModeBox = false;
        private const string ConfigFileName = "HamlibGUI.json";
//        private Color ColorRx = Colors.Green;
//       private Color ColorTx = Colors.Yellow;
//        private Color ColorPTT = Colors.Red;
//        private Color ColorNA = Colors.Gray;

        public MainWindow()
        {

            InitializeComponent();
#if DEBUG
            this.AttachDevTools();
#endif
            this.Opened += OnWindowOpened;
            this.Closing += OnWindowClosing;

            _joinButton = this.FindControl<Button>("JoinButton");
            if (_joinButton != null && _joinButton.Background != null) _defaultButtonColor = _joinButton.Background;
            /*
            PlotModel = new PlotModel { Title = "Cosine Function" };
            var cosineSeries = new LineSeries { Title = "Cosine" };

            for (double x = 0; x < 2 * Math.PI; x += 0.1)
            {
                cosineSeries.Points.Add(new DataPoint(x, Math.Cos(x)));
            }

            PlotModel.Series.Add(cosineSeries);
            */

            // Handle our events
            var VFOAFreqBox = this.FindControl<TextBox>("VFOAFreqBox");
            var VFOBFreqBox = this.FindControl<TextBox>("VFOBFreqBox");
            var VFOAModeBox = this.FindControl<ComboBox>("VFOAModeBox");
            var VFOBModeBox = this.FindControl<ComboBox>("VFOBModeBox");
            var DebugBox = this.FindControl<TextBlock>("DebugBox");
            var idBox = this.FindControl<ComboBox>("IdBox");
            var joinButton = this.FindControl<CheckBox>("JoinAtStartup");


            if (VFOAFreqBox != null)
            {
                // Handle Enter key press
                VFOAFreqBox.KeyDown += (sender, e) =>
                {
                    if (e.Key == Key.Enter)
                    {
                        // Enter key was pressed
                        // Your code to handle Enter key press here
                        var freq = VFOAFreqBox.Text;
                        var id = idBox!.SelectedItem!.ToString();
                        //if (id != null && freq != null)
                            //RigSetFreq(id, "VFOA", Double.Parse(freq));
                        //var buf = (Encoding.UTF8.GetBytes("{\"cmd\":\"set_vfoa\",\"freq\":" + VFOAFreqBox.Text + "}"), Encoding.UTF8.GetBytes("{\"cmd\":\"set_vfoa\",\"freq\":" + VFOAFreqBox.Text + "}").Length);
                        //_udpClient!.Client.SendTo(buf, remoteEP);
                    }
                
                };

                // Handle Escape key press
                VFOAFreqBox.KeyDown += (sender, e) =>
                {
                    if (e.Key == Key.Escape)
                    {
                        // Escape key was pressed
                        // Your code to handle Escape key press here
                    }
                };

                // Handle focus loss
                VFOAFreqBox.LostFocus += (sender, e) =>
                {
                    DebugBox!.Text += "VFOAFreqBox Lost Focus\n";   
                    // VFOAFreqBox lost focus
                    // Your code to handle focus loss here
                };
            }
            if (VFOBFreqBox != null)
            {
                // Handle Enter key press
                VFOBFreqBox.KeyDown += (sender, e) =>
                {
                    if (e.Key == Key.Enter)
                    {
                        var buf = "{\"cmd\":\"set_vfob\",\"freq\":" + VFOBFreqBox.Text + "}";   
                        // Enter key was pressed
                        // Your code to handle Enter key press here
                    }
                };

                // Handle Escape key press
                VFOBFreqBox.KeyDown += (sender, e) =>
                {
                    if (e.Key == Key.Escape)
                    {
                        // Escape key was pressed
                        // Your code to handle Escape key press here
                    }
                };

                // Handle focus loss
                VFOBFreqBox.LostFocus += (sender, e) =>
                {
                    DebugBox!.Text += "VFOBFreqBox Lost Focus\n";
                    // VFOAFreqBox lost focus
                    // Your code to handle focus loss here
                };
            }
            if (VFOAModeBox != null)
            {
                VFOAModeBox.SelectionChanged += (sender, e) =>
                {
                    if (isSelectionEventEnabledVFOAModeBox)
                    { 
                        var selectedMode = VFOAModeBox.SelectedItem?.ToString();
                        DebugBox!.Text += DateTime.Now.ToString("HH:mm:ss") + " " + "VFOAMode=" + selectedMode + "\n";
                        DebugBox!.Text += sender!.ToString() + "\n";
                    }
                };
            }
            if (VFOBModeBox != null)
            {
                VFOBModeBox.SelectionChanged += (sender, e) =>
                {
                    if (isSelectionEventEnabledVFOBModeBox)
                    {
                        var selectedMode = VFOBModeBox.SelectedItem?.ToString();
                        DebugBox!.Text += DateTime.Now.ToString("HH:mm:ss") + " " + "VFOBMode=" + selectedMode + "\n";
                    }
                };
            }
        }

        private void OnWindowOpened(object sender, EventArgs e)
        {
            var joinButton = this.FindControl<CheckBox>("JoinAtStartup");

            // Restore the window size and position
            if (File.Exists(ConfigFileName))
            {
                var settings = JsonConvert.DeserializeObject<WindowSettings>(File.ReadAllText(ConfigFileName));
                if (settings != null)
                {
                    this.Width = settings.Width;
                    this.Height = settings.Height;
                    this.Position = new PixelPoint(settings.Left, settings.Top);
                    joinButton!.IsChecked = settings.JoinAtStartup;
                }
                if (joinButton == null) return;
                if ((bool)joinButton.IsChecked!)
                {
                    JoinMulticast();
                }
            }
        }

        public class WindowSettings
        {
            public int Width { get; set; }
            public int Height { get; set; }
            public int Left { get; set; }
            public int Top { get; set; }
            public bool JoinAtStartup { get; set; }
        }
        private void OnWindowClosing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            var joinButton = this.FindControl<CheckBox>("JoinAtStartup");

            // Save the window size and position
            var settings = new WindowSettings
            {
                Width = (int)this.Width,
                Height = (int)this.Height,
                Left = this.Position.X,
                Top = this.Position.Y,
                JoinAtStartup = (bool)joinButton!.IsChecked!
            };

            File.WriteAllText(ConfigFileName, JsonConvert.SerializeObject(settings));
        }
    
        public class Id
        {
            public string ?id { get; set; }
            public DateTime time { get; set; }
        }

        List<Id> ids = new List<Id>();

        public class SetFreq
        {
            public string? id { get; set;}
            public string? cmd { get; set; }
            public string? vfo { get; set; }
            public double freq { get; set; }    
        }

        private void RigSetFreq(string id, string vfo, double freq)
        {
            var idBox = this.FindControl<ComboBox>("IdBox");
            var setFreq = new SetFreq { id = id, cmd = "set_freq", vfo = vfo, freq = freq };
            string jsonString = System.Text.Json.JsonSerializer.Serialize(setFreq);
            var DebugBox = this.FindControl<TextBlock>("DebugBox");
            DebugBox!.Text += DateTime.Now.ToString("HH:mm:ss") + " " + jsonString + "\n";
        }

        /*
        public PlotModel PlotModel { get; }
         */

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        private static string PrettifyJson(string jsonString)
        {
            using (JsonDocument jsonDocument = JsonDocument.Parse(jsonString))
            {
                // Convert the JsonDocument back to a pretty-printed JSON string
                string prettifiedJson =
                System.Text.Json.JsonSerializer.Serialize(jsonDocument.RootElement, new
                    JsonSerializerOptions
                {
                    WriteIndented = true
                });
                return prettifiedJson;
            }
        }
        private void ClearItemFromIdBox(string itemToRemove)
        {
            var idBox = this.FindControl<ComboBox>("IdBox");

            if (idBox != null && idBox.Items != null)
            {
                for (int i = 0; i < idBox.Items.Count; i++)
                {
                    if (idBox.Items[i]!.ToString() == itemToRemove)
                    {
                        idBox.Items.RemoveAt(i);
                        break; // Exit the loop after removing the item
                    }
                }
            }
        }

        void IdCheck(string id)
        {
            var idBox = this.FindControl<ComboBox>("IdBox");
            if (!idBox!.Items.Contains(id)) idBox.Items.Add(id);
            bool foundIt = false;
            foreach (Id item in ids)
            {
                if (item.id == id)
                {
                    foundIt = true;
                    item.time = DateTime.Now;
                    break;
                }
            }
            if (!foundIt) 
                ids.Add(new Id { id = id, time = DateTime.Now });
            var now = DateTime.Now;
            foreach (Id item in ids)
            { 
                if (now.Subtract(item.time).TotalSeconds > 5)
                {
                    ClearItemFromIdBox(item.id);
                    ids.Remove(item);
                    break;
                }
            }
        }
        private async void JoinMulticast()
        {
            var ipAddressBox = this.FindControl<TextBox>("IpAddressBox");
            var idBox = this.FindControl<ComboBox>("IdBox");
            var portBox = this.FindControl<TextBox>("PortBox");
            var messageTextBox = this.FindControl<TextBox>("MessageTextBox");
            var VFOAFreqBox = this.FindControl<TextBox>("VFOAFreqBox");
            var VFOBFreqBox = this.FindControl<TextBox>("VFOBFreqBox");
            var VFOAModeBox = this.FindControl<ComboBox>("VFOAModeBox");
            var VFOBModeBox = this.FindControl<ComboBox>("VFOBModeBox");
            var DebugBox = this.FindControl<TextBlock>("DebugBox");
            var PTTButton = this.FindControl<Button>("PTT");
            var TabSpectrum = this.FindControl<TabItem>("Spectrum");

            if (_isConnected)
            {
                _udpClient?.Close();
                _udpClient = null;
                _isConnected = false;
                if (_joinButton != null)
                {
                    _joinButton.Background = _defaultButtonColor;
                    _joinButton.SetValue(Button.ContentProperty, "Join");
                    idBox!.Items.Clear();
                    VFOAFreqBox!.Clear();
                    VFOAModeBox!.Items.Clear();
                    VFOBFreqBox!.Clear();
                    VFOBModeBox!.Items.Clear();
                    DebugBox!.Text = "";
                }
            }
            else
            {
                try
                {

                    IPAddress multicastAddress = IPAddress.Parse("224.0.0.1");
                    if (ipAddressBox != null && ipAddressBox.Text != null) multicastAddress = IPAddress.Parse(ipAddressBox.Text);

                    int port = 4532;
                    if (portBox != null && portBox.Text != null) port = int.Parse(portBox.Text);

                    //_udpClient = new UdpClient(port);
                    _udpClient = new UdpClient();
                    _udpClient.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
                    _udpClient.Client.Bind(new IPEndPoint(IPAddress.Any, port));
                    //_udpClient.Client.Connect(multicastAddress, port);
                    NetworkInterface[] nics = NetworkInterface.GetAllNetworkInterfaces();
                    foreach (NetworkInterface adapter in nics)
                    {
                        if (adapter.SupportsMulticast && adapter.OperationalStatus == OperationalStatus.Up)
                        {
                            IPInterfaceProperties ip_properties = adapter.GetIPProperties();
                            foreach (IPAddressInformation uniCast in ip_properties.UnicastAddresses)
                            {
                                if (uniCast.Address.AddressFamily == AddressFamily.InterNetwork)
                                {
                                    _udpClient.JoinMulticastGroup(multicastAddress, uniCast.Address);
                                    break;
                                }
                            }
                        }
                    }
                    //_udpClient.JoinMulticastGroup(multicastAddress);

                    _isConnected = true;
                    if (_joinButton != null)
                    {
                        _joinButton.Background = new SolidColorBrush(Colors.Green);
                        _joinButton.SetValue(Button.ContentProperty, "Stop");
                    }

                    double frequencyA = 0;
                    double frequencyB = 0;
                    string modeA = "None";
                    string modeB = "None";
                    bool modeAChanged = false;
                    bool modeBChanged = false;

                    while (_isConnected)
                    {
                        UdpReceiveResult result = await _udpClient.ReceiveAsync();
                        //_udpClient.Send(Encoding.UTF8.GetBytes("{\"cmd\":\"get_status\"}"), Encoding.UTF8.GetBytes("{\"cmd\":\"get_status\"}").Length, result.RemoteEndPoint);
                        string message = Encoding.UTF8.GetString(result.Buffer);
                        if (message != null)
                        {
                            if (message.Contains("spectrum")) TabSpectrum!.IsVisible = true;
                            else TabSpectrum!.IsVisible = false;
                            RootObject? json = DataParser.ParseMulticastDataPacket(message);
                            if (json == null || json.vfos == null)
                            {
                                DebugBox!.Text = message;
                                continue;
                            }
                            var id = json.rig?.id?.model + " " + json.rig?.id?.endpoint + " " + json.rig?.id?.process;
                            if (json.rig?.id?.deviceId != null)
                            {
                                id += " " + json.rig?.id?.deviceId;
                                IdCheck(id);
                                if (idBox!.Items.Count == 1) idBox.SelectedItem = idBox.Items[0];
                                if (idBox!.SelectedItem != null && idBox.SelectedItem.ToString() != id)
                                {
                                    if (!(idBox.SelectedItem.ToString() == id))  // if not our selected rig we ignore
                                    {
                                        continue;
                                    }
                                }
                                messageTextBox!.Text = PrettifyJson(message);
                                if (json.vfos[0].freq != frequencyA)
                                {
                                    frequencyA = json.vfos[0].freq;
                                    isSelectionEventEnabledVFOAFreqBox = false;
                                    VFOAFreqBox!.Text = json.vfos[0].freq.ToString();
                                    isSelectionEventEnabledVFOAFreqBox = true;
                                }
                                if (json.vfos[1].freq != frequencyB)
                                {
                                    frequencyB = json.vfos[1].freq;
                                    isSelectionEventEnabledVFOBFreqBox = false;
                                    VFOBFreqBox!.Text = json.vfos[1].freq.ToString();
                                    isSelectionEventEnabledVFOBFreqBox = true;
                                }
                                if (json.vfos[0] != null && json.vfos[0].mode != modeA)
                                {
                                    if (json.vfos[0].mode != null && json.vfos.Count >= 1)
                                    {

                                        modeA = json.vfos[0].mode ?? "";
                                        isSelectionEventEnabledVFOAModeBox = false;
                                        VFOAModeBox!.SelectedItem = modeA;
                                        isSelectionEventEnabledVFOAModeBox = true;
                                    }
                                }
                                if (json.vfos[1] != null && json.vfos[1].mode != modeB)
                                {
                                    if (json.vfos != null && json.vfos.Count >= 2 && json.vfos[1] != null && json.vfos[1].mode != null)
                                    {
                                        modeB = json.vfos[1].mode ?? "";
                                        isSelectionEventEnabledVFOBModeBox = false;
                                        VFOBModeBox!.SelectedItem = modeB;
                                        isSelectionEventEnabledVFOBModeBox = true;
                                    }
                                    else
                                    {
                                        modeB = "None";
                                    }
                                }
                                if (VFOAModeBox!.Items.Count > 0 && VFOAModeBox!.SelectedItem != null && VFOAModeBox!.SelectedItem.ToString() != modeA)
                                {
                                    modeA = VFOAModeBox!.SelectedItem!.ToString()!;
                                    // set new mode
                                    modeAChanged = true;
                                }
                                if (VFOAModeBox.Items.Count > 0 && VFOBModeBox!.SelectedItem != null && VFOBModeBox.SelectedItem.ToString() != modeB)
                                {
                                    modeB = VFOBModeBox!.SelectedItem!.ToString()!;
                                    // set new mode
                                    modeBChanged = true;
                                }
                                if (json.rig != null && json.rig.modes != null && (modeAChanged || modeBChanged || VFOAModeBox.Items.Count == 0))
                                {
                                    modeAChanged = false;
                                    modeBChanged = false;
                                    foreach (string token in json.rig.modes)
                                    {
                                        VFOAModeBox!.Items.Add(token);
                                        VFOBModeBox!.Items.Add(token);
                                    }
                                    VFOAModeBox.SetCurrentValue(ComboBox.SelectedItemProperty, modeA);
                                    VFOBModeBox!.SetCurrentValue(ComboBox.SelectedItemProperty, modeB);
                                }
                                /* need to part mode arrqay now
                                if ((json.rig != null && json.rig.modelist != modeList) || modeAChanged || modeBChanged)
                                {
                                    modeAChanged = false;
                                    modeBChanged = false;
                                    if (json.rig != null && json.rig.modelist != null) modeList = json.rig.modelist;
                                    if (VFOAModeBox != null && VFOBModeBox != null && modeList != null)
                                    {
                                        VFOAModeBox.Items.Clear();
                                        VFOBModeBox.Items.Clear();
                                        List<string> tokens = modeList.Split(' ').OrderBy(x => x).ToList();
                                        foreach (string token in tokens)
                                        {
                                            VFOAModeBox.Items.Add(token);
                                            VFOBModeBox.Items.Add(token);
                                        }
                                        VFOAModeBox.SetCurrentValue(ComboBox.SelectedItemProperty, modeA);
                                        if (modeB != "None")
                                        {
                                            VFOBModeBox.IsVisible = true;
                                            VFOBModeBox.SetCurrentValue(ComboBox.SelectedItemProperty, modeB);
                                        }
                                        else
                                        {
                                            VFOBModeBox.IsVisible = false;
                                        }
                                    }
                                }
                                */
                                var ColorRx = new SolidColorBrush(Colors.Green);
                                var ColorTx = new SolidColorBrush(Colors.Yellow);
                                var ColorPTT = new SolidColorBrush(Colors.Red);
                                var ColorNA = new SolidColorBrush(Colors.Gray);
                                var ColorWhite = new SolidColorBrush(Colors.White);

                                PTTButton!.Foreground = ColorWhite;
                                if (json.vfos![0].ptt == true)
                                {
                                    VFOAFreqBox!.Foreground = ColorPTT;
                                    PTTButton!.Foreground = ColorPTT;
                                }
                                else if (json.vfos![1].ptt == true)
                                {
                                    VFOBFreqBox!.Foreground = ColorPTT;
                                    PTTButton!.Foreground = ColorPTT;
                                }
                                else if (json.vfos != null && json.vfos[0].rx == true && json.vfos[0].tx == true)
                                {
                                    VFOAFreqBox!.Foreground = ColorRx;
                                    VFOBFreqBox!.Foreground = ColorNA;
                                }
                                else if (json.vfos != null && json.vfos[1].rx == true && json.vfos[1].tx == true)
                                {
                                    VFOAFreqBox!.Foreground = ColorNA;
                                    VFOBFreqBox!.Foreground = ColorRx;
                                }
                                else if (json.vfos != null && json.vfos[0].rx == true && json.vfos[1].tx == true)
                                {
                                    VFOAFreqBox!.Foreground = ColorRx;
                                    VFOBFreqBox!.Foreground = ColorTx;
                                }
                                else if (json.vfos != null && json.vfos[0].tx == true && json.vfos[1].rx == true)
                                {
                                    VFOAFreqBox!.Foreground = ColorTx;
                                    VFOBFreqBox!.Foreground = ColorRx;
                                }
                                else if (json.vfos![0].ptt == true)
                                {
                                    VFOAFreqBox!.Foreground = ColorTx;
                                    PTTButton!.Foreground = ColorTx;
                                }
                                else if (json.vfos![1].ptt == true)
                                {
                                    VFOBFreqBox!.Foreground = ColorTx;
                                    PTTButton!.Foreground = ColorTx;
                                }

                                else
                                {
                                    VFOAFreqBox!.Foreground = ColorNA;
                                    VFOBFreqBox!.Foreground = ColorNA;
                                }
                            }
                        }
                    }
                }

                catch (Exception ex)
                {
                    var messageTextBox2 = this.FindControl<TextBox>("MessageTextBox");
                    if (messageTextBox2 != null) messageTextBox2.Text = "Error: " + ex.Message;
                    _udpClient?.Close();
                    _udpClient = null;
                    _isConnected = false;
                    if (_joinButton != null)
                    {
                        _joinButton.Background = _defaultButtonColor;
                        _joinButton.SetValue(Button.ContentProperty, "Join");
                    }
                }
            }

        }
        private void OnJoinButtonClick(object sender, RoutedEventArgs e)
        {
            JoinMulticast();
        }
        private void OnPTTClick(object sender, RoutedEventArgs e)
        {

        }
        private void OnSplitClick(object sender, RoutedEventArgs e)
        {

        }

    }
}
