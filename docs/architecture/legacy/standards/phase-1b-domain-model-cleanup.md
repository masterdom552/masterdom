# Phase 1B - Domain Model Cleanup Standard

## Document Metadata

- Document Type: Repository Governance Standard
- Version: 1.0
- Status: Approved
- Applies To: Entire Masterdom Repository
- Audience:
  - ChatGPT
  - GitHub Copilot
  - Future AI Coding Agents
  - Human Contributors
- Architecture Authority: Masterdom Architecture Standards
- Parent Documents:
  - Masterdom Working Constitution
  - Architecture Standards
  - ADR Repository
  - Repository Instructions
  - PKG Framework

## 1. Purpose

Phase 1B exists to improve the quality of the Domain Model.

This phase does not add functionality.

This phase does not redesign Masterdom.

This phase improves the architecture that already exists.

Every modification shall make the repository:

- simpler
- more consistent
- easier to understand
- easier to maintain
- easier to extend

without changing business behavior.

The Domain Model is the primary deliverable.

Everything else is secondary.

## 2. Mission

Masterdom is an enterprise business platform.

It is not a CRUD application.

The objective of this phase is to ensure that the Domain Model accurately represents the business.

Infrastructure exists to support the Domain.

The Domain never exists to support Infrastructure.

## 3. Scope

Included:

- Aggregates
- Entities
- Value Objects
- Domain Events
- Domain Services
- Specifications
- Policies
- Repository Contracts
- Namespace organization
- File organization
- Aggregate boundaries
- Dependency cleanup
- Removal of obsolete code
- Technical debt reduction

Excluded:

- New Features
- UI
- APIs
- Controllers
- DTOs
- Application Services
- MediatR
- CQRS
- Authentication redesign
- Database optimization
- Performance tuning
- UX improvements
- Reporting
- External integrations

If work falls outside this scope, it belongs in another Implementation Package.

## 4. Guiding Philosophy

Every change shall improve one or more of:

- correctness
- clarity
- consistency
- maintainability
- extensibility

Never make a change merely because:

- it is easier
- EF prefers it
- Visual Studio suggests it
- a tool recommends it

Architecture drives implementation.

Never the reverse.

## 5. Primary Objectives

During Phase 1B:

- Eliminate ambiguity.
- Reduce duplication.
- Strengthen aggregate boundaries.
- Clarify ownership.
- Improve naming.
- Normalize namespaces.
- Remove obsolete code.
- Reduce architectural debt.
- Preserve business behavior.
- Leave the repository healthier than it was found.

## 6. Success Criteria

Phase 1B succeeds when:

- the Domain becomes easier to understand without reading Infrastructure.
- every aggregate has a clear responsibility.
- every Entity belongs to exactly one Aggregate.
- every Value Object has one canonical implementation.
- every business concept has one owner.
- repository organization reflects architectural organization.
- no duplicate concepts remain.
- no accidental coupling remains.
- the solution builds.
- tests pass.
- documentation matches implementation.

## 7. Repository Health Principle

Repository health is more important than implementation speed.

A smaller improvement that strengthens architecture is preferred over a larger change that introduces uncertainty.

Prefer incremental improvement.

Never perform unnecessary rewrites.

## 8. Implementation Package Requirement

All work shall execute under an approved PKG.

A PKG shall define:

- purpose
- scope
- affected modules
- dependencies
- acceptance criteria
- deliverables
- architectural risks

No implementation may proceed outside the approved PKG.

If new work is discovered:

- record it
- do not implement it unless the PKG is updated

Avoid scope creep.

## 9. Working Mode

This is a stabilization phase.

Not a feature phase.

Work shall prioritize:

- correctness
- consistency
- maintainability
- simplicity

No architectural shortcuts shall be introduced.

Temporary fixes shall be minimized.

If temporary work is unavoidable:

- record it
- do not hide it

## 10. Architectural Authority

The following order is authoritative:

