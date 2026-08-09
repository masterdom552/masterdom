# CAP-017 — Policy Framework: Cross-Module Policy Catalog Contracts

## Document Header

| Field                  | Value                                                                                                                                                                  |
| ---------------------- | ---------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| Package ID             | `CAP-017`                                                                                                                                                              |
| Title                  | Policy Framework — Cross-Module Policy Catalog Contracts and Consumer Integration                                                                                      |
| Status                 | **VERIFIED / CLOSED**                                                                                                                                                  |
| Author                 | Architecture                                                                                                                                                           |
| Architect              | Architect                                                                                                                                                              |
| Target Release         | Unscheduled                                                                                                                                                            |
| Date                   | 2026-08-09                                                                                                                                                             |
| Governing Architecture | [Policy Framework Foundation](../../docs/architecture/POLICY_FRAMEWORK.md)                                                                                             |
| Related ADRs           | [ADR-0002 Configuration First](../../docs/adr/ADR-0002_Configuration_First.md), [ADR-0005 Versioned Configuration](../../docs/adr/ADR-0005_Versioned_Configuration.md) |

> **IMPLEMENTATION COMPLETE — AWAITING ARCHITECT VERIFICATION.**
> **ARCHITECT DECISION: VERIFIED. PACKAGE CLOSED.**
>
> Implementation and final validation completed within the approved package scope.

---

## 1. Objective

Complete the remaining CAP-017 work through the smallest end-to-end vertical slice that:

1. defines a canonical, business-neutral Policy Catalog contract;
2. allows a consuming capability to request an applicable policy without referencing Policy
   Framework domain or application types;
3. adapts the existing Policy Framework applicability resolution to that contract;
4. proves the contract through one real Lease consumer; and
5. preserves consumer ownership of all business-rule execution and workflow behavior.

This package standardizes an existing capability boundary. It does not redesign the Policy
Framework aggregate, lifecycle, persistence, API, or runtime.

---

## 2. Existing Foundation

CAP-017 already contains:

- the `Policy` aggregate and policy identity, type, category, reference, scope, condition,
  assignment, version, snapshot, metadata, effective-date, and status concepts;
- create, version, assign, activate, expire, archive, and applicable-policy application flows;
- `IPolicyRepository` persistence and EF Core mappings for policies, versions, assignments,
  and snapshots;
- `GetApplicablePolicyQuery` and existing exact/global scope applicability resolution;
- Policy Framework dependency injection and platform orchestration;
- secured `/api/policies` endpoints;
- runtime composition and endpoint tests proving create, activate, and resolve behavior.

These foundations are retained. No domain redesign or replacement runtime is authorized.

---

## 3. Remaining Gap

The existing applicability API exposes Policy Framework-owned domain/application types. Other
modules cannot consume it through a stable published contract without taking a direct dependency
on `Masterdom.Modules.PolicyFramework` or creating local duplicate policy DTOs.

Lease already stores renewal, termination, and late-fee policy references as strings, but there is
no canonical contract connecting those references to Policy Framework governance and
applicability resolution.

CAP-017 remains incomplete until a shared contract and one real consumer prove this boundary.

---

## 4. Architectural Boundary

```text
Policy Framework
    |
    +-- Policy definition
    +-- Policy lifecycle
    +-- Policy catalog
    +-- Policy applicability resolution
             |
             v
    Published policy contract
             |
             v
      Consumer capability
             |
             +-- Owns business-rule execution
             +-- Owns workflows and domain behavior
```

Policy Framework owns governance and resolution. It does not evaluate Lease rules, execute Lease
workflows, mutate Lease aggregates, or interpret Lease outcomes.

The published contract SHALL be business-neutral and SHALL NOT expose `Policy`, `PolicyVersion`,
`PolicyCondition`, repositories, handlers, or other Policy Framework implementation types.

---

## 5. Selected Consumer — Lease

Lease is the single consumer selected for this package because:

- `RenewalTerms` already owns `RenewalPolicyReference`;
- `TerminationTerms` already owns `TerminationPolicyReference` and
  `LateFeePolicyReference`;
- the existing Policy Framework runtime test already uses a module-scoped `lease` policy;
- Lease can prove policy lookup without changing aggregate ownership or introducing a new
  business rule; and
- both modules currently depend only on `Masterdom.Core`, so a business-neutral abstraction
  preserves dependency direction.

The proof SHALL use one existing Lease policy-reference use case only. It SHALL NOT migrate every
Lease policy reference or alter renewal, termination, penalty, billing, or tenancy behavior.

