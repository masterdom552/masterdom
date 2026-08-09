# Migration Definition of Done

## Done Criteria

A migration is complete only when all of the following are true:

- The build succeeds.
- The migration has been reviewed.
- The snapshot has been reviewed.
- No contamination is present.
- The validation script passes.
- The migration is ready to be committed.

## Not Done When

A migration is not done if any of the following remain true:

- The working tree is dirty for unrelated reasons.
- The build fails.
- The snapshot was not reviewed.
- The migration contains unrelated schema changes.
- The migration cannot be reproduced from the documented workflow.
