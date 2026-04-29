#!/bin/bash
set -e

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
ENV_FILE="$SCRIPT_DIR/.env"
SECRET_NAME="btsx-secrets"
NAMESPACE="btsx"

echo "BTSX Kubernetes Secrets Management"
echo "===================================="
echo ""

if [ ! -f "$ENV_FILE" ]; then
    echo "❌ Error: .env file not found at $ENV_FILE"
    echo ""
    echo "Usage:"
    echo "  1. Copy .env.template to .env:"
    echo "     cp $SCRIPT_DIR/.env.template $SCRIPT_DIR/.env"
    echo ""
    echo "  2. Edit .env and set all required environment variables:"
    echo "     - Persistence__EncryptionKey"
    echo "     - GoogleOAuth__ClientId"
    echo "     - GoogleOAuth__ClientSecret"
    echo "     - GoogleOAuth__RedirectUri"
    echo "     - AppConfig__SupportEmail"
    echo ""
    echo "  3. Run this script again:"
    echo "     $0"
    exit 1
fi

echo "✓ Found .env file at $ENV_FILE"
echo ""

REQUIRED_VARS=(
    "Persistence__EncryptionKey"
    "GoogleOAuth__ClientId"
    "GoogleOAuth__ClientSecret"
    "GoogleOAuth__RedirectUri"
    "AppConfig__SupportEmail"
)

echo "Validating required environment variables..."
source "$ENV_FILE"

MISSING_VARS=()
for var in "${REQUIRED_VARS[@]}"; do
    if [ -z "${!var}" ]; then
        MISSING_VARS+=("$var")
    else
        echo "  ✓ $var is set"
    fi
done

if [ ${#MISSING_VARS[@]} -gt 0 ]; then
    echo ""
    echo "❌ Error: The following required environment variables are not set in .env:"
    for var in "${MISSING_VARS[@]}"; do
        echo "  - $var"
    done
    echo ""
    echo "Please edit $ENV_FILE and set all required variables."
    exit 1
fi

echo ""
echo "All required variables are set ✓"
echo ""

echo "Deleting existing secret if present..."
kubectl delete secret "$SECRET_NAME" --namespace="$NAMESPACE" --ignore-not-found=true

# Ensure the namespace exists
echo ""
echo "Creating the namespace..."
kubectl create ns $NAMESPACE || true

echo ""
echo "Creating Kubernetes secret..."
kubectl create secret generic "$SECRET_NAME" --from-env-file="$ENV_FILE" --namespace="$NAMESPACE"

echo ""
echo "✅ Success! Secret '$SECRET_NAME' has been created in namespace '$NAMESPACE'"
echo ""
echo "You can verify the secret with:"
echo "  kubectl get secret $SECRET_NAME -n $NAMESPACE"
echo "  kubectl describe secret $SECRET_NAME -n $NAMESPACE"
