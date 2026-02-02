<template>
  <div class="container mt-4">
    <h1 class="mb-4">Contact Migration Tool</h1>
    
    <div id="contactsForm">
      <div class="row">
        <div class="col-md-6">
          <div class="card mb-3">
            <div class="card-header bg-primary text-white">
              <h5 class="mb-0">Source Account</h5>
            </div>
            <div class="card-body">
              <div class="mb-3">
                <label for="sourceServerType" class="form-label">Server Type</label>
                <select class="form-select" id="sourceServerType" v-model="sourceServerType">
                  <option value="nextcloud">NextCloud</option>
                  <option value="gmail">GMail</option>
                </select>
              </div>
              <div class="mb-3">
                <label for="sourceServer" class="form-label">Server</label>
                <input type="text" class="form-control" :class="{ 'bg-secondary bg-opacity-25': !isSourceNextCloud }" id="sourceServer" v-model="sourceServer" :readonly="!isSourceNextCloud" required>
              </div>
              <div class="mb-3">
                <label for="sourceUser" class="form-label">Username</label>
                <input type="text" class="form-control" :class="{ 'bg-secondary bg-opacity-25': !isSourceNextCloud }" id="sourceUser" v-model="sourceUser" :readonly="!isSourceNextCloud" required>
              </div>
              <div class="mb-3" v-show="isSourceNextCloud">
                <label for="sourcePassword" class="form-label">Password</label>
                <div class="input-group">
                  <input :type="showSourcePassword ? 'text' : 'password'" class="form-control" id="sourcePassword" v-model="sourcePassword" :required="isSourceNextCloud">
                  <button class="btn btn-outline-secondary" type="button" @click="showSourcePassword = !showSourcePassword">
                    <i :class="showSourcePassword ? 'bi bi-eye-slash' : 'bi bi-eye'"></i>
                  </button>
                </div>
              </div>
              <div class="mb-3" v-show="isSourceNextCloud">
                <button type="button" class="btn btn-outline-primary" @click="authenticateNextCloud('source')" :disabled="sourceAuthenticating || !canAuthenticateSource">
                  <span v-if="sourceAuthenticating">
                    <span class="spinner-border spinner-border-sm me-1" role="status" aria-hidden="true"></span>
                    Authenticating...
                  </span>
                  <span v-else>
                    <i class="bi bi-key"></i> Authenticate
                  </span>
                </button>
                <div class="mt-2" v-if="sourceAuthStatus">
                  <span :class="sourceAuthSuccess ? 'text-success' : 'text-danger'">
                    <i :class="sourceAuthSuccess ? 'bi bi-check-circle' : 'bi bi-x-circle'"></i> {{ sourceAuthStatus }}
                  </span>
                </div>
              </div>
              <div class="mb-3" v-show="!isSourceNextCloud">
                <button type="button" class="btn btn-outline-primary" @click="authenticateWithGoogle('source')">
                  <i class="bi bi-google"></i> Authenticate with Google
                </button>
                <div class="mt-2" v-if="sourceOAuthStatus">
                  <span class="text-success"><i class="bi bi-check-circle"></i> {{ sourceOAuthStatus }}</span>
                </div>
              </div>
            </div>
          </div>
        </div>
        
        <div class="col-md-6">
          <div class="card mb-3">
            <div class="card-header bg-success text-white">
              <h5 class="mb-0">Destination Account</h5>
            </div>
            <div class="card-body">
              <div class="mb-3">
                <label for="destServerType" class="form-label">Server Type</label>
                <select class="form-select" id="destServerType" v-model="destServerType">
                  <option value="nextcloud">NextCloud</option>
                  <option value="gmail">GMail</option>
                </select>
              </div>
              <div class="mb-3">
                <label for="destServer" class="form-label">Server</label>
                <input type="text" class="form-control" :class="{ 'bg-secondary bg-opacity-25': !isDestNextCloud }" id="destServer" v-model="destServer" :readonly="!isDestNextCloud" required>
              </div>
              <div class="mb-3">
                <label for="destUser" class="form-label">Username</label>
                <input type="text" class="form-control" :class="{ 'bg-secondary bg-opacity-25': !isDestNextCloud }" id="destUser" v-model="destUser" :readonly="!isDestNextCloud" required>
              </div>
              <div class="mb-3" v-show="isDestNextCloud">
                <label for="destPassword" class="form-label">Password</label>
                <div class="input-group">
                  <input :type="showDestPassword ? 'text' : 'password'" class="form-control" id="destPassword" v-model="destPassword" :required="isDestNextCloud">
                  <button class="btn btn-outline-secondary" type="button" @click="showDestPassword = !showDestPassword">
                    <i :class="showDestPassword ? 'bi bi-eye-slash' : 'bi bi-eye'"></i>
                  </button>
                </div>
              </div>
              <div class="mb-3" v-show="isDestNextCloud">
                <button type="button" class="btn btn-outline-primary" @click="authenticateNextCloud('dest')" :disabled="destAuthenticating || !canAuthenticateDest">
                  <span v-if="destAuthenticating">
                    <span class="spinner-border spinner-border-sm me-1" role="status" aria-hidden="true"></span>
                    Authenticating...
                  </span>
                  <span v-else>
                    <i class="bi bi-key"></i> Authenticate
                  </span>
                </button>
                <div class="mt-2" v-if="destAuthStatus">
                  <span :class="destAuthSuccess ? 'text-success' : 'text-danger'">
                    <i :class="destAuthSuccess ? 'bi bi-check-circle' : 'bi bi-x-circle'"></i> {{ destAuthStatus }}
                  </span>
                </div>
              </div>
              <div class="mb-3" v-show="!isDestNextCloud">
                <button type="button" class="btn btn-outline-primary" @click="authenticateWithGoogle('dest')">
                  <i class="bi bi-google"></i> Authenticate with Google
                </button>
                <div class="mt-2" v-if="destOAuthStatus">
                  <span class="text-success"><i class="bi bi-check-circle"></i> {{ destOAuthStatus }}</span>
                </div>
              </div>
            </div>
          </div>
        </div>
      </div>
      
      <div class="text-center mb-3">
        <button id="migrateBtn" class="btn btn-primary btn-lg" @click="startMigration" :disabled="!canStartMigration">
          <i class="bi bi-arrow-left-right"></i> Migrate Contacts
        </button>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, watch, computed } from 'vue';
