[![Docker Pulls](https://img.shields.io/docker/pulls/andreikondratov/pills-bot)](https://hub.docker.com/r/andreikondratov/pills-bot)

# Pills bot

Create a `docker-compose.yml`:

```yml
version: "3"

services:
  bot:
    image: andreikondratov/pills-bot:latest # or latest-arm64v8
    environment:
      PILLSBOT__CONNECTION__APITOKEN: "" # your Telegram bot's API token
      PILLSBOT__CONNECTION__CHATID: "" # your Telegram group chat ID
      PILLSBOT__REMINDER__BEGINS: "2022-09-11T10:00:00" # the begin date and time (local)
    restart: unless-stopped
```

Run `docker compose up -d` on your server and see in the logs:

```sh
$ docker compose logs
pills-bot  | [17:17:47 INF] Starting host
pills-bot  | [17:17:48 INF] Starting bot...
pills-bot  | [17:17:48 INF] Creating the client...
pills-bot  | [17:17:48 INF] Application started. Press Ctrl+C to shut down.
pills-bot  | [17:17:48 INF] Hosting environment: Production
pills-bot  | [17:17:48 INF] Content root path: /app
pills-bot  | [17:17:49 INF] Started receiving updates.
pills-bot  | [17:17:49 INF] Bot started.
pills-bot  | [17:17:49 INF] Next reminder comes off at 06/18/2023 20:00:00
```

