# MobaServerSD

SpacetimeDB server module for a MOBA game.

**Hosted on:** `maincloud.spacetimedb.com`
**Module name:** `moba-server`

## Как это работает

SpacetimeDB — это база данных с серверной логикой внутри. Вся игровая логика (движение, атаки, спавн крипов, победа) пишется на C# и компилируется в **WebAssembly**. Этот WASM-модуль загружается прямо в базу данных и выполняется там — никакого отдельного игрового сервера не нужно.

Клиент (Unity) подключается к базе по **WebSocket** и подписывается на таблицы (`Champion`, `Creep`, `Structure` и т.д.). Как только сервер меняет запись в таблице — клиент мгновенно получает обновление. Игрок отправляет только команды (редьюсеры): `MoveToPosition`, `IssueAttack`, `SetReady` и т.п.

**Почему хостится на `maincloud.spacetimedb.com`:**
SpacetimeDB предоставляет бесплатный облачный хостинг для модулей. Не нужно арендовать VPS, настраивать Docker или открывать порты — достаточно одной команды `spacetime publish`, и модуль уже работает в облаке с публичным адресом. Все клиенты подключаются к нему напрямую по WebSocket.

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
