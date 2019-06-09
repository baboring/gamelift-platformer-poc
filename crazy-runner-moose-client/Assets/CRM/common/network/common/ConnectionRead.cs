

using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;



public class ConnectionRead {
  public static MessageSubscription Read(
    CancelableTcpClient client,
    MonoBehaviour parent,
    MessageTransferLookUp lookUp
  ) {
    var crossThread = new ConnectionReadCrossThread(parent, lookUp, client.GetCancellationTokenSource());
    byte[] readBuffer = new byte[ushort.MaxValue];
    var dataStream = new MemoryStream(readBuffer);
    var reader = new BinaryReader(dataStream);
    Task.Factory.StartNew(() => {
      while (true) {
        dataStream.Position = 0;
        reader.BaseStream.Position = 0;
        int read = 0;
        while (read < 4) {
          read = read + client.Read(readBuffer, read, 4 - read);
        }
        var dataLength = reader.ReadUInt16();
        var opCode = reader.ReadUInt16();
        while (read < 4 + dataLength) {
          read = read + client.Read(readBuffer, read, (dataLength + 4) - read);
        }
        var data = lookUp.Deserialize(opCode, reader);
        crossThread.Recieve(opCode, data);
      }
    }, client.GetCancellationTokenSource().Token).ContinueWith((task) => {
        Debug.LogError("exception in task");
        Debug.LogError(task.Exception);
    }, TaskContinuationOptions.OnlyOnFaulted);
    return crossThread;;
  }
}