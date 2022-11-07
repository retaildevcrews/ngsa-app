# NGSA App

NGSA App is inteneded for platform testing and monitoring in one or many Kubernetes clusters and/or cloud deployments.

## Prerequisites

- Bash shell (tested on Visual Studio Codespaces, Mac, Ubuntu, Windows with WSL2)
  - Will not work with WSL1 or Cloud Shell
- Azure CLI ([download](https://docs.microsoft.com/en-us/cli/azure/install-azure-cli?view=azure-cli-latest))
- Docker CLI ([download](https://docs.docker.com/install/))
- .NET 5.0 ([download](https://docs.microsoft.com/en-us/dotnet/core/install/))
- Visual Studio Code (optional) ([download](https://code.visualstudio.com/download))

## Ngsa-app Usage

```text

Usage:
  Ngsa.Application [options]

Options:
  -a, --app-type <App|WebAPI>                                                      Application Type [default: App]
  -p, --prometheus                                                                 Send metrics to Prometheus [default: False]
  -m, --in-memory                                                                  Use in-memory database [default: False]
  -n, --no-cache                                                                   Don't cache results [default: False]
  --url-prefix <url-prefix>                                                        URL prefix for ingress mapping [default: ]
  --port <port>                                                                    Listen Port [default: 8080]
  -d, --cache-duration <cache-duration>                                            Cache for duration (seconds) [default: 300]
  --retries <retries>                                                              Cosmos 429 retries [default: 10]
  --timeout <timeout>                                                              Request timeout [default: 10]
  -s, --data-service <data-service>                                                Data Service URL [default: ]
  -v, --secrets-volume <secrets-volume>                                            Secrets Volume Path [default: secrets]
  -z, --zone <zone>                                                                Zone for log [default: dev]
  -r, --region <region>                                                            Region for log [default: dev]
  -l, --log-level <Critical|Debug|Error|Information|None|Trace|Warning>            Log Level [default: Error]
  -q, --request-log-level <Critical|Debug|Error|Information|None|Trace|Warning>    Request Log Level [default: Information]
  --use-mi-for-cosmos                                                              Use Managed Idendity to authenticate CosmosDB
  --dry-run                                                                        Validates configuration
  --version                                                                        Show version information
  -?, -h, --help                                                                   Show help and usage information

```

## Run the Application

### Using Visual Studio Codespaces

> Visual Studio Codespaces is the easiest way to evaluate ngsa.

To open with codespaces:

- Click the `Code` button on this repo
- Click the `Codespaces` tab
- Click `New Codespace`
- Choose the `4 core` option

### Using bash shell

> This will work from a terminal in Visual Studio Codespaces as well

1. Clone the repo

> git clone https://github.com/retaildevcrews/ngsa-app.git

2. Change to the app root directory

> cd ngsa-app

3. Run the application in memory mode

  Running the application in memory mode allows us to run the application without setting up the rest of the supporting infrastructure.

```bash

# run the application
dotnet run -- -m

```

 You should see the following response:
 > Hosting environment: Production
Content root path: /mnt/c/Users/t-anbassey/Source/ngsa-app
Now listening on: http://[::]:8080
Application started. Press Ctrl+C to shut down.

### Testing the application

Open a new bash shell

> Visual Studio Codespaces allows you to open multiple shells by clicking on the `Split Terminal` icon

```bash

# test the application

# test using httpie (installed automatically in Codespaces)
http localhost:8080/version

# test using curl
curl localhost:8080/version

```

Stop ngsa by typing Ctrl-C or the stop button if run via F5

### [Alternative to secrets] Visual Studio: CosmosDB access using Identity

Below will illistrate how to add your user's principal Id to the correct group so that local development can take advantage of the managed identity.  

In bash add your AAD user to CosmosDB:

```bash
# Get your own Principal ID (replace the email with yours)
export PRINCIPAL=$(az ad user show --id YOUR-MSFT-ID@microsoft.com --query 'id' -o tsv)

export COSMOS_RG=rg-ngsa-asb-dev-cosmos
export COSMOS_NAME=ngsa-asb-dev-cosmos
export COSMOS_SCOPE=$(az cosmosdb show -g $COSMOS_RG -n $COSMOS_NAME --query id -o tsv)

# Add yourself to CosmosDB SQL Access
az cosmosdb sql role assignment create -g $COSMOS_RG --account-name $COSMOS_NAME --role-definition-id 00000000-0000-0000-0000-000000000002 --principal-id $PRINCIPAL --scope $COSMOS_SCOPE

```

## Autogitops

The Ngsa application when running as `--in-memory` mode, utilizes its built-in data storage, so having more than one replica deployed into a cluster will cause each `pod` to have its own local data storage. As a result, requests made to ngsa app endpoints will be managed by the default `load balancer` and can end up at a different `pod` each time where data requested may or may not exist returning unexpected error codes.

🛑 According to the above explanation, there is a restriction for [Ngsa-memory template](./autogitops/dev/ngsa-memory.yaml#replicas), the deployment should only have `1` replica.

## Deploying With Local Cluster

```bash
# delete cluster if exists, create cluster, and build/deploy application
# makes ngsa-memory, to make ngsa-cosmos, run: make create deploy-ngsa-cosmos
make all

# deploy latest changes locally if cluster already exists
make deploy-ngsa-memory

# check if cluster and application is deployed
make check
```

## Run Checkov scan

- Navigate to `Codespaces main menu` (top left icon with three horizontal lines)
- Click on `Terminal` menu item, then `Run Task`
- From tasks menu locate `Run Checkov Scan` and click on it
- Task terminal will show up executing substasks and indicating when scan completed
- Scan results file `checkov_scan_results` will be created at root level, and automatically will get open by VSCode
- Review the file and evaluate failed checks. For instance:

```bash
  kubernetes scan results:

  Passed checks: 860, Failed checks: 146, Skipped checks: 0
  ...
  ...

  dockerfile scan results:

  Passed checks: 22, Failed checks: 4, Skipped checks: 0

  ...
  ...

```

## Contributing

This project welcomes contributions and suggestions. Most contributions require you to agree to a
Contributor License Agreement (CLA) declaring that you have the right to, and actually do, grant us
the rights to use your contribution. For details, visit [Microsoft Contributor License Agreement](https://cla.opensource.microsoft.com).

When you submit a pull request, a CLA bot will automatically determine whether you need to provide
a CLA and decorate the PR appropriately (e.g., status check, comment). Simply follow the instructions
provided by the bot. You will only need to do this once across all repos using our CLA.

This project has adopted the [Microsoft Open Source Code of Conduct](https://opensource.microsoft.com/codeofconduct/).
For more information see the [Code of Conduct FAQ](https://opensource.microsoft.com/codeofconduct/faq/) or
contact [opencode@microsoft.com](mailto:opencode@microsoft.com) with any additional questions or comments.
