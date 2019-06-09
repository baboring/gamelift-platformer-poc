import { GameLift } from 'aws-sdk';
import { getFleetId } from './get-fleet-id';
import { GameSession } from 'aws-sdk/clients/gamelift';

export const createGameSession = async (gameLift: GameLift) => {
  console.log('creating new session');
  let gameSession: GameSession|undefined;
  try {
      const data = await gameLift.createGameSession({ MaximumPlayerSessionCount: 2, FleetId: getFleetId()}).promise();
      gameSession = data.GameSession;
  } catch (error) {
      console.error(error);
  }
  if (!gameSession) {
    throw new Error('failed to create game session');
  }
  return gameSession;
};
