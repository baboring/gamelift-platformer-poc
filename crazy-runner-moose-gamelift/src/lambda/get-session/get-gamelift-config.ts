import { ClientConfiguration } from 'aws-sdk/clients/gamelift';

export const getGameLiftConfig = () => {
  const endPointOverride = process.env.GAMELIFT_ENDPOINT;
  const gameLiftClientConfig: ClientConfiguration = {region: 'us-east-1'};
  if (endPointOverride) {
    gameLiftClientConfig.endpoint = endPointOverride;
  }
  return gameLiftClientConfig;
};
