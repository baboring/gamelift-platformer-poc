using System;


public class OpCode{
  public const ushort PLAYER_JOIN = 0;
  public const ushort PLAYER_ACCEPTED = 1;
  public const ushort PLAYER_POSITION = 2;
  public const ushort PLAYER_INPUT = 3;
}

public class MessageAssociations {
  public static MessageTuple[] ALL = new MessageTuple[]{
    new MessageTuple(OpCode.PLAYER_JOIN, typeof(PlayerJoinMessage)),
    new MessageTuple(OpCode.PLAYER_ACCEPTED, typeof(PlayerAcceptMessage)),
    new MessageTuple(OpCode.PLAYER_POSITION, typeof(PositionMessage)),
    new MessageTuple(OpCode.PLAYER_INPUT, typeof(PlayerInputMessage)),
  };
}