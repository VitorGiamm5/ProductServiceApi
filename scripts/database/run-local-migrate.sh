#!/usr/bin/env bash
set -euo pipefail

operation="update"
name="UpdateProductSeed"
database_write="Server=localhost;Port=9000;Database=dbproducts;Username=randandan;Password=randandan_XLR;"

while [[ $# -gt 0 ]]; do
  case "$1" in
    -o|--operation|-operation)
      operation="${2:-}"
      shift 2
      ;;
    -n|--name|-name)
      name="${2:-}"
      shift 2
      ;;
    --database-write|-databaseWrite)
      database_write="${2:-}"
      shift 2
      ;;
    -h|--help)
      echo "Usage: bash ./scripts/database/run-local-migrate.sh [--operation add|update|remove] [--name <migration-or-target>] [--database-write <connection-string>]"
      exit 0
      ;;
    *)
      echo "Unknown argument: $1" >&2
      exit 1
      ;;
  esac
done

script_dir="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
repository_root="$(cd "$script_dir/../.." && pwd)"

if [[ -n "$database_write" ]]; then
  export FLEET_DATABASE_WRITE="$database_write"
fi

echo "INICIANDO ------- "
echo "# Iniciando o migrations"

pushd "$repository_root/src" >/dev/null
case "${operation,,}" in
  add)
    if [[ -z "$name" ]]; then
      echo "Operacoes de Entity Framework do tipo add precisam de um nome" >&2
      exit 1
    fi
    dotnet ef migrations add "$name" -p ./ProductServiceApp.Infrastructure/ -s ./ProductServiceApp.Api/ --context ApplicationDbContext --verbose
    ;;
  remove)
    dotnet ef migrations remove -p ./ProductServiceApp.Infrastructure/ -s ./ProductServiceApp.Api/ --context ApplicationDbContext --verbose
    ;;
  update)
    if [[ -z "$name" ]]; then
      dotnet ef database update -p ./ProductServiceApp.Infrastructure/ -s ./ProductServiceApp.Api/ --context ApplicationDbContext --verbose
    else
      dotnet ef database update "$name" -p ./ProductServiceApp.Infrastructure/ -s ./ProductServiceApp.Api/ --context ApplicationDbContext --verbose
    fi
    ;;
  *)
    echo "Operacao invalida: $operation. Use add, update ou remove." >&2
    exit 1
    ;;
esac
popd >/dev/null