1. Domain Model
2. Architecture Standards
3. Approved ADRs
4. Repository Instructions
5. Implementation Package
6. Infrastructure
7. Build
8. IDE Suggestions
9. AI Recommendations

If conflicts exist, follow the highest authority.

## 11. Domain First Principle

The Domain Model is immutable from an architectural perspective.

Infrastructure shall adapt to the Domain.

The Domain shall never adapt to Infrastructure.

Therefore:

- Do not rename concepts for EF.
- Do not weaken invariants for serialization.
- Do not expose mutable state for mapping.
- Do not introduce parameterless constructors unless justified.
- Do not leak persistence concerns into business logic.

## 12. Definition of Done

A task is complete only when:

- the architecture is improved
- the code compiles
- tests pass
- documentation remains correct
- no new technical debt has been introduced

Compilation alone does not constitute completion.

## 13. Engineering Principles

Every implementation decision shall reinforce the architecture.

When multiple technically correct solutions exist, select the one that best preserves long-term maintainability.

Never optimize for today's implementation at the expense of tomorrow's architecture.

Preferred order of concern:

1. Business Correctness
2. Architectural Integrity
3. Maintainability
4. Consistency
5. Extensibility
6. Performance
7. Developer Convenience

Developer convenience shall never determine architecture.

## 14. Core Engineering Rules

Every modification shall satisfy all of the following:

- preserve business behavior
- reduce architectural debt
- improve readability
- improve maintainability
- reduce ambiguity
- preserve encapsulation
- preserve aggregate consistency

If a change satisfies only build requirements, it is incomplete.

## 15. Decision Framework

Whenever a decision is required:

1. Understand the business purpose.
2. Understand the current implementation.
3. Identify architectural constraints.
4. Identify root cause.
5. Evaluate alternatives.
6. Recommend the smallest correct solution.
7. Implement.
8. Validate.

Never skip investigation.

Never guess.

Never modify code simply because it appears incorrect.

## 16. Root Cause First

Symptoms shall never drive implementation.

Examples of symptoms:

- build failure
- warning
- duplicate code
- failing test
- EF mapping error

These are indicators, not causes.

Always identify why the problem exists before changing code.

## 17. Principle of Small Safe Changes

Prefer changes that are:

- small
- isolated
- reversible
- incremental
- architecturally sound

Avoid:

- large rewrites
- mass renames
- unnecessary redesign
- wide-ranging edits

One correct improvement is preferred over ten speculative ones.

## 18. Architectural Decision Rules

When choosing between two implementations, prefer the implementation that:

- reduces duplication
- improves consistency
- strengthens invariants
- clarifies ownership
- improves discoverability
- reduces coupling
- supports future extension

Never choose an implementation merely because it requires fewer edits.

## 19. Domain Priority Matrix

When multiple valid tasks exist, work in the following order:

1. Business correctness
2. Aggregate consistency
3. Aggregate boundaries
4. Value Objects
5. Domain Events
6. Entities
7. Repository Contracts
8. Domain Services
9. Specifications
10. Policies
11. Namespaces
12. Infrastructure
13. EF Configuration
14. Compiler Warnings
15. Formatting

Never optimize lower priorities while higher priorities remain incorrect.

## 20. Investigation Workflow

Before changing any file, understand:

- why it exists
- who owns it
- who references it
- whether it duplicates another concept
- whether it belongs elsewhere

Only after investigation may implementation begin.

## 21. Existing Code First

Always attempt Modify before Create.

Before introducing a new class, determine whether:

- an existing class can be improved
- an existing abstraction already exists
- a reusable framework already exists
- duplication can be removed

New code is the last option.

## 22. Delete Before Create

When duplicate concepts exist, attempt:

1. Rename
2. Move
3. Merge
4. Simplify
5. Delete
6. Create

Avoid parallel implementations.

There should be one authoritative implementation of every business concept.

## 23. Single Responsibility Principle

Every type should have one reason to change.

If a class performs unrelated business responsibilities:

