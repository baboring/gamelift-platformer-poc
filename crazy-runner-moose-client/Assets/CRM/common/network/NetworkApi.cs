
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

public class NetworkApi {
  public static readonly MessageTransferLookUp LOOK_UP = new MessageTransferLookUp(MessageAssociations.ALL);

  public static Task<NetworkMessageMultiStream> AcceptInbound(MonoBehaviour parent, int port) {
    return ConnectionInBound.Accept(port, parent, LOOK_UP);
  }

  public static NetworkMessageStream OpenOutbound(int port, string address, MonoBehaviour parent) { 
    return ConnectionOutBound.Open(port, address, parent, LOOK_UP);
  }

}
