---
description: "Masterdom naming conventions for code, folders, namespaces, tables, and columns"
applyTo: "src/**/*.cs,tests/**/*.cs,src/Masterdom.Infrastructure/Migrations/**/*.cs"
---

# Masterdom Naming Conventions

## Code Naming

- Use clear domain language.
- Types: PascalCase.
- Members: PascalCase for public API, camelCase for locals/parameters.
- Avoid abbreviations unless industry-standard.

## Namespace and Folder Alignment

- Keep namespaces aligned with folder structure.
- Use module-first organization for bounded contexts.

## File Naming

- One primary type per file where practical.
- File name should match the primary type name.

## Persistence Naming

- Table names: plural snake_case.
- Column names: snake_case.
- FK columns: explicit owner/reference names in snake_case.

## Migration Naming

- Migration class names should communicate intent.
- Avoid generic names that hide schema purpose.

## Related Files

- Persistence specifics: `ef-core-persistence.instructions.md`
- Migration workflow: `migrations.instructions.md`
