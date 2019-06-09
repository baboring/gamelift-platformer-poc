
public interface PlayerGuidMessage : MessageTransferable{
  string PlayerGuid {get;}
}

public class PlayerGuidUtil {
  public static bool IsFor(MessageTransferable toCheck, string guid){
    return toCheck is PlayerGuidMessage ? ((PlayerGuidMessage)toCheck).PlayerGuid.Equals(guid) : false;
  }

  public static string Get(MessageTransferable toCheck){
    return toCheck is PlayerGuidMessage ? ((PlayerGuidMessage)toCheck).PlayerGuid : string.Empty;
  }
}