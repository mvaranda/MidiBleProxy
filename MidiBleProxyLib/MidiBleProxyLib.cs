using System;
using System.Text;

using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Diagnostics;

/*
 Protocol:
 =========

    frame:
      first byte: cmd
      second and third bytes: payload len (zero if no payload) 
      forth byte and beyond: payload

    From network client (MIDI Desktop App) to this server:
      see MidiBleProxyLib CMD__xxx definitions

    To network client notifications:
      See MidiBleProxyLib NOTIF__xxx definitions

 */

// State object for reading client data asynchronously  
public class StateObject
{
    // Client  socket.  
    public Socket workSocket = null;
    // Size of receive buffer.  
    public const int BufferSize = 3; //1024;
    // Receive buffer.  
    public byte[] buffer = new byte[BufferSize];
    // Received data string.  
    public StringBuilder sb = new StringBuilder();
}

namespace MidiBleCommLib
{
    public class MidiBleProxyLib
    {
        const Boolean FULL_PROTOCOL = false;

        const int CMD__START_SCAN = 0x30;
        const int CMD__STOP_SCAN = 0x31;
        const int CMD__CONNECT = 0x32;         // payload = device name
        const int CMD__DISCONNECT = 0x33;
        const int CMD__DATA = 0x34;            // payload = data (max len 65535)

        //  To network client notifications (to MIDI Desktop App):
        const int NOTIF__SCAN_STARTED = 0x41;
        const int NOTIF__SCAN_STOPPED = 0x42;
        const int NOTIF__SCAN_ENTRY = 0x43;      //  payload with name, address and rssi(ASCII separated by ",")
        const int NOTIF__CONNECTED = 0x44;
        const int NOTIF__DISCONNECTED = 0x45;
        const int NOTIF__CONNECT_ERROR = 0x46;
        const int NOTIF__DATA = 0x47;            // (max payload len 65535)
        const int NOTIF__WRITE_OK = 0x48;
        const int NOTIF__WRITE_ERROR = 0x49;
        //const int NOTIF__PROXY_CONNECTED = 0x50;
        //const int NOTIF__PROXY_DISCONNECTED = 0x51;

        private static MidiComm stComm;
        private static int port;
        Thread ProxyServerThreadRef;
        static Socket listener;
        static Socket connClientSocket = null;

        private static readonly object lockerObj = new object();
        private const int MAX_NUM_SOCKETS = 5;
        private static Socket[] allSockets = new Socket[MAX_NUM_SOCKETS];
        private static Boolean exitFlag = false;

        public static void closeAllSockets()
        {
            if (connClientSocket != null) {
                try
                {
                    connClientSocket.Shutdown(SocketShutdown.Both);
                }
                catch (SocketException e)
                {
                    Debug.WriteLine("socket with no connection... tolerates");
                }
                connClientSocket.Close();
                connClientSocket = null;
            }

            if (listener != null)
            {
                try
                {
                    listener.Shutdown(SocketShutdown.Both);
                }
                catch (SocketException e)
                {
                    Debug.WriteLine("socket with no connection... tolerates");
                }
                listener.Close();
                listener = null;
            }

        }

        public MidiBleProxyLib(int _port, MidiComm _stcomm)
        {
            stComm = _stcomm;
            port = _port;
            Notifier.MidiCommNotify += this.onMidiCommNotification;
            ProxyServerThreadRef = new Thread(new ThreadStart(StartListening));
            ProxyServerThreadRef.Start();
        }

        // Thread signal.  
        public static ManualResetEvent allDone = new ManualResetEvent(false);

        public static void StartListening()
        {
            IPAddress ipAddress = System.Net.IPAddress.Parse("127.0.0.1");
            IPEndPoint localEndPoint = new IPEndPoint(ipAddress, port);

            // Create a TCP/IP socket.  
            listener = new Socket(ipAddress.AddressFamily,
                SocketType.Stream, ProtocolType.Tcp);

            // Bind the socket to the local endpoint and listen for incoming connections.  
            try
            {
                listener.Bind(localEndPoint);
                listener.Listen(100);

                while (true)
                {
                    // Set the event to nonsignaled state.  
                    allDone.Reset();

                    // Start an asynchronous socket to listen for connections.  
                    Debug.WriteLine("Waiting for a connection...");
                    listener.BeginAccept(
                        new AsyncCallback(AcceptCallback),
                        listener);

                    // Wait until a connection is made before continuing.  
                    allDone.WaitOne();
                }

            }
            catch (Exception e)
            {
                Debug.WriteLine(e.ToString());
            }

            Debug.WriteLine("\nProxy: Leaving Server Loop");

        }

