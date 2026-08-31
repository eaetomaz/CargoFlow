# Sobe a Api e o frontend do CargoFlow em janelas minimizadas, e abre o
# navegador assim que o frontend responder. Feito pra ser chamado por um
# atalho (start-cargoflow.bat), nao pra rodar manualmente.
#
# Usa $PSScriptRoot (pasta onde este arquivo esta, nao um caminho fixo) para
# funcionar em qualquer maquina, independente de onde o repositorio foi
# clonado. Mesmo padrao do start-product-hunter.ps1 do projeto irmao. O banco
# e um arquivo SQLite local (sem servidor, sem Docker), entao nao ha nenhum
# processo de infraestrutura pra subir antes da Api.

$root = $PSScriptRoot

Write-Host "Iniciando CargoFlow..."

function Test-Port($port) {
    $test = Test-NetConnection -ComputerName localhost -Port $port -WarningAction SilentlyContinue
    return $test.TcpTestSucceeded
}

function Wait-ForPort($port, $maxSeconds) {
    for ($i = 0; $i -lt $maxSeconds; $i++) {
        if (Test-Port $port) { return $true }
        Start-Sleep -Seconds 1
    }
    return $false
}

Start-Process -WindowStyle Minimized -FilePath "cmd.exe" `
    -ArgumentList '/c', 'dotnet run --project CargoFlow.Api --urls https://localhost:7099;http://localhost:5099' `
    -WorkingDirectory $root

Start-Process -WindowStyle Minimized -FilePath "cmd.exe" `
    -ArgumentList '/c', 'npm run dev' `
    -WorkingDirectory (Join-Path $root 'frontend')

Write-Host "Aguardando os servidores ficarem prontos..."

$apiReady = Wait-ForPort -port 7099 -maxSeconds 45
$frontendReady = Wait-ForPort -port 5180 -maxSeconds 30

if (-not $apiReady) {
    Write-Host "A Api demorou mais que o esperado. Confira a janela minimizada 'cmd.exe' na barra de tarefas."
}
if (-not $frontendReady) {
    Write-Host "O frontend demorou mais que o esperado. Confira a janela minimizada 'cmd.exe' na barra de tarefas."
}

Start-Sleep -Seconds 1
Start-Process "http://localhost:5180"