- split responsibilities, not behavior

Never split classes simply to reduce file size.

## 24. Cohesion Rule

Business concepts that change together belong together.

Business concepts that change independently belong apart.

Prefer high cohesion.

Avoid accidental coupling.

## 25. Coupling Rule

Dependencies should point toward stability.

The Domain should depend on nothing.

Infrastructure may depend on the Domain.

Application may depend on the Domain.

The Domain shall not depend upon:

- EF Core
- ASP.NET
- Serialization
- Database providers
- Logging frameworks
- Messaging frameworks
- Configuration libraries
- UI frameworks

## 26. Dependency Direction

Allowed:

- Infrastructure -> Domain
- Application -> Domain

Forbidden:

- Domain -> Infrastructure
- Domain -> Application
- Domain -> UI

## 27. Business Language Rule

Every name shall use business terminology.

Avoid technical names such as:

- Manager
- Helper
- Utility
- Processor
- Thing
- Data
- Object
- Info
- Misc
- Temp

Use ubiquitous language.

If the business would not use the term, the code should not either.

## 28. Naming Consistency

Equivalent concepts shall use identical terminology.

Never use Customer, Resident, Tenant, and Occupant to describe the same concept.

Select one.

Apply it consistently.

The repository shall contain one business vocabulary.

## 29. Refactoring Philosophy

Refactoring exists to improve architecture, not to satisfy personal preference.

A refactoring is justified only if it:

- reduces duplication
- clarifies intent
- improves ownership
- improves maintainability
- strengthens the Domain

Otherwise, leave the code unchanged.

## 30. Risk Management

Every change shall be evaluated for:

- architectural risk
- business risk
- migration risk
- testing impact
- future maintenance impact

When risk exceeds value, do not implement.

Record the recommendation instead.

## 31. Architectural Escalation

Stop implementation immediately if:

- aggregate ownership becomes unclear
- bounded contexts overlap
- business terminology conflicts
- duplicate aggregates are discovered
- business rules conflict
- domain invariants cannot be preserved

Document findings.

Recommend an architectural decision.

Resume implementation only after resolution.

## 32. End-of-Batch Review

At the conclusion of every logical batch, verify:

- repository builds
- tests pass
- architecture improved
- duplication reduced
- documentation still correct
- no new technical debt introduced

Only then proceed to the next batch.

## 33. Domain Audit Objective

The objective of the Domain Audit is to ensure that every business concept is:

- correctly modeled
- correctly owned
- correctly named
- correctly encapsulated
- correctly located

The audit is concerned with business correctness, not implementation convenience.

## 34. Domain Audit Workflow

Every Domain type shall be reviewed using the following sequence:

1. Identify its business purpose.
2. Identify its owner.
3. Identify its invariants.
4. Identify its collaborators.
5. Identify duplicate implementations.
6. Identify architectural violations.
7. Refactor.
8. Validate.

Never perform implementation before completing the audit.

## 35. Aggregate Definition

An Aggregate is the consistency boundary of the Domain.

An Aggregate protects business rules.

An Aggregate is not a persistence object.

An Aggregate is not a DTO.

An Aggregate is not an EF entity.

## 36. Aggregate Responsibilities

Every Aggregate shall:

- own business rules
- protect invariants
- control mutations
- coordinate internal entities
- raise domain events
- maintain consistency

Nothing outside the Aggregate may violate its invariants.

## 37. Aggregate Root Rules

Every Aggregate shall expose exactly one Aggregate Root.

The Aggregate Root shall:

- control all mutations
- protect child entities
- validate business rules
- prevent invalid state

Child entities shall never be modified directly.

## 38. Aggregate Boundary Rules

Every Aggregate shall own one business responsibility.

Examples:

- Property owns Units and does not own Billing.
- User owns Identity and does not own Property.
- Invoice owns Invoice Lines and does not own Payments.

Never allow responsibilities to overlap.

## 39. Aggregate Ownership Checklist

