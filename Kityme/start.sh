#!/bin/bash

if [ ! -e Lavalink/Lavalink.jar ]; then
    wget https://github.com/davidffa/lavalink/releases/download/v1.0.27/Lavalink.jar -O ./Lavalink/Lavalink.jar
    wait
fi

if [ ! $1 ]; then
    cd Lavalink && java -jar ./Lavalink.jar &
    dotnet run && fg
fi

if [ $1 = "--docker" ]; then
    cd /src/Lavalink && ./start.sh &
    /src/build/Kityme
fi