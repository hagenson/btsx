---
title: "Kubernetes Deployment"
weight: 2
---

# Kubernetes Deployment

Deploy BTSX in your Kubernetes cluster for production use with high availability, scalability, and automated management.

## Prerequisites

### Required Tools

- **Kubernetes Cluster**: Version 1.19 or later
- **kubectl**: Configured to access your cluster
- **Docker**: For building container images
- **Container Registry**: Docker Hub, GCR, ECR, or private registry

### Verify Prerequisites

```bash
# Check kubectl is configured
kubectl version --short

# Check cluster access
kubectl cluster-info

# Check Docker
docker --version
```

## Quick Start

For the impatient, here's the TL;DR:

```bash
# Build and push image
docker build -t your-registry/btsx:latest .
docker push your-registry/btsx:latest

# Deploy
kubectl apply -f k8s/
```

## Detailed Deployment

### 1. Build Container Image

#### Create Dockerfile

Create `Dockerfile` in the repository root:

```dockerfile
# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy solution and projects
COPY BTSX.sln .
COPY btsx/*.csproj ./btsx/
COPY ImapMove/*.csproj ./ImapMove/
COPY btsxweb/*.csproj ./btsxweb/
RUN dotnet restore

# Copy source code
COPY . .

# Build frontend
FROM node:18 AS frontend
WORKDIR /src/btsxweb
COPY btsxweb/package*.json ./
RUN npm install
COPY btsxweb/ ./
RUN npm run build

# Build backend
FROM build AS publish
WORKDIR /src/btsxweb
RUN dotnet publish -c Release -o /app/publish

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=publish /app/publish .
COPY --from=frontend /src/btsxweb/wwwroot/dist ./wwwroot/dist

EXPOSE 80
ENTRYPOINT ["dotnet", "btsxweb.dll"]
```

![Dockerfile](screenshots/dockerfile.png)

#### Build and Push Image

```bash
# Build image
docker build -t your-registry/btsx:latest .

# Tag with version
docker tag your-registry/btsx:latest your-registry/btsx:1.0.0

# Push to registry
docker push your-registry/btsx:latest
docker push your-registry/btsx:1.0.0
```

![Docker Build](screenshots/docker-build.png)

### 2. Create Kubernetes Manifests

#### Namespace

Create `k8s/namespace.yaml`:

```yaml
apiVersion: v1
kind: Namespace
metadata:
  name: btsx
  labels:
    app: btsx
```

#### Secret for Encryption Key

Create `k8s/secret.yaml`:

```yaml
apiVersion: v1
kind: Secret
metadata:
  name: btsx-secrets
  namespace: btsx
type: Opaque
stringData:
  encryption-key: "your-base64-encoded-encryption-key"
  # Optional: OAuth credentials
  google-client-id: "your-google-client-id"
  google-client-secret: "your-google-client-secret"
```

**Generate encryption key**:
```bash
# Linux/macOS
openssl rand -base64 32

# Windows PowerShell
[Convert]::ToBase64String((1..32 | ForEach-Object { Get-Random -Minimum 0 -Maximum 256 }))
```

![Secret Creation](screenshots/k8s-secret.png)

#### Deployment

Create `k8s/deployment.yaml`:

```yaml
apiVersion: apps/v1
kind: Deployment
metadata:
  name: btsx
  namespace: btsx
  labels:
    app: btsx
spec:
  replicas: 2
  selector:
    matchLabels:
      app: btsx
  template:
    metadata:
      labels:
        app: btsx
    spec:
      containers:
      - name: btsx
        image: your-registry/btsx:latest
        imagePullPolicy: Always
        ports:
        - containerPort: 80
          name: http
          protocol: TCP
        env:
        - name: ENCRYPTION_KEY
          valueFrom:
            secretKeyRef:
              name: btsx-secrets
              key: encryption-key
        - name: GOOGLE_CLIENT_ID
          valueFrom:
            secretKeyRef:
              name: btsx-secrets
              key: google-client-id
              optional: true
        - name: GOOGLE_CLIENT_SECRET
          valueFrom:
            secretKeyRef:
              name: btsx-secrets
              key: google-client-secret
              optional: true
        - name: ASPNETCORE_URLS
          value: "http://+:80"
        resources:
          requests:
            memory: "512Mi"
            cpu: "250m"
          limits:
            memory: "2Gi"
            cpu: "1000m"
        livenessProbe:
          httpGet:
            path: /health
            port: http
          initialDelaySeconds: 30
          periodSeconds: 10
        readinessProbe:
          httpGet:
            path: /health
            port: http
          initialDelaySeconds: 5
          periodSeconds: 5
```

![Deployment YAML](screenshots/deployment-yaml.png)

#### Service

Create `k8s/service.yaml`:

```yaml
apiVersion: v1
kind: Service
metadata:
  name: btsx
  namespace: btsx
  labels:
    app: btsx
spec:
  type: ClusterIP
  ports:
  - port: 80
    targetPort: http
    protocol: TCP
    name: http
  selector:
    app: btsx
```

