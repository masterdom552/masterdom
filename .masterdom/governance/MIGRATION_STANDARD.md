# Migration Standard

## Purpose

This standard defines the required workflow for creating and reviewing EF Core migrations in the Masterdom repository.

The goal is to keep every migration reproducible, isolated, reviewable, deterministic, and traceable.

## Migration Workflow

1. Confirm the working tree is clean.
2. Build the solution before generating a migration.
3. Generate exactly one migration for the intended change.
4. Review the migration and the updated model snapshot.
5. Validate the result with the migration validation script.
6. Commit the migration together with the intended model change.

## Prerequisites

- The working tree must be clean before generation.
- The current branch must contain only the intended model change.
- The solution must build successfully.
- The developer must know the migration name, target project, startup project, and DbContext.
- Only one feature branch should be active for the model change being migrated.

## Review Process

Review every migration before commit.

Confirm that:

- The migration contains only intended schema changes.
- The `Up` and `Down` methods are symmetric where appropriate.
- The snapshot reflects the intended final model.
- No unrelated module tables, indexes, or columns changed.
- The migration is non-empty and names the change clearly.

## Troubleshooting

Common issues to check first:

- The working tree is not clean.
- The build fails before migration generation.
- Pending model changes exist outside the intended migration.
- The snapshot no longer matches the expected schema shape.
- More than one feature branch or pending change set is active.
- The migration file was generated but remains empty or incomplete.

## Acceptance Rules

A migration is acceptable only when:

- The build succeeds.
- The migration review is complete.
- The snapshot review is complete.
- The validation script passes.
- No unrelated schema changes are present.
- The result can be traced back to the intended model change.
