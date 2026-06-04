param(
    [ValidateSet('Process', 'User', 'Machine')]
    [string]$Scope = 'User'
)

$ErrorActionPreference = 'Stop'
$EnvironmentTarget = [System.Enum]::Parse([System.EnvironmentVariableTarget], $Scope)

function Read-RequiredSecret {
    param([string]$Prompt)

    $secure = Read-Host $Prompt -AsSecureString
    if ($secure.Length -eq 0) {
        throw "$Prompt is required."
    }

    $ptr = [Runtime.InteropServices.Marshal]::SecureStringToBSTR($secure)
    try {
        [Runtime.InteropServices.Marshal]::PtrToStringBSTR($ptr)
    }
    finally {
        [Runtime.InteropServices.Marshal]::ZeroFreeBSTR($ptr)
    }
}

function Read-OptionalValue {
    param(
        [string]$Prompt,
        [string]$DefaultValue
    )

    $value = Read-Host "$Prompt [$DefaultValue]"
    if ([string]::IsNullOrWhiteSpace($value)) {
        return $DefaultValue
    }

    return $value.Trim()
}

function Set-PayOsEnvironmentVariable {
    param(
        [string]$Name,
        [string]$Value
    )

    $envName = "PayOs__$Name"
    Set-Item -Path "Env:$envName" -Value $Value
    [Environment]::SetEnvironmentVariable($envName, $Value, $EnvironmentTarget)
}

$clientId = Read-RequiredSecret 'PayOS ClientId'
$apiKey = Read-RequiredSecret 'PayOS ApiKey'
$checksumKey = Read-RequiredSecret 'PayOS ChecksumKey'
$returnUrl = Read-OptionalValue 'PayOS ReturnUrl' 'http://localhost/payos/return'
$cancelUrl = Read-OptionalValue 'PayOS CancelUrl' 'http://localhost/payos/cancel'
$webhookUrl = Read-OptionalValue 'Public PayOS WebhookUrl; leave blank to skip auto-confirm' ''
$webhookListenPrefix = Read-OptionalValue 'Local WebhookListenPrefix' 'http://localhost:5126/payos/webhook/'
$autoConfirmWebhook = 'false'

if (-not [string]::IsNullOrWhiteSpace($webhookUrl)) {
    $answer = Read-Host 'Auto-confirm webhook on app startup? y/N'
    $autoConfirmWebhook = if ($answer -match '^(y|yes)$') { 'true' } else { 'false' }
}

Set-PayOsEnvironmentVariable 'ClientId' $clientId
Set-PayOsEnvironmentVariable 'ApiKey' $apiKey
Set-PayOsEnvironmentVariable 'ChecksumKey' $checksumKey
Set-PayOsEnvironmentVariable 'BaseUrl' 'https://api-merchant.payos.vn'
Set-PayOsEnvironmentVariable 'ReturnUrl' $returnUrl
Set-PayOsEnvironmentVariable 'CancelUrl' $cancelUrl
Set-PayOsEnvironmentVariable 'WebhookUrl' $webhookUrl
Set-PayOsEnvironmentVariable 'WebhookListenPrefix' $webhookListenPrefix
Set-PayOsEnvironmentVariable 'AutoConfirmWebhook' $autoConfirmWebhook

Write-Host "PayOS environment variables saved to $Scope scope. Restart Visual Studio/terminal if it was already open."
Write-Host 'Configured keys: PayOs__ClientId, PayOs__ApiKey, PayOs__ChecksumKey, PayOs__BaseUrl, PayOs__ReturnUrl, PayOs__CancelUrl, PayOs__WebhookUrl, PayOs__WebhookListenPrefix, PayOs__AutoConfirmWebhook.'
