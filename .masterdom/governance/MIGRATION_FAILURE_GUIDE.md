# Migration Failure Guide

## Common Failure Scenarios

### Working tree not clean

Symptom: the validation script reports modified or untracked files before migration generation.

Likely cause: unrelated edits are present or the migration is being generated on top of a dirty branch.

Response: stop, clean the change set, and regenerate from a focused branch.

### Pending model changes

Symptom: the migration or snapshot changes more than the intended schema.

Likely cause: the model was already modified before migration generation.

Response: isolate the model change and regenerate the migration.

### Failed build

Symptom: the build fails before or after migration generation.

Likely cause: the code does not compile or the solution is incomplete.

Response: fix the build before reviewing the migration.

### Multiple feature branches

Symptom: the migration includes unrelated tables or columns.

Likely cause: more than one schema change was present in the working tree.

Response: split the work and generate one migration per isolated change.

### Snapshot mismatch

Symptom: the snapshot no longer reflects the intended final schema.

Likely cause: the migration was generated from stale or inconsistent model state.

Response: regenerate after confirming the model and build are stable.
