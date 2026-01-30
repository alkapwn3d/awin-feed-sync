# Fish Shell Rules

## Critical
- ALL commands run in fish (not bash)
- NO: heredocs, &&, ||, cat << EOF

## Multi-line Files
USE: echo -e with \n
Example: echo -e "line1\nline2" > file

## Syntax
- Variables: set VAR value
- Export: set -gx VAR value
- Sequential: ; or ; and / ; or
- Pipes work: cmd1 | cmd2

## Tools
- Use fd not find
- Use rg not grep -r
- fsWrite/strReplace: workspace only
