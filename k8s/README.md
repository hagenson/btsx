# Kubernetes Deployment Guide for BtsxWeb

This guide covers deploying BtsxWeb to a Kubernetes cluster with TLS support via cert-manager and Let's Encrypt.

## Prerequisites

1. **Kubernetes Cluster**: Access to a Kubernetes cluster (v1.19+)
2. **kubectl**: Configured to communicate with your cluster
3. **Docker**: For building and pushing container images
4. **cert-manager**: Installed in your cluster for automatic TLS certificate management
5. **DNS Access**: Ability to configure DNS records for `btsx.nz`
6. **Ingress Controller**: NGINX Ingress Controller with openappsec support

### Install cert-manager

If cert-manager is not already installed in your cluster:

```bash
kubectl apply -f https://github.com/cert-manager/cert-manager/releases/download/v1.13.3/cert-manager.yaml
```

Verify installation:

```bash
kubectl get pods -n cert-manager
```

Wait until all cert-manager pods are running before proceeding.

## Step 1: Build and Push Docker Image

Build the Docker image from the repository root:

```bash
# Build the image
docker build -t your-registry/btsx:latest .

# Tag for versioning (optional)
docker tag your-registry/btsx:latest your-registry/btsx:v1.0.0

# Push to your container registry
docker push your-registry/btsx:latest
docker push your-registry/btsx:v1.0.0
```

**Note**: Replace `your-registry` with your actual container registry (e.g., `docker.io/yourusername`, `gcr.io/your-project`, `registry.example.com`).

Update the image reference in `k8s/deployment.yaml` to match your pushed image.

## Step 2: Configure Environment Variables

Copy the environment template and populate with your values:

```bash
# Copy the template
cp k8s/.env.template k8s/.env

# Edit the .env file with your actual values
```

### Required Values

Edit `k8s/.env` and set the following values:

#### Encryption Key

Generate a secure random encryption key:

```bash
# Linux/macOS
openssl rand -base64 32

# Windows PowerShell
[Convert]::ToBase64String((1..32 | ForEach-Object { Get-Random -Minimum 0 -Maximum 256 }))
```

Copy the output and set it as the `ENCRYPTION_KEY` value in `.env`.

#### Google OAuth Credentials