Verify:

- single responsibility
- correct ownership
- no duplicated state
- no duplicated behavior
- invariants protected
- lifecycle complete
- persistence independent
- terminology correct

Only after all checks pass may the Aggregate be considered complete.

## 40. Aggregate Behavior Rules

Business behavior belongs inside Aggregates.

Examples:

- Approve()
- Assign()
- Transfer()
- Activate()
- Deactivate()
- Archive()
- Merge()
- Split()
- Suspend()
- Restore()

Avoid anemic models.

Business rules belong in behavior, not services, controllers, or repositories.

## 41. Aggregate Mutation Rules

All state changes shall occur through methods.

Avoid:

- public setters
- mutable collections
- external state manipulation

Prefer explicit methods describing business intent.

## 42. Aggregate Lifecycle

Every Aggregate shall have an explicit lifecycle.

Example:

Created -> Active -> Suspended -> Archived -> Deleted (if permitted)

Lifecycle transitions shall enforce business rules.

## 43. Entity Definition

Entities possess identity.

Entities are not identified by property values.

Entities may change over time.

Identity remains constant.

## 44. Entity Rules

Every Entity shall:

- have one identity
- belong to one Aggregate
- contain meaningful behavior
- protect its own consistency

Avoid Entities that exist only to store data.

## 45. Entity Ownership

Every Entity shall belong to exactly one Aggregate.

No Entity may belong to multiple Aggregates.

Shared data shall become Value Objects or Reference IDs.

Never shared ownership.

## 46. Entity Checklist

Verify:

- identity correct
- ownership correct
- behavior appropriate
- invariants protected
- naming correct
- no duplicate responsibilities
- no unnecessary mutability

## 47. Entity Behavior

Entities may perform behavior.

Examples:

- Rename()
- Move()
- Approve()
- Reject()
- Replace()
- Expire()
- Activate()
- Deactivate()

Avoid utility methods.

Prefer business language.

## 48. Value Object Definition

A Value Object represents meaning, not identity.

Two equal Value Objects represent the same business value.

## 49. Value Object Characteristics

Every Value Object shall be:

- immutable
- self-validating
- self-contained
- equality-based
- side-effect free

## 50. Value Object Rules

A Value Object shall never:

- have an identity
- have mutable state
- depend on Infrastructure
- depend on EF
- depend on serialization

## 51. Validation Responsibility

Validation belongs inside the Value Object.

Invalid Value Objects shall never exist.

Never create first and validate later.

Constructors shall enforce validity.

## 52. Equality

Equality shall be based upon business value, not reference, persistence, or memory location.

## 53. Canonical Implementations

Every business concept shall have one Value Object.

Examples:

- Money
- EmailAddress
- PhoneNumber
- PersonName
- PostalAddress
- PropertyCode
- UnitCode

If duplicates exist, merge them and delete obsolete versions.

## 54. Primitive Obsession

Avoid primitive types representing business concepts.

Examples:

- string Email
- string Phone
- string Currency
- string PropertyCode
- string UnitNumber

Prefer dedicated Value Objects.

## 55. Invariant Definition

An invariant is a business rule that must always remain true.

The Domain is responsible for protecting invariants.

Not the UI, EF, or controllers.

## 56. Invariant Checklist

For every Aggregate, verify:

- impossible states prevented
- invalid transitions prevented
- null state prevented
- duplicate state prevented
- business constraints enforced

## 57. Invariant Ownership

Every invariant shall have exactly one owner.

Never duplicate validation.

Never split validation.

Never validate the same business rule in multiple layers.

## 58. Ubiquitous Language

Business terminology shall remain consistent.

One concept, one name, repository-wide.

Examples of prohibited duplication for one concept:

- Resident
- Occupant
- Tenant
- Customer
- User

Select one canonical term and apply consistently.

## 59. Naming Review

Every type shall answer: What business concept am I?

If the answer cannot be expressed in business language, rename the type.

