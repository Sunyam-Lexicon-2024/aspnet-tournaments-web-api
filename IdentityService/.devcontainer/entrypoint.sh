#!/bin/bash

sudo git clone https://github.com/BorisWilhelms/create-dotnet-devcert
sudo chmod +x create-dotnet-devcert/scripts/*.sh
sudo ./create-dotnet-devcert/scripts/ubuntu-create-dotnet-devcert.sh

exec "$@"