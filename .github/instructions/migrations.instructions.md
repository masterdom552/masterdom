---
description: "Masterdom migration workflow, naming, review expectations, and design-time DbContext rules"
applyTo: "src/Masterdom.Infrastructure/Migrations/**/*.cs,src/Masterdom.Infrastructure/Persistence/DesignTimeDbContextFactory.cs,src/Masterdom.Infrastructure/Persistence/MasterdomDbContextFactory.cs"
---

# Masterdom Migration Conventions

## Workflow

Always follow this sequence:

1. Build.
2. Run tests.
3. Generate migration.
4. Review migration output.

Never generate migrations before build passes.

## Migration Naming

- Use descriptive names that communicate model intent.
- Avoid vague names such as `Update` or `Fix`.

## Design-Time Context Rules

- Keep design-time context creation deterministic.
- Ensure migration commands use the correct project and startup project.

## Review Checklist

- Verify intended table/column/constraint changes.
- Verify ownership mappings (`OwnsOne` / `OwnsMany`) produce expected tables and FKs.
- Verify no accidental schema drift from unrelated modules.

## Domain-First Rule

- If migration generation fails due to model mismatch, investigate mapping first.
- Do not weaken Domain invariants to make migrations succeed.

## Related Files

- EF mapping rules: `ef-core-persistence.instructions.md`
- Naming rules: `naming.instructions.md`