1. Go to [Google Cloud Console](https://console.cloud.google.com/)
2. Create a new project or select an existing one
3. Navigate to **APIs & Services** > **Credentials**
4. Click **Create Credentials** > **OAuth 2.0 Client ID**
5. Configure the OAuth consent screen if prompted
6. Select **Web application** as the application type
7. Add authorized redirect URI: `https://btsx.nz/OAuthCallback/google`
8. Click **Create** and note the **Client ID** and **Client Secret**

Set the following values in `.env`:
- `GoogleOAuth__ClientId`: Your OAuth Client ID
- `GoogleOAuth__ClientSecret`: Your OAuth Client Secret
- `GoogleOAuth__RedirectUri`: `https://btsx.nz/OAuthCallback/{0}`

#### Support Email

Set `AppConfig__SupportEmail` to your support email address (e.g., `support@btsx.nz`).

## Step 3: Create Kubernetes Secret

Run the provided script to create the Kubernetes secret from your `.env` file:

```bash
./k8s/create-secrets.sh
```

This script will:
- Validate that all required environment variables are set
- Create the `btsx-secrets` secret in the `btsx` namespace
- Automatically handle proper encoding of values

**Important**: Never commit the `.env` file to version control. It contains sensitive credentials.

## Step 4: Configure DNS

Point your domain to the cluster's ingress controller:

1. Get the ingress controller's external IP:

```bash
kubectl get svc -n ingress-nginx
# Or for your specific ingress controller namespace
kubectl get svc -A | grep ingress
```

2. Create an A record for `btsx.nz` pointing to the external IP:

```
Type: A
Name: @
Value: <your-ingress-external-ip>
TTL: 300 (or as preferred)
```

3. Verify DNS propagation:

```bash
nslookup btsx.nz
# or
dig btsx.nz
```

## Step 5: Deploy to Kubernetes

### Option A: Deploy with Kustomize (Recommended)

Deploy all resources with a single command:

```bash
kubectl apply -k k8s/
```

### Option B: Deploy Individual Resources

Deploy resources in order:

```bash
# Create namespace
kubectl apply -f k8s/namespace.yaml

# Create secrets (if not already created in Step 3)
./k8s/create-secrets.sh

# Create cert-manager ClusterIssuer
kubectl apply -f k8s/cert-issuer.yaml

# Create PersistentVolumeClaim
kubectl apply -f k8s/pvc.yaml

# Create deployment
kubectl apply -f k8s/deployment.yaml

# Create service
kubectl apply -f k8s/service.yaml

# Create ingress
kubectl apply -f k8s/ingress.yaml
```

## Step 6: Verify Deployment

Check the status of all resources:

```bash
# Check namespace
kubectl get ns btsx

# Check pods
kubectl get pods -n btsx

# Check deployment
kubectl get deployment -n btsx

# Check service
kubectl get svc -n btsx

# Check ingress
kubectl get ingress -n btsx

# Check certificate (created by cert-manager)
kubectl get certificate -n btsx

# Check certificate request status
kubectl describe certificate btsx-tls -n btsx
```

View pod logs:

```bash
kubectl logs -n btsx -l app=btsx -f
```

## Step 7: Access the Application

Once the certificate is issued (may take 1-2 minutes):

```bash
# Check certificate status
kubectl get certificate -n btsx

# Should show READY=True
```

Access the application at: **https://btsx.nz**

## Troubleshooting

### Pod Not Starting

Check pod status and logs:

```bash
kubectl describe pod -n btsx -l app=btsx
kubectl logs -n btsx -l app=btsx
```

Common issues:
- Image pull errors: Verify image name and registry credentials
- Missing secrets: Verify secret is created and mounted correctly
- Resource constraints: Check cluster has sufficient CPU/memory

### Certificate Issues

Check cert-manager logs:

```bash
kubectl logs -n cert-manager -l app=cert-manager
```

Check certificate request:

```bash
kubectl describe certificaterequest -n btsx
kubectl describe challenge -n btsx
```

Common issues:
- DNS not propagated: Wait for DNS to propagate fully
- HTTP-01 challenge failing: Ensure ingress controller can receive traffic on port 80
- Rate limiting: Let's Encrypt has rate limits; use staging issuer for testing

### Ingress Not Working

Check ingress controller logs:

```bash
kubectl logs -n ingress-nginx -l app.kubernetes.io/component=controller
```

Verify ingress configuration:

```bash
kubectl describe ingress -n btsx
```

## Updating the Deployment

### Update Application Image

```bash
# Build and push new image
docker build -t your-registry/btsx:v1.1.0 .
docker push your-registry/btsx:v1.1.0

# Update deployment
kubectl set image deployment/btsx-deployment btsx=your-registry/btsx:v1.1.0 -n btsx

# Or edit deployment directly
kubectl edit deployment btsx-deployment -n btsx
```

### Update Secrets

```bash
# Edit secrets
kubectl edit secret btsx-secret -n btsx

# Or delete and recreate
kubectl delete secret btsx-secret -n btsx
kubectl apply -f k8s/secret.yaml

# Restart pods to pick up new secrets
kubectl rollout restart deployment/btsx-deployment -n btsx
```

### Scale Deployment

```bash
# Scale to multiple replicas
kubectl scale deployment btsx-deployment --replicas=3 -n btsx
```

**Note**: When scaling horizontally, ensure your PersistentVolumeClaim supports ReadWriteMany access mode, or use a shared storage solution.

## Uninstalling

Remove all resources:

```bash
# Using Kustomize
kubectl delete -k k8s/

# Or manually
kubectl delete namespace btsx
kubectl delete clusterissuer letsencrypt-prod
```

## Security Notes

1. **Secrets Management**: Never commit `k8s/secret.yaml` with actual values to version control
2. **RBAC**: Consider implementing Role-Based Access Control for production deployments
3. **Network Policies**: Implement NetworkPolicies to restrict pod-to-pod communication
4. **Image Scanning**: Scan Docker images for vulnerabilities before deployment
5. **Encryption at Rest**: Enable encryption at rest for PersistentVolumes containing sensitive data
6. **Regular Updates**: Keep dependencies, base images, and Kubernetes components up to date

## Production Considerations

1. **High Availability**: Deploy multiple replicas across different nodes/zones
2. **Resource Limits**: Adjust CPU/memory limits based on actual usage patterns
3. **Monitoring**: Implement monitoring with Prometheus/Grafana
4. **Logging**: Centralize logs with ELK stack or similar
5. **Backup**: Regular backups of PersistentVolume data
6. **Autoscaling**: Configure Horizontal Pod Autoscaler (HPA) based on metrics
7. **Health Checks**: Fine-tune liveness/readiness probe settings

## Support

For issues or questions:
- Check application logs: `kubectl logs -n btsx -l app=btsx`
- Review Kubernetes events: `kubectl get events -n btsx --sort-by='.lastTimestamp'`
- Contact: support@btsx.nz
