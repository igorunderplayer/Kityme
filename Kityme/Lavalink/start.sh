#!/bin/bash

sed -i "s|DYNAMICPORT|$PORT|" application.yml && java -jar Lavalink.jar