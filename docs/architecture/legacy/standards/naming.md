# Naming Standard

## Scope

This standard defines naming conventions for source, persistence, and architecture artifacts.

## Code Naming

- Use domain language consistently.
- Type names: PascalCase.
- Member names: PascalCase for public API.
- Local and parameter names: camelCase.

## Namespace and Folder Naming

- Namespaces should align with folder structure.
- Module names should reflect bounded domains.
- Avoid ambiguous or overloaded naming.

## File Naming

- File name should match the primary type.
- Keep naming stable to reduce architectural drift and discovery cost.

## Persistence Naming

- Table names: plural snake_case.
- Column names: snake_case.
- Foreign keys: explicit owner/reference names in snake_case.

## Artifact Naming

- ADR titles should communicate decision scope clearly.
- Migration names should communicate schema intent.
- Architecture document names should map to one concern each.