#### Ingress

Create `k8s/ingress.yaml`:

```yaml
apiVersion: networking.k8s.io/v1
kind: Ingress
metadata:
  name: btsx
  namespace: btsx
  annotations:
    cert-manager.io/cluster-issuer: "letsencrypt-prod"
    nginx.ingress.kubernetes.io/ssl-redirect: "true"
spec:
  ingressClassName: nginx
  tls:
  - hosts:
    - btsx.example.com
    secretName: btsx-tls
  rules:
  - host: btsx.example.com
    http:
      paths:
      - path: /
        pathType: Prefix
        backend:
          service:
            name: btsx
            port:
              number: 80
```

Replace `btsx.example.com` with your domain.

![Ingress Configuration](screenshots/ingress-yaml.png)

### 3. Deploy to Kubernetes

```bash
# Create namespace
kubectl apply -f k8s/namespace.yaml

# Create secrets (edit first!)
kubectl apply -f k8s/secret.yaml

# Deploy application
kubectl apply -f k8s/deployment.yaml
kubectl apply -f k8s/service.yaml
kubectl apply -f k8s/ingress.yaml
```

![kubectl Apply](screenshots/kubectl-apply.png)

### 4. Verify Deployment

```bash
# Check pods are running
kubectl get pods -n btsx

# Check service
kubectl get svc -n btsx

# Check ingress
kubectl get ingress -n btsx

# View logs
kubectl logs -n btsx -l app=btsx --tail=50
```

![kubectl Status](screenshots/kubectl-status.png)

## Configuration

### Environment Variables

Configure via deployment environment variables:

```yaml
env:
- name: ASPNETCORE_ENVIRONMENT
  value: "Production"
- name: ASPNETCORE_URLS
  value: "http://+:80"
- name: Logging__LogLevel__Default
  value: "Information"
```

### Resource Limits

Adjust based on your workload:

```yaml
resources:
  requests:
    memory: "512Mi"   # Minimum memory
    cpu: "250m"       # Minimum CPU
  limits:
    memory: "2Gi"     # Maximum memory
    cpu: "1000m"      # Maximum CPU (1 core)
```

### Replicas

Scale horizontally:

```yaml
spec:
  replicas: 3  # Run 3 instances
```

Or use Horizontal Pod Autoscaler:

```yaml
apiVersion: autoscaling/v2
kind: HorizontalPodAutoscaler
metadata:
  name: btsx
  namespace: btsx
spec:
  scaleTargetRef:
    apiVersion: apps/v1
    kind: Deployment
    name: btsx
  minReplicas: 2
  maxReplicas: 10
  metrics:
  - type: Resource
    resource:
      name: cpu
      target:
        type: Utilization
        averageUtilization: 70
```

## Persistent Storage

For job persistence, add a PersistentVolumeClaim:

### PersistentVolumeClaim

Create `k8s/pvc.yaml`:

```yaml
apiVersion: v1
kind: PersistentVolumeClaim
metadata:
  name: btsx-data
  namespace: btsx
spec:
  accessModes:
    - ReadWriteOnce
  resources:
    requests:
      storage: 10Gi
  storageClassName: standard
```

### Mount in Deployment

Update deployment to use the PVC:

```yaml
spec:
  template:
    spec:
      containers:
      - name: btsx
        volumeMounts:
        - name: data
          mountPath: /app/data
      volumes:
      - name: data
        persistentVolumeClaim:
          claimName: btsx-data
```

![PVC Configuration](screenshots/pvc-yaml.png)

## Health Checks

Add health check endpoint in your ASP.NET Core app:

```csharp
// In Program.cs or Startup.cs
app.MapGet("/health", () => Results.Ok(new { status = "healthy" }));
```

Configure probes:

```yaml
livenessProbe:
  httpGet:
    path: /health
    port: http
  initialDelaySeconds: 30
  periodSeconds: 10
  timeoutSeconds: 5
  failureThreshold: 3

readinessProbe:
  httpGet:
    path: /health
    port: http
  initialDelaySeconds: 5
  periodSeconds: 5
  timeoutSeconds: 3
  failureThreshold: 3
```

## Monitoring

### Logging

View logs:

```bash
# All pods
kubectl logs -n btsx -l app=btsx --tail=100 -f

# Specific pod
kubectl logs -n btsx btsx-7d8f9c5b6-abc12 -f
```

### Metrics

If using Prometheus:

Create `k8s/servicemonitor.yaml`:

```yaml
apiVersion: monitoring.coreos.com/v1
kind: ServiceMonitor
metadata:
  name: btsx
  namespace: btsx
spec:
  selector:
    matchLabels:
      app: btsx
  endpoints:
  - port: http
    path: /metrics
    interval: 30s
```

### Dashboard

Create a Grafana dashboard to monitor:
- Pod CPU and memory usage
- Request rate and latency
- Active migrations
- Error rates

![Grafana Dashboard](screenshots/grafana-dashboard.png)

