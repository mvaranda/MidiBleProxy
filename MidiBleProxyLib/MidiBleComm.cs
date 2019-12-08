using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.Devices.Enumeration;
using Windows.Devices.Bluetooth;
using Windows.Devices.Bluetooth.GenericAttributeProfile;
using System.Threading;
using System.Text.RegularExpressions;
using System.Collections.Concurrent;
using System.Data;
using System.Diagnostics;
using Windows.Storage.Streams;
using System.Runtime.InteropServices.WindowsRuntime;



namespace MidiBleCommLib
{
    public class Notifier
    {
        public delegate void StBleCommEventHandler(object src, EventArgs args);
        static public event StBleCommEventHandler MidiCommNotify;

        static public void onNotify(EventArgs n)
        {
            if (MidiCommNotify != null)
            {
                MidiCommNotify(null, n); // EventArgs.Empty);
            }
        }
    }

    public class MidiComm
    {

        private class CService
        {
            public String uuid;
            public GattDeviceService service;
            private IReadOnlyList<GattCharacteristic> mCharacteristics; // = new List<GattCharacteristic>();
            public CService(GattDeviceService service)
            {
                this.service = service;
                uuid = service.Uuid.ToString();
                populateCharList();
            }

            async void populateCharList()
            {
                {
                    // BT_Code: Get all the child characteristics of a service. Use the cache mode to specify uncached characterstics only 
                    // and the new Async functions to get the characteristics of unpaired devices as well.
                    var result = await service.GetCharacteristicsAsync(BluetoothCacheMode.Uncached);
                    if (result.Status == GattCommunicationStatus.Success)
                    {
                        mCharacteristics = result.Characteristics;
                        Debug.WriteLine("Service: " + uuid);

                        if (mCharacteristics.Count > 0)
                        {
                            for (int i = 0; i < mCharacteristics.Count; i++)
                            {
                                Debug.WriteLine("  Char UUID: " + mCharacteristics[i].Uuid.ToString());
                            }
                        }
                        else
                        {
                            Debug.WriteLine("  Service don't have any characteristic.");
                            return;
                        }
                    }
                    else
                    {
                        Debug.WriteLine("Error accessing service.");
                    }
                }

            }
            public IReadOnlyList<GattCharacteristic> getCharList() { return mCharacteristics; }
        }
        static List<CService> mServices = new List<CService>();

        private const String LIB_VERSION = "0.0.1";

        public Notifier notifier = new Notifier();
        private readonly object lockerObj = new object();
        private Boolean isConnected = false;
        private Boolean scanActive = false;


        // Request queue:
        private const int BQUEUE_MAX_SIZE = 5;
        BlockingCollection<MidiCommReq> bQueue = new BlockingCollection<MidiCommReq>(BQUEUE_MAX_SIZE);

        static string CLRF = (Console.IsOutputRedirected) ? "" : "\r\n";

        // "Magic" string for all BLE devices
        static string _aqsAllBLEDevices = "(System.Devices.Aep.ProtocolId:=\"{bb7bb05e-5972-42b5-94fc-76eaa7084d49}\")";
        static string[] _requestedBLEProperties = { "System.Devices.Aep.DeviceAddress",
                                                    "System.Devices.Aep.Bluetooth.Le.IsConnectable",
                                                    "System.Devices.Aep.SignalStrength",};

        static List<DeviceInformation> _deviceList = new List<DeviceInformation>();
        static BluetoothLEDevice _selectedDevice = null;

        // Only one registered characteristic at a time.
        static List<GattCharacteristic> _subscribers = new List<GattCharacteristic>();

        // Variables for "foreach" loop implementation
        //static List<string> _forEachCommands = new List<string>();
        //static List<string> _forEachDeviceNames = new List<string>();

        static bool _primed = false;

        static TimeSpan _timeout = TimeSpan.FromSeconds(3);

        private Thread bleServicethreadRef;

        private DeviceWatcher gWatcher = null;

        // Public functions called from client thread

        public MidiComm()
        {
            Debug.WriteLine("MidiComm Constructor called");
            bleServicethreadRef = new Thread(new ThreadStart(bleServicethread));
            bleServicethreadRef.Start();
        }

