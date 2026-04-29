<#
    Script para PowerShell 7 ou maior
	Script para criação ou atualização de migrations
 	Parâmetros:
 		-operation: 		(add | update | remove): tipo de operação
		-name:				nome da migração (obrigatório se -operation add)
 		-databaseWrite: 	(opcional) string de conexão que será atribuída a DATABASE_WRITE
#>

# Verifica os parâmetros
param (
    [string]$operation = "update",
    [string]$name = "UpdateProductSeed",
    [string]$databaseWrite = "Server=localhost;Port=9000;Database=dbproducts;Username=randandan;Password=randandan_XLR;"
)

# Define a variável de ambiente caso tenha sido informada na linha de comando.
if (-not [string]::IsNullOrWhiteSpace($databaseWrite)) {
    $Env:FLEET_DATABASE_WRITE = $databaseWrite
}

Write-Host "INICIANDO ------- "
try
{
    # Iniciando o migrations
	Write-Host "# Iniciando o migrations" 

	# Verifica o tipo de operação solicitada
    if ($operation.ToLower() -eq "add") {
		# Verifica o parametro name pois trata-se de um add
		if ([bool][string]::IsNullOrWhitespace($name)) { 
			Write-Host "Operações de Entity Framework do tipo Add precisam de um nome" 
			$(throw "Erro") 
		}
        cd src
		dotnet ef migrations add $name -p ./ProductServiceApp.Infrastructure/ -s ./ProductServiceApp.Api/ --context ApplicationDbContext --verbose
		cd ..
    } elseif ($operation.ToLower() -eq "remove") {
		cd src
		dotnet ef migrations remove -p ./ProductServiceApp.Infrastructure/ -s ./ProductServiceApp.Api/ --context ApplicationDbContext --verbose
		cd ..
    } else {
        cd src
		if ([bool][string]::IsNullOrWhitespace($name)) { 
			dotnet ef database update -p ./ProductServiceApp.Infrastructure/ -s ./ProductServiceApp.Api/ --context ApplicationDbContext --verbose
		} else {
			dotnet ef database update $name -p ./ProductServiceApp.Infrastructure/ -s ./ProductServiceApp.Api/ --context ApplicationDbContext --verbose
		}
		cd ..
    }

}
catch
{
	Write-Host "Erro no Migrations. Processo nao continuou..." -ForegroundColor red
}