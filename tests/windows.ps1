# The Windows test, run as administrator with the folder of the published program as the argument. It adds one of
# the listed roots to the machine's Root store, then checks that scan sees it trusted, that revoke takes it out and
# puts every listed certificate into Disallowed, and that restore puts the stores back as they were. A failure is
# also written as a GitHub Actions error, with what emudhra-patch printed, so that it shows on the run's page.
param([Parameter(Mandatory)] [string] $App)
$ErrorActionPreference = 'Stop'

$patch = Join-Path (Resolve-Path $App) 'emudhra-patch.exe'
$pem = Join-Path (Resolve-Path $App) 'certs\7DD78D5F4F13459A83DFF9ABBBA62EDBAF6F2D102BF257FD712F4D9F2746ED8D.pem'
$root = [System.Security.Cryptography.X509Certificates.X509Certificate2]::CreateFromPemFile($pem)

function Fail([string] $message) {
    if ($env:GITHUB_ACTIONS) { Write-Host ('::error title=Windows test::' + ($message -replace '%', '%25' -replace "`r", '' -replace "`n", '%0A')) }
    throw $message
}

function Expect([int] $want, [string[]] $arguments) {
    $output = (& $patch @arguments 2>&1 | Out-String)
    Write-Host $output
    if ($LASTEXITCODE -ne $want) { Fail "emudhra-patch $arguments exited $LASTEXITCODE, expected $want`n$output" }
}

function Count([string] $store) {
    @(Get-ChildItem "Cert:\LocalMachine\$store" | Where-Object { $_.Subject -match 'emSign|eMudhra' }).Count
}

function Where-Is() {
    Get-ChildItem Cert:\LocalMachine, Cert:\CurrentUser -Recurse |
        Where-Object { $_.Subject -match 'emSign|eMudhra' } |
        ForEach-Object { "$($_.PSParentPath.Split(':')[-1])  $($_.Subject.Split(',')[0])" } | Out-String
}

try {
    Write-Host "=== before`n$(Where-Is)"
    $disallowedBefore = Count 'Disallowed'
    $machineRoot = New-Object System.Security.Cryptography.X509Certificates.X509Store('Root', 'LocalMachine')
    $machineRoot.Open('ReadWrite'); $machineRoot.Add($root); $machineRoot.Close()

    Write-Host '=== scan: the root is trusted'
    Expect 2 @('scan')

    Write-Host '=== revoke'
    Expect 0 @('revoke', '--yes')
    if (Test-Path "Cert:\LocalMachine\Root\$($root.Thumbprint)") { Fail "the root is still in LocalMachine\Root`n$(Where-Is)" }
    if ((Count 'Disallowed') -lt 13) { Fail "not every listed certificate is in LocalMachine\Disallowed`n$(Where-Is)" }

    Write-Host '=== scan: nothing trusted'
    Expect 0 @('scan')

    Write-Host '=== restore'
    Expect 0 @('restore')
    if (-not (Test-Path "Cert:\LocalMachine\Root\$($root.Thumbprint)")) { Fail "the root was not put back into LocalMachine\Root`n$(Where-Is)" }
    if ((Count 'Disallowed') -ne $disallowedBefore) { Fail "LocalMachine\Disallowed is not what it was`n$(Where-Is)" }

    $machineRoot.Open('ReadWrite'); $machineRoot.Remove($root); $machineRoot.Close()
    Write-Host 'PASS'
}
catch {
    if ($env:GITHUB_ACTIONS -and "$_" -notmatch '^emudhra-patch|^the root|^not every|^LocalMachine') {
        Write-Host ('::error title=Windows test::' + ("$_`n$($_.ScriptStackTrace)" -replace '%', '%25' -replace "`r", '' -replace "`n", '%0A'))
    }
    throw
}
