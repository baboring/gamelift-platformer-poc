(Get-Content ".\.temp\access.pem") | Set-Content ".\.temp\access.pem"
.\openssl.exe rsa -in .\.temp\access.pem -out .\.temp\converted.ppk