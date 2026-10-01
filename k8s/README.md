# Kubernetes manifests (Minikube)

Run these from the repo root, with Minikube started (minikube start --driver=docker).

## 1. Build images into Minikube
    minikube image build -t mini-dev-api:v2 ./api
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

## 4. Enable the Ingress and open the app
    minikube addons enable ingress
    kubectl apply -f k8s/ingress.yaml
    minikube tunnel
Run minikube tunnel in an Administrator PowerShell window and keep it open.
Then browse to http://127.0.0.1

## Notes
- Never delete the db-pvc PVC: the storage class reclaim policy is Delete, so the data goes with it.
- If you edit nginx.conf, recreate the ConfigMap and restart the Pod:
  kubectl rollout restart deployment/nginx
