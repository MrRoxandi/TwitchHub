# HardwareLib

## Available Functions and Objects

### Keyboard

| Name              | Arguments                  | Description                                                                    |
| :---------------- | :------------------------- | :----------------------------------------------------------------------------- |
| `keycodes`        | -                          | A table containing all available key codes for emulation.                      |
| `parsekeycode`    | `string key`               | Converts a string representation of a key into its key code.                   |
| `keycodetostring` | `int code`                 | Converts a key code into its string representation.                            |
| `keydown`         | `int code`                 | Simulates pressing a key down.                                                 |
| `keyup`           | `int code`                 | Simulates releasing a key.                                                     |
| `keytype`         | `int code`                 | Simulates a brief keystroke (press and release).                               |
| `keyhold`         | `int code`, `int duration` | Simulates holding a key down for `duration` milliseconds.                      |
| `typetext`        | `string text`              | Simulates typing a text string (Does not support Cyrillic/Russian characters). |
| `keyisblocked`    | `int code`                 | Checks if a key is currently blocked.                                          |
| `keyblock`        | `int code`                 | Blocks a key from physical input.                                              |
| `keyunblock`      | `int code`                 | Unblocks a key.                                                                |
| `keytoggle`       | `int code`                 | Toggles the blocked state of a key.                                            |

### Mouse

| Name               | Arguments                  | Description                                                             |
| :----------------- | :------------------------- | :---------------------------------------------------------------------- |
| `parsemousebutton` | `string button`            | Converts a string representation of a mouse button into its code.       |
| `buttontostring`   | `int button`               | Converts a mouse button code into its string representation.            |
| `mousedown`        | `int button`               | Simulates pressing a mouse button down.                                 |
| `mouseup`          | `int button`               | Simulates releasing a mouse button.                                     |
| `mouseclick`       | `int button`               | Simulates a brief mouse click.                                          |
| `mousehold`        | `int button, int duration` | Simulates holding a mouse button for `duration` milliseconds.           |
| `scrollvertical`   | `int delta`                | Simulates a vertical mouse wheel scroll.                                |
| `scrollhorizontal` | `int delta`                | Simulates a horizontal mouse wheel scroll.                              |
| `setmouseposition` | `int x, int y`             | Moves the cursor to the absolute coordinates (x, y).                    |
| `mousemove`        | `int dx, int dy`           | Moves the cursor by a relative distance (dx, dy) from current position. |
| `buttonisblock`    | `int keyCode`              | Checks if a mouse button is blocked.                                    |
| `buttonblock`      | `int keyCode`              | Blocks a mouse button.                                                  |
| `buttonunblock`    | `int keyCode`              | Unblocks a mouse button.                                                |
| `buttontoggle`     | `int keyCode`              | Toggles the blocked state of a mouse button.                            |

### Usage Example

Simulating a Left Mouse Click:

```lua
-- other code
local code = hardwarelib:parsemousebutton('left')
hardwarelib:mouseclick(code)
```
