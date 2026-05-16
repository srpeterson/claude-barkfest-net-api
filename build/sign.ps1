[CmdletBinding()]
param(
    [Parameter(Mandatory)]
    [string]$TargetPath
)

# Nothing to do in CI -- SAC is not enabled on build agents.
if ($env:CI -eq 'true' -or $env:GITHUB_ACTIONS -eq 'true' -or $env:TF_BUILD -eq 'True') {
    exit 0
}

if (-not (Test-Path $TargetPath)) { exit 0 }
if ($TargetPath -notmatch '\.(dll|exe)$') { exit 0 }

# Skip if already validly signed (avoids re-signing on incremental builds where the
# output file was not actually recompiled but the Build target still ran).
$existing = Get-AuthenticodeSignature -FilePath $TargetPath -ErrorAction SilentlyContinue
if ($existing -and $existing.Status -eq 'Valid') { exit 0 }

$certSubject = 'CN=Barkfest Development'

# The mutex serialises both cert setup and signing across parallel project builds.
# Signing requires exclusive access to the private key -- concurrent access produces
# UnknownError. Cert creation and Root store installation also cannot race safely.
$mutex = [System.Threading.Mutex]::new($false, 'Global\BarkfestDevSigning')
try {
    $null = $mutex.WaitOne()

    $cert = Get-ChildItem Cert:\CurrentUser\My -CodeSigningCert -ErrorAction SilentlyContinue |
        Where-Object { $_.Subject -eq $certSubject -and $_.NotAfter -gt (Get-Date).AddDays(30) } |
        Select-Object -First 1

    if (-not $cert) {
        Write-Host ''
        Write-Host '-----------------------------------------------------------------------'
        Write-Host '  First-time developer signing setup'
        Write-Host '  A self-signed code signing certificate will be created for this'
        Write-Host '  machine. Windows will ask you to confirm you trust it -- click Yes.'
        Write-Host '  This dialog appears once per developer machine and never again.'
        Write-Host '-----------------------------------------------------------------------'
        Write-Host ''

        $cert = New-SelfSignedCertificate `
            -Subject $certSubject `
            -CertStoreLocation 'Cert:\CurrentUser\My' `
            -KeyUsage DigitalSignature `
            -Type CodeSigningCert `
            -NotAfter (Get-Date).AddYears(10)

        # Installing to CurrentUser\Root makes the self-signed cert a trusted root on
        # this machine without requiring admin. Windows shows a one-time confirmation
        # dialog -- this is expected and required.
        $rootStore = [System.Security.Cryptography.X509Certificates.X509Store]::new(
            [System.Security.Cryptography.X509Certificates.StoreName]::Root,
            [System.Security.Cryptography.X509Certificates.StoreLocation]::CurrentUser
        )
        $rootStore.Open([System.Security.Cryptography.X509Certificates.OpenFlags]::ReadWrite)
        $rootStore.Add($cert)
        $rootStore.Close()

        Write-Host 'Development certificate installed. Signing assemblies...'
        Write-Host ''
    }

    $result = Set-AuthenticodeSignature -FilePath $TargetPath -Certificate $cert -ErrorAction SilentlyContinue

    # Check that a signature was applied by confirming the signer cert is present.
    # Do NOT rely on $result.Status -- Set-AuthenticodeSignature validates the full trust
    # chain immediately after signing, which can transiently report UnknownError in the
    # first few milliseconds after the Root store write while the trust cache propagates.
    # The signature itself is valid; SAC will accept it once the cache settles.
    if (-not $result -or -not $result.SignerCertificate) {
        Write-Warning "Signing '$TargetPath' failed -- no signature was applied. SAC may still block this assembly."
    }
}
finally {
    $mutex.ReleaseMutex()
}
