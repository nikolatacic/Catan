# Package Structure & Extraction Guide

## Overview
This document outlines the package structure, dependencies, and extraction strategy for converting project systems into reusable Unity packages. This is critical for the endgame phases when systems are extracted and the template project is created.

**Last Updated**: 2024-12-19

---

## Package Dependency Hierarchy

```
Infrastructure Package (Foundation)
    ↓
Core Systems Package (Depends on Infrastructure)
    ↓
Catan Game Systems Package (Depends on Core Systems + Infrastructure)
    ↓
Template Project (Uses all packages)
```

---

## Package 1: Infrastructure

### Package Details
- **Name**: `com.yourcompany.infrastructure`
- **Version**: 1.0.0
- **Dependencies**: None (foundation package)
- **Purpose**: Core infrastructure for event-driven architecture

### Contents
```
Infrastructure/
├── package.json
├── README.md
├── Runtime/
│   ├── EventBus/
│   │   ├── EventBus.cs
│   │   └── IEvent.cs
│   ├── ServiceLocator/
│   │   └── ServiceLocator.cs
│   └── Logger/
│       ├── ILogger.cs
│       └── UnityLogger.cs
└── Tests/
    └── InfrastructureTest.cs (optional, for package testing)
```

### package.json
```json
{
  "name": "com.yourcompany.infrastructure",
  "version": "1.0.0",
  "displayName": "Infrastructure",
  "description": "Core infrastructure for event-driven architecture. Includes EventBus, ServiceLocator, and Logger systems.",
  "unity": "2022.3",
  "author": {
    "name": "Your Name",
    "email": "your.email@example.com"
  },
  "dependencies": {}
}
```

### Usage in Other Packages
Other packages reference this via:
```json
"dependencies": {
  "com.yourcompany.infrastructure": "1.0.0"
}
```

---

## Package 2: Core Systems

### Package Details
- **Name**: `com.yourcompany.coresystems`
- **Version**: 1.0.0
- **Dependencies**: Infrastructure package
- **Purpose**: Generic, reusable game systems

### Contents
```
CoreSystems/
├── package.json
├── README.md
├── Runtime/
│   ├── Dice/
│   │   ├── IDiceSystem.cs
│   │   ├── DiceSystem.cs
│   │   ├── DiceRollResult.cs
│   │   ├── DiceConfig.cs (ScriptableObject)
│   │   └── Events/
│   │       └── DiceRolledEvent.cs
│   ├── Cards/
│   │   └── [Card system files]
│   ├── Trading/
│   │   └── [Trading system files]
│   ├── Resources/
│   │   └── [Resource system files]
│   └── TurnManagement/
│       └── [Turn system files]
└── Tests/
    └── [Test files]
```

### package.json
```json
{
  "name": "com.yourcompany.coresystems",
  "version": "1.0.0",
  "displayName": "Core Systems",
  "description": "Generic, reusable game systems for dice, cards, trading, resources, and turn management.",
  "unity": "2022.3",
  "author": {
    "name": "Your Name",
    "email": "your.email@example.com"
  },
  "dependencies": {
    "com.yourcompany.infrastructure": "1.0.0"
  }
}
```

### Key Features
- **No game-specific logic**: All systems are generic
- **Event-driven**: Uses Infrastructure EventBus
- **Configurable**: Uses ScriptableObjects for configuration
- **Reusable**: Can be used in any game project

---

## Package 3: Catan Game Systems (Optional)

### Package Details
- **Name**: `com.yourcompany.catansystems`
- **Version**: 1.0.0
- **Dependencies**: Core Systems + Infrastructure
- **Purpose**: Catan-specific game system implementations

### Contents
```
CatanSystems/
├── package.json
├── README.md
├── Runtime/
│   ├── CatanDice/
│   │   ├── CatanDiceSystem.cs
│   │   └── Events/
│   │       └── CatanDiceRolledEvent.cs
│   ├── CatanCards/
│   │   └── [Catan card system files]
│   ├── CatanTrading/
│   │   └── [Catan trading system files]
│   ├── Building/
│   │   └── [Building system files]
│   ├── Robber/
│   │   └── [Robber system files]
│   └── Victory/
│       └── [Victory system files]
└── Tests/
    └── [Test files]
```