## Scaling

### Manual Scaling

```bash
# Scale to 5 replicas
kubectl scale deployment btsx -n btsx --replicas=5

# Scale down to 2
kubectl scale deployment btsx -n btsx --replicas=2
```

### Auto-scaling

Apply HPA:

```bash
kubectl apply -f k8s/hpa.yaml
```

Monitor autoscaling:

```bash
kubectl get hpa -n btsx
```

## Updating

### Rolling Update

Update image version:

```bash
# Build new image
docker build -t your-registry/btsx:1.1.0 .
docker push your-registry/btsx:1.1.0

# Update deployment
kubectl set image deployment/btsx btsx=your-registry/btsx:1.1.0 -n btsx

# Watch rollout
kubectl rollout status deployment/btsx -n btsx
```

### Rollback

If something goes wrong:

```bash
# Rollback to previous version
kubectl rollout undo deployment/btsx -n btsx

# Rollback to specific revision
kubectl rollout undo deployment/btsx -n btsx --to-revision=2
```

![Rollout Status](screenshots/rollout-status.png)

## Security

### Network Policies

Restrict network access:

Create `k8s/networkpolicy.yaml`:

```yaml
apiVersion: networking.k8s.io/v1
kind: NetworkPolicy
metadata:
  name: btsx
  namespace: btsx
spec:
  podSelector:
    matchLabels:
      app: btsx
  policyTypes:
  - Ingress
  - Egress
  ingress:
  - from:
    - namespaceSelector:
        matchLabels:
          name: ingress-nginx
    ports:
    - protocol: TCP
      port: 80
  egress:
  - to:
    - namespaceSelector: {}
    ports:
    - protocol: TCP
      port: 993  # IMAP SSL
    - protocol: TCP
      port: 53   # DNS
  - to:
    - podSelector: {}
    ports:
    - protocol: TCP
      port: 80
```

### Pod Security

Add security context:

```yaml
spec:
  template:
    spec:
      securityContext:
        runAsNonRoot: true
        runAsUser: 1000
        fsGroup: 1000
      containers:
      - name: btsx
        securityContext:
          allowPrivilegeEscalation: false
          readOnlyRootFilesystem: true
          capabilities:
            drop:
            - ALL
```

### Secrets Management

Use external secrets manager (e.g., Sealed Secrets, External Secrets Operator):

```yaml
apiVersion: external-secrets.io/v1beta1
kind: ExternalSecret
metadata:
  name: btsx-secrets
  namespace: btsx
spec:
  secretStoreRef:
    name: aws-secrets-manager
    kind: SecretStore
  target:
    name: btsx-secrets
  data:
  - secretKey: encryption-key
    remoteRef:
      key: btsx/encryption-key
```

## Troubleshooting

### Pods Not Starting

Check pod status:

```bash
kubectl describe pod -n btsx -l app=btsx
```

Common issues:
- Image pull errors: Check registry credentials
- Resource limits: Adjust requests/limits
- Missing secrets: Verify secrets exist

### Connection Issues

Check service and ingress:

```bash
kubectl get svc,ingress -n btsx
kubectl describe ingress btsx -n btsx
```

### Application Errors

View logs:

```bash
kubectl logs -n btsx -l app=btsx --tail=200
```

### Performance Issues

Check resource usage:

```bash
kubectl top pods -n btsx
```

Consider:
- Increasing resource limits
- Scaling replicas
- Optimizing application code

## Helm Chart (Optional)

For easier deployment, create a Helm chart:

```bash
# Create chart structure
helm create btsx-chart

# Install
helm install btsx ./btsx-chart -n btsx

# Upgrade
helm upgrade btsx ./btsx-chart -n btsx

# Uninstall
helm uninstall btsx -n btsx
```

## Complete Example

Full deployment command sequence:

```bash
# Build and push
docker build -t myregistry/btsx:1.0.0 .
docker push myregistry/btsx:1.0.0

# Generate encryption key
ENCRYPTION_KEY=$(openssl rand -base64 32)

# Create secret
kubectl create namespace btsx
kubectl create secret generic btsx-secrets \
  --from-literal=encryption-key="$ENCRYPTION_KEY" \
  -n btsx

# Deploy
kubectl apply -f k8s/

# Verify
kubectl get all -n btsx
kubectl logs -n btsx -l app=btsx
```

## Next Steps

- **[Getting Started](../../web/start/)** - Use the deployed web interface
- **[Monitoring](../../web/status/)** - Monitor migrations
- **[Local Development](../local/)** - Set up local environment for development

## Best Practices

1. **Use specific image tags**, not `latest`
2. **Set resource limits** to prevent resource exhaustion
3. **Enable health checks** for automatic recovery
4. **Use secrets management** for sensitive data
5. **Implement monitoring** and alerting
6. **Regular updates** and security patches
7. **Backup persistent data** if using PVC
8. **Use namespaces** for isolation
9. **Apply network policies** for security
10. **Test rollbacks** before production deployment
