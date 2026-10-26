# The Windows test, run as administrator with the folder of the published program as the argument. It adds one of
# the listed roots to the machine's Root store, then checks that scan sees it trusted, that revoke takes it out and
# puts every listed certificate into Disallowed, and that restore puts the stores back as they were.
param([Parameter(Mandatory)] [string] $App)
$ErrorActionPreference = 'Stop'

$patch = Join-Path $App 'emudhra-patch.exe'
$pem = Join-Path $App 'certs\7DD78D5F4F13459A83DFF9ABBBA62EDBAF6F2D102BF257FD712F4D9F2746ED8D.pem'
$root = [System.Security.Cryptography.X509Certificates.X509Certificate2]::CreateFromPemFile($pem)

function Expect([int] $want, [string[]] $arguments) {
    & $patch @arguments | Out-Host
    if ($LASTEXITCODE -ne $want) { throw "emudhra-patch $arguments exited $LASTEXITCODE, expected $want" }
}

function Count([string] $store) {
    @(Get-ChildItem "Cert:\LocalMachine\$store" | Where-Object { $_.Subject -match 'emSign|eMudhra' }).Count
}

$disallowedBefore = Count 'Disallowed'
$machineRoot = New-Object System.Security.Cryptography.X509Certificates.X509Store('Root', 'LocalMachine')
$machineRoot.Open('ReadWrite'); $machineRoot.Add($root); $machineRoot.Close()

Write-Host '=== scan: the root is trusted'
Expect 2 @('scan')

Write-Host '=== revoke'
Expect 0 @('revoke', '--yes')
if (Test-Path "Cert:\LocalMachine\Root\$($root.Thumbprint)") { throw 'the root is still in LocalMachine\Root' }
if ((Count 'Disallowed') -lt 13) { throw 'not every listed certificate is in LocalMachine\Disallowed' }

Write-Host '=== scan: nothing trusted'
Expect 0 @('scan')

Write-Host '=== restore'
Expect 0 @('restore')
if (-not (Test-Path "Cert:\LocalMachine\Root\$($root.Thumbprint)")) { throw 'the root was not put back into LocalMachine\Root' }
if ((Count 'Disallowed') -ne $disallowedBefore) { throw 'LocalMachine\Disallowed is not what it was' }

$machineRoot.Open('ReadWrite'); $machineRoot.Remove($root); $machineRoot.Close()
Write-Host 'PASS'