        // ---- multithead access variables ----
        // locker: lockerObj = new object();
        // vars: isConnected, scanActive
        private void updateIsConnected(Boolean b)
        {
            lock (lockerObj)
            {
                isConnected = b;
            }
        }
        private Boolean getIsConnected()
        {
            Boolean b;
            lock (lockerObj)
            {
                b = isConnected;
            }
            return b;
        }
        private void updateScanActive(Boolean b)
        {
            lock (lockerObj)
            {
                scanActive = b;
            }
        }
        private Boolean getScanActive()
        {
            Boolean b;
            lock (lockerObj)
            {
                b = scanActive;
            }
            return b;
        }
        //---------------------------------------


        public Boolean reqStartScan(int scan_time)
        {
            // TODO: decide or not to allow scan while connected?
            if (getIsConnected() == true)
            {
                Debug.WriteLine("reqStartScan: Scan disable while connected");
                return false;
            }

            if (getScanActive() == true)
            {
                Debug.WriteLine("reqStartScan: Scan already active, ignoring it");
                return false;
            }
            MidiCommReq req = new MidiCommReq(MidiCommReq.CMD.REQ_START_SCAN, null, scan_time, null);
            bQueue.Add(req);
            updateScanActive(true);
            return true;
        }
        public Boolean reqStopScan()
        {
            // for now we fake we stop scanning as the MS has a 30 seconds fixed scanning time.
            // We just reset scanActive so the watcher start ignoring new devices.

            //MidiCommReq req = new MidiCommReq(MidiCommReq.CMD.REQ_STOP_SCAN, null, 0, null);
            //bQueue.Add(req);

            if (getScanActive() == false)
            {
                Debug.WriteLine("reqStopScan: Scan is not active, ignoring it");
                return false;
            }

            updateScanActive(false);
            MidiCommNotif n = new MidiCommNotif(MidiCommNotif.NOTIF.SCAN_STOPPED, null, null, 0, null, null, null);
            Notifier.onNotify(n);
            return true;

        }
        public void reqConnect(String Addr)
        {
            MidiCommReq req = new MidiCommReq(MidiCommReq.CMD.REQ_CONNECT, Addr, 0, null);
            bQueue.Add(req);
        }
        public void reqDisconnect(String Addr)
        {
            MidiCommReq req = new MidiCommReq(MidiCommReq.CMD.REQ_DISCONNECT, Addr, 0, null);
            bQueue.Add(req);
        }

        public void reqMidiWrite(byte[] data)
        {
            MidiCommReq req = new MidiCommReq(MidiCommReq.CMD.REQ_MIDI_WRITE, null, 0, data);
            bQueue.Add(req);
        }

        public void reqUartWrite(String char_id, byte[] data)
        {
            MidiCommReq req = new MidiCommReq(MidiCommReq.CMD.REQ_UART_WRITE, char_id, 0, data);
            bQueue.Add(req);
        }

        public void reqExit()
        {
            MidiCommReq req = new MidiCommReq(MidiCommReq.CMD.REQ_EXIT, null, 0, null);
            bQueue.Add(req);
        }

        //--------- private methods processed from internal thread ------

        private void bleServicethread()
        {
            Boolean loop = true;
            MidiCommReq req;
            int ret;

            while (loop)
            {
                Debug.WriteLine("Wait for a Request");
                req = bQueue.Take();
                switch (req.req)
                {
                    case MidiCommReq.CMD.REQ_START_SCAN: ret = onReqStartScan(req.v_int); break;
                    case MidiCommReq.CMD.REQ_STOP_SCAN: ret = onReqStopScan(); break;
                    case MidiCommReq.CMD.REQ_CONNECT: ret = onReqConnect(req.v_txt); break;
                    case MidiCommReq.CMD.REQ_DISCONNECT: ret = onReqDisconnect(req.v_txt); break;
                    case MidiCommReq.CMD.REQ_UART_WRITE: ret = onReqUartWrite(req.v_bytes); break;
                    case MidiCommReq.CMD.REQ_MIDI_WRITE: ret = onReqMidiWrite(req.v_bytes); break;

                    case MidiCommReq.CMD.REQ_EXIT: loop = false; break;
                    default: break;
                }

            }
            Debug.WriteLine("bleServicethread: Ending thread");
        }

