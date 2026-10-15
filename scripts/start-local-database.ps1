param(
    [int]$Port = 5433,
    [string]$PostgresBin = '',
    [switch]$Stop
)
$ErrorActionPreference = 'Stop'
$root = Split-Path $PSScriptRoot -Parent
$local = Join-Path $root '.local'
$cluster = Join-Path $local 'postgres-data'
if (-not $PostgresBin) {
    $PostgresBin = Join-Path $local 'postgresql/pgsql/bin'
}
$pgCtl = Join-Path $PostgresBin 'pg_ctl.exe'
if (-not (Test-Path -LiteralPath $pgCtl)) {
    throw 'PostgreSQL binaries not found. Extract the official Windows binary archive to .local/postgresql, or pass -PostgresBin.'
}
if ($Stop) {
    if (Test-Path -LiteralPath (Join-Path $cluster 'postmaster.pid')) {
        & $pgCtl stop -D $cluster -m fast -w
        if ($LASTEXITCODE -ne 0) { throw 'Unable to stop the project database.' }
    }
    return
}
New-Item -ItemType Directory -Force -Path $local | Out-Null
$passwordFile = Join-Path $local 'postgres-password.txt'
if (-not (Test-Path -LiteralPath $passwordFile)) {
    if (Test-Path -LiteralPath (Join-Path $cluster 'PG_VERSION')) {
        throw 'The cluster exists but its password file is missing. Restore credentials; do not recreate the cluster.'
    }
    $randomBytes = New-Object byte[] 32
    $random = [Security.Cryptography.RandomNumberGenerator]::Create()
    try { $random.GetBytes($randomBytes) } finally { $random.Dispose() }
    $password = [Convert]::ToBase64String($randomBytes)
    [IO.File]::WriteAllText($passwordFile, $password, [Text.UTF8Encoding]::new($false))
}
$password = [IO.File]::ReadAllText($passwordFile).Trim()
if (-not (Test-Path -LiteralPath (Join-Path $cluster 'PG_VERSION'))) {
    & (Join-Path $PostgresBin 'initdb.exe') -D $cluster -U lms_owner --pwfile=$passwordFile --auth-host=scram-sha-256 --auth-local=trust --encoding=UTF8 --locale=C
    if ($LASTEXITCODE -ne 0) { throw 'Database initialization failed.' }
}
& $pgCtl status -D $cluster 2>$null | Out-Null
if ($LASTEXITCODE -ne 0) {
    & $pgCtl start -D $cluster -l (Join-Path $local 'postgres.log') -o "-p $Port -h 127.0.0.1" -w
    if ($LASTEXITCODE -ne 0) { throw 'Database startup failed; inspect .local/postgres.log.' }
}
$env:PGPASSWORD = $password
try {
    $psql = Join-Path $PostgresBin 'psql.exe'
    $exists = & $psql -h 127.0.0.1 -p $Port -U lms_owner -d postgres -tAc "SELECT 1 FROM pg_database WHERE datname='summer_lms'"
    if ($LASTEXITCODE -ne 0) { throw 'Cannot connect to the database.' }
    if ($exists -ne '1') {
        & $psql -h 127.0.0.1 -p $Port -U lms_owner -d postgres -v ON_ERROR_STOP=1 -c 'CREATE DATABASE summer_lms'
        if ($LASTEXITCODE -ne 0) { throw 'Database creation failed.' }
    }
} finally { Remove-Item Env:PGPASSWORD -ErrorAction SilentlyContinue }
$connection = "Host=127.0.0.1;Port=$Port;Database=summer_lms;Username=lms_owner;Password=$password;Include Error Detail=false"
[IO.File]::WriteAllText((Join-Path $local 'lms-connection.txt'), $connection, [Text.UTF8Encoding]::new($false))
Write-Host "Project database summer_lms is running on 127.0.0.1:$Port. Credentials stay in ignored .local files."