---

## 6. Approved Implementation Scope

### 6.1 Canonical Policy Catalog Contract

Add the minimum business-neutral published contract under `Masterdom.Abstractions` using existing
Policy Framework vocabulary where it is semantically stable.

The contract SHALL provide:

- a canonical policy reference/code supplied by the consumer;
- policy type or catalog classification required by existing applicability resolution;
- scope kind and scope identifier;
- an as-of date;
- an immutable resolved-policy result containing only consumer-safe identity, reference,
  version/effective-date, condition/selector, and metadata values needed by the consumer; and
- an explicit not-found outcome that does not leak persistence or domain exceptions.

The contract SHALL expose one resolution abstraction for applicable-policy lookup. It SHALL NOT
reproduce policy lifecycle methods or create a second Policy aggregate representation.

### 6.2 Policy Framework Adapter

Implement the published resolution abstraction by adapting to the existing Policy Framework
application service/query and applicability rules.

The adapter SHALL:

- translate the shared request into existing `PolicyType`, `PolicyScope`, and as-of-date concepts;
- reuse existing applicability resolution;
- project the resolved active version into the immutable shared result;
- preserve the existing no-applicable-policy semantics; and
- remain outside the Policy Framework domain model.

No new repository, applicability algorithm, scope hierarchy, policy engine, or lifecycle path is
permitted.

### 6.3 Lease Consumer Integration

Add the smallest Lease application-layer consumer that:

- accepts one existing Lease policy reference plus the scope and as-of date required for lookup;
- requests the applicable policy through the shared abstraction;
- handles both resolved and not-found outcomes explicitly; and
- returns the resolved governance input to Lease-owned application behavior without executing or
  relocating Lease business rules.

Lease domain entities SHALL continue to own their current invariants and policy-reference values.
The Policy Framework SHALL NOT import Lease types, and Lease SHALL NOT reference Policy Framework
domain or application assemblies.

### 6.4 Runtime Composition

Extend the existing Policy Framework runtime registration to bind the shared resolution contract
to the Policy Framework adapter.

Register only the minimum Lease consumer service/handler required for the proof. Reuse the existing
Host and Infrastructure composition path; do not create a second Policy Framework runtime or a
service-locator path.

### 6.5 CAP-018 Compatibility

The resulting shared request/result/resolver boundary SHALL be consumable by CAP-018 without a
reference to Policy Framework internals. This package does not add Security policy behavior,
authorization rules, permission evaluation, or CAP-018 runtime integration.

---

## 7. Dependency Direction

Permitted compile-time direction:

```text
Masterdom.Modules.Lease --------------------+
                                             v
                                  Masterdom.Abstractions
                                             ^
                                             |
Infrastructure Policy Framework adapter ----+
                                             |
                                             v
                              Policy Framework application
```

Required constraints:

- `Masterdom.Abstractions` remains business-neutral and depends on neither module.
- Lease consumes only the published abstraction.
- Infrastructure may compose the abstraction with Policy Framework application behavior.
- Policy Framework does not reference Lease.
- No local DTO namespace may become an accidental published cross-module API.

Architecture tests SHALL enforce these constraints.

---

## 8. Out of Scope

- CAP-018 Security implementation or Security-specific policy behavior.
- Expense & Vendor Management, Vendor, Expense, procurement, or supplier behavior.
- New business capabilities or successor packages.
- Policy aggregate, lifecycle, versioning, assignment, scope, persistence, or API redesign.
- A universal business-rule engine or execution of consumer rules.
- Workflow execution or replacement of the existing workflow framework.
- Module-specific configuration ownership.
- Notifications, Reporting, analytics, or UI work.
- Inventory enhancements.
- Integrating or migrating every existing module or policy reference.
- New scope inheritance beyond existing global/exact-scope applicability.
- Unrelated refactoring, migrations, or schema changes unless implementation evidence proves a
  schema change is unavoidable; any such finding requires Architect escalation before proceeding.

---

## 9. Acceptance Criteria

1. A single canonical Policy Catalog request/result/resolver contract exists in the established
   shared-abstraction boundary.
2. The contract uses existing policy reference, type, scope, effective-date, condition/selector,
   and metadata semantics without duplicating the Policy domain model.
3. Policy Framework implements the contract by delegating to existing applicability resolution.
4. A Lease application consumer requests an applicable policy through the shared contract.
5. A matching active policy is returned with the expected reference, version, scope/effective
   information, and consumer-safe payload.
