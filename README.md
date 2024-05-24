# Tournaments API

## Index
- [Description](#description)
- [Features](#tbd)
- [Testing/Development](#testingdevelopment)
- [Contact](#contact)

## Description
A Controller based Web API, using ASP.NET Core, to handle getting, creating and 
editing information about Tournaments and their corresponding Games.

## Features
- [x] Model Validations
- [x] Repositories
- [x] Unit of Work
- [x] Sorting
- [x] Filtering
- [x] Searching
- [x] Pagination
- [x] Seed Data
- [x] xUnit Tests
- [x] Logging
- [x] Authentication (OIDC)

## Testing/Development
```
# Terminal; .NET SDK 8.x required
cd Tournament.API && \
dotnet dev-certs https -t && \
dotnet run -lp seed-data
```

## Contact
[visualarea.1@gmail.com](mailto:visualarea.1@gmail.com)

## Resources
- [Linux .NET dev certs bash script](https://github.com/BorisWilhelms/create-dotnet-devcert)
