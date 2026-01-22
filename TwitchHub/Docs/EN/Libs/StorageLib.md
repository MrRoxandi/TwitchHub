# StorageLib

## Available Functions

| Name       | Arguments                     | Description                                                 |
| :--------- | :---------------------------- | :---------------------------------------------------------- |
| `load`     | -                             | Loads saved data from the main storage file into memory.    |
| `save`     | -                             | Saves current data to the main storage file.                |
| `backup`   | `string suffix`               | Saves current data to a backup file appended with `suffix`. |
| `contains` | `string key`                  | Checks if an entry exists for the given `key`.              |
| `count`    | -                             | Returns the total number of entries in storage.             |
| `keys`     | -                             | Returns a list of all keys currently in storage.            |
| `get`      | `string key`                  | Retrieves the value (`LuaValue`) associated with `key`.     |
| `set`      | `string key`,`LuaValue value` | Sets the value for the given `key`.                         |
| `remove`   | `string key`                  | Deletes the entry associated with `key`.                    |
| `clear`    | -                             | Clears all data from storage.                               |

### Usage Example

Implementing a simple note storage system:

```lua
local userid = '1298asdanj332' -- Assuming unique ID

function callfn(userid, args)
  if #args == 0 then
    if storagelib:contains(userid) then
      return storagelib:get(userid)
    end
  else
    storagelib:set(userid, args)
  end
  return nil
end

callfn(userid, {'some text'})
local value = callfn(userid, {}) -- Returns: 'some text'
```