## 60. End of Domain Audit

Before leaving an audited module, verify:

- aggregate ownership clear
- entities correctly owned
- value objects consolidated
- invariants protected
- business terminology consistent
- duplicate concepts removed
- architecture strengthened

Only then proceed to the next module.

## 61. Objective

The purpose of this section is to ensure that repository organization accurately reflects the Domain Model.

Business concepts shall determine repository structure.

Frameworks shall not determine repository structure.

Repository organization shall communicate architecture.

## 62. Repository Purpose

Repositories represent Aggregate persistence.

Repositories are not query engines, business services, or transaction managers.

Repositories exist only to retrieve and persist Aggregate Roots.

## 63. Repository Responsibilities

Repositories may:

- load Aggregate Roots
- persist Aggregate Roots
- delete Aggregate Roots when permitted
- support optimistic concurrency

Nothing more.

## 64. Repository Prohibitions

Repositories shall never contain:

- business rules
- validation
- calculations
- permission logic
- workflow logic
- decision logic
- formatting
- infrastructure-independent utilities

Repositories are persistence abstractions only.

## 65. Repository Interfaces

Repository interfaces belong to the Domain.

Repository implementations belong to Infrastructure.

Never expose implementation details through repository contracts.

## 66. Repository Return Types

Preferred return types:

- Aggregate Root
- Strongly Typed IDs
- Collections of Aggregate Roots

Never expose:

- DbContext
- DbSet
- IQueryable
- EF tracking objects
- SQL
- provider-specific abstractions

## 67. Repository Naming

Repository names shall reflect Aggregate ownership.

Examples:

- IPropertyRepository
- IUserRepository
- IPersonRepository
- IInvoiceRepository

Avoid:

- IDataRepository
- IGenericRepository
- IEntityRepository
- IRepositoryBase

Repository names shall communicate business ownership.

## 68. Domain Service Definition

A Domain Service exists only when behavior spans multiple Aggregates.

If behavior belongs naturally to one Aggregate, it shall remain inside that Aggregate.

## 69. Domain Service Checklist

Verify:

- business language
- cross-aggregate behavior
- stateless
- no persistence logic
- no UI concerns
- no infrastructure concerns

## 70. Domain Service Prohibitions

Domain Services shall not become:

- Utility classes
- Managers
- Helpers
- Processors
- Coordinators

If a service becomes a collection of unrelated methods, split or eliminate it.

## 71. Specification Purpose

Specifications express business rules.

Specifications answer business questions.

They do not express persistence concerns.

## 72. Appropriate Examples

Examples:

- CanAssignUnit
- CanTransferTenant
- CanArchiveProperty
- CanDeleteInvoice
- CanDeactivateUser

Examples are illustrative, not exhaustive.

## 73. Specification Rules

Specifications shall:

- be reusable
- be composable
- be deterministic
- use business terminology
- avoid infrastructure dependencies

## 74. Specification Review

Review every Specification.

Determine:

- Is it duplicated?
- Is it obsolete?
- Can it be merged?
- Does it belong inside an Aggregate?

Keep only canonical implementations.

## 75. Policy Purpose

Policies define configurable business behavior.

Policies do not execute workflows.

Policies do not persist data.

Policies describe business decisions.

## 76. Policy Rules

Policies shall:

- use business terminology
- be independently testable
- be reusable
- be versionable
- remain Infrastructure-independent

## 77. Policy Review

Verify:

- single responsibility
- no duplicate policies
- no persistence logic
- no UI logic
- business language only

## 78. Domain Event Purpose

Domain Events communicate significant business facts.

They describe something that has already occurred.

Events shall never express commands.

## 79. Event Naming

Events shall use past tense.

Examples:

- PropertyCreated
- TenantAssigned
- InvoiceGenerated
- UserActivated
- PaymentRecorded

Avoid imperative names.

Examples:

- CreateProperty
- AssignTenant
- ActivateUser
- GenerateInvoice

## 80. Event Placement

