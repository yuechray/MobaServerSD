# MobaServerSD

SpacetimeDB server module for a MOBA game.

**Hosted on:** `maincloud.spacetimedb.com`
**Module name:** `moba-server`

## Deploy

```bash
spacetime publish moba-server --clear-database -y --project-path server
```

## Logs

```bash
spacetime logs moba-server
```

## Generate Unity bindings

```bash
spacetime generate --lang csharp --out-dir <path>/Assets/SpacetimeDB --project-path server
```
