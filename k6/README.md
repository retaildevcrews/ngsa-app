# TODO: Move it under k6 labs repo instead, if so, it will require to makesure the tooling to setup local Cluster is available?

# K6 overview

This repository leverages GitHub Codespaces to deploy a local Cluster using k3d, then shows the user how to run k6 to perform load testing again ngsa-memory and then enable and leverage observability tools.

The final goal of this repository is to provide an environment that helps the user to get familiar with k6 and have a basic undestanding of scripting load test file. The repository contains guided hands-on learning scenarios to familiarize users with k6.

## What is k6?

Grafana k6 is an open-source load testing tool that makes performance testing easy and productive for engineering teams. k6 is free, developer-centric, and extensible.

Using k6, you can test the reliability and performance of your systems and catch performance regressions and problems earlier, k6 will help you to build resilient and performant applications that scale, one of the most K6 powerful feature is that it's a load testing tool scriptable in JavaScript. More documentation can be found [here](https://k6.io/docs/).

## Use cases

The most common case is to use k6 for testing the performance and reliability of APIs, microservices, and websites. More information can be found [here](https://k6.io/docs/#use-cases)

## Get started by opening this repo in CodeSpaces

- Click the Code button
- Click the Codespaces tab
- Click the "Create codespace on main" button
- Choose the `4 core` option

## Run k6 locally from docker image

First of all let's identify the Test Lifecycle stages for a k6 test file.

- Init
- Setup
- VU code
- Teardown

Load test code snippet

![Script Sample File](/k6/images/javascript-sample-file.png)

More information related to the topics listed above can be found here.

- [Test Lifecycle Stages](https://k6.io/docs/using-k6/test-lifecycle/)
- [Options](https://k6.io/docs/using-k6/k6-options/)

Typically, VU code is inside the `default` function, VU code runs over and over through the test duration from start to end in sequence. Once the VU reaches the end of the function, it loops back to the start and executes the code all over.

When creating new load test, usually the first step is to define [HTTP requests](https://k6.io/docs/using-k6/http-requests/) to test and validate endpoints. In this case, VU code makes a series of Get HTTP requests, and performs a check to validate the reponse.

For instance we could check for one or more conditions at the time such as:

- HTTP response code
- Text in the response body
- Response body size

Check definitions can be labeled so when the script includes checks, the summary report shows what type of `check` and how many passed or failed

![Script Sample File](/k6/images/javascript-checks.png)

More information about Checks can be found [here](https://k6.io/docs/using-k6/checks/)


### Load Test Files

Let's identify eacch load test file utilized for this lab.

- `baseline-k6-local.js`, provides a baseline load test for local deployment `http://localhost:8080`
- `benchmark-k6-local.js`, provides a benchmark load test for local deployment `http://localhost:8080`
- `benchmark-k6-cluster.js`, provides a benchmark load test for cluster deployment `http://ngsa-memory.ngsa.svc.cluster.local:8080`


Now let's take a closer look at the load test file utilized for this example, and review its contents with special attention to VU code stage and different type of Checks.

```bash
# Open the file
code baseline-k6-local.js
```

## Run the ngsa container

From codespaces open up two `zsh` terminals

Navigate to terminal `1` to run ngsa-memory

```bash
# Run ngsa-memory
docker run --net=host ghcr.io/retaildevcrews/ngsa-app:beta --in-memory
```

## Run the k6 container

Navigate to the second terminal to run k6

For example, we are going to run a 5-second, 2-VU load test. In order to do that we are going to approach it in two different ways

- Run the load test script by setting VU and duration values in the file under Options.
- Run the load test script by supplying VU and duration values as arguments, this will ovewrite the values under Option if any.

More information about running k6 can be found [here](https://k6.io/docs/get-started/running-k6/)

```bash
# verify ngsa-memory is accessible from the second terminal, curl should return a 200 http code.
curl -I localhost:8080/version

# Option 1: Run k6 with default options for VUs and duration
docker run --rm -v $(pwd)/k6:/scripts --net=host grafana/k6 run /scripts/baseline-k6-local.js

# Option 2: Run k6 with arguments. overwrite VUs and duration by setting those arguments in-line
docker run --rm -v $(pwd)/k6:/scripts --net=host grafana/k6 run --vus 2 --duration 5s  /scripts/baseline-k6-local.js
```

## Review Summary Report

As k6 generates load for your test, it also makes metrics that measure the performance of the system. One way to [analyze results](https://k6.io/docs/get-started/results-output/) is by looking at the summary statistics, at the very end of the test summary report.

The report shows an aggregated summary of all [built-in and custom metrics outputs](https://k6.io/docs/using-k6/metrics/) collected and measure when you run a test, including:

- Checks performed
- Data received and sent
- Median and average values
- Minimum and maximum values
- p90, p95, and p99 values

Now let's take a look at an example of an summary report.

![Script Sample File](/k6/images/javascript-summary.png)

## Deploying Local Cluster and ngsa-memory

```bash
# create cluster, and build/deploy application
make all

# check if cluster and application is deployed
make check
```

## Verify ngsa-memory is up and running

```bash

kubectl get pods -n ngsa

# check ngsa logs
kubectl logs <ngsa-memory pod name> -n ngsa --tail 10
```

## Deploy k6 from docker image

We are deploying the [k6 docker image](https://hub.docker.com/r/loadimpact/k6), however k6 package can be installed on multiple operating systems such as Linux, Mac, and Windows. [here](https://k6.io/docs/get-started/installation/).

Also, we are storing the load test script into a Configmap so it can accessable when k6 scales up.

## How to run a test

The k6 deployment is configured to run 10 VU for 30 seconds

```bash
# create name space
kubectl create namespace k6

# create configmap from load test file
kubectl create configmap k6-config --namespace k6 --from-file=k6/benchmark-k6-cluster.js

# deploy k6 image to local cluster
kubectl apply -f k6/k6.yaml
```

## Verify k6 is up and running

```bash

kubectl get pods -n k6

# check k6 logs
kubectl logs <k6 pod name> -n k6 --tail 20
```

## Verify ngsa-memory is receiving requests from k6

Check logs for ngsa-memory pod and locate the "UserAgent" attribute, and verify that looks like this. `UserAgent":"k6/0.44.0 (https://k6.io/)`

```bash

kubectl get pods -n ngsa
# check ngsa logs
kubectl logs <ngsa-memory pod name> -n ngsa --tail 20
```

## TODO ---- Add Observability

# - Update k6 deployment to enable K6_PROMETHEUS_RW_SERVER_URL

# - how to import dashbords into grafana, try to use kubectl apply ?

- Add k6 official grafana dashboard for k6 prometheus  - [Time series visualization](https://k6.io/docs/results-output/real-time/prometheus-remote-write/#time-series-visualization)

[Official k6 Test Result](https://grafana.com/grafana/dashboards/18030-test-result/)

### Install Grafana and Prometheus

```bash

#  deploy prometheus and grafana
kubectl apply -f k6/deploy/monitoring

kubectl port-forward service/grafana 3000:3000 -n monitoring

kubectl port-forward service/prometheus-service 8080:9090 -n monitoring

Open solution in VS code desktop

# open browser on  http://127.0.0.1:3000 or http://localhost:3000/
```

### Install a jumpbox to test connectivity

```bash
kubectl run jumpbox --image=ghcr.io/cse-labs/jumpbox --restart=Always
```

<!-- ### Install Prometheus and Grafana

```bash

## Install Prometheus
helm repo add prometheus-community https://prometheus-community.github.io/helm-charts

kubectl create namespace monitoring
helm install prometheus-community prometheus-community/prometheus -f k6/monitoring/prometheus_values-app-k6-kube.yaml -n monitoring --version 19.0.2
kubectl apply -f k6/monitoring/prometheus-k6-service.yaml

## Install Grafana

helm repo add grafana https://grafana.github.io/helm-charts
helm install grafana grafana/grafana -f k6/monitoring/grafana_values.yaml -n monitoring
```

### Uninstall Prometheus and Grafana

```bash
 helm uninstall grafana -n monitoring
 helm uninstall prometheus-community -n monitoring
``` -->
