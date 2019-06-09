$fileName = Read-Host 'file';
(Get-Content $fileName) | Set-Content $fileName