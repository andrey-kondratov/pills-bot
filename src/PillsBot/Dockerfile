FROM mcr.microsoft.com/dotnet/runtime-deps:8.0-bookworm-slim AS base

LABEL org.opencontainers.image.source=https://github.com/andrey-kondratov/pills-bot
LABEL org.opencontainers.image.description="A Telegram bot to remind about taking pills. (For my cat.)"
LABEL org.opencontainers.image.licenses=MIT

FROM --platform=$BUILDPLATFORM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

ARG TARGETARCH
ARG TARGETOS

RUN dotnet tool install dotnet-ef -a ${TARGETARCH} --create-manifest-if-needed

COPY . .

WORKDIR /src/PillsBot.Server

RUN dotnet ef migrations bundle -c PillsBotDbContext -o /app/migrate \
--configuration Release -r ${TARGETOS}-${TARGETARCH} --self-contained

RUN dotnet publish -o /app -c Release -a ${TARGETARCH} --self-contained

FROM base AS final
WORKDIR /app
COPY --from=build /app .

ENTRYPOINT ["./bot"]