        public static void AcceptCallback(IAsyncResult ar)
        {
            // Signal the main thread to continue.  
            allDone.Set();

            if (exitFlag) return;

            // Get the socket that handles the client request.  
            Socket _listener = (Socket)ar.AsyncState;
            Socket handler = _listener.EndAccept(ar);

            if(connClientSocket != null) // close existing connection
            {
                try
                {
                    connClientSocket.Shutdown(SocketShutdown.Both);
                }
                catch (SocketException e)
                {
                    Debug.WriteLine("socket with no connection... tolerates");
                }
                connClientSocket.Close();
            }
            connClientSocket = handler;

            // Create the state object.  
            StateObject state = new StateObject();
            state.workSocket = handler;

            // TODO: this RX is bad... TCP may send partial data or consecutive packages... we should have a state machine
            //   and first only ask for 3 bytes (in loop until get it) and then other request(s) for the remaining payload if any.
            //   will implement it later (as for localhost it should be fine).
            handler.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0,
                new AsyncCallback(ReadCallback), state);
        }

        public static void ReadCallback(IAsyncResult ar)
        {
            if (exitFlag)
            {
                return;
            }
            String content = String.Empty;

            // Retrieve the state object and the handler socket  
            // from the asynchronous state object.  
            StateObject state = (StateObject)ar.AsyncState;
            Socket handler = state.workSocket;
            int bytesRead = 0;

            // Read data from the client socket.  
            try
            {
                bytesRead = handler.EndReceive(ar);
            }
            catch (SocketException e)
            {
                if (e.ErrorCode == 10054) // client disconnection
                {
                    // disconnec
                    handler.Close();
                    //removeSocket(handler);
                    connClientSocket = null;
                    stComm.reqDisconnect("dummy");
                    return;
                }
            }
            catch (System.ObjectDisposedException)
            {
                Debug.WriteLine("Warning... fix me: ObjectDisposedException");
            }

            byte[] payload = null;

            if (bytesRead > 0)
            {
                if (FULL_PROTOCOL == false)
                {
                    payload = new byte[bytesRead];
                    System.Buffer.BlockCopy(state.buffer, 0, payload, 0, bytesRead);
                    stComm.reqMidiWrite(payload);
                }

                else {

                    int len = 0;
                    if (bytesRead >= 3)
                    {
                        len = (state.buffer[1] << 8) + state.buffer[2];
                        if (len > 0)
                        {
                            payload = new byte[len];
                            //byte[] byteArray = ASCIIEncoding.ASCII.GetBytes(state.sb.ToString());
                            System.Buffer.BlockCopy(state.buffer, 3, payload, 0, len); // CRASH HERE
                        }
                    }
                    else
                    {
                        Debug.WriteLine("Warning... incoming package too short");
                        handler.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0,
                        new AsyncCallback(ReadCallback), state);
                        return;
                    }

                    switch ((int)state.buffer[0])
                    {
                        case CMD__START_SCAN:
                            stComm.reqStartScan(10000);
                            break;
                        case CMD__STOP_SCAN:
                            stComm.reqStopScan();
                            break;
                        case CMD__CONNECT:              // payload = device name
                            if (len > 0)
                            {
                                stComm.reqConnect(Encoding.ASCII.GetString(payload, 0, payload.Length));
                            }
                            break;
                        case CMD__DISCONNECT:              // payload = device name
                            stComm.reqDisconnect(null);
                            break;
                        case CMD__DATA:                 // payload = data (max len 65535)
                            if (len > 0)
                            {
                                stComm.reqMidiWrite(payload);
                            }
                            break;
                        default:
                            Debug.WriteLine("unknown package");
                            break;

                    }
                }

                try
                {
                    handler.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0,
                    new AsyncCallback(ReadCallback), state);
                }
                catch (SocketException e)
                {
                    Debug.WriteLine("BeginReceive: TODO: check if need to close socket or if already done");
                    Debug.WriteLine(e.ToString());
                    return;
                }
            }
        }

        private static void Send(Socket handler, byte[] byteData)
        {
            // Begin sending the data to the remote device. 
            try
            {
                handler.BeginSend(byteData, 0, byteData.Length, 0,
                new AsyncCallback(SendCallback), handler);
            }
            catch (SocketException e)
            {
                Debug.WriteLine("BeginReceive: TODO: check if need to close socket or if already done");
                Debug.WriteLine(e.ToString());
                return;
            }
        }

        private static void SendCallback(IAsyncResult ar)
        {
            try
            {
                // Retrieve the socket from the state object.  
                Socket handler = (Socket)ar.AsyncState;

                // Complete sending the data to the remote device.  
                int bytesSent = handler.EndSend(ar);
                Debug.WriteLine("Sent {0} bytes to client.", bytesSent);

            }
            catch (Exception e)
            {
                Debug.WriteLine(e.ToString());
            }
        }

        public void closeProxy()
        {
            exitFlag = true;
            closeAllSockets();
        }

        private void onMidiCommNotification(object src, EventArgs args)
        {

            MidiCommNotif notif = (MidiCommNotif)args;
            String txt = "";
            byte[] shortPayload = new byte[3];
            shortPayload[1] = 0;
            shortPayload[2] = 0;
            if (notif.name != null) txt = "  txt: " + notif.name;
            String log_txt = "Proxy onMidiCommNotification: " + notif.notif2txt() + txt;
            Debug.WriteLine(log_txt);
            //t_log.Text += "new Log Message" + Environment.NewLine;
            if (connClientSocket == null)
            {
                Debug.WriteLine("Proxy: ignoring notification (client socket=null)");
                return;
            }

            switch (notif.notification)
            {
                case MidiCommNotif.NOTIF.SCAN_STARTED:
                    if (FULL_PROTOCOL == true)
                    {
                        shortPayload[0] = NOTIF__SCAN_STARTED;
                        Send(connClientSocket, shortPayload);
                    }
                    break;

                case MidiCommNotif.NOTIF.SCAN_STOPPED:
                    if (FULL_PROTOCOL == true)
                    {
                        shortPayload[0] = NOTIF__SCAN_STOPPED;
                        Send(connClientSocket, shortPayload);
                    }
                        break;

                case MidiCommNotif.NOTIF.SCAN_ENTRY:
                    if (FULL_PROTOCOL == true)
                    {
                        log_txt = "Proxy:   rssi: " + notif.signal.ToString() + ", addr = " + notif.addr;
                        Debug.WriteLine(log_txt);
                        {
                            String s = notif.name + "," + notif.addr + "," + notif.signal.ToString();
                            byte[] payload = Encoding.ASCII.GetBytes(s);
                            byte[] p = new byte[s.Length + 3];

                            p[0] = NOTIF__SCAN_ENTRY;
                            p[1] = (byte)((s.Length >> 8) & 0x000000ff);
                            p[2] = (byte)(s.Length & 0x000000ff);
                            System.Buffer.BlockCopy(payload, 0, p, 3, payload.Length);
                            Send(connClientSocket, p);
                        }
                    }
                    break;

                case MidiCommNotif.NOTIF.CONNECTED:
                    if (FULL_PROTOCOL == true)
                    {
                        shortPayload[0] = NOTIF__CONNECTED;
                        Send(connClientSocket, shortPayload);
                    }
                    break;

                case MidiCommNotif.NOTIF.DISCONNECTED:
                    if (FULL_PROTOCOL == true)
                    {
                        shortPayload[0] = NOTIF__DISCONNECTED;
                        Send(connClientSocket, shortPayload);
                    }
                    break;

                case MidiCommNotif.NOTIF.CONNECT_ERROR:
                    if (FULL_PROTOCOL == true)
                    {
                        shortPayload[0] = NOTIF__CONNECT_ERROR;
                        Send(connClientSocket, shortPayload);
                    }
                    break;

                case MidiCommNotif.NOTIF.UART_WRITE_OK:
                case MidiCommNotif.NOTIF.UART_WRITE_ERROR:
                    break;

                case MidiCommNotif.NOTIF.MIDI_WRITE_OK:
                    if (FULL_PROTOCOL == true)
                    {
                        shortPayload[0] = NOTIF__WRITE_OK;
                        Send(connClientSocket, shortPayload);
                    }
                    break;

                case MidiCommNotif.NOTIF.MIDI_WRITE_ERROR:
                    if (FULL_PROTOCOL == true)
                    {
                        shortPayload[0] = NOTIF__WRITE_ERROR;
                        Send(connClientSocket, shortPayload);
                    }
                    break;

                case MidiCommNotif.NOTIF.UART_RX_DATA:
                    break;

                case MidiCommNotif.NOTIF.MIDI_RX_DATA:
                    if (FULL_PROTOCOL != true)
                    {
                        Send(connClientSocket, notif.data);
                        break;
                    }

                    txt = System.Text.Encoding.UTF8.GetString(notif.data, 0, (int)notif.data.Length);
                    txt = txt.Replace("\n", "\r\n");
                    Debug.WriteLine("Proxy RX Data:\n" + txt);
                    {
                        byte[] payload = notif.data;
                        byte[] p = new byte[payload.Length + 3];

                        p[0] = NOTIF__DATA;
                        p[1] = (byte)((payload.Length >> 8) & 0x000000ff);
                        p[2] = (byte)(payload.Length & 0x000000ff);
                        System.Buffer.BlockCopy(payload, 0, p, 3, payload.Length);
                        Send(connClientSocket, p);
                    }
                    break;

                default: Debug.WriteLine("Proxy onMidiCommNotification: unknown notification"); break;
            }
        }

    }

}
