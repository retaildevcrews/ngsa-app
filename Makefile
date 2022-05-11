SHELL=/bin/bash
.PHONY: build-ngsa-app create delete

all: create deploy-ngsa-app

delete:
	@k3d cluster delete ngsa-app

create: delete
	@k3d cluster create ngsa-app --registry-use k3d-registry.localhost:5000 --config deploy/k3d.yaml --k3s-arg "--no-deploy=traefik@server:0"

	@kubectl wait node --for condition=ready --all --timeout=60s
	@sleep 5
	@kubectl wait pod -A --all --for condition=ready --timeout=60s

	@istioctl install -y --set profile=demo -f deploy/istio-operator.yaml

	@kubectl create namespace ngsa

	@kubectl label namespace ngsa istio-injection=enabled --overwrite

build-ngsa-app:
	docker build . -t localhost:5000/ngsa-app:local
	docker push localhost:5000/ngsa-app:local

deploy-ngsa-app: build-ngsa-app
	@kubectl create secret generic ngsa-secrets -n ngsa \
      --from-file=CosmosDatabase=secrets/CosmosDatabase \
      --from-file=CosmosCollection=secrets/CosmosCollection \
      --from-file=CosmosKey=secrets/CosmosKey \
      --from-file=CosmosUrl=secrets/CosmosUrl
	
	@kubectl apply -f deploy/ngsa-app.yaml

check:
	@http http://localhost:30000/version
	@http http://localhost:30080/version
