# BitDuc Scene Injection

This package provides a simple injection mechanism for Unity that relies on the scene structure for configuring dependencies.

## Features

- Configure dependency lookup with `[FromRoot]`, `[FromSelf]` `[FromParent]`, `[FromChild]` and `[FromSibling]` attributes.
- Optional dependencies are supported through the `[Optional]` attribute.
- Designed to integrate smoothly into any Unity project using `asmdef` files for modularity.

## Installation

### Via Unity Package Manager

1. Clone the repository or download it as a ZIP.
2. Open your Unity project.
3. Go to `Window -> Package Manager` and click the `+` button.
4. Select `Add package from git URL`.
5. Input the following url: `git@github.com:RowDaBoat/scene-injection.git?path=/Assets`.

### Manual Installation

1. Clone or download the repository.
2. Copy the `BitDuc` folder from `Assets/Scripts` into your project's `Assets` folder.

## Usage

Use 
```csharp
myBehavior.Inject()
```
on any `MonoBehavior` to resolve its dependencies.

### Injecting from a root GameObject
Get a service from a `GameObject` atop of the scene.

```
[Services: OneService, OtherService, SomeService]
[Characters]
  |--[MyCharacter1: MyBehavior]
  |--[MyCharacter2: MyBehavior]
  '--[MyCharacter3: MyBehavior]
```

```csharp
class MyBehavior : MonoBehavior {
    [FromRoot] SomeService someService;

    void Awake() {
        Inject();
    }
}
```

### Optionally injecting from a child GameObject
All three instances of `MyBehavior` will resolve the `SomeService` dependency on the "Services" `GameObject` on awake.

Get components in children `GameObject`s

```
[MyCharacter: CharacterBehavior]
  |--[Sword: Sword]
  |--[Shield: Shield]
  '--[Armor: Armor]
```

```csharp
class CharacterBehavior : MonoBehavior {
    [FromChild, Optional] Sword sword;
    [FromChild, Optional] Shield shield;
    [FromChild, Optional] Armor armor;

    void Awake() {
        Inject();
    }
}
```

`MyCharacter` will resolve the `sword`, `shield`, and `armor` dependencies, if there are any in the character's hierarchy.
