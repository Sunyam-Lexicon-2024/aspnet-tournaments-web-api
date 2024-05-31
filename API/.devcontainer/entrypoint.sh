#!/bin/bash

dotnet tool install --global dotnet-ef
sudo git clone https://github.com/BorisWilhelms/create-dotnet-devcert
sudo chmod +x create-dotnet-devcert/scripts/*.sh
sh ./create-dotnet-devcert/scripts/ubuntu-create-dotnet-devcert.sh

exec "$@"
