$openAiKey = (Get-ItemProperty -Path 'HKCU:\Environment' -ErrorAction Stop).OPENAI_API_KEY
if ([string]::IsNullOrWhiteSpace($openAiKey)) {
    throw 'OPENAI_API_KEY is missing from the Windows User Environment.'
}

$env:OPENAI_API_KEY = $openAiKey
$env:JWT_SECRET = 'local-development-secret-at-least-32-bytes-please'
$env:ASPNETCORE_ENVIRONMENT = 'Development'
$sepayApiKey = [Environment]::GetEnvironmentVariable('SEPAY_WEBHOOK_API_KEY', 'User')
$sepaySecret = [Environment]::GetEnvironmentVariable('SEPAY_WEBHOOK_SECRET', 'User')
if (-not [string]::IsNullOrWhiteSpace($sepayApiKey)) { $env:SEPAY_WEBHOOK_API_KEY = $sepayApiKey }
if (-not [string]::IsNullOrWhiteSpace($sepaySecret)) { $env:SEPAY_WEBHOOK_SECRET = $sepaySecret }

dotnet run --project "$PSScriptRoot\..\backend\src\StudyAI.Api\StudyAI.Api.csproj" --urls 'http://127.0.0.1:5194' --no-launch-profile
