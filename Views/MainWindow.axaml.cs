using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Media;

namespace MyApp
{
    public partial class MainWindow : Window
    {
        private UdpClient? _udpClient;
        private bool _isConnected;
        private Button? _joinButton;
        private readonly IBrush _defaultButtonColor = new SolidColorBrush(Colors.Gray);
        public MainWindow()
        {

            InitializeComponent();
#if DEBUG
            this.AttachDevTools();
#endif
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
        private async void OnJoinButtonClick(object sender, RoutedEventArgs e)
        {
            if (_isConnected)
            {
                _udpClient?.Close();
                _udpClient = null;
                _isConnected = false;
                if (_joinButton != null)
                {
                    _joinButton.Background = _defaultButtonColor;
                    _joinButton.SetValue(Button.ContentProperty, "Join");
                }
            }
            else
            {
                try
                {
                    var ipAddressBox = this.FindControl<TextBox>("IpAddressBox");
                    var portBox = this.FindControl<TextBox>("PortBox");
                    var messageTextBox = this.FindControl<TextBox>("MessageTextBox");
                    var VFOAFreqBox = this.FindControl<TextBox>("VFOAFreqBox");
                    var VFOBFreqBox = this.FindControl<TextBox>("VFOBFreqBox");
                    var VFOAModeBox = this.FindControl<ComboBox>("VFOAModeBox");
                    var VFOBModeBox = this.FindControl<ComboBox>("VFOBModeBox");

                    IPAddress multicastAddress = IPAddress.Parse("224.0.0.1");
                    if (ipAddressBox != null && ipAddressBox.Text != null) multicastAddress = IPAddress.Parse(ipAddressBox.Text);

                    int port = 4532;
                    if (portBox != null && portBox.Text != null) port = int.Parse(portBox.Text);

                    _udpClient = new UdpClient(port);
                    _udpClient.JoinMulticastGroup(multicastAddress);

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
                    string modeList = "VFOA";

                    while (_isConnected)
                    {
                        UdpReceiveResult result = await _udpClient.ReceiveAsync();
                        string message = Encoding.UTF8.GetString(result.Buffer);
                        if (message != null)
                        {
                            messageTextBox!.Text = PrettifyJson(message);
                            RootObject? json = DataParser.ParseMulticastDataPacket(message);
                            if (json == null || json.vfos == null)
                            {
                                continue;
                            }

                            if (json.vfos[0].freq != frequencyA)
                            {
                                frequencyA = json.vfos[0].freq;
                                VFOAFreqBox!.Text = json.vfos[0].freq.ToString();
                            }
                            if (json.vfos[1].freq != frequencyB)
                            {
                                frequencyB = json.vfos[1].freq;
                                VFOBFreqBox!.Text = json.vfos[1].freq.ToString();
                            }
                            if (json.vfos[0] != null)
                            {
                                if (json.vfos[0].mode != modeA)
                                {
                                    if (json.vfos[0].mode != null && json.vfos.Count >= 1)
                                    {
                                        
                                    modeA = json.vfos[0].mode ?? "";
                                    VFOAModeBox!.SelectedItem = modeA;
                                    }
                                }
                            }
                            if (json.vfos[1].mode != modeB)
                            {
                                if (json.vfos != null && json.vfos.Count >= 2 && json.vfos[1] != null && json.vfos[1].mode != null)
                                {
                                    modeB = json.vfos[1].mode ?? "";
                                    VFOBModeBox!.SelectedItem = modeB;
                                }
                                else
                                {
                                    modeB = "None";
                                }
                            }
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
                            if (json.vfos != null && json.vfos[0].rx == true && json.vfos[0].tx == true)
                            {
                                VFOAFreqBox!.Foreground = new SolidColorBrush(Colors.White);
                                VFOBFreqBox!.Foreground = new SolidColorBrush(Colors.Gray);
                            }
                            else if (json.vfos != null && json.vfos[1].rx == true && json.vfos[1].tx == true)
                            {
                                VFOAFreqBox!.Foreground = new SolidColorBrush(Colors.Gray);
                                VFOBFreqBox!.Foreground = new SolidColorBrush(Colors.White);
                            }
                            else if (json.vfos != null && json.vfos[0].rx == true && json.vfos[1].tx == true)
                            {
                                VFOAFreqBox!.Foreground = new SolidColorBrush(Colors.Green);
                                VFOBFreqBox!.Foreground = new SolidColorBrush(Colors.Red);
                            }
                            else if (json.vfos != null && json.vfos[0].tx == true && json.vfos[1].rx == true)
                            {
                                VFOAFreqBox!.Foreground = new SolidColorBrush(Colors.Red);
                                VFOBFreqBox!.Foreground = new SolidColorBrush(Colors.Green);
                            }
                            else
                            {
                                VFOAFreqBox!.Foreground = new SolidColorBrush(Colors.Gray);
                                VFOBFreqBox!.Foreground = new SolidColorBrush(Colors.Gray);
                            }
                        }
                    }
                }
                
                catch (Exception ex)
                {
                    var messageTextBox = this.FindControl<TextBox>("MessageTextBox");
                    if (messageTextBox != null) messageTextBox.Text = "Error: " + ex.Message;
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
    }
}