### package.json
```json
{
  "name": "com.yourcompany.catansystems",
  "version": "1.0.0",
  "displayName": "Catan Game Systems",
  "description": "Catan-specific game system implementations built on top of Core Systems.",
  "unity": "2022.3",
  "author": {
    "name": "Your Name",
    "email": "your.email@example.com"
  },
  "dependencies": {
    "com.yourcompany.coresystems": "1.0.0",
    "com.yourcompany.infrastructure": "1.0.0"
  }
}
```

### Key Features
- **Game-specific**: Implements Catan rules
- **Wraps Core Systems**: Uses composition, not inheritance
- **Reusable for Catan**: Can be used in any Catan-like game

---

## Template Project Structure

### Project Details
- **Purpose**: Starting template for new game projects
- **Uses**: All packages as dependencies
- **Contains**: Game-specific code and presentation layer

### Structure
```
TemplateProject/
├── Packages/
│   └── manifest.json (lists all package dependencies)
├── Assets/
│   ├── GameSpecific/
│   │   ├── Managers/
│   │   │   ├── GameManager.cs
│   │   │   └── PlayerManager.cs
│   │   └── Data/
│   │       └── GameState.cs
│   └── Presentation/
│       ├── UI/
│       └── Visuals/
├── ProjectSettings/
└── README.md
```

### manifest.json
```json
{
  "dependencies": {
    "com.yourcompany.infrastructure": "1.0.0",
    "com.yourcompany.coresystems": "1.0.0",
    "com.yourcompany.catansystems": "1.0.0",
    "com.unity.textmeshpro": "3.0.7",
    "com.unity.ugui": "1.0.0"
  }
}
```

---

## Package Extraction Process

### Phase 1: Prepare for Extraction
1. **Ensure code is clean**: Follow coding standards
2. **Remove game-specific code**: Keep only generic logic
3. **Add package.json**: Define package metadata
4. **Create README**: Document package usage
5. **Test in isolation**: Ensure package works standalone

### Phase 2: Create Package Structure
1. **Create package folder**: `Packages/com.yourcompany.packagename/`
2. **Move code**: Move relevant files to `Runtime/` folder
3. **Add package.json**: Define dependencies
4. **Update namespaces**: Ensure proper namespace usage
5. **Test package**: Verify it works as a package

### Phase 3: Update Project to Use Package
1. **Remove old code**: Delete code that's now in package
2. **Update manifest.json**: Add package dependency
3. **Update using statements**: Use package namespaces
4. **Test integration**: Ensure everything still works
5. **Update documentation**: Document package usage

### Phase 4: Version Management
1. **Semantic versioning**: Use MAJOR.MINOR.PATCH
2. **Changelog**: Document changes between versions
3. **Breaking changes**: Increment major version
4. **New features**: Increment minor version
5. **Bug fixes**: Increment patch version

---

## Package Installation Methods

### Method 1: Local Package (Development)
Add to `manifest.json`:
```json
{
  "dependencies": {
    "com.yourcompany.infrastructure": "file:../Packages/Infrastructure"
  }
}
```

### Method 2: Git URL (Version Control)
Add to `manifest.json`:
```json
{
  "dependencies": {
    "com.yourcompany.infrastructure": "https://github.com/yourusername/infrastructure.git#v1.0.0"
  }
}
```

### Method 3: Unity Package Manager Registry (Published)
Add to `manifest.json`:
```json
{
  "dependencies": {
    "com.yourcompany.infrastructure": "1.0.0"
  },
  "scopedRegistries": [
    {
      "name": "Your Company",
      "url": "https://packages.yourcompany.com",
      "scopes": ["com.yourcompany"]
    }
  ]
}
```

---

## Dependency Resolution

### How Unity Resolves Dependencies
1. **Reads manifest.json**: Checks all dependencies
2. **Resolves transitive dependencies**: Automatically includes dependencies of dependencies
3. **Checks versions**: Ensures compatible versions
4. **Loads packages**: Loads in dependency order
5. **Reports conflicts**: Warns about version conflicts

