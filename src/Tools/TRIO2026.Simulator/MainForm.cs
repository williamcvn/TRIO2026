using System;
using System.Drawing;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TRIO2026.Simulator
{
    public class MainForm : Form
    {
        private Label lblConnectionStatus;
        private Label lblUvStatus;
        private Label lblDoorStatus;
        private Button btnDoorOpen;
        private Button btnDoorClose;
        private TextBox txtLog;
        private Label lblTitle;

        private TcpListener _listener;
        private TcpClient _connectedClient;
        private NetworkStream _clientStream;
        private CancellationTokenSource _cts;

        private bool _isDoorOpen = false;
        private bool _isUvRunning = false;

        public MainForm()
        {
            InitializeComponent();
            StartServer();
        }

        private void InitializeComponent()
        {
            this.Text = "TRIO2026 Hardware Simulator";
            this.Size = new Size(500, 400);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.Font = new Font("Segoe UI", 10F, FontStyle.Regular, GraphicsUnit.Point);

            lblTitle = new Label { Text = "TRIO 2026 Hardware & Firmware Simulator", Font = new Font("Segoe UI", 12F, FontStyle.Bold), AutoSize = true, Location = new Point(20, 20) };
            
            lblConnectionStatus = new Label { Text = "Connection: Waiting for App...", ForeColor = Color.Orange, AutoSize = true, Location = new Point(20, 60) };
            
            lblUvStatus = new Label { Text = "UV Status: Stopped", AutoSize = true, Location = new Point(20, 90) };
            
            lblDoorStatus = new Label { Text = "Door Status: Closed", AutoSize = true, Location = new Point(20, 115) };

            btnDoorOpen = new Button { Text = "Simulate Door Open", Size = new Size(150, 30), Location = new Point(20, 140) };
            btnDoorOpen.Click += BtnDoorOpen_Click;

            btnDoorClose = new Button { Text = "Simulate Door Close", Size = new Size(150, 30), Location = new Point(180, 140) };
            btnDoorClose.Click += BtnDoorClose_Click;

            txtLog = new TextBox { Multiline = true, ReadOnly = true, ScrollBars = ScrollBars.Vertical, Location = new Point(20, 180), Size = new Size(440, 150) };

            this.Controls.Add(lblTitle);
            this.Controls.Add(lblConnectionStatus);
            this.Controls.Add(lblUvStatus);
            this.Controls.Add(lblDoorStatus);
            this.Controls.Add(btnDoorOpen);
            this.Controls.Add(btnDoorClose);
            this.Controls.Add(txtLog);

            this.FormClosing += MainForm_FormClosing;
        }

        private void Log(string message)
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new Action<string>(Log), message);
                return;
            }
            txtLog.AppendText($"[{DateTime.Now:HH:mm:ss}] {message}{Environment.NewLine}");
        }

        private void UpdateConnectionStatus(bool connected)
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new Action<bool>(UpdateConnectionStatus), connected);
                return;
            }
            if (connected)
            {
                lblConnectionStatus.Text = "Connection: App Connected";
                lblConnectionStatus.ForeColor = Color.Green;
            }
            else
            {
                lblConnectionStatus.Text = "Connection: Waiting for App...";
                lblConnectionStatus.ForeColor = Color.Orange;
                _connectedClient = null;
                _clientStream = null;
            }
        }

        private void UpdateUvStatus(bool isRunning)
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new Action<bool>(UpdateUvStatus), isRunning);
                return;
            }
            if (isRunning)
            {
                lblUvStatus.Text = "UV Status: Running";
                lblUvStatus.ForeColor = Color.Blue;
            }
            else
            {
                lblUvStatus.Text = "UV Status: Stopped";
                lblUvStatus.ForeColor = Color.Black;
            }
        }

        private void StartServer()
        {
            _cts = new CancellationTokenSource();
            Task.Run(async () =>
            {
                try
                {
                    _listener = new TcpListener(IPAddress.Loopback, 5020);
                    _listener.Start();
                    Log("TCP Server started on 127.0.0.1:5020");

                    while (!_cts.IsCancellationRequested)
                    {
                        var client = await _listener.AcceptTcpClientAsync(_cts.Token);
                        Log("App connected.");
                        UpdateConnectionStatus(true);
                        _connectedClient = client;
                        _clientStream = client.GetStream();

                        // Send current states upon connection
                        SendEvent(_isDoorOpen ? "DoorOpened" : "DoorClosed");
                        SendEvent(_isUvRunning ? "UvStarted" : "UvStopped");

                        _ = Task.Run(() => HandleClientAsync(client));
                    }
                }
                catch (OperationCanceledException) { }
                catch (Exception ex)
                {
                    Log($"Server error: {ex.Message}");
                }
            });
        }

        private async Task HandleClientAsync(TcpClient client)
        {
            try
            {
                using var reader = new StreamReader(client.GetStream(), Encoding.UTF8);
                while (client.Connected && !_cts.IsCancellationRequested)
                {
                    var line = await reader.ReadLineAsync();
                    if (line == null) break;

                    Log($"Received: {line}");
                    ProcessCommand(line);
                }
            }
            catch (Exception ex)
            {
                Log($"Client error: {ex.Message}");
            }
            finally
            {
                Log("App disconnected.");
                UpdateConnectionStatus(false);
                client.Close();
            }
        }

        private void ProcessCommand(string json)
        {
            try
            {
                using var doc = JsonDocument.Parse(json);
                if (doc.RootElement.TryGetProperty("Command", out var cmdProp))
                {
                    var cmd = cmdProp.GetString();
                    if (cmd == "StartUV")
                    {
                        _isUvRunning = true;
                        UpdateUvStatus(true);
                        SendEvent("UvStarted");
                    }
                    else if (cmd == "StopUV")
                    {
                        _isUvRunning = false;
                        UpdateUvStatus(false);
                        SendEvent("UvStopped");
                    }
                }
            }
            catch (Exception ex)
            {
                Log($"Parse error: {ex.Message}");
            }
        }

        private void SendEvent(string eventName)
        {
            if (_connectedClient == null || !_connectedClient.Connected || _clientStream == null)
            {
                Log($"Cannot send {eventName}: No client connected.");
                return;
            }

            try
            {
                var json = $"{{\"Event\": \"{eventName}\"}}\n";
                var bytes = Encoding.UTF8.GetBytes(json);
                _clientStream.Write(bytes, 0, bytes.Length);
                Log($"Sent: {json.Trim()}");
            }
            catch (Exception ex)
            {
                Log($"Send error: {ex.Message}");
            }
        }

        private void BtnDoorOpen_Click(object sender, EventArgs e)
        {
            _isDoorOpen = true;
            lblDoorStatus.Text = "Door Status: Open";
            lblDoorStatus.ForeColor = Color.Red;
            SendEvent("DoorOpened");
        }

        private void BtnDoorClose_Click(object sender, EventArgs e)
        {
            _isDoorOpen = false;
            lblDoorStatus.Text = "Door Status: Closed";
            lblDoorStatus.ForeColor = Color.Black;
            SendEvent("DoorClosed");
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            _cts?.Cancel();
            _listener?.Stop();
            _connectedClient?.Close();
        }
    }
}
