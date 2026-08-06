# Technical Debt

## Debt Categories

| Category          | Description                                                                |
| ----------------- | -------------------------------------------------------------------------- |
| Architecture      | Structural debt that affects boundaries, cohesion, or dependency direction |
| Infrastructure    | Deployment, automation, and environment debt                               |
| Performance       | Throughput, latency, and scaling debt                                      |
| Documentation     | Missing, stale, or inconsistent governance documentation                   |
| Testing           | Validation and regression coverage debt                                    |
| Future Migrations | Planned structural moves deferred until the repository is ready            |

## Debt Items

| Identifier | Description                                                                              | Category          | Impact   | Priority | Proposed Resolution                                                                                                                                                                                          | Target Milestone             | Status   |
| ---------- | ---------------------------------------------------------------------------------------- | ----------------- | -------- | -------- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------ | ---------------------------- | -------- |
| DEBT-001   | Consolidate shared integration contracts into a dedicated contracts assembly             | Future Migrations | Moderate | Deferred | Move Masterdom.Abstractions to Masterdom.Contracts when the repository is ready                                                                                                                              | Deferred Contracts Assembly  | Deferred |
| DEBT-002   | Business documentation folder migration from docs/Business to docs/business-capabilities | Documentation     | Low      | Deferred | Migrate capability framework documents after repository references can be updated safely without disruptive broad replacements                                                                               | Governance Stabilization     | Deferred |
| DEBT-003   | Business Documentation Folder Standardization                                            | Documentation     | Low      | Deferred | Future migration from docs/Business to docs/business-capabilities; retained temporarily because existing references already exist                                                                            | Future Documentation Cleanup | Deferred |
| DEBT-004   | Documentation Information Architecture                                                   | Documentation     | Low      | Deferred | Introduce future hierarchy under docs/00-governance, docs/10-business, docs/20-architecture, docs/30-development, docs/40-operations; current structure remains valid                                        | Future Documentation Cleanup | Deferred |
| DEBT-005   | Architecture Review Checklist                                                            | Architecture      | Medium   | Deferred | Introduce a repository-wide architecture review checklist for future implementation workstreams                                                                                                              | Future Documentation Cleanup | Deferred |
| DEBT-006   | Repository-wide read-model provider filtering                                            | Architecture      | Medium   | Deferred | Status: Deferred. Reason: No demonstrated repository consumer currently requires executable provider-side filtering. Trigger: Implement only when a repository consumer has a proven functional requirement. | Reporting and Read Models    | Deferred |

## Notes

- Every debt item must be tracked here rather than in chat.
- Deferred debt requires explicit justification and a target milestone.
- This document tracks planned debt only and does not authorize implementation outside the active workstream.
