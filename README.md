# Newbie Logger / Console

Feature rich U# Logger / Console for VRChat.

## Features

- Java like log levels.
- Execute commands on locally, globally or on a target player.
- Super weird glitchy syntax for commands.
    - e.x. `executeglobal if (is (me) DerpyNewbie) echo "woo hoo!"`
- Shortcuts for focusing on input field.
- Custom command handlers.

## How to Import

### Import using VCC

1. Open this [link](https://derpynewbie.github.io/vpm-repos/) and add repository to your VCC.
2. Add NewbieLogger package from DerpyNewbie repos
3. Done!

### Import using unitypackage

1. Open [releases](https://github.com/DerpyNewbie/NewbieLogger/releases)
2. Download unitypackage in assets.
3. Import downloaded unitypackage to your project.
4. Done!

## FAQ

### Q. Where is the proper documentation?

I'm lazy, so there's no documentation yet. though it should be written later on.

### Q. How do I write custom command handlers?

Extend `NewbieConsoleCommandHandler` or some derivative class, Implement that abstract method, Register it
to `NewbieConsole` instance, Done!

## Known Issues

- Nested Brackets are misbehaving when used multiple time.
