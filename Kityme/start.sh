
if [ ! -e Lavalink/Lavalink.jar ]
then
    wget https://github.com/davidffa/lavalink/releases/download/v1.0.21/Lavalink.jar -O ./Lavalink/Lavalink.jar
    wait
fi
cd Lavalink && java -jar ./Lavalink.jar &
dotnet run &&
fg
