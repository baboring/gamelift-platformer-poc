import uuid from 'uuid';
import { GameLift } from 'aws-sdk';
import { CreatePlayerSessionInput } from 'aws-sdk/clients/gamelift';
import { getGameLiftConfig } from './get-session/get-gamelift-config';
import { findAvailableGameSessions } from './get-session/find-available-game-sessions';
import { createGameSession } from './get-session/create-game-session';
import { pollForActiveSession } from './get-session/poll-for-active-session';

const gameLift = new GameLift(getGameLiftConfig());

export const handler = async () => {
  const gameSessions = await findAvailableGameSessions(gameLift);
  const selectedGameSession = gameSessions.length > 0 ? gameSessions[0] : await createGameSession(gameLift);
  console.log('game session created or found: ' + selectedGameSession.GameSessionId);
  const activeGameSession = await pollForActiveSession(gameLift, selectedGameSession);
  const input: CreatePlayerSessionInput = {
    GameSessionId : activeGameSession.GameSessionId || '' ,
    PlayerId: uuid.v4(),
  };
  const data = await gameLift.createPlayerSession(input).promise();
  if (!data.PlayerSession) {
    throw new Error('player session missing on create');
  }
  console.log('created player session ID: ', data.PlayerSession.PlayerSessionId);
  return data.PlayerSession;
};