### Example Resolution
When you add `CoreSystems` package:
1. Unity sees it depends on `Infrastructure`
2. Automatically loads `Infrastructure` first
3. Then loads `CoreSystems`
4. Both are available in your project

---

## Version Compatibility

### Version Ranges
```json
{
  "dependencies": {
    "com.yourcompany.infrastructure": "1.0.0"        // Exact version
    "com.yourcompany.infrastructure": "^1.0.0"      // Compatible version (1.0.0 to <2.0.0)
    "com.yourcompany.infrastructure": "~1.0.0"      // Patch updates (1.0.0 to <1.1.0)
    "com.yourcompany.infrastructure": "1.0.0"       // Latest 1.0.x
  }
}
```

### Breaking Changes
- **Major version (2.0.0)**: Breaking changes, may require code updates
- **Minor version (1.1.0)**: New features, backward compatible
- **Patch version (1.0.1)**: Bug fixes, backward compatible

---

## Package Testing Strategy

### Unit Testing
- Test each package in isolation
- Mock dependencies when needed
- Use Unity Test Framework

### Integration Testing
- Test packages together
- Test in template project
- Verify event flow between packages

### Version Testing
- Test package updates
- Test backward compatibility
- Test dependency resolution

---

## Best Practices

### Package Design
1. **Single Responsibility**: Each package has one clear purpose
2. **Minimal Dependencies**: Only depend on what's necessary
3. **Clear Interfaces**: Well-defined public APIs
4. **Documentation**: Comprehensive README and XML docs
5. **Versioning**: Follow semantic versioning

### Package Structure
1. **Consistent Layout**: Use standard Unity package structure
2. **Namespace Organization**: Clear namespace hierarchy
3. **Assembly Definitions**: Use .asmdef files for better organization
4. **Editor Tools**: Separate editor code from runtime code
5. **Tests**: Include test files for validation

### Dependency Management
1. **Explicit Dependencies**: Always declare dependencies
2. **Version Pinning**: Pin to specific versions for stability
3. **Update Carefully**: Test before updating dependencies
4. **Document Breaking Changes**: Clear changelog
5. **Backward Compatibility**: Maintain when possible

---

## Migration Checklist

When extracting a system to a package:

- [ ] Code is clean and follows standards
- [ ] No game-specific logic remains
- [ ] All dependencies are declared
- [ ] package.json is created
- [ ] README.md documents usage
- [ ] Namespaces are properly organized
- [ ] Tests are included (optional)
- [ ] Version number is set
- [ ] Changelog is updated
- [ ] Package tested in isolation
- [ ] Package tested in project
- [ ] Documentation updated

---

## Template Project Setup

### Creating a New Project from Template

1. **Copy template structure**: Copy template project folder
2. **Update manifest.json**: Ensure all packages are listed
3. **Install packages**: Unity will automatically resolve dependencies
4. **Verify imports**: Check that all namespaces are available
5. **Start development**: Begin building your game

### Template Project Contents

- **Game-specific managers**: GameManager, PlayerManager, etc.
- **Game data models**: GameState, PlayerData, etc.
- **Presentation layer**: UI scripts, visual components
- **Configuration**: ScriptableObjects for game settings
- **Scenes**: Basic scene setup
- **Documentation**: How to use the template

---

## Future Considerations

### Package Registry
- Set up Unity Package Manager registry
- Host packages privately or publicly
- Enable easy package distribution

### CI/CD Pipeline
- Automate package building
- Automated testing
- Automated versioning
- Automated publishing

### Package Documentation
- API documentation
- Usage examples
- Tutorial videos
- Community support

---

## References

- [Unity Package Manager Documentation](https://docs.unity3d.com/Manual/Packages.html)
- [Semantic Versioning](https://semver.org/)
- [Unity Package Manifest Schema](https://docs.unity3d.com/Manual/upm-manifestPrj.html)

---

**Note**: This document should be updated as packages are extracted and the template project is created. Keep it synchronized with actual package structure.

