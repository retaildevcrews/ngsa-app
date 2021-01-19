### Build and Test the App
FROM mcr.microsoft.com/dotnet/core/sdk:3.1 AS test

SHELL ["/bin/bash", "-c"]

EXPOSE 4120

### copy the source and tests
COPY src /src

WORKDIR /src/Ngsa.App

# build the app
RUN dotnet publish -c Release -o /app

###########################################################


### Build the runtime container
FROM mcr.microsoft.com/dotnet/core/aspnet:3.1-alpine AS release

### if port is changed, also update value in Constants.cs
EXPOSE 4120
WORKDIR /app

### create a user
### dotnet needs a home directory
RUN addgroup -S ngsa && \
    adduser -S ngsa -G ngsa && \
    mkdir -p /home/ngsa && \
    chown -R ngsa:ngsa /home/ngsa

### run as ngsa user
USER ngsa

### copy the app
COPY --from=test /app .

ENTRYPOINT [ "dotnet",  "Ngsa.App.dll" ]
