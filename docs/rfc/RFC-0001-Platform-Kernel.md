# RFC-0001: Platform Kernel

**Status:** Accepted

**Version:** 1.0

**Last Updated:** 2026-07-21

---

## 1. Purpose

The Platform Kernel is the runtime foundation of Masterdom. It is responsible for loading, initializing, managing, and shutting down platform modules in a deterministic and reliable manner.

The kernel contains no business logic.

---

## 2. Responsibilities

The Platform Kernel SHALL:

- Maintain the platform lifecycle.
- Register platform modules.
- Initialize modules in registration order.
- Shut down modules in reverse initialization order.
- Roll back partially initialized modules if startup fails.
- Expose a shared platform context.

---

## 3. Responsibilities Explicitly Excluded

The Platform Kernel SHALL NOT:

- Perform dependency injection.
- Execute business workflows.
- Persist application data.
- Manage users or security.
- Execute scheduled jobs.
- Discover modules automatically (deferred to a future RFC).

---

## 4. Lifecycle

The kernel lifecycle is:

Created
→ Starting
→ Running
→ Stopping
→ Stopped

If startup fails:

Created
→ Starting
→ Faulted

---

## 5. Module Lifecycle

Each module SHALL implement:

- Initialize()
- Shutdown()

Initialize() is called exactly once.

Shutdown() is called exactly once for every successfully initialized module.

Modules are shut down in reverse initialization order.

---

## 6. Error Handling

If any module throws during Initialize():

- startup immediately stops;
- previously initialized modules are shut down;
- the kernel enters the Faulted state;
- the original exception is rethrown.

Exceptions during Shutdown() are ignored so that remaining modules continue shutting down.

---

## 7. Thread Safety

Version 1 of the kernel is single-threaded.

Thread-safe module loading is outside the scope of this RFC.

---

## 8. Public Contracts

The kernel exposes:

- IModule
- IModuleMetadata
- IModuleCatalog
- IModuleRegistry
- IModuleLoader
- IPlatformContext

---

## 9. Future Extensions

Future RFCs may add:

- automatic module discovery;
- dependency ordering;
- module version compatibility;
- optional modules;
- hot loading/unloading;
- digital signatures;
- sandboxed modules.

These additions SHALL preserve backward compatibility where practical.

---

## 10. Non-Goals

The kernel is not an IoC container, plugin marketplace, ORM, workflow engine, rules engine, or messaging framework.