        // Processed in 
        private int onReqStartScan(int scan_time) {
            MidiCommNotif n = new MidiCommNotif(MidiCommNotif.NOTIF.SCAN_STARTED, null, null, 0, null, null, null);
            Notifier.onNotify(n);
            _deviceList.Clear();

            // Start endless BLE device watcher
            var watcher = DeviceInformation.CreateWatcher(_aqsAllBLEDevices, _requestedBLEProperties, DeviceInformationKind.AssociationEndpoint);
            gWatcher = watcher;
            watcher.Added += (DeviceWatcher sender, DeviceInformation devInfo) =>
            {
                if (getScanActive() == true)
                {
                   // Debug.WriteLine("Scan: new device (or update)");
                    if (_deviceList.FirstOrDefault(d => d.Id.Equals(devInfo.Id) || d.Name.Equals(devInfo.Name)) == null) {
                        if (devInfo.Name.Length > 1) // TODO: also filter out non ST9 terminals
                            //if (devInfo.Name.IndexOf("ORBC_UART") == 0) {
                                {
                                    int rssi = (int)devInfo.Properties.Single(d => d.Key == "System.Devices.Aep.SignalStrength").Value;
                                    String addr = (String)devInfo.Properties.Single(d => d.Key == "System.Devices.Aep.DeviceAddress").Value;
                                    Debug.WriteLine("RSSI: " + rssi.ToString());
                                    Debug.WriteLine("ADDR: " + addr);
                                    _deviceList.Add(devInfo);
                                    n = new MidiCommNotif(MidiCommNotif.NOTIF.SCAN_ENTRY, devInfo.Name, addr, rssi, null, null, null);
                                    Notifier.onNotify(n);
                                }
                            //}
                    }
                }
                //else
                //{
                //    Debug.WriteLine("Scan: ignoring new device (or update)");
                //}
            };
            watcher.Updated += (_, __) => { }; // We need handler for this event, even an empty!
            /*watcher.Updated += (DeviceWatcher sender, DeviceInformationUpdate devInfo) =>
            {
                Debug.WriteLine("Update name: " + devInfo.Properties.Single(d => d.Key == "System.Devices.Aep.SignalStrength").Value.ToString());
            };*/


            //Watch for a device being removed by the watcher
            //watcher.Removed += (DeviceWatcher sender, DeviceInformationUpdate devInfo) =>
            //{
            //    _deviceList.Remove(FindKnownDevice(devInfo.Id));
            //};
            watcher.EnumerationCompleted += (DeviceWatcher sender, object arg) => {
                gWatcher = null;
                sender.Stop();
                if (getScanActive() == true)
                {
                    updateScanActive(false);
                    n = new MidiCommNotif(MidiCommNotif.NOTIF.SCAN_STOPPED, null, null, 0, null, null, null);
                    Notifier.onNotify(n);
                }
            };

            watcher.Stopped += (DeviceWatcher sender, object arg) => {
                //_deviceList.Clear(); 
                sender.Start(); 
            };
            watcher.Start();

            return 0;
        }
        private int onReqStopScan()
        {
            // Not really called as we handle at reqStopScan (while it seem to be impossible to Stop the Watcher.. we just start ignoring new entries)
            Debug.WriteLine("onReqStopScan: !!!");

            if (getScanActive() == false)
            {
                Debug.WriteLine("reqStopScan: Scan is not active, ignoring it");
                return -1; // false;
            }

            updateScanActive(false);
            MidiCommNotif n = new MidiCommNotif(MidiCommNotif.NOTIF.SCAN_STOPPED, null, null, 0, null, null, null);
            Notifier.onNotify(n);

            return 0;
        }
        private int onReqConnect(String addr)
        {
            //OpenDevice(name, notifier);
            OpenDeviceByAddress(addr, notifier);
            return 0;
        }
        private int onReqDisconnect(String Addr)
        {
            CloseDevice(notifier);
            return 0;
        }

