# TODO:
# Move it under k6 labs repo instead, if so, it will require to makesure the tooling to setup local Cluster is available?

This repository leverages GitHub Codespaces to deploy a local Cluster using k3d, then shows the user how to run k6 to perform load testing again ngsa-memory and then enable and leverage observability tools.

The final goal of this repository is to provide an environment that helps the user to get familiar with k6 and have a basic undestanding of scripting load test file. The repository contains guided hands-on learning scenarios to familiarize users with k6.

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
- Choose the `4 core` option

## Run k6 locally from docker image

First of all let's take a closer look at the load test file utilized for this example and review its contents and learn about `The four lifecycle stages`

```bash
# Open the file
code benchmark-k6-local.js
```




 More information related to the concepts listed above can be found here.
 - [Test Lifecycle Stages](https://k6.io/docs/using-k6/test-lifecycle/)
 - [Options](https://k6.io/docs/using-k6/k6-options/)

## Run the ngsa container

From codespaces open up two `zsh` terminals

Navigate to terminal `1` to run ngsa-memory

```bash
# Run ngsa-memory
docker run --net=host ghcr.io/retaildevcrews/ngsa-app:beta --in-memory
```

## Run the k6 container

Navigate to the second terminal to run k6

For example, we are going to run a 5-second, 2-VU load test. More information can be found [here](https://k6.io/docs/get-started/running-k6/)

```bash
# verify ngsa-memory is accessible from the second terminal, curl should return a 200 http code.
curl -I localhost:8080/version

docker run --rm -v $(pwd)/k6:/scripts --net=host grafana/k6 run /scripts/baseline-k6-local.js

# Then run k6 and overwrite VUs and duration
docker run --rm -v $(pwd)/k6:/scripts --net=host grafana/k6 run --vus 2 --duration 5s  /scripts/baseline-k6-local.js
```

Now let's take a look at the results
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
# - describe how the scripting file looks like and what it does

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
