using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;


class ConnectionWrite {
  
  public static MessageHandler Write(CancelableTcpClient client, MessageTransferLookUp lookUp) { 
    var messageData = NetworkMessageSerializer.Get(lookUp);
    return (opCode, toSend) => {
      int length = messageData.serialize(opCode, toSend);
      client.Write(messageData.buffer, 0, length);
      return length;
    };
  }

}