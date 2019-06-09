$fleetId = $env:FLEET_ID
New-Item -ItemType Directory -Force -Path .\.temp
Write-Host "Getting Access For: " $fleetId
$description = aws gamelift describe-instances --fleet-id $fleetId | ConvertFrom-Json
$instanceId = $description.Instances[0].InstanceId
$access = aws gamelift get-instance-access --fleet-id $fleetId --instance-id $instanceId | ConvertFrom-Json
$ipAddress = $access.InstanceAccess.IpAddress
$secret = $access.InstanceAccess.Credentials.Secret
$userName = $access.InstanceAccess.Credentials.UserName
$secret| Out-File -FilePath .\.temp\access.pem
$myIp = (Invoke-WebRequest -uri "http://ifconfig.me/ip").Content
aws gamelift update-fleet-port-settings --fleet-id  $fleetId  --inbound-permission-authorizations "FromPort=22,ToPort=22,IpRange=$myIp/32,Protocol=TCP"
"ssh -i .\.temp\access.ppk $userName@$ipAddress" |Out-File -FilePath .\.temp\connect.ps1
(Get-Content ".\.temp\access.pem") | Set-Content ".\.temp\access.pem"
.\openssl.exe rsa -in .\.temp\access.pem -out .\.temp\access.ppk

