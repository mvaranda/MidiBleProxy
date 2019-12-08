
import mido
import sys
import time
#import socket

def play_file(filename):
  mid = mido.MidiFile(filename)
  for i, track in enumerate(mid.tracks):
    print('Track {}: {}'.format(i, track.name))
    for msg in track:
      #if not msg.is_meta:
      print ("sleep time = " + str(msg.time))
      time.sleep(msg.time/1000)
      print(msg)
      
def play_file2(filename, server):
  with mido.open_output() as output:
    try:
        for message in mido.MidiFile(filename).play():
            try:
              message.note
            except AttributeError:
              note_exists = False
            else:
              note_exists = True

            if note_exists == False:
              print("Not a note, skip")
              continue
            print(message)
            print("  --> " + message.hex())
            #output.send(message)
            if server != None:
              try:
                server.send(message)
              finally:
                print("!!!!  socket send ERROR")
                server.reset()
            else:
              output.send(message)
            time.sleep(1)

    except KeyboardInterrupt:
        print()
        output.reset()
    
if __name__== "__main__":
  if len(sys.argv) < 2:
    print("missing filename")
    exit(0)
  serverport = mido.sockets.connect("localhost", 8082)
  #serverport = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
  #serverport.connect(("localhost", 8082))
  if serverport == None:
    print("!!! Fail to connect to server")
    
  play_file2(sys.argv[1], serverport)
  
  
