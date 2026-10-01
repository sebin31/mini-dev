# Kubernetes manifests (Minikube)

Run these from the repo root, with Minikube started (minikube start --driver=docker).

## 1. Build images into Minikube
    minikube image build -t mini-dev-api:v1 ./api
    minikube image build -t mini-dev-frontend:v1 ./frontend

## 2. Create the Secret and ConfigMap (these exist only in the cluster, not in Git)
    kubectl create secret generic db-secret --from-literal=POSTGRES_PASSWORD='<your-password>'
    kubectl create configmap nginx-config --from-file=default.conf=nginx/nginx.conf

## 3. Apply the manifests in this order
    kubectl apply -f k8s/db-pvc.yaml
    kubectl apply -f k8s/postgres.yaml
    kubectl apply -f k8s/api.yaml
    kubectl apply -f k8s/frontend.yaml
    kubectl apply -f k8s/nginx.yaml

## 4. Open the app
    kubectl port-forward service/nginx 8080:80
Then browse to http://localhost:8080 (keep that window open).

## Notes
- Never delete the db-pvc PVC: the storage class reclaim policy is Delete, so the data goes with it.
- If you edit nginx.conf, recreate the ConfigMap and restart the Pod:
  kubectl rollout restart deployment/nginx
