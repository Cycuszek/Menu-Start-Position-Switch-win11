# Generowanie certyfikatu z podpisem własnym
$cert = New-SelfSignedCertificate `
    -Subject "CN=Menu Start Position Switch" `
    -Type CodeSigningCert `
    -KeyUsage DigitalSignature `
    -KeyAlgorithm RSA `
    -KeyLength 2048 `
    -CertStoreLocation "Cert:\CurrentUser\My"

# Eksport certyfikatu do pliku .cer
$certPath = ".\MenuStartSwitchCert.cer"
Export-Certificate -Cert $cert -FilePath $certPath

# Podpisanie pliku .exe
$exePath = ".\publish\Menu Start Position Switch (win11).exe"
Set-AuthenticodeSignature -FilePath $exePath -Certificate $cert

# Dodanie certyfikatu do zaufanych wydawców
$trustedPublisher = "Cert:\CurrentUser\TrustedPublisher"
Move-Item -Path $cert.PSPath -Destination $trustedPublisher

Write-Host "Aplikacja została pomyślnie podpisana i certyfikat dodany do zaufanych wydawców."