All Domain Events belong inside the Events namespace.

Never place events inside:

- Entities
- ValueObjects
- Repositories
- Infrastructure

## 81. Event Review

Verify:

- event represents business fact
- correct naming
- no duplicate events
- correct namespace
- no infrastructure dependency

## 82. Bounded Context Principle

Each bounded context owns its own business language.

Ownership shall never overlap.

## 83. Context Independence

Examples:

- Identity owns identity.
- Property owns property.
- Billing owns billing.
- Documents own documents.
- Inventory owns inventory.
- Security owns authorization.
- Settings owns configuration.

Communication between contexts shall occur through contracts, not shared implementation.

## 84. Cross-Context Rules

Do not move business rules across contexts.

Do not duplicate entities across contexts.

Do not duplicate value objects without explicit justification.

Reference another context by identity, not by direct ownership.

## 85. Cross-Context Checklist

Verify:

- no circular dependencies
- no duplicated concepts
- no leaked behavior
- ownership explicit
- dependencies intentional

## 86. Namespace Hierarchy

Namespace hierarchy shall exactly mirror folder hierarchy.

There shall be one canonical namespace for every folder.

## 87. Namespace Rules

Avoid:

- mixed namespaces
- legacy namespaces
- temporary namespaces
- experimental namespaces

Rename to match repository organization.

## 88. Dependency Audit

For every project, determine:

- Who depends upon it?
- Who should depend upon it?
- Who should not depend upon it?

Remove unnecessary dependencies.

## 89. Circular Dependency Rule

Circular dependencies are prohibited.

If discovered:

- identify the business responsibility
- move it to the proper owner
- remove the cycle

Never solve circular dependencies by introducing utility projects.

## 90. Architecture Review Completion

Before leaving a module, verify:

- repositories expose aggregate persistence only
- domain services are justified
- specifications are canonical
- policies are canonical
- events correctly placed
- bounded contexts respected
- namespaces normalized
- dependencies simplified

Only then proceed to implementation.

## 91. Implementation Philosophy

Implementation exists to improve architecture.

Not to maximize lines of code.

Not to maximize commits.

Not to satisfy tooling.

Every implementation decision shall improve repository quality.

## 92. Behavior Preservation

Every refactoring shall preserve business behavior.

Acceptable changes:

- improved naming
- improved organization
- improved encapsulation
- removal of duplication
- improved consistency

Unacceptable changes:

- modified business rules
- altered workflows
- changed permissions
- changed calculations
- changed business decisions

Behavioral changes require a separate Implementation Package.

## 93. One Primary Type Per File

Every file shall contain one primary type.

Exceptions:

- small enums
- nested helper types
- private nested classes

Do not group unrelated types together.

## 94. Folder Organization

Folders communicate architecture.

Folders shall represent business concepts.

Never organize folders by technical implementation.

Preferred:

- Identity/
- Property/
- Billing/
- Documents/
- Inventory/
- Communication/

Avoid:

- Helpers/
- Utilities/
- Common/
- Misc/
- Temp/

## 95. File Naming

File names shall exactly match the primary type.

Examples:

- Property.cs
- Unit.cs
- Invoice.cs
- PropertyId.cs
- PropertyCode.cs

Avoid abbreviations.

Avoid generic names.

## 96. Preferred Refactoring Order

Always attempt improvements in this order:

1. Rename
2. Move
3. Merge
4. Simplify
5. Delete
6. Create

Creating new code is the final option.

## 97. Rename Rules

Rename when:

- business terminology is incorrect
- ownership is unclear
- intent is ambiguous

Do not rename solely for stylistic preference.

## 98. Merge Rules

Merge duplicate implementations.

Do not leave competing implementations.

One business concept.

One implementation.

## 99. Delete Rules

Delete:

- obsolete classes
- unused methods
- dead code
- temporary implementations
- unused constructors
- unused factories
- legacy compatibility code no longer required

Never leave unused code "just in case."

