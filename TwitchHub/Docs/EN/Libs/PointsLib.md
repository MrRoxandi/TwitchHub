# PointsLib

## Available Functions

| Name   | Arguments                       | Description                                                                    |
| :----- | :------------------------------ | :----------------------------------------------------------------------------- |
| `get`  | `string userid`                 | Returns the point balance for the user with `userid`.                          |
| `set`  | `string userid`, `long ammount` | Sets the balance for `userid` to `ammount`.                                    |
| `add`  | `string userid`, `long ammount` | Adds `ammount` points to the balance of `userid`.                              |
| `take` | `string userid`, `long ammount` | Deducts `ammount` points from `userid`. Returns a success result (true/false). |

### Usage Example

Checking and topping up user points:

```lua
local userid = '1298asdanj332'
local upoints = pointslib:get(userid)

if upoints < 150 then
  local needed = 150 - upoints
  pointslib:add(userid, needed)
end
```
