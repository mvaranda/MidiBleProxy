# The client program connects to server and sends data to other connected 
# clients through the server
import socket
import thread
import sys
import time
import binascii

PORT = 8082
DEFAULT_DEVICE = " ec:45:f8:27:52:76"


CMD__START_SCAN = 0x30;
CMD__STOP_SCAN = 0x31;
CMD__CONNECT = 0x32;         # payload = device name
CMD__DISCONNECT = 0x33;
CMD__DATA = 0x34;        # payload = data (max len 65535)

        #  To network client notifications (to OrbUI Desktop App):
NOTIF__SCAN_STARTED = 0x41;
NOTIF__STOPPED = 0x42;
NOTIF__SCAN_ENTRY = 0x43;      #  payload with name, address and rssi(ASCII separated by ",")
NOTIF__CONNECTED = 0x44;
NOTIF__DISCONNECTED = 0x45;
NOTIF__CONNECT_ERROR = 0x46;
NOTIF__DATA = 0x47;            # (max payload len 65535)

def recv_data():
    "Receive data from other clients connected to server"
    while 1:
        try:
            recv_data = client_socket.recv(4096)            
        except:
            #Handle the case when server process terminates
            print "Server closed connection, thread exiting."
            thread.interrupt_main()
            break
        if not recv_data:
                # Recv with no data, server closed connection
                print "Server closed connection, thread exiting."
                thread.interrupt_main()
                break
        else:
                print "Received data: ", recv_data
               
                print(binascii.hexlify(recv_data.encode("utf8")))

USAGE = """Menu:
  1- Start Scan
  2- Stop Scan
  3- Connect to hardcoded device
  4- Send data "blink.json"
  5- Disconnect
  
  Q- exit
"""

def usage():
  print(USAGE)

def send_frame(cmd, payload):
  m = bytearray()
  m.append(cmd)
  sz = len(payload)
  m.append(sz >> 8)
  m.append(sz & 0x000000ff)
  if sz > 0:
    m.extend(payload)
  client_socket.send(m)
  return m

def send_data():
    "Send data from other clients connected to server"
    while 1:
        option = str(raw_input(USAGE))
        if option == "q" or option == "Q":
            #client_socket.send(send_data)
            thread.interrupt_main()
            break
        elif option == "1":
          send_frame(CMD__START_SCAN, "")
        elif option == "2":
          send_frame(CMD__STOP_SCAN, "")        
        elif option == "3":
          send_frame(CMD__CONNECT, DEFAULT_DEVICE)
        elif option == "4":
          #send_frame(CMD__DATA, "blink.json")
          note = b'\x90\x44\x64'
          m = ""
          for n in note:
            m = m + str(n)
          client_socket.send(m)
        elif option == "5":
          send_frame(CMD__DISCONNECT, "")             
        else:
            print ("Invalid option\n\n")
        
if __name__ == "__main__":

    print "*******TCP/IP Chat client program********"
    print "Connecting to server at 127.0.0.1:" + str(PORT)

    client_socket = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
    client_socket.connect(('127.0.0.1', PORT))

    print "Connected to server at 127.0.0.1:" + str(PORT)

    thread.start_new_thread(recv_data,())
    thread.start_new_thread(send_data,())
    
    

    try:
        while 1:
            time.sleep(1)
            continue
    except:
        print "Client program quits...."
        client_socket.close() 