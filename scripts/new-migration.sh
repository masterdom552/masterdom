#!/usr/bin/env bash

set -euo pipefail

usage() {
    cat <<'EOF'
Usage: new-migration.sh --name NAME

Workflow:
  1. Build
  2. Generate migration
  3. Validate migration
  4. Review the result
EOF
}

die() {
    echo "new-migration: $1" >&2
    exit 1
}

migration_name=""
while [[ $# -gt 0 ]]; do
    case "$1" in
        --name)
            migration_name="${2:-}"
            [[ -n "$migration_name" ]] || die "--name requires a value"
            shift 2
            ;;
        -h|--help)
            usage
            exit 0
            ;;
        *)
            die "unknown argument: $1"
            ;;
    esac
done

[[ -n "$migration_name" ]] || {
    usage
    exit 1
}

repo_root="$(git rev-parse --show-toplevel)"
cd "$repo_root"

build_args=(
    build
    src/Masterdom.Host/Masterdom.Host.csproj
    /property:GenerateFullPaths=true
    '/consoleloggerparameters:NoSummary;ForceNoAlign'
)

ef_args=(
    ef
    migrations
    add
    "$migration_name"
    --project
    src/Masterdom.Infrastructure
    --startup-project
    src/Masterdom.Host
    --context
    MasterdomDbContext
    --output-dir
    Migrations
    --no-build
)

echo "== Build =="
dotnet "${build_args[@]}"

echo "== Generate =="
dotnet "${ef_args[@]}"

echo "== Validate =="
bash scripts/validate-migration.sh post --migration-name "$migration_name"

cat <<EOF
== Review ==
Review the generated migration and snapshot for:
- intended schema changes only
- matching Up and Down intent
- no unrelated module changes
- non-empty migration files

Checklist:
- .masterdom/governance/MIGRATION_REVIEW_CHECKLIST.md
EOF
