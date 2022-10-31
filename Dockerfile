FROM mcr.microsoft.com/dotnet/sdk:6.0-alpine AS build
WORKDIR /src

COPY ./Kityme /src
RUN dotnet restore -r linux-musl-x64 && dotnet publish -c Release -r linux-musl-x64 --no-restore --self-contained -p:PublishSingleFile=true -p:EnableCompressionInSingleFile=true -p:DebugType=embedded

FROM alpine:latest
WORKDIR /src

COPY --from=build /src/bin/Release/net6.0/linux-musl-x64/publish /src/build
COPY --from=build /src/Lavalink /src/Lavalink
COPY --from-build /src/start.sh /src/start.sh
RUN apk upgrade --update-cache --available && apk add openssl libstdc++ icu-libs && rm -rf /var/cache/apk/*

CMD ./Kityme/start.sh