# Migration Examples

## Good Migration

The migration changes one intended schema area and nothing else.

```csharp
migrationBuilder.CreateIndex(
    name: "IX_property_units_PropertyId_Code",
    table: "property_units",
    columns: new[] { "PropertyId", "Code" },
    unique: true);
```

This is good because the change is targeted, traceable, and directly tied to one schema rule.

## Bad Migration

The migration mixes unrelated tables or adds changes outside the intended module.

```csharp
migrationBuilder.CreateIndex(
    name: "IX_properties_Code",
    table: "properties",
    column: "Code",
    unique: true);

migrationBuilder.CreateIndex(
    name: "IX_bills_bill_number",
    table: "bills",
    column: "bill_number",
    unique: true);
```

This is bad because one migration is carrying two unrelated concerns.

## Contaminated Migration

The migration includes the intended change plus unrelated churn in a different module or historical carry-forward noise.

```csharp
migrationBuilder.AlterColumn<string>(
    name: "Name",
    table: "properties",
    type: "character varying(201)",
    maxLength: 201,
    nullable: false,
    oldClrType: typeof(string),
    oldType: "character varying(200)",
    oldMaxLength: 200);

migrationBuilder.CreateIndex(
    name: "IX_ledgers_ledger_code",
    table: "ledgers",
    column: "ledger_code",
    unique: true);
```

This is contaminated because the secondary ledger change is unrelated to the property change.
