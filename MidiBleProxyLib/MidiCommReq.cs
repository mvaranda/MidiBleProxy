using System;
using System.Collections.Generic;
using System.Text;

namespace MidiBleCommLib
{
    // Request
    public class MidiCommReq
    {

        public enum CMD : int
        {
            REQ_START_SCAN,
            REQ_STOP_SCAN,
            REQ_CONNECT,
            REQ_DISCONNECT,
            REQ_UART_WRITE,
            REQ_MIDI_WRITE,
            REQ_EXIT,
        }

        public MidiCommReq(CMD req, String txt, int i, byte[] bytes)
        {
            this.req = req;
            this.v_int = i;
            this.v_txt = txt;
            this.v_bytes = bytes;
        }

        public String req2Text()
        {
            switch (req)
            {
                case CMD.REQ_START_SCAN: return "REQ_START_SCAN";
                case CMD.REQ_STOP_SCAN: return "REQ_STOP_SCAN";
                case CMD.REQ_CONNECT: return "REQ_CONNECT";
                case CMD.REQ_DISCONNECT: return "REQ_DISCONNECT";
                case CMD.REQ_UART_WRITE: return "REQ_UART_WRITE";
                case CMD.REQ_MIDI_WRITE: return "REQ_MIDI_WRITE";

                case CMD.REQ_EXIT: return "REQ_EXIT";
                default: return "INVALID";
            }
        }


        public CMD req;
        public int v_int;
        public String v_txt;
        public byte[] v_bytes;
    }
}
