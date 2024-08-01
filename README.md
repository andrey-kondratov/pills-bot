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

## Environment variables

Environment variables are listed below:

|Environment variable|Required|Default value|Description|
|---|---|---|---|
|PILLSBOT__CONNECTION__APITOKEN|Yes||The API token for your bot in Telegram|
|PILLSBOT__CONNECTION__CHATID|No||Ignore messages from chats or users other than this one|
|PILLSBOT__REMINDER__BEGINS|No|Start + 5 sec|The local date and time after which to schedule reminders|
|PILLSBOT__REMINDER__INTERVAL|No|12 hours|The interval after which a new reminder will be sent|
|PILLSBOT__REMINDER__MESSAGE|No|Pills time!|The default message to send|
|PILLSBOT__AI__ENABLED|No|False|Enabled AI features (requires a model in Azure OpenAI)|
|PILLSBOT__AI__LANGUAGES|No|English|Comma-separated list of languages|
|PILLSBOT__AI__PETNAMES|No|unknown|The name(s) the cat is called|
|PILLSBOT__AI__PETGENDER|No|unknown|The gender of the cat (male or female)|
|PILLSBOT__AI__LOGLEVEL|No|Warning|The minimum log level for Semantic Kernel diagnostics|
|PILLSBOT__AI__AZURE__ENDPOINT|If AI enabled||The Azure OpenAI endpoint|
|PILLSBOT__AI__AZURE__KEY|If AI enabled||The Azure OpenAI API key|
|PILLSBOT__AI__AZURE__DEPLOYMENTNAME|If AI enabled||The Azure OpenAI deployment name|