        private int onReqMidiWrite(byte[] data)
        {
            MidiProxyProtocol.sendMsg(data);
            return 0;
        }

        private int onReqUartWrite(byte[] data)
        {
            //UartProtocol.sendMsg(data);
            return 0;
        }
        private int onReqSetNotification(String CHAR_ID, int set_reset)
        {

            return 0;
        }

        public String getVersion()
        {
            return LIB_VERSION;
        }

        static async Task<int> OpenDevice(string deviceName, Notifier notifier)
        //private void openDevice(String deviceName)
        {
            int retVal = 0;
            MidiCommNotif n;

            if (!string.IsNullOrEmpty(deviceName))
            {
                var devs = _deviceList.OrderBy(d => d.Name).Where(d => !string.IsNullOrEmpty(d.Name)).ToList();
                string foundId = Utilities.GetIdByNameOrNumber(devs, deviceName);
                // Id sample = "BluetoothLE#BluetoothLEf4:b7:e2:03:0b:7e-d6:7a:16:22:f5:c4"

                // If device is found, connect to device and enumerate all services
                if (!string.IsNullOrEmpty(foundId))
                {
                    //_selectedCharacteristic = null;
                    //_selectedService = null;
                    //_services.Clear();
                    mServices.Clear();
                    try
                    {
                        // only allow for one connection to be open at a time
                        if (_selectedDevice != null)
                            CloseDevice(notifier);

                        _selectedDevice = await BluetoothLEDevice.FromIdAsync(foundId).AsTask().TimeoutAfter(_timeout);
                        Debug.WriteLine($"Connecting to {_selectedDevice.Name}.");

                        var result = await _selectedDevice.GetGattServicesAsync(BluetoothCacheMode.Uncached);
                        if (result.Status == GattCommunicationStatus.Success)
                        {
                            Debug.WriteLine($"Found {result.Services.Count} services:");

                            for (int i = 0; i < result.Services.Count; i++)
                            {
                                var serviceToDisplay = new BluetoothLEAttributeDisplay(result.Services[i]);
                                CService s = new CService(result.Services[i]);
                                mServices.Add(s);
                            }

                            // hack:
                            enableNotification(MidiProxyProtocol.MIDI_SERVICE_UUID, MidiProxyProtocol.RX_CHAR_UUID);

                            n = new MidiCommNotif(MidiCommNotif.NOTIF.CONNECTED, "Connected, num Services: " + result.Services.Count, null, 0, null, null, null);
                            Notifier.onNotify(n);
                        }
                        else
                        {
                            Debug.WriteLine($"Device {deviceName} is unreachable.");
                            n = new MidiCommNotif(MidiCommNotif.NOTIF.CONNECT_ERROR, "Device unreachable.", null, 0, null, null, null);
                            Notifier.onNotify(n);
                            retVal += 1;
                        }
                    }
                    catch
                    {
                        Debug.WriteLine($"Device {deviceName} is unreachable.");
                        n = new MidiCommNotif(MidiCommNotif.NOTIF.CONNECT_ERROR, "Device unreachable.", null, 0, null, null, null);
                        Notifier.onNotify(n);
                        retVal += 1;
                    }
                }
                else
                {
                    n = new MidiCommNotif(MidiCommNotif.NOTIF.CONNECT_ERROR, "Device not found.", null, 0, null, null, null);
                    Notifier.onNotify(n);
                    retVal += 1;
                }
            }
            else
            {
                Debug.WriteLine("Device name can not be empty.");
                n = new MidiCommNotif(MidiCommNotif.NOTIF.CONNECT_ERROR, "Device name can not be empty.", null, 0, null, null, null);
                Notifier.onNotify(n);
                retVal += 1;
            }
            return retVal;
        }

