using System;
using System.Collections.Generic;
using System.Text;

namespace MidiBleCommLib
{

    public class MidiCommNotif : EventArgs
    {
        public enum NOTIF : int
        {
            SCAN_STARTED,
            SCAN_STOPPED,
            SCAN_ENTRY,

            CONNECTED,
            DISCONNECTED,
            CONNECT_ERROR,

            UART_WRITE_OK,
            UART_WRITE_ERROR,

            MIDI_WRITE_OK,
            MIDI_WRITE_ERROR,

            UART_RX_DATA,
            MIDI_RX_DATA
        }

        public NOTIF notification;
        public String name;
        public String addr;
        public int signal;
        public byte[] data;
        public String serviceID;
        public String charID;

        public MidiCommNotif( NOTIF notif,
                            String name,
                            String addr,
                            int signal,
                            byte[] data,
                            String serviceID,
                            String charID)
        {
            notification = notif;
            this.name = name;
            this.addr = addr;
            this.signal = signal;
            this.data = data;
            this.serviceID = serviceID;
            this.charID = charID;
        }

        public String notif2txt()
        {
            switch (notification)
            {
                case NOTIF.SCAN_STARTED:            return "SCAN_STARTED";
                case NOTIF.SCAN_STOPPED:            return "SCAN_STOPPED";
                case NOTIF.SCAN_ENTRY:              return "SCAN_ENTRY";
                case NOTIF.CONNECTED:               return "CONNECTED";
                case NOTIF.DISCONNECTED:            return "DISCONNECTED";
                case NOTIF.CONNECT_ERROR:           return "CONNECT_ERROR";
                case NOTIF.UART_WRITE_OK:           return "UART_WRITE_OK";
                case NOTIF.UART_WRITE_ERROR:        return "UART_WRITE_ERROR";

                case NOTIF.MIDI_WRITE_OK:          return "MIDI_WRITE_OK";
                case NOTIF.MIDI_WRITE_ERROR:       return "MIDI_WRITE_ERROR";

                case NOTIF.UART_RX_DATA:            return "UART_RX_DATA";
                case NOTIF.MIDI_RX_DATA:           return "MIDI_RX_DATA";
                default: return "unknown";
            }

        }
    }


}
