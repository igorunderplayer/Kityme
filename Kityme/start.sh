#!/bin/bash
if [ ! -e Lavalink/Lavalink.jar ]; then
    wget https://github.com/davidffa/lavalink/releases/download/v1.0.27/Lavalink.jar -O ./Lavalink/Lavalink.jar
    wait
fi

cd Lavalink && java -jar ./Lavalink.jar &

if [ ! $1 ]; then
    dotnet run && fg
fi

if [ $1 = "--heroku" ]; then
    cd $HOME/heroku_output && ./Kityme && fg
fi
