using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

class ConnectionOutBound {
  
  public static NetworkMessageStream Open(int port, string address, MonoBehaviour parent, MessageTransferLookUp lookUp) { 
    var addr = IPAddress.Parse(address);
    var client = new CancelableTcpClient(new TcpClient(address, port));
    var messageData = NetworkMessageSerializer.Get(lookUp);
    var recieve =ConnectionRead.Read(client, parent, lookUp);
    MessageHandler send = ConnectionWrite.Write(client, lookUp);
    return new NetworkMessageStream(send, recieve);
  }
}