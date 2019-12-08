using System;
using System.Linq;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Runtime.InteropServices.WindowsRuntime;

/*
     Copied from lua implementation:

protocol:
========

Frame format:

A HTTP request is sent by a client, mobile or desktop device acting as a BLE central, inside a HTTP Request Message to the ST9 Terminal.
The terminal then sends back a reply inside a HTTP Reply message.
HTTP Messages do not have a size limitation. As the BLE implementation for
 Generic Messages has a size limitation (about 157 bytes) then the messages are carried by Frames.
Each Frame has a 2 bytes header and a payload that can carry up to 150 bytes. If a HTTP Message length is grater than
 the payload maximoun size then 150 bytes then the message is carried by multiple frames.
NOte: Android has 20 bytes limitation for sending BLE packages even increasing MTU. 
Therefore, From central to peripheral we limit frame size in 20 bytes. But from Peripheral to central Android is happy with ~150 bytes.


Request:
========

Link layer: 
|------|-----|----------------------------------------------------|
| type | seq |                       payload                      |
|------|-----|----------------------------------------------------|


App layer for a HTTP Request Message:
             |---------------------------------------|
             | Target Name with optional query pairs |
             |---------------------------------------|

App layer for a HTTP Reply Message:
             |------|--------------------------------|
             | mime |            HTTP body           |
             |------|--------------------------------|

  where mime ends with a '\n' character

Where:
  type = 0x80: message is completed in a single frame;
  type = 0x81: first frame of a multi-frame message;
  type = 0x82: intermediate frame of a multi-frame message; 
               In case a multi-frame message fits in 2 frames then the will be no intermediate frame for the message
  type = 0x83: final frame of a multi-frame message;
  seq: a byte


Note: Requests/Replies are serialized. Therefore, no need to keep track of who has requested as an unique cycle happens at a given time.

*/

namespace MidiBleCommLib
{
    class MidiProxyProtocol
    {
        //public static String MIDI_SERVICE_UUID = "813b0001-9a08-4262-8a10-8527ff2dbce8";
        //public static String TX_CHAR_UUID = "813b0005-9a08-4262-8a10-8527ff2dbce8"; // to ST
        //public static String RX_CHAR_UUID = "813b0006-9a08-4262-8a10-8527ff2dbce8"; // from st (notification)

        public static String MIDI_SERVICE_UUID = "03b80e5a-ede8-4b33-a751-6ce34ec4c700";
        public static String CHAR_UUID =       "7772e5db-3868-4112-a1a9-f2669d106bf3";
        public static String TX_CHAR_UUID = "7772e5db-3868-4112-a1a9-f2669d106bf3"; // to ST
        public static String RX_CHAR_UUID = "7772e5db-3868-4112-a1a9-f2669d106bf3"; // from st (notification)

        static int rxSec = 0;
        static int FRAME_TYPE__SINGLE = 0x80;
        static int FRAME_TYPE__MULTI_FIRST = 0x81;
        static int FRAME_TYPE__MULTI_INTER = 0x82;
        static int FRAME_TYPE__MULTI_FINAL = 0x83;
        static int ST_IDLE = 1;
        static int ST_RX_MULTIFRAME = 2;
        static int rxState = ST_IDLE;
        static int rxSeq = ST_IDLE;
        static byte[] msg;

        static int sendState = ST_RX_MULTIFRAME;
        static int MAX_PAYLOAD_SIZE = 18;
        static int sendIdx = 0;
        static int totalToSend;
        static int toSend;
        static int txSeq;
        static byte[] msgToSend;

        static public byte[] getEncodedMidi(byte[] midiMsg)
        {
            byte[] ret = new byte[midiMsg.Length + 2];
            DateTime dt = DateTime.Now;
            //getting Milliseconds only from the currenttime
            int ms = dt.Millisecond; // hhhhhhlllllll
            ret[0] = (byte)(ms >> 7);
            ret[0] &= 0x3f;
            ret[0] |= 0x80;
            ret[1] = (byte) ms;
            ret[1] |= 0x80;
            System.Buffer.BlockCopy(midiMsg, 0, ret, 2, midiMsg.Length);
            return ret;

        }


        static public void handleRxPackage(byte[] frame)
        {
            MidiCommNotif n = new MidiCommNotif(MidiCommNotif.NOTIF.MIDI_RX_DATA, null, null, 0, frame, null, null);
            Notifier.onNotify(n);

            //if (frame.Length < 2)
            //{
            //    Debug.WriteLine("handleRxPackage: package too short, ignoring");
            //    return;
            //}
            //int seq = frame[1] & 0xff;
            //int frame_type = frame[0] & 0xff;
            //byte[] payload = new byte[frame.Length - 2];

            //MidiCommNotif n = new MidiCommNotif(MidiCommNotif.NOTIF.MIDI_RX_DATA, null, null, 0, payload, null, null);
            //Notifier.onNotify(n);
        }

        public static async Task<Boolean> sendMsg(byte[] msg)
        { // callback calls with msg = null to process the completion of the message in progress
            int frame_type;
            byte[] m;

            if (msg != null)
            { // function called from upper App layer
                m = getEncodedMidi(msg);
                Debug.WriteLine("sendMsg: len = " + m.Length.ToString());
                await MidiComm.writeChar(MidiProxyProtocol.MIDI_SERVICE_UUID, MidiProxyProtocol.TX_CHAR_UUID, m);
                return true;
            }
            return false;
        }

    }
}
