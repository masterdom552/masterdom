# Migration CI Recommendation

## Current Recommendation

Repository CI should run migration validation for changes that touch:

- `src/Masterdom.Infrastructure/Migrations/**`
- `src/Masterdom.Infrastructure/Persistence/**`
- `scripts/**`
- `.masterdom/governance/**`

## Recommended Checks

- Restore dependencies.
- Build the solution.
- Run the migration validation script in pre-generation mode.
- Fail the job if the working tree is not clean or the build fails.

## Rationale

The repository already has GitHub Actions-based CI. Migration integrity should be validated in the same automation surface so future contamination is blocked before merge.

If CI coverage changes in the future, the same checks should remain part of the pull request review gate.
