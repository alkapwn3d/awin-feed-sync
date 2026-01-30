# System Management

## Package Manager
Prefer paru: `paru -S/-R/-Syu`
Use pacman for things paru can't do: `pacman -[flags]`

## System
- Services: `systemctl status/start/enable`
- Logs: `journalctl -u service`

## Paths
- User configs: `~/.config/`
- System configs: `/etc/`
- User scripts: `~/.local/bin/` or `~/documents/code/`
- Systemd user: `~/.config/systemd/user/`