6. No matching policy produces an explicit not-found outcome and does not execute Lease behavior.
7. Lease retains ownership of renewal, termination, late-fee, and all other business behavior.
8. Policy Framework retains ownership of policy governance, lifecycle, and applicability.
9. Neither business module directly references the other module's domain or application assembly.
10. Existing authorization requirements on Policy Framework APIs remain unchanged.
11. Existing DI resolves the shared contract and selected Lease consumer using scoped runtime
    composition.
12. Architecture tests prove contract ownership and dependency direction.
13. Existing Policy Framework domain, application, endpoint, persistence, and runtime tests pass.
14. The shared contract is suitable for future CAP-018 consumption without Security behavior or a
    Policy Framework redesign.
15. No unrelated capability, module migration, UI, or successor package enters the implementation.

---

## 10. Focused Test Plan

### Policy Framework

- Map a shared catalog request to existing applicability resolution.
- Return the active applicable policy projection for reference/type/scope/as-of date.
- Return the explicit not-found result when no active applicable policy exists.
- Preserve existing create, version, activate, expire, archive, and applicability tests.

### Lease Consumer

- Request the selected Lease policy through the shared resolver.
- Receive and expose the applicable policy contract.
- Handle no applicable policy without mutating Lease state or running fallback business logic in
  Policy Framework.
- Prove Lease domain behavior remains in Lease and does not move into the contract adapter.

### Runtime

- Resolve the shared policy resolver and selected Lease consumer from DI.
- Execute one end-to-end in-process Policy Framework-to-Lease contract path.
- Confirm existing secured Policy Framework endpoint boundaries remain enforced.

### Architecture and Regression

- Assert the shared contract remains business-neutral.
- Assert Lease does not reference `Masterdom.Modules.PolicyFramework`.
- Assert Policy Framework does not reference `Masterdom.Modules.Lease`.
- Assert no owner-local DTO is consumed as a published cross-module contract.
- Run existing Policy Framework tests unchanged except where contract registration requires focused
  extension.

---

## 11. Implementation Order

1. Finalize the canonical Policy Catalog request/result/resolver contract.
2. Finalize the consumer-facing resolution semantics, including not-found behavior.
3. Implement the Policy Framework application adapter.
4. Register the adapter through existing runtime composition.
5. Integrate the single Lease consumer.
6. Add focused contract, consumer, runtime, and architecture tests.
7. Run regression verification.

Implementation steps are complete and awaiting Architect verification.

---

## 12. Validation Gates

### Gate 1 — Build

Build only the affected projects and their dependency closure:

- `Masterdom.Abstractions`
- `Masterdom.Modules.PolicyFramework`
- `Masterdom.Modules.Lease`
- `Masterdom.Infrastructure`
- `Masterdom.Host`
- affected test projects

### Gate 2 — Targeted Tests

Run:

- Policy Framework domain/application tests;
- Policy Framework runtime composition and endpoint tests;
- selected Lease consumer tests;
- contract-ownership and module-boundary architecture tests.

Broaden testing only if an affected shared contract creates a demonstrated wider dependency.

### Gate 3 — Architectural Verification

Confirm that:

- Policy Framework exclusively owns policy governance and applicability;
- Lease exclusively owns Lease business behavior;
- shared contracts contain no module implementation types;
- compile-time dependency direction is valid;
- existing authorization boundaries are unchanged;
- CAP-018 can consume the resulting contract without redesign; and
- no unrelated capability entered the implementation.

---

## 13. Escalation Conditions

Stop implementation and return to the Architect only if repository evidence shows that:

1. the shared contract cannot remain business-neutral without importing module domain types;
2. the selected Lease proof requires a new Lease business rule or workflow decision;
3. existing applicability semantics cannot support reference/type/scope/as-of-date resolution;
4. correct runtime composition requires a direct Lease-to-PolicyFramework module reference;
5. persistence or migration changes are unavoidable; or
6. CAP-018-specific behavior is required to complete the contract.

Ordinary implementation detail is not an escalation condition.

---

## 14. Package Status

**VERIFIED / CLOSED.**

- Repository investigation: **Complete**
- Architecture foundation: **Existing and retained**
- Selected consumer: **Lease**
- Implementation authorization: **Granted**
- Implementation: **Complete**
- Architect Decision: **VERIFIED**
- Package: **Closed**
- Verification date: **2026-08-09**
- Final validation: **Architecture, ownership, dependency direction, runtime/DI, targeted tests, warning regression, and scope passed**
- Open architecture decisions: **None identified**

`VERIFIED / CLOSED`
