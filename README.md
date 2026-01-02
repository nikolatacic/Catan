# Catan Unity Project

A Unity implementation of the Catan board game, designed with reusability in mind. Systems are built to be extracted into packages for use in future projects.

## 🎯 Project Goal

Build Catan while creating reusable systems that can be extracted into Unity packages and used as a template for future game projects.

## 📁 Project Structure

The project follows a **5-layer architecture** where each layer depends only on layers below it:

```
Infrastructure (Foundation)
    ↓
Core Systems (Reusable Game Systems)
    ↓
Game Systems (Catan-Specific Implementations)
    ↓
Game-Specific (Catan Game Flow)
    ↓
Presentation (UI & Visuals)
```

### Layer Breakdown

- **`Infrastructure/`** - Foundation systems (EventBus, ServiceLocator, Logger)
  - Zero dependencies
  - Package-ready
  
- **`CoreSystems/`** - Generic, reusable game systems
  - Depends on: Infrastructure
  - Examples: Dice, Cards, Trading, Resources
  - Package-ready
  
- **`GameSystems/`** - Catan-specific implementations
  - Depends on: Core Systems + Infrastructure
  - Examples: CatanDice, CatanCards, Building, Robber
  - Wraps Core Systems with Catan rules
  
- **`GameSpecific/`** - Catan game flow and coordination
  - Depends on: Game Systems + Core Systems + Infrastructure
  - Examples: GameManager, PlayerManager, GameState
  
- **`Presentation/`** - UI and visual representation
  - Depends on: All layers below
  - Examples: UIManager, BuildingPlaceholder, CameraController

## 🚀 Getting Started

1. **Open in Unity**: Unity 2022.3.62f2 or later
2. **Review Documentation**: See `Assets/Documentation/` for detailed guides
3. **Run Tests**: Add test scripts to GameObjects to validate systems
4. **Start Building**: Follow the phase plan in `PROJECT_PHASES.md`

## 📚 Documentation

- **[PROJECT_PHASES.md](Assets/Documentation/PROJECT_PHASES.md)** - Implementation phases
- **[ARCHITECTURE.md](Assets/Documentation/ARCHITECTURE.md)** - Detailed architecture
- **[PACKAGE_STRUCTURE.md](Assets/Documentation/PACKAGE_STRUCTURE.md)** - Package extraction guide
- **[CODING_STANDARDS.md](Assets/Documentation/CODING_STANDARDS.md)** - Coding standards

## 🎮 Current Status

**Phase 3 Complete** - Catan Dice System implemented

- ✅ Infrastructure Layer (EventBus, ServiceLocator, Logger)
- ✅ Core Systems - Dice System (generic, reusable)
- ✅ Game Systems - Catan Dice System (Catan-specific wrapper)
- ⏳ Next: Core Systems - Card System

## 🏗️ Architecture Principles

- **Event-Driven**: Systems communicate via events (EventBus)
- **Composition**: Game systems wrap core systems, don't inherit
- **Reusability**: Infrastructure and Core Systems are package-ready
- **Separation of Concerns**: Each layer has a clear responsibility

## 📦 Package Extraction

Systems are designed to be extracted into Unity packages:
- `com.yourcompany.infrastructure` - Foundation
- `com.yourcompany.coresystems` - Generic game systems
- `com.yourcompany.catansystems` - Catan-specific systems

See [PACKAGE_STRUCTURE.md](Assets/Documentation/PACKAGE_STRUCTURE.md) for details.

## 🛠️ Technology Stack

- **Unity**: 2022.3.62f2
- **Language**: C#
- **Architecture**: Event-driven, layered architecture
- **Patterns**: Service Locator, Observer, Composition

## 📝 License

[Add your license here]

## 👤 Author

[Add your name/info here]

---

**Note**: This project is in active development. See `Assets/Documentation/PROGRESS.md` for current status.

