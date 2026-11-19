<#
.SYNOPSIS
    Script de empaquetado y despliegue serverless para OpsHub Backend (.NET 10 en AWS Lambda).
.DESCRIPTION
    Automatiza la publicación self-contained para linux-x64, la creación del paquete ZIP
    compatible con el runtime provided.al2023 de AWS Lambda tras API Gateway HTTP API v2,
    o el despliegue directo con AWS SAM CLI / dotnet-lambda.
.EXAMPLE
    .\deploy-lambda.ps1 -Action Package
    .\deploy-lambda.ps1 -Action SamDeploy -Stage dev
#>

[CmdletBinding()]
param (
    [ValidateSet('Package', 'SamDeploy', 'DockerBuild')]
    [string]$Action = 'Package',

    [string]$Stage = 'prod',
    [string]$Region = 'us-east-2',
    [string]$Profile = 'idat-cli',
    [string]$S3Bucket = 'opshub-deployments-artifacts'
)

$ErrorActionPreference = 'Stop'
$ScriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$ProjectDir = Join-Path $ScriptDir "src\OpsHub.Api"
$OutputDir = Join-Path $ScriptDir "bin\lambda-package"
$ZipFile = Join-Path $ScriptDir "bin\opshub-backend-lambda.zip"

Write-Host "======================================================================" -ForegroundColor Cyan
Write-Host " OpsHub Backend - Despliegue Serverless en AWS Lambda (.NET 10)       " -ForegroundColor Cyan
Write-Host "======================================================================" -ForegroundColor Cyan

switch ($Action) {
    'Package' {
        Write-Host "`n[1/3] Limpiando artefactos previos..." -ForegroundColor Yellow
        if (Test-Path $OutputDir) { Remove-Item -Recurse -Force $OutputDir }
        if (Test-Path $ZipFile) { Remove-Item -Force $ZipFile }
        New-Item -ItemType Directory -Path $OutputDir -Force | Out-Null

        Write-Host "`n[2/3] Publicando binario self-contained para linux-x64 (.NET 10)..." -ForegroundColor Yellow
        dotnet publish $ProjectDir `
            -c Release `
            -r linux-x64 `
            --self-contained true `
            -p:PublishSingleFile=false `
            -o $OutputDir

        # En AWS Lambda Custom Runtime (provided.al2023), el archivo ejecutable debe llamarse 'bootstrap'
        Write-Host "`n[3/3] Configurando bootstrap para runtime provided.al2023..." -ForegroundColor Yellow
        $apiBinary = Join-Path $OutputDir "OpsHub.Api"
        $bootstrap = Join-Path $OutputDir "bootstrap"
        if (Test-Path $apiBinary) {
            Copy-Item $apiBinary $bootstrap -Force
        }

        # Comprimir a archivo ZIP
        Compress-Archive -Path "$OutputDir\*" -DestinationPath $ZipFile -Force
        $zipSize = (Get-Item $ZipFile).Length / 1MB
        Write-Host "`n¡Paquete Lambda generado con éxito!" -ForegroundColor Green
        Write-Host "Archivo ZIP: $ZipFile ($([Math]::Round($zipSize, 2)) MB)" -ForegroundColor Green
        Write-Host "Listo para cargar directamente en AWS Lambda Console o via AWS CLI:" -ForegroundColor Gray
        Write-Host "aws lambda update-function-code --function-name opshub-backend-$Stage --zip-file fileb://$ZipFile --profile $Profile --region $Region" -ForegroundColor DarkGray
    }

    'SamDeploy' {
        Write-Host "`nDesplegando con AWS SAM CLI..." -ForegroundColor Yellow
        Set-Location $ProjectDir
        sam build -t serverless.template
        sam deploy `
            --stack-name "opshub-backend-$Stage" `
            --parameter-overrides "Stage=$Stage S3BucketName=opshub-adjuntos-$Stage" `
            --capabilities CAPABILITY_IAM `
            --region $Region `
            --profile $Profile `
            --resolve-s3
        Set-Location $ScriptDir
    }

    'DockerBuild' {
        Write-Host "`nConstruyendo imagen Docker para AWS Lambda Container Image / App Runner..." -ForegroundColor Yellow
        docker build -t "opshub-backend:$Stage" -f "$ScriptDir\Dockerfile" $ScriptDir
        Write-Host "Imagen Docker 'opshub-backend:$Stage' construida correctamente." -ForegroundColor Green
    }
}
