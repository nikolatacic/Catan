# Catan Unity Project - Quick Reference Guide

## Purpose
This is a quick reference guide for developers working on the Catan Unity project. For detailed information, see the full documentation files.

---

## Documentation Files

- **[PROJECT_PHASES.md](./PROJECT_PHASES.md)**: Complete phase breakdown with tasks and deliverables
- **[ARCHITECTURE.md](./ARCHITECTURE.md)**: Detailed architecture documentation
- **[PROGRESS.md](./PROGRESS.md)**: Current progress and status tracking
- **[CODING_STANDARDS.md](./CODING_STANDARDS.md)**: Coding standards and best practices
- **[QUICK_REFERENCE.md](./QUICK_REFERENCE.md)**: This file - quick reference

---

## Current Status

**Phase**: Phase 0 - Project Setup & Documentation  
**Status**: 🟡 In Progress  
**Next**: Phase 1 - Infrastructure Layer

---

## Architecture Layers (Bottom to Top)

1. **Infrastructure** - EventBus, ServiceLocator, Logger
2. **Core Systems** - Generic game systems (Dice, Cards, Trading, etc.)
3. **Game Systems** - Catan-specific implementations
4. **Game-Specific** - Catan game flow and coordination
5. **Presentation** - UI and visual representation

**Rule**: Lower layers never depend on higher layers.

---

## Communication Patterns

1. **Event-Driven** (Preferred): `System A → EventBus → System B`
2. **Service Locator** (For Queries): `ServiceLocator.Get<SystemType>()`
3. **Composition** (Within System): `CatanDiceSystem contains DiceSystem`

---

## Folder Structure

```
Assets/
├── Infrastructure/      # Package-ready, zero dependencies
├── CoreSystems/         # Package-ready, depends on Infrastructure
├── GameSystems/         # Catan-specific, depends on CoreSystems
├── GameSpecific/        # Catan implementation
├── Presentation/        # UI and visuals
└── ScriptableObjects/   # Configuration assets
```

---

## Key Principles

1. **One-way dependencies**: Lower → Higher only
2. **Event-driven**: Systems communicate via events
3. **Composition**: Game systems wrap core systems
4. **Reusability**: Infrastructure and Core Systems are package-ready
5. **Testing**: Each layer testable independently

---

## Naming Conventions Quick Reference

- **Classes**: `PascalCase` (DiceSystem)
- **Interfaces**: `I + PascalCase` (IDiceSystem)
- **Methods**: `PascalCase` (RollDice)
- **Events**: `PascalCase + Event` (DiceRolledEvent)
- **Private Fields**: `_camelCase` (_instance)
- **Properties**: `PascalCase` (CurrentPlayer)
- **Constants**: `UPPER_SNAKE_CASE` (MAX_PLAYERS)

---

## Event Subscription Pattern

```csharp
private void OnEnable()
{
    EventBus.Subscribe<DiceRolledEvent>(OnDiceRolled);
}

private void OnDisable()
{
    EventBus.Unsubscribe<DiceRolledEvent>(OnDiceRolled);
}

private void OnDiceRolled(DiceRolledEvent evt)
{
    // Handle event
}
```

---

## Service Locator Pattern

```csharp
// Registration (in Awake or initialization)
ServiceLocator.Register<IDiceSystem>(diceSystem);

// Usage
var diceSystem = ServiceLocator.Get<IDiceSystem>();
diceSystem.Roll(2);
```

---

## Phase Overview

| Phase | Name | Duration | Status |
|-------|------|----------|--------|
| 0 | Project Setup | 1 day | 🟡 In Progress |
| 1 | Infrastructure | 2-3 days | ⚪ Not Started |
| 2 | Core Dice System | 2-3 days | ⚪ Not Started |
| 3 | Catan Dice System | 1-2 days | ⚪ Not Started |
| 4-15 | [See PROJECT_PHASES.md] | - | ⚪ Not Started |

---

## Quick Links

- **Current Phase Details**: See [PROJECT_PHASES.md](./PROJECT_PHASES.md)
- **Architecture Details**: See [ARCHITECTURE.md](./ARCHITECTURE.md)
- **Progress Tracking**: See [PROGRESS.md](./PROGRESS.md)
- **Coding Standards**: See [CODING_STANDARDS.md](./CODING_STANDARDS.md)

---

**Last Updated**: [Will be updated as we progress]

