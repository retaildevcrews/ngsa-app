SHELL=/bin/bash
.PHONY: build-ngsa-app create delete

all: create deploy-ngsa-memory

delete:
	@k3d cluster delete ngsa-app

create: delete
	@k3d cluster create ngsa-app --registry-use k3d-registry.localhost:5000 --config deploy/k3d.yaml --k3s-arg "--disable=traefik@server:0"

	@kubectl wait node --for condition=ready --all --timeout=60s
	@sleep 5
	@kubectl wait pod -A --all --for condition=ready --timeout=60s

	@istioctl install -y --set profile=demo -f deploy/istio-operator.yaml

	@kubectl create namespace ngsa

	@kubectl label namespace ngsa istio-injection=enabled --overwrite

build-ngsa-app:
	docker build . -t localhost:5000/ngsa-app:local
	docker push localhost:5000/ngsa-app:local

delete-ngsa-deploys:
	-@kubectl delete --ignore-not-found -f deploy/ngsa-memory.yaml
	-@kubectl delete --ignore-not-found -f deploy/ngsa-cosmos.yaml
	-@kubectl delete --ignore-not-found secret ngsa-secrets -n ngsa

deploy-ngsa-cosmos: build-ngsa-app delete-ngsa-deploys
	@kubectl create secret generic ngsa-secrets -n ngsa \
      --from-file=CosmosDatabase=secrets/CosmosDatabase \
      --from-file=CosmosCollection=secrets/CosmosCollection \
      --from-file=CosmosKey=secrets/CosmosKey \
      --from-file=CosmosUrl=secrets/CosmosUrl
	@kubectl apply -f deploy/ngsa-cosmos.yaml

deploy-ngsa-memory: build-ngsa-app delete-ngsa-deploys
	@kubectl apply -f deploy/ngsa-memory.yaml

check:
	@http http://localhost:30000/version
	@http http://localhost:30080/version

test-baseline:
	@docker run -it --rm --net=host  ghcr.io/retaildevcrews/ngsa-lr:beta --server localhost:30080 --files baseline.json

test-benchmark:
	@docker run -it --rm --net=host  ghcr.io/retaildevcrews/ngsa-lr:beta --server localhost:30080 --files benchmark.json
