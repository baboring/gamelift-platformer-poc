$name = Read-Host 'build name:'
aws gamelift upload-build --name $name --build-version 0.0.2 --build-root F:\Projects\crazy-runner-moose\builds\server --operating-system AMAZON_LINUX --region us-east-1
$builds = aws gamelift list-builds | ConvertFrom-Json
$runtime = '--runtime-configuration file://runtime.json'
$role = '--instance-role-arn arn:aws:iam::726826185741:role/gamelift-server'
$inbound = '--ec2-inbound-permissions file://inbound-permissions.json'
$instanceType = '--ec2-instance-type c4.large'
Foreach($build in $builds.Builds) {
  if($build.name -eq $name){
    $buildId = $build.BuildId
    Write-Host $buildId
    $expression = "aws gamelift create-fleet --name $name-fleet --build-id $buildId --cli-input-json file://create-fleet.json"
    Write-Host "running: $expression"
    Invoke-Expression $expression
  }
}