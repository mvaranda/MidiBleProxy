using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using MidiBleCommLib;
using System.Diagnostics;

namespace StBleCommTest
{
    public partial class FormMain : Form
    {
        MidiComm stComm;
        enum CONN_STATE {
            DISCONNECTED,
            CONNECTED,
            CONNECTING,
            DISCONNECTING
        };

        CONN_STATE conn_state = CONN_STATE.DISCONNECTED;
        MidiBleProxyLib proxy;

        public FormMain()
        {
            stComm = new MidiComm ();
            Notifier.MidiCommNotify += this.onMidiCommNotification;
            InitializeComponent();
            Console.WriteLine("Getting version");
            lb_version.Text = stComm.getVersion();
            //bt_connect.Enabled = false;
            t_midi_tx.Enabled = false;
            Font font = new Font("Lucida Console", 10.0f);
            t_midi_rx.Font = font; // new Font(t_midi_rx.Font, FontStyle..Regular);
            t_midi_tx.Font = font;
            t_uart_rx.Font = font;
            t_uart_tx.Font = font;
            t_log.Font = font;

            // Start proxy
            proxy = new MidiBleProxyLib(8082, stComm);
            web.ScriptErrorsSuppressed = true;
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            stComm.reqExit();
            proxy.closeProxy();
        }

        public void updateTextControl(Control c, String text)
        {
            String newText = text;
            c.Invoke((MethodInvoker)delegate {
                // Running on the UI thread
                c.Text = newText;
            });
        }

        public void appendEntryComboBox(ComboBox c, String text)
        {
            String newText = text;
            c.Invoke((MethodInvoker)delegate {
                // Running on the UI thread
                c.Items.Add(text); // new { Text = newText, Value = "" });
                if (c.Items.Count == 1)
                {
                    c.Text = text;
                }
            });
        }

        public void appendTextControl(Control c, String text)
        {
            String newText = text;
            c.Invoke((MethodInvoker)delegate {
                // Running on the UI thread
                c.Text += newText;
            });
        }

        
        public void log_(String text)
        {
            String newText = text + Environment.NewLine;
            t_log.Invoke((MethodInvoker)delegate {
                // Running on the UI thread
                t_log.AppendText(newText);
                t_log.ScrollToCaret();
            });
        }

        private void logColor(String text, Color color)
        {
            t_log.SelectionStart = t_log.TextLength;
            t_log.SelectionLength = 0;

            t_log.SelectionColor = color;
            t_log.AppendText(text + Environment.NewLine);
            t_log.SelectionColor = t_log.ForeColor;
            t_log.ScrollToCaret();
        }

        public void log(String text)
        {
            t_log.AppendText(text + Environment.NewLine);
            t_log.ScrollToCaret();
        }
        public void log_e(String text)
        {
            logColor(text, Color.Red);
        }
        public void log_w(String text)
        {
            logColor(text, Color.DarkSalmon);
        }
        private void onMidiCommNotification(object src, EventArgs args)
        {
            object _src = src;
            EventArgs _args = args;
            // we use t_log but could be any control in the UI thread.
            t_log.Invoke((MethodInvoker)delegate {
                // Running on the UI thread
                onMidiCommNotificationUIThread(_src, _args);
            });
        }

