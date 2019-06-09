# Gamelift Platformer Proof Of Concept
A proof of concept for an authoritative server multiplayer platformer using unity and AWS gamelift

## Crazy Runner Moose Common
Common unity project with assets shared by both client and server
* Assets/CRM/common: Exported for use in either client or server
* Assets/CRM/test: test scene used to confirm client/server interactions prior to export
  
## Crazy Runner Moose Client
Client side unity game build. 
* Assets/CRM/ClientStartController: See isLocal toggle logic for utilizing gamelift endpoint deployed to AWS API Gateway verses local server

## Crazy Runner Moose Server
Server side unity game build. Acts as an authoritative server between clients. Requires gamelift sub project running local instance of gamelift sdk to run locally

## Crazy Runner Moose Gamelift
Contains node tasks to deploy gamelift api endpoints for session creation / query and scripts to run gamelift SDK and endpoints locally
* aws-tooling.json relies on non release custom code to deploy /src/lambda code as AWS lambda with supporting API Gateway. 
* Implement by deploying lambda code with aws-tooling provided env vars and linking to API Gateway
* Powershell scripts provided:
  *  upload-build: automate uploading server builds to Gamelift and deploying fleet for provided build
  *  terminate-fleet: quickly termiante fleet by fleetname
  *  get-access: gain access keys for SSHing into fleet instance