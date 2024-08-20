# Pills bot

If you have a cat that needs pills given to him on a regular schedule, and you sometimes forget - this Telegram bot will remind you.

## Prerequisites

- a Telegram bot registered with an API key
- a VM, a PC or a Raspberry Pi 4 with docker runtime installed
- (optional) an Azure OpenAI `gpt-4o-mini` or similar language model (takes approx. 200 input and 500 output tokens for every 20 reminders, which amounts to 0.011972 EUR per year if reminders are set to be sent twice a day)
- (optional) a Postgres DB server to store messages from AI

## Run

```sh
docker run ghcr.io/andrey-kondratov/pills-bot \
  -e PILLSBOT__TELEGRAM__APITOKEN: "YOUR TOKEN" \
  -e PILLSBOT__TELEGRAM__CHATID: "YOUR CHAT OR USER ID" # send the bot a message to see it in the logs \
  -e PILLSBOT__REMINDER__BEGINS: "2022-09-11T10:00:00" \
  -e PILLSBOT__AI__ENABLED: true \
  -e PILLSBOT__AI__PETNAMES: Whisker McFluffington \
  -e PILLSBOT__AI__PETGENDER: male \
  -e PILLSBOT__AI__AZURE__ENDPOINT: <YOUR AZURE OPENAI ENDPOINT> \
  -e PILLSBOT__AI__AZURE__KEY: <YOUR API KEY TO IT> \
  -e PILLSBOT__AI__AZURE__DEPLOYMENTNAME: gpt-4o-mini \
  -e CONNECTIONSTRINGS__PILLSBOTDBCONTEXT: <YOUR_POSTGRES_DB_CONNECTION>
```

If you prefer not to use Azure OpenAI and send a static message:

```sh
docker run ghcr.io/andrey-kondratov/pills-bot \
  -e PILLSBOT__TELEGRAM__APITOKEN: "YOUR TOKEN" \
  -e PILLSBOT__TELEGRAM__CHATID: "YOUR CHAT OR USER ID" # send the bot a message to see it in the logs \
  -e PILLSBOT__REMINDER__BEGINS: "2022-09-11T10:00:00"
```

## Configuration

All the configuration parameters are listed below. 

|Environment variable|Required|Default value|Description|
|---|---|---|---|
|PILLSBOT__TELEGRAM__APITOKEN|Yes||The API token for your bot in Telegram|
|PILLSBOT__TELEGRAM__CHATID|No||Ignore messages from chats or users other than this one|
|PILLSBOT__REMINDER__BEGINS|No|Start + 5 sec|The UTC date and time after which to schedule reminders|
|PILLSBOT__REMINDER__INTERVAL|No|12 hours|The interval after which a new reminder will be sent|
|PILLSBOT__REMINDER__MESSAGE|No|Pills time!|The default message to send|
|PILLSBOT__AI__ENABLED|No|False|Enabled AI features (requires a model in Azure OpenAI)|
|PILLSBOT__AI__LANGUAGES|No|en-US|Comma-separated list of languages|
|PILLSBOT__AI__PETNAMES|No|unknown|The name(s) the cat is called|
|PILLSBOT__AI__PETGENDER|No|unknown|The gender of the cat (male or female)|
|PILLSBOT__AI__CHOICESCOUNT|No|20|Number of messages to request from AI each time|
|PILLSBOT__AI__MAXTOKENS|No|1000|Max tokens to allow in AI responses|
|PILLSBOT__AI__LOGLEVEL|No|Warning|The minimum log level for Semantic Kernel diagnostics|
|PILLSBOT__AI__AZURE__ENDPOINT|If AI enabled||The Azure OpenAI endpoint|
|PILLSBOT__AI__AZURE__KEY|If AI enabled||The Azure OpenAI API key|
|PILLSBOT__AI__AZURE__DEPLOYMENTNAME|If AI enabled||The Azure OpenAI deployment name|
|APPLICATIONINSIGHTS__CONNECTIONSTRING|No||Azure App Insights connection string|
|CONNECTIONSTRINGS__PILLSBOTDBCONTEXT|Yes||Postgres database connection string|