Version control preserves history.

## 100. Creating New Files

A new file requires architectural justification.

Examples:

- missing Aggregate
- missing Repository
- missing Domain Event
- missing Specification
- missing Policy
- missing Domain Service
- missing EF Configuration
- missing Test

Do not create files because existing files are large.

## 101. Duplicate Detection

Before creating any new type, determine whether:

- an equivalent implementation already exists
- the concept already exists under another name
- the behavior belongs elsewhere

Duplicate implementations are prohibited.

## 102. Technical Debt Philosophy

Technical debt shall be:

- visible
- intentional
- temporary

Never accidental.

## 103. Acceptable Technical Debt

Debt is acceptable only when:

- business behavior would otherwise change
- migration is incomplete
- future architectural work is required
- implementation risk is excessive

## 104. Recording Technical Debt

Every remaining debt item shall include:

- Description
- Reason
- Affected Files
- Architectural Impact
- Recommended Future Package
- Priority
- Owner

Never leave undocumented temporary work.

## 105. Phase 1A Workarounds

Review all temporary work introduced during Phase 1A.

Examples:

- ignored EF collections
- temporary JSON persistence
- temporary constructors
- mapping workarounds
- compatibility wrappers

Remove them whenever possible.

Otherwise document them.

## 106. Validation Frequency

Validation shall occur after every logical batch.

Never postpone validation until the end.

## 107. Validation Sequence

Execute:

1. dotnet restore
2. dotnet build
3. dotnet test

If any stage fails:

- stop implementation
- fix the failure
- revalidate
- continue

## 108. Build Quality

A successful build is necessary.

It is not sufficient.

The architecture must also improve.

## 109. Test Quality

Tests shall verify business behavior.

Tests are not merely compilation checks.

Update tests only when architecture genuinely changes.

Never rewrite tests merely to satisfy implementation.

## 110. Documentation Synchronization

Whenever architecture changes, determine whether updates are required for:

- Architecture Standards
- ADRs
- Repository Instructions
- Implementation Packages
- README
- XML Documentation
- Developer Guides
- Tests

Documentation shall remain synchronized.

## 111. Batch Deliverables

Every completed batch shall report:

Repository:

- Files Created
- Files Deleted
- Files Renamed
- Files Moved

Domain:

- Aggregates Reviewed
- Aggregates Modified
- Entities Simplified
- Value Objects Consolidated
- Events Corrected
- Specifications Updated
- Policies Updated

Architecture:

- Namespace Corrections
- Dependency Corrections
- Aggregate Boundary Corrections

Validation:

- Restore Status
- Build Status
- Test Status

Technical Debt:

- Removed
- Remaining

Do not provide vague summaries.

Report exact implementation work.

## 112. Self Review Checklist

Before considering a batch complete, verify:

- business behavior preserved
- architecture improved
- duplication reduced
- ownership clarified
- naming improved
- documentation synchronized
- build successful
- tests successful
- no new debt introduced

## 113. Architect Review Checklist

The Architect shall confirm:

- aggregate boundaries correct
- invariants protected
- value objects canonical
- repositories clean
- domain services justified
- specifications reusable
- policies reusable
- events correctly located
- namespaces normalized
- dependencies simplified
- repository health improved

## 114. Completion Criteria

Phase 1B is complete only when:

- PKG scope completed
- repository builds successfully
- tests pass
- aggregate responsibilities explicit
- aggregate boundaries respected
- duplicate concepts removed
- duplicate value objects removed
- duplicate domain events removed
- repository contracts simplified
- specifications consolidated
- policies consolidated
- namespaces normalized
- bounded contexts respected
- obsolete code removed
- technical debt documented
- documentation synchronized

## 115. Exit Criteria

Do not begin Phase 1C until:

- all completion criteria have passed
- all remaining debt has been documented
- the repository is stable
- the architecture review has been completed
- the implementation report has been accepted

Phase 1B is considered complete only after formal architectural acceptance.
