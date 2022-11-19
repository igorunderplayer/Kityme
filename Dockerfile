FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src

COPY ./Kityme /src
RUN dotnet restore -r linux-x64 && dotnet publish -c Release -r linux-x64 --no-restore --self-contained -p:PublishSingleFile=true -p:EnableCompressionInSingleFile=true -p:DebugType=embedded

FROM azul/zulu-openjdk:13-latest
WORKDIR /src

RUN apt update && apt upgrade -y
RUN apt install libicu-dev -y

COPY --from=build /src/bin/Release/net6.0/linux-x64/publish /src/build
COPY --from=build /src/Lavalink /src/Lavalink
COPY --from=build /src/start.sh /src/start.sh

ENTRYPOINT /src/start.sh --docker