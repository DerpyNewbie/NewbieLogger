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

### Import using Unity Package Manager

1. Make sure [Git](https://git-scm.com/) is installed on your PC
2. Open Unity Package Manager
3. Press upper left `+` button
4. Do `Add package from git URL` with this
   URL `https://github.com/DerpyNewbie/NewbieLogger.git?path=/Packages/dev.derpynewbie.logger`
5. Done!

## FAQ

### Q. Where is the proper documentation?

I'm lazy, so there's no documentation yet. though it should be written later on.

### Q. How do I write custom command handlers?

Extend `NewbieConsoleCommandHandler` or some derivative class, Implement that abstract method, Register it
to `NewbieConsole` instance, Done!

## Known Issues

- Nested Brackets are misbehaving when used multiple time.
