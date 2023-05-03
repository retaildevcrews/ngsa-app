# What is k6?

Grafana k6 is an open-source load testing tool that makes performance testing easy and productive for engineering teams. k6 is free, developer-centric, and extensible.

Using k6, you can test the reliability and performance of your systems and catch performance regressions and problems earlier. k6 will help you to build resilient and performant applications that scale.

k6 is a high-performing load testing tool, scriptable in JavaScript

More documentation can be found [here](https://k6.io/docs/).

## Use cases

The most common case is to use k6 for testing the performance and reliability of APIs, microservices, and websites. More information can be found [here](https://k6.io/docs/#use-cases)

## Get started by opening this repo in CodeSpaces

- Click the Code button
- Click the Codespaces tab
- Click the "Create codespace on main" button
- Choose the `4 core` option   // TBD is 4 core enough? should we pick 8 or 16 cores since we need Grafana and Prometheus ?

## Run k6 locally from docker image

From codespaces open up two `zsh` terminals

Navigate to terminal 1 to run ngsa-memory

```bash
# Run ngsa-memory
docker run --net=host ghcr.io/retaildevcrews/ngsa-app:beta --in-memory

```

Navigate to the second terminal to run k6

For example, we are going to run a 5-second, 2-VU load test. More information can be found [here](https://k6.io/docs/get-started/running-k6/)

```bash
# verify ngsa-memory is accessible
curl -s localhost:8080/version
or
curl -I localhost:8080/version

# Run k6
docker run --rm -v $(pwd)/k6:/scripts --net=host  loadimpact/k6:latest run --vus 2 --duration 5s  /scripts/baseline-k6-local.js
```

# TODO:
  - analyze load tests configuration from 'baseline-k6-local.js' , show different type pf checks and Metrics

     - ✓ contentType
     - ✓ minLength
     - ✓ maxLength
     - ✓ contains1
     - ✓ contains2
     - ✓ notContains1
     - ✓ jsonArray
     - ✓ statusCode

        - [HTTP request](https://k6.io/docs/using-k6/http-requests/)
        - [Checks](https://k6.io/docs/using-k6/checks/)
        - [Rampimg up/down VUs](https://k6.io/docs/get-started/running-k6/#stages-ramping-up-down-vus)
  - add instructions about how to [Analyze results](https://k6.io/docs/get-started/results-output/)

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

# TODO:
# - describe how does the scripting file looks like and what it does

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
- Add k6 official grafana dashboard for k6 prometheus  - [Time series visualization](https://k6.io/docs/results-output/real-time/prometheus-remote-write/#time-series-visualization)

[Official k6 Test Result](https://grafana.com/grafana/dashboards/18030-test-result/)

### Install Grafana and Prometheus

```bash
#  deploy prometheus and grafana
  kubectl apply -f k6/deploy/monitoring
```

### Install a jumpbox to test connectivity

```bash
kubectl run jumpbox --image=ghcr.io/cse-labs/jumpbox --restart=Always
```
