import { GameLift } from 'aws-sdk';
import { GameSession } from 'aws-sdk/clients/gamelift';
import { getFleetId } from './get-fleet-id';

const hasAvailablePlayers = 'hasAvailablePlayerSessions=true';

export const findAvailableGameSessions = async (gameLift: GameLift) => {
  let gameSessions: GameSession[];
  try {
    const data = await gameLift.searchGameSessions({ FleetId: getFleetId(), FilterExpression: hasAvailablePlayers}).promise();
    gameSessions = data.GameSessions || [];
  } catch (error) {
      console.error('error searching game sessions:' + error);
      console.log('failing over to describe');
      const allSessionsData = await gameLift.describeGameSessions({FleetId: getFleetId()}).promise();
      const allSessions = allSessionsData.GameSessions || [];
      gameSessions = allSessions.filter((session) => (session.PlayerSessionCreationPolicy || 0) < (session.MaximumPlayerSessionCount || 0));
  }
  return gameSessions;
};
