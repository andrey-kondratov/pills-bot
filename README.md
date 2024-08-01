[![Docker Pulls](https://img.shields.io/docker/pulls/andreikondratov/pills-bot)](https://hub.docker.com/r/andreikondratov/pills-bot)

# Pills bot

If you have a cat that needs pills given to him on a regular schedule, and you sometimes forget - this Telegram bot will remind you.

## Prerequisites

- a Telegram bot registered with an API key
- a VM, a PC or a Raspberry Pi 4 with docker runtime installed
- (optional) an Azure OpenAI `gpt-4o` or similar language model (approx. 200 tokens per reminder or $1 per year)

## Configuration

Create a `docker-compose.yml`:

```yml
services:
  bot:
    image: andreikondratov/pills-bot:latest # or latest-arm64v8 if running on Raspberry Pi
    environment:
      PILLSBOT__CONNECTION__APITOKEN: "YOUR TOKEN"
      PILLSBOT__CONNECTION__CHATID: "YOUR CHAT OR USER ID" # send the bot a message to see it in the logs
      PILLSBOT__REMINDER__BEGINS: "2022-09-11T10:00:00"
      PILLSBOT__AI__ENABLED: true
      PILLSBOT__AI__PETNAMES: Whisker McFluffington
      PILLSBOT__AI__PETGENDER: male
      PILLSBOT__AI__AZURE__ENDPOINT: <YOUR AZURE OPENAI ENDPOINT>
      PILLSBOT__AI__AZURE__KEY: <YOUR API KEY TO IT>
      PILLSBOT__AI__AZURE__DEPLOYMENTNAME: gpt-4o
    restart: unless-stopped
```

If you prefer not to use Azure OpenAI and simply send a static message:

```yml
services:
  bot:
    image: andreikondratov/pills-bot:latest
    environment:
      PILLSBOT__CONNECTION__APITOKEN: "YOUR TOKEN"
      PILLSBOT__CONNECTION__CHATID: "YOUR CHAT OR USER ID"
      PILLSBOT__REMINDER__BEGINS: "2022-09-11T10:00:00"
    restart: unless-stopped
```

## Configuration

All the configuration parameters are listed below. 

|Environment variable|Required|Default value|Description|
|---|---|---|---|
|PILLSBOT__CONNECTION__APITOKEN|Yes||The API token for your bot in Telegram|
|PILLSBOT__CONNECTION__CHATID|No||Ignore messages from chats or users other than this one|
|PILLSBOT__REMINDER__BEGINS|No|Start + 5 sec|The UTC date and time after which to schedule reminders|
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