import type * as signalR from '@microsoft/signalr';

interface Props {
  connection: signalR.HubConnection;
}

const props = defineProps<Props>();

const sourceServerType = ref<string>('nextcloud');
const sourceServer = ref<string>('');
const sourceUser = ref<string>('');
const sourcePassword = ref<string>('');
const sourceOAuthToken = ref<string>('');
const sourceOAuthStatus = ref<string>('');
const sourceAuthStatus = ref<string>('');
const sourceAuthSuccess = ref<boolean>(false);
const sourceAuthenticating = ref<boolean>(false);

const destServerType = ref<string>('nextcloud');
const destServer = ref<string>('');
const destUser = ref<string>('');
const destPassword = ref<string>('');
const destOAuthToken = ref<string>('');
const destOAuthStatus = ref<string>('');
const destAuthStatus = ref<string>('');
const destAuthSuccess = ref<boolean>(false);
const destAuthenticating = ref<boolean>(false);

const showSourcePassword = ref<boolean>(false);
const showDestPassword = ref<boolean>(false);

const isSourceNextCloud = computed(() => sourceServerType.value === 'nextcloud');
const isDestNextCloud = computed(() => destServerType.value === 'nextcloud');

const canAuthenticateSource = computed(() => {
  return sourceServer.value && sourceUser.value && sourcePassword.value;
});

const canAuthenticateDest = computed(() => {
  return destServer.value && destUser.value && destPassword.value;
});

const isSourceAuthenticated = computed(() => {
  if (!isSourceNextCloud.value) {
    return !!sourceOAuthToken.value;
  } else {
    return sourceAuthSuccess.value;
  }
});

const isDestAuthenticated = computed(() => {
  if (!isDestNextCloud.value) {
    return !!destOAuthToken.value;
  } else {
    return destAuthSuccess.value;
  }
});

const canStartMigration = computed(() => {
  return isSourceAuthenticated.value && isDestAuthenticated.value;
});

