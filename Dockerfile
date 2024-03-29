### Build and Test the App
#checkov:skip=CKV_DOCKER_2: No healthcheck is needed 
FROM mcr.microsoft.com/dotnet/sdk:6.0-alpine AS build

### copy the source and tests
COPY . /src

WORKDIR /src

# build the app
RUN dotnet publish -c Release -o /app

###########################################################


### Build the runtime container
FROM mcr.microsoft.com/dotnet/aspnet:6.0-alpine AS release

### if port is changed, also update value in Config
EXPOSE 8080
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
COPY --from=build /app .

ENTRYPOINT [ "dotnet",  "aspnetapp.dll" ]
