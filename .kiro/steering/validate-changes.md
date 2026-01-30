# Validate Changes

## Rule
After making any change, verify it worked. Don't assume success.

## How
- File edits: Read back the file or use getDiagnostics
- Package installs: Check with `paru -Q package-name`
- Service changes: Check with `systemctl status service-name`
- Config changes: Test the actual behavior
- Command execution: Verify output shows success

## Examples
- Installed package → `paru -Q package-name`
- Modified config → Read file or test functionality
- Started service → `systemctl status service-name`
- Created file → `ls -la path/to/file`
- Changed setting → Test that setting actually changed

## Why
Prevents silent failures and ensures changes actually took effect.
