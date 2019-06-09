import { GameSession, GameSessionStatus } from 'aws-sdk/clients/gamelift';
import { GameLift } from 'aws-sdk';

const activeStatus: GameSessionStatus = 'ACTIVE';

export const pollForActiveSession = async (gameLift: GameLift, gameSession: GameSession) => {
  let pollSession = gameSession;
  let attemptCount = 0;
  while (attemptCount < 3 && pollSession.Status !== activeStatus) {
      await new Promise( (accept) => setTimeout(accept, 3000));
      const pollResponse = await gameLift.describeGameSessions({GameSessionId: gameSession.GameSessionId}).promise();
      if (!pollResponse.GameSessions || pollResponse.GameSessions.length === 0) {
          throw new Error('exception polling session');
      }
      pollSession = pollResponse.GameSessions[0];
      attemptCount++;
  }
  if (pollSession.Status !== activeStatus) {
      throw new Error('session did not activate');
  }
  return pollSession;
};
