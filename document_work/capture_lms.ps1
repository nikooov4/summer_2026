param(
    [string]$BaseUrl = 'http://localhost:5293',
    [string]$OutputDirectory = "$PSScriptRoot\screenshots"
)

$edge = 'C:\Program Files (x86)\Microsoft\Edge\Application\msedge.exe'
if (-not (Test-Path -LiteralPath $edge)) {
    throw 'Microsoft Edge was not found.'
}

New-Item -ItemType Directory -Path $OutputDirectory -Force | Out-Null
$profileRoot = Join-Path $PSScriptRoot 'browser_profiles'
New-Item -ItemType Directory -Path $profileRoot -Force | Out-Null

function ConvertTo-Base64Url([string]$Value) {
    return [Convert]::ToBase64String([Text.Encoding]::UTF8.GetBytes($Value)).TrimEnd('=').Replace('+', '-').Replace('/', '_')
}

function Get-DemoToken([string]$Username) {
    $header = ConvertTo-Base64Url '{"alg":"none","typ":"JWT"}'
    $payload = ConvertTo-Base64Url ("{`"username`":`"$Username`",`"name`":`"$Username`",`"demo`":true}")
    return "$header.$payload.demo"
}

function Invoke-EdgeCapture([string]$ProfileName, [string]$Route, [string]$FileName, [int]$Height = 1000) {
    $profile = Join-Path $profileRoot $ProfileName
    $target = Join-Path $OutputDirectory $FileName
    & $edge --headless=new --disable-gpu --no-first-run --no-default-browser-check --hide-scrollbars `
        --window-size=1440,$Height --virtual-time-budget=6000 --user-data-dir="$profile" `
        --screenshot="$target" "$BaseUrl$Route" | Out-Null
    if (-not (Test-Path -LiteralPath $target)) {
        throw "Screenshot was not created: $target"
    }
}

Invoke-EdgeCapture 'anonymous' '/login' '01-login.png'

foreach ($username in @('admin', 'teacher', 'student')) {
    $token = Get-DemoToken $username
    Invoke-EdgeCapture $username "/login?token=$token" "warmup-$username.png"
}

Invoke-EdgeCapture 'teacher' '/attendance' '02-teacher-attendance.png'
Invoke-EdgeCapture 'student' '/attendance' '03-student-attendance.png'
Invoke-EdgeCapture 'teacher' '/attendance/monitoring' '04-teacher-monitoring.png'
Invoke-EdgeCapture 'admin' '/admin' '05-admin-catalog.png'
Invoke-EdgeCapture 'admin' '/statements/constructor' '06-statement-constructor.png'
Invoke-EdgeCapture 'student' '/attendance/monitoring' '07-student-monitoring.png'
Invoke-EdgeCapture 'admin' '/admin' '08-admin-catalog-full.png' 2600