watch(sourceServerType, (newType) => {
  if (newType === 'gmail') {
    sourceServer.value = 'imap.gmail.com';
  } else {
    sourceServer.value = '';
    sourceUser.value = '';
    sourceOAuthToken.value = '';
    sourceOAuthStatus.value = '';
  }
  sourceAuthStatus.value = '';
  sourceAuthSuccess.value = false;
});

watch(destServerType, (newType) => {
  if (newType === 'gmail') {
    destServer.value = 'imap.gmail.com';
  } else {
    destServer.value = '';
    destUser.value = '';
    destOAuthToken.value = '';
    destOAuthStatus.value = '';
  }
  destAuthStatus.value = '';
  destAuthSuccess.value = false;
});

watch([sourceServer, sourceUser, sourcePassword], () => {
  if (isSourceNextCloud.value) {
    sourceAuthStatus.value = '';
    sourceAuthSuccess.value = false;
  }
});

watch([destServer, destUser, destPassword], () => {
  if (isDestNextCloud.value) {
    destAuthStatus.value = '';
    destAuthSuccess.value = false;
  }
});

async function authenticateNextCloud(type: 'source' | 'dest') {
  if (type === 'source') {
    sourceAuthenticating.value = true;
    sourceAuthStatus.value = '';
    sourceAuthSuccess.value = false;
  } else {
    destAuthenticating.value = true;
    destAuthStatus.value = '';
    destAuthSuccess.value = false;
  }

  try {
    const request = {
      server: type === 'source' ? sourceServer.value : destServer.value,
      user: type === 'source' ? sourceUser.value : destUser.value,
      password: type === 'source' ? sourcePassword.value : destPassword.value
    };

    const response = await fetch('/Contacts?handler=TestAuth', {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json'
      },
      body: JSON.stringify(request)
    });

    if (!response.ok) {
      const error = await response.text();
      if (type === 'source') {
        sourceAuthStatus.value = 'Authentication failed';
        sourceAuthSuccess.value = false;
      } else {
        destAuthStatus.value = 'Authentication failed';
        destAuthSuccess.value = false;
      }
      return;
    }

    const data = await response.json();
    
    if (type === 'source') {
      sourceAuthStatus.value = data.message;
      sourceAuthSuccess.value = data.success;
    } else {
      destAuthStatus.value = data.message;
      destAuthSuccess.value = data.success;
    }
  } catch (error: any) {
    console.error('Error testing authentication:', error);
    if (type === 'source') {
      sourceAuthStatus.value = 'Error: ' + error.message;
      sourceAuthSuccess.value = false;
    } else {
      destAuthStatus.value = 'Error: ' + error.message;
      destAuthSuccess.value = false;
    }
  } finally {
    if (type === 'source') {
      sourceAuthenticating.value = false;
    } else {
      destAuthenticating.value = false;
    }
  }
}

async function authenticateWithGoogle(type: 'source' | 'dest') {
  try {
    const response = await fetch(`/Contacts?handler=GoogleAuth&type=${type}`);
    if (!response.ok) {
      const error = await response.text();
      alert('Error: ' + error);
      return;
    }
    
    const data = await response.json();
    
    const width = 600;
    const height = 700;
    const left = (screen.width / 2) - (width / 2);
    const top = (screen.height / 2) - (height / 2);
    
    const authWindow = window.open(
      data.authUrl,
      'Google OAuth',
      `width=${width},height=${height},left=${left},top=${top}`
    );
    
    window.addEventListener('message', function(event) {
      if (event.origin !== window.location.origin) {
        return;
      }
      
      if (event.data.type === 'oauth-success') {
        if (event.data.serverType === type) {
          if (type === 'source') {
            sourceOAuthToken.value = event.data.token;
            sourceUser.value = event.data.email || '';
            sourceOAuthStatus.value = 'Authenticated';
          } else {
            destOAuthToken.value = event.data.token;
            destUser.value = event.data.email || '';
            destOAuthStatus.value = 'Authenticated';
          }
        }
      } else if (event.data.type === 'oauth-error') {
        alert('OAuth Error: ' + event.data.error);
      }
    }, false);
    
  } catch (error: any) {
    console.error('Error initiating OAuth:', error);
    alert('Error initiating OAuth: ' + error.message);
  }
}

function startMigration() {
  console.log('Starting contact migration...');
  alert('Contact migration functionality not yet implemented');
}
</script>
