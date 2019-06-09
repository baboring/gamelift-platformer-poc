$fleets = aws gamelift list-fleets | ConvertFrom-Json
$fleetDetails = aws gamelift describe-fleet-attributes --fleet-ids $fleets.FleetIds | ConvertFrom-Json
$i = 0;
Foreach($details in $fleetDetails.FleetAttributes) {
  Write-Host $i ": "  $details.Name
  $i = $i + 1
}
$chosen = Read-Host 'pick'
$toDelete = $fleetDetails.FleetAttributes[$chosen].FleetId
aws gamelift delete-fleet --fleet-id $toDelete