        private void onMidiCommNotificationUIThread(object src, EventArgs args)
        {
            
            MidiCommNotif notif = (MidiCommNotif) args;
            String txt = "";
            String log_txt;
            if (notif.name != null) txt = "  txt: " + notif.name;
            if (notif.notification != MidiCommNotif.NOTIF.MIDI_RX_DATA) // avoid high log traffic
            {
                log_txt = "onMidiCommNotification: " + notif.notif2txt() + txt;
                log(log_txt);
            }
            //t_log.Text += "new Log Message" + Environment.NewLine;

            switch (notif.notification)
            {
                case MidiCommNotif.NOTIF.SCAN_STARTED: 
                    bt_scan.Enabled = true;
                    break;

                case MidiCommNotif.NOTIF.SCAN_STOPPED:
                    
                    if (conn_state == CONN_STATE.DISCONNECTED) 
                    {
                        bt_scan.Enabled = true;
                    }
                    else
                    {
                        bt_scan.Enabled = false;
                    }
                    isScanning = false;
                    bt_scan.Text = "Scan";
                    break;

                case MidiCommNotif.NOTIF.SCAN_ENTRY:
                    //bt_connect.Enabled = true;
                    //appendEntryComboBox(cb_devices, notif.name);
                    appendEntryComboBox(cb_devices, notif.name + ", " + notif.addr);
                    log_txt = "  rssi: " + notif.signal.ToString() + ", addr = " + notif.addr;
                        log_w (log_txt);
                        break; 

                case MidiCommNotif.NOTIF.CONNECTED:
                    conn_state = CONN_STATE.CONNECTED;
                    bt_connect.Text = "Disconnect";
                    bt_scan.Enabled = false;
                    bt_connect.Enabled = true;
                    t_midi_tx.Enabled = true;
                    break;

                case MidiCommNotif.NOTIF.DISCONNECTED:
                    conn_state = CONN_STATE.DISCONNECTED;
                    bt_connect.Text = "Connect";
                    bt_connect.Enabled = true;
                    t_midi_tx.Enabled = false;
                    bt_scan.Enabled = true;
                    break;

                case MidiCommNotif.NOTIF.CONNECT_ERROR:
                    conn_state = CONN_STATE.DISCONNECTED;
                    bt_connect.Text = "Connect";
                    bt_connect.Enabled = true;
                    t_midi_tx.Enabled = false;
                    break;

                case MidiCommNotif.NOTIF.UART_WRITE_OK:
                case MidiCommNotif.NOTIF.UART_WRITE_ERROR:
                    break;

                case MidiCommNotif.NOTIF.MIDI_WRITE_OK:
                case MidiCommNotif.NOTIF.MIDI_WRITE_ERROR:
                    break;

                case MidiCommNotif.NOTIF.UART_RX_DATA:
                    break;

                case MidiCommNotif.NOTIF.MIDI_RX_DATA:
                    //txt = System.Text.Encoding.UTF8.GetString(notif.data, 0, (int)notif.data.Length);
                    //txt = txt.Replace("\n", "\r\n");
                    txt = "RX: " + BitConverter.ToString(notif.data) + "\r\n";
                    t_midi_rx.AppendText(txt);
                    break;

                default: Debug.WriteLine("onMidiCommNotification: unknown notification"); break;
            }
        }

        Boolean isScanning = false;
        private void bt_scan_Click(object sender, EventArgs e)
        {
            if (isScanning == false)
            {
                cb_devices.Text = "";
                cb_devices.Items.Clear();
                stComm.reqStartScan(5000);
                bt_scan.Text = "Stop Scan";
                isScanning = true;
            }
            else
            {
                stComm.reqStopScan();
                bt_scan.Text = "Scan";
                bt_scan.Enabled = false;
                isScanning = false;
            }
        }

        private void bt_stop_Click(object sender, EventArgs e)
        {
            stComm.reqStopScan();
        }

        private void bt_connect_Click(object sender, EventArgs e)
        {
            if (cb_devices.Text.Length < 1)
            {

            }
            if (conn_state == CONN_STATE.DISCONNECTED)
            {
                int addr_start = cb_devices.Text.IndexOf(", ");
                if (addr_start < 1)
                {
                    log("Invalid device name/address");
                    return;
                }
                string addr = cb_devices.Text.Substring(addr_start + 2);

                stComm.reqConnect(addr); // cb_devices.Text);
                conn_state = CONN_STATE.CONNECTING;
                bt_connect.Text = "Connecting";
                bt_connect.Enabled = false;
            }
            if (conn_state == CONN_STATE.CONNECTED)
            {
                stComm.reqDisconnect(cb_devices.Text);
                conn_state = CONN_STATE.DISCONNECTING;
                bt_connect.Text = "Disconnecting";
                bt_connect.Enabled = false;
            }
        }

        public static byte[] StringToByteArray(string hex)
        {
            return Enumerable.Range(0, hex.Length)
                             .Where(x => x % 2 == 0)
                             .Select(x => Convert.ToByte(hex.Substring(x, 2), 16))
                             .ToArray();
        }

        private void onMidiKeyPressed(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == '\r')
            {
                t_log.AppendText("Return pressed\n");
                if (conn_state == CONN_STATE.CONNECTED)
                {
                    byte[] data = StringToByteArray(t_midi_tx.Text); //System.Text.Encoding.UTF8.GetBytes(t_midi_tx.Text);
                    stComm.reqMidiWrite(data);
                    t_midi_tx.Text = "";
                }
                e.Handled = true;
            }
        }

        private void Navigate(String address)
        {
            if (String.IsNullOrEmpty(address)) return;
            if (address.Equals("about:blank")) return;
            if (!address.StartsWith("http://") &&
                !address.StartsWith("https://"))
            {
                address = "http://localhost:8080/" + address;
            }
            try
            {
                web.Navigate(new Uri(address));
            }
            catch (System.UriFormatException)
            {
                return;
            }
        }

        private void b_go_Click(object sender, EventArgs e)
        {
            Navigate(t_addr.Text);
        }

        private void bt_menu_Click(object sender, EventArgs e)
        {
            //web.InvokeScript("myFunction");
        }

        private void b_home_Click(object sender, EventArgs e)
        {
            t_addr.Text = "index.html";
            Navigate("index.html");
        }
    }
}
