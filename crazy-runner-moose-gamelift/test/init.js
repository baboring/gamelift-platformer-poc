const awsSdk = require('aws-sdk');
const credentials = new awsSdk.Credentials(process.env.AWS_KEY, process.env.AWS_SECRET_KEY);
const region = 'us-east-1';
const config = new awsSdk.Config({region, credentials});
awsSdk.config.update(config);
