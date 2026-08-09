# Migration Lifecycle

## Lifecycle

Draft

Generate

Review

Validate

Commit

## Stage Notes

- Draft: identify the intended schema change and confirm the change set is isolated.
- Generate: create a single migration after a successful build.
- Review: inspect the migration files and the snapshot for unrelated changes.
- Validate: run the migration validation script and confirm the result is clean.
- Commit: commit the migration together with the model change that produced it.