        static async Task<int> OpenDeviceByAddress(string addr, Notifier notifier)
        {
            int retVal = 0;
            MidiCommNotif n;

            try
            {
                string hex = addr.Replace(":", "");
                ulong u_addr = Convert.ToUInt64(hex, 16);

                _selectedDevice = await BluetoothLEDevice.FromBluetoothAddressAsync(u_addr).AsTask().TimeoutAfter(_timeout);
                Debug.WriteLine($"Connecting to {_selectedDevice.Name}.");

                var result = await _selectedDevice.GetGattServicesAsync(BluetoothCacheMode.Uncached);
                if (result.Status == GattCommunicationStatus.Success)
                {
                    Debug.WriteLine($"Found {result.Services.Count} services:");

                    for (int i = 0; i < result.Services.Count; i++)
                    {
                        var serviceToDisplay = new BluetoothLEAttributeDisplay(result.Services[i]);
                        CService s = new CService(result.Services[i]);
                        mServices.Add(s);
                    }

                    // hack:
                    enableNotification(MidiProxyProtocol.MIDI_SERVICE_UUID, MidiProxyProtocol.RX_CHAR_UUID);

                    n = new MidiCommNotif(MidiCommNotif.NOTIF.CONNECTED, "Connected, num Services: " + result.Services.Count, null, 0, null, null, null);
                    Notifier.onNotify(n);
                }
                else
                {
                    Debug.WriteLine($"Device {addr} is unreachable.");
                    n = new MidiCommNotif(MidiCommNotif.NOTIF.CONNECT_ERROR, "Device unreachable.", null, 0, null, null, null);
                    Notifier.onNotify(n);
                    retVal += 1;
                }
            }
            catch
            {
                Debug.WriteLine($"Device {addr} is unreachable.");
                n = new MidiCommNotif(MidiCommNotif.NOTIF.CONNECT_ERROR, "Device unreachable.", null, 0, null, null, null);
                Notifier.onNotify(n);
                retVal += 1;
            }
            return retVal;
        }

        static async Task<Boolean> CloseDevice(Notifier notifier)
        {
            // Remove all subscriptions
            // if (_subscribers.Count > 0) Unsubscribe("all");

            if (_selectedDevice != null)
            {
                var status = await disableNotification(MidiProxyProtocol.MIDI_SERVICE_UUID, MidiProxyProtocol.RX_CHAR_UUID);
                if (status == false)
                {
                    Debug.WriteLine("CloseDevice: disableNotification fail");
                }
                Debug.WriteLine($"Device {_selectedDevice.Name} is disconnected.");

                foreach (CService s in mServices)
                {
                    s.service?.Dispose();
                }




                mServices.Clear();
                _selectedDevice?.Dispose();
            }
            MidiCommNotif n = new MidiCommNotif(MidiCommNotif.NOTIF.DISCONNECTED, null, null, 0, null, null, null);
            Notifier.onNotify(n);
            return true;

        }


        static async Task<Boolean> writeChar(GattCharacteristic c, byte[] bytes)
        {
            MidiCommNotif n;
            // Write data to characteristic
            IBuffer buffer = bytes.AsBuffer();
            //var buffer = Utilities.FormatData(data, _dataFormat);
            GattWriteResult result = await c.WriteValueWithResultAsync(buffer);
            if (result.Status != GattCommunicationStatus.Success)
            {
                Debug.WriteLine($"writeChar failed: {result.Status}");
                n = new MidiCommNotif(MidiCommNotif.NOTIF.MIDI_WRITE_ERROR, null, null, 0, null, null, null);
                Notifier.onNotify(n);
                return false;
            }
            n = new MidiCommNotif(MidiCommNotif.NOTIF.MIDI_WRITE_OK, null, null, 0, null, null, null);
            Notifier.onNotify(n);
            return true;
        }
        public static async Task<Boolean> writeChar(String service_uuid, String char_uuid, byte[] data)
        {
            // Write data to characteristic
            GattCharacteristic c;
            //CService s;
            if (mServices.Count < 1)
            {
                Debug.WriteLine($"writeChar failed: no service (list is empty)");
                return false;
            }
            foreach (CService s in mServices)
            {
                if (s.uuid.Equals(service_uuid))
                {
                    IReadOnlyList<GattCharacteristic> chars = s.getCharList();
                    if (chars.Count < 1)
                    {
                        Debug.WriteLine($"writeChar failed: no Characteristic (list is empty)");
                        return false;
                    }
                    if (chars.Count > 0)
                    {
                        foreach (GattCharacteristic _char in chars)
                        {
                            if (_char.Uuid.ToString().Equals(char_uuid))
                            {
                                return await writeChar(_char, data);
                            }
                        }
                        Debug.WriteLine($"writeChar failed: no matching Characteristic uuid (1)");
                        return false;
                    }
                }
            }
            Debug.WriteLine($"writeChar failed: no matching service uuid");
            return false;
        }

