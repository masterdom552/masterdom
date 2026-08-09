#!/usr/bin/env bash

set -euo pipefail

usage() {
    cat <<'EOF'
Usage: validate-migration.sh <pre|post> [--migration-name NAME]

Modes:
  pre   Validate repository readiness before migration generation.
  post  Validate a generated migration and its snapshot.
EOF
}

die() {
    echo "validate-migration: $1" >&2
    exit 1
}

mode="${1:-}"
[[ -n "$mode" ]] || {
    usage
    exit 1
}
shift || true

migration_name=""
while [[ $# -gt 0 ]]; do
    case "$1" in
        --migration-name)
            migration_name="${2:-}"
            [[ -n "$migration_name" ]] || die "--migration-name requires a value"
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

repo_root="$(git rev-parse --show-toplevel)"
cd "$repo_root"

build_args=(
    build
    src/Masterdom.Host/Masterdom.Host.csproj
    /property:GenerateFullPaths=true
    '/consoleloggerparameters:NoSummary;ForceNoAlign'
)

git_status() {
    git status --short
}

check_clean_worktree() {
    if [[ -n "$(git_status)" ]]; then
        die "working tree is not clean"
    fi
}

check_build() {
    dotnet "${build_args[@]}"
}

check_snapshot_exists() {
    [[ -f src/Masterdom.Infrastructure/Migrations/MasterdomDbContextModelSnapshot.cs ]] || die "snapshot file is missing"
}

check_migration_exists() {
    shopt -s nullglob
    local matches=(src/Masterdom.Infrastructure/Migrations/*_"$migration_name".cs)
    shopt -u nullglob
    [[ ${#matches[@]} -gt 0 ]] || die "migration '$migration_name' was not found"

    local non_empty=0
    for file in "${matches[@]}"; do
        [[ -s "$file" ]] || die "migration file is empty: $file"
        non_empty=1
    done

    [[ "$non_empty" -eq 1 ]] || die "migration '$migration_name' has no non-empty files"
}

check_repo_migration_inventory() {
    shopt -s nullglob
    local migrations=(src/Masterdom.Infrastructure/Migrations/*.cs)
    shopt -u nullglob
    [[ ${#migrations[@]} -gt 0 ]] || die "no migration files exist"
}

check_allowed_pending_changes() {
    local status
    status="$(git status --porcelain)"
    [[ -n "$status" ]] || return 0

    while IFS= read -r line; do
        [[ -n "$line" ]] || continue
        local path="${line:3}"
        case "$path" in
            src/Masterdom.Infrastructure/Migrations/*)
                ;;
            *)
                die "unexpected pending change: $path"
                ;;
        esac
    done <<< "$status"
}

case "$mode" in
    pre)
        check_clean_worktree
        check_build
        check_snapshot_exists
        check_repo_migration_inventory
        ;;
    post)
        [[ -n "$migration_name" ]] || die "--migration-name is required in post mode"
        check_build
        check_snapshot_exists
        check_migration_exists
        check_allowed_pending_changes
        ;;
    *)
        usage
        die "unknown mode: $mode"
        ;;
esac

echo "validate-migration: ok"