        static async Task<Boolean>enableNotification(String service_uuid, String char_uuid)
        {
            // Write data to characteristic
            GattCharacteristic c;
            //CService s;
            if (mServices.Count < 1)
            {
                Debug.WriteLine($"writeChar failed: no service (list is empty)");
                return false;
            }
            foreach (CService s in mServices)
            {
                if (s.uuid.Equals(service_uuid))
                {
                    IReadOnlyList<GattCharacteristic> chars = s.getCharList();
                    if (chars.Count < 1)
                    {
                        Debug.WriteLine($"writeChar failed: no Characteristic (list is empty)");
                        return false;
                    }
                    if (chars.Count > 0)
                    {
                        foreach (GattCharacteristic _char in chars)
                        {
                            if (_char.Uuid.ToString().Equals(char_uuid))
                            {
                                var status = await _char.WriteClientCharacteristicConfigurationDescriptorAsync(GattClientCharacteristicConfigurationDescriptorValue.Notify);
                                if (status == GattCommunicationStatus.Success)
                                {
                                    _char.ValueChanged += Characteristic_ValueChanged;
                                    return true;
                                }
                                else
                                {
                                    Debug.WriteLine($"Can't enable notification to characteristic");
                                    return false;
                                }
                            }
                        }
                        Debug.WriteLine("writeChar failed: no matching Characteristic uuid: " + char_uuid);
                        return false;
                    }
                }
            }
            Debug.WriteLine($"writeChar failed: no matching service uuid");
            return false;

        }

        static async Task<Boolean> disableNotification(String service_uuid, String char_uuid)
        {
            // Write data to characteristic
            GattCharacteristic c;
            //CService s;
            if (mServices.Count < 1)
            {
                Debug.WriteLine($"writeChar failed: no service (list is empty)");
                return false;
            }
            foreach (CService s in mServices)
            {
                if (s.uuid.Equals(service_uuid))
                {
                    IReadOnlyList<GattCharacteristic> chars = s.getCharList();
                    if (chars.Count < 1)
                    {
                        Debug.WriteLine($"writeChar failed: no Characteristic (list is empty)");
                        return false;
                    }
                    if (chars.Count > 0)
                    {
                        foreach (GattCharacteristic _char in chars)
                        {
                            if (_char.Uuid.ToString().Equals(char_uuid))
                            {
                                //var status = await _char.WriteClientCharacteristicConfigurationDescriptorAsync(GattClientCharacteristicConfigurationDescriptorValue.Notify);
                                var status = await _char.WriteClientCharacteristicConfigurationDescriptorAsync(GattClientCharacteristicConfigurationDescriptorValue.None);
                                if (status == GattCommunicationStatus.Success)
                                {
                                    _char.ValueChanged -= Characteristic_ValueChanged;
                                    return true;
                                }
                                else
                                {
                                    Debug.WriteLine($"Can't enable notification to characteristic");
                                    return false;
                                }
                            }
                        }
                        Debug.WriteLine("writeChar failed: no matching Characteristic uuid: " + char_uuid);
                        return false;
                    }
                }
            }
            Debug.WriteLine($"writeChar failed: no matching service uuid");
            return false;

        }

        static void Characteristic_ValueChanged(GattCharacteristic sender, GattValueChangedEventArgs args)
        {

            byte[] rawBytes = new byte[args.CharacteristicValue.Length];
            using (var reader = DataReader.FromBuffer(args.CharacteristicValue))
            {
                reader.ReadBytes(rawBytes);
            }
            String txt = System.Text.Encoding.UTF8.GetString(rawBytes, 2, (int) args.CharacteristicValue.Length - 2);

            Debug.WriteLine($"Value changed for {sender.Uuid}: {txt}");
            MidiProxyProtocol.handleRxPackage(rawBytes);
        }
    }

}
