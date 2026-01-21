<template>
  <div class="container mt-4">
    <h1 class="mb-4">Mail Migration Tool</h1>
    
    <div id="migrationForm">
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
                  <option value="generic">IMAP</option>
                  <option value="gmail">GMail</option>
                </select>
              </div>
              <div class="mb-3">
                <label for="sourceServer" class="form-label">Server</label>
                <input type="text" class="form-control" :class="{ 'bg-secondary bg-opacity-25': !isSourceGeneric }" id="sourceServer" v-model="sourceServer" :readonly="!isSourceGeneric" required>
              </div>
              <div class="mb-3">
                <label for="sourceUser" class="form-label">Username</label>
                <input type="text" class="form-control" :class="{ 'bg-secondary bg-opacity-25': !isSourceGeneric }" id="sourceUser" v-model="sourceUser" :readonly="!isSourceGeneric" required>
              </div>
              <div class="mb-3" v-show="isSourceGeneric">
                <label for="sourcePassword" class="form-label">Password</label>
                <div class="input-group">
                  <input :type="showSourcePassword ? 'text' : 'password'" class="form-control" id="sourcePassword" v-model="sourcePassword" :required="isSourceGeneric">
                  <button class="btn btn-outline-secondary" type="button" @click="showSourcePassword = !showSourcePassword">
                    <i :class="showSourcePassword ? 'bi bi-eye-slash' : 'bi bi-eye'"></i>
                  </button>
                </div>
              </div>
              <div class="mb-3" v-show="isSourceGeneric">
                <button type="button" class="btn btn-outline-primary" @click="authenticateGeneric('source')" :disabled="sourceAuthenticating || !canAuthenticateSource">
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
              <div class="mb-3" v-show="!isSourceGeneric">
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
                  <option value="generic">IMAP</option>
                  <option value="gmail">GMail</option>
                </select>
              </div>
              <div class="mb-3">
                <label for="destServer" class="form-label">Server</label>
                <input type="text" class="form-control" :class="{ 'bg-secondary bg-opacity-25': !isDestGeneric }" id="destServer" v-model="destServer" :readonly="!isDestGeneric" required>
              </div>
              <div class="mb-3">
                <label for="destUser" class="form-label">Username</label>
                <input type="text" class="form-control" :class="{ 'bg-secondary bg-opacity-25': !isDestGeneric }" id="destUser" v-model="destUser" :readonly="!isDestGeneric" required>
              </div>
              <div class="mb-3" v-show="isDestGeneric">
                <label for="destPassword" class="form-label">Password</label>
                <div class="input-group">
                  <input :type="showDestPassword ? 'text' : 'password'" class="form-control" id="destPassword" v-model="destPassword" :required="isDestGeneric">
                  <button class="btn btn-outline-secondary" type="button" @click="showDestPassword = !showDestPassword">
                    <i :class="showDestPassword ? 'bi bi-eye-slash' : 'bi bi-eye'"></i>
                  </button>
                </div>
              </div>
              <div class="mb-3" v-show="isDestGeneric">
                <button type="button" class="btn btn-outline-primary" @click="authenticateGeneric('dest')" :disabled="destAuthenticating || !canAuthenticateDest">
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
              <div class="mb-3" v-show="!isDestGeneric">
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
      
      <div class="card mb-3">
        <div class="card-header">
          <h5 class="mb-0">Migration Options</h5>
        </div>
        <div class="card-body">
          <div class="row">
            <div class="col-md-3">
              <div class="form-check">
                <input class="form-check-input" type="checkbox" id="deleteSource" v-model="deleteSource">
                <label class="form-check-label" for="deleteSource">
                  Delete Source
                </label>
              </div>
            </div>
            <div class="col-md-3">
              <div class="form-check">
                <input class="form-check-input" type="checkbox" id="foldersOnly" v-model="foldersOnly">
                <label class="form-check-label" for="foldersOnly">
                  Folders Only
                </label>
              </div>
            </div>
            <div class="col-md-3">
              <div class="form-check">
                <input class="form-check-input" type="checkbox" id="progressUpdates" v-model="progressUpdates">
                <label class="form-check-label" for="progressUpdates">
                  Progress Updates
                </label>
              </div>
            </div>
            <div class="col-md-3">
              <div class="form-check">
                <input class="form-check-input" type="checkbox" id="replaceExisting" v-model="replaceExisting">
                <label class="form-check-label" for="replaceExisting">
                  Replace Existing
                </label>
              </div>
            </div>
          </div>
        </div>
      </div>
      
      <div class="text-center mb-3">
        <div class="form-check d-inline-block mb-3">
          <input class="form-check-input" type="checkbox" id="tosCheckbox" v-model="tosAccepted">
          <label class="form-check-label" for="tosCheckbox">
            I agree to the <a href="/Tos" target="_blank">Terms of Service</a>
          </label>
        </div>
      </div>
      
      <div class="text-center mb-3">
        <button id="startBtn" class="btn btn-primary btn-lg" @click="startMigration" :disabled="!canStartMigration">
          <i class="bi bi-play-fill"></i> Start Migration
        </button>
      </div>
    </div>
    
    <FeedbackModal ref="feedbackModal" />
  </div>
</template>

<script setup lang="ts">
import { ref, watch, computed } from 'vue';
import type * as signalR from '@microsoft/signalr';
import FeedbackModal from './FeedbackModal.vue';

interface Props {
  connection: signalR.HubConnection;
}

const props = defineProps<Props>();

const sourceServerType = ref<string>('generic');
const sourceServer = ref<string>('');
const sourceUser = ref<string>('');
const sourcePassword = ref<string>('');
const sourceOAuthToken = ref<string>('');
const sourceOAuthStatus = ref<string>('');
const sourceAuthStatus = ref<string>('');
const sourceAuthSuccess = ref<boolean>(false);
const sourceAuthenticating = ref<boolean>(false);

const destServerType = ref<string>('generic');
const destServer = ref<string>('');
const destUser = ref<string>('');
const destPassword = ref<string>('');
const destOAuthToken = ref<string>('');
const destOAuthStatus = ref<string>('');
const destAuthStatus = ref<string>('');
const destAuthSuccess = ref<boolean>(false);
const destAuthenticating = ref<boolean>(false);

const deleteSource = ref<boolean>(false);
const foldersOnly = ref<boolean>(false);
const progressUpdates = ref<boolean>(true);
const replaceExisting = ref<boolean>(false);

const tosAccepted = ref<boolean>(false);

const isStarting = ref<boolean>(false);

const showSourcePassword = ref<boolean>(false);
const showDestPassword = ref<boolean>(false);

const feedbackModal = ref<InstanceType<typeof FeedbackModal> | null>(null);

const isSourceGeneric = computed(() => sourceServerType.value === 'generic');
const isDestGeneric = computed(() => destServerType.value === 'generic');

const canAuthenticateSource = computed(() => {
  return sourceServer.value && sourceUser.value && sourcePassword.value;
});

const canAuthenticateDest = computed(() => {
  return destServer.value && destUser.value && destPassword.value;
});

const isSourceAuthenticated = computed(() => {
  if (!isSourceGeneric.value) {
    return !!sourceOAuthToken.value;
  } else {
    return sourceAuthSuccess.value;
  }
});

const isDestAuthenticated = computed(() => {
  if (!isDestGeneric.value) {
    return !!destOAuthToken.value;
  } else {
    return destAuthSuccess.value;
  }
});

const canStartMigration = computed(() => {
  return !isStarting.value && isSourceAuthenticated.value && isDestAuthenticated.value && tosAccepted.value;
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
  if (isSourceGeneric.value) {
    sourceAuthStatus.value = '';
    sourceAuthSuccess.value = false;
  }
});

watch([destServer, destUser, destPassword], () => {
  if (isDestGeneric.value) {
    destAuthStatus.value = '';
    destAuthSuccess.value = false;
  }
});

async function authenticateGeneric(type: 'source' | 'dest') {
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

    const response = await fetch('/?handler=TestAuth', {
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
    const response = await fetch(`/?handler=GoogleAuth&type=${type}`);
    if (!response.ok) {
      const error = await response.text();
      feedbackModal.value?.alert({
        title: 'Error',
        message: 'Error: ' + error,
        iconClass: 'x-circle'
      });
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
        feedbackModal.value?.alert({
          title: 'OAuth Error',
          message: 'OAuth Error: ' + event.data.error,
          iconClass: 'x-circle'
        });
      }
    }, false);
    
  } catch (error: any) {
    console.error('Error initiating OAuth:', error);
    feedbackModal.value?.alert({
      title: 'Error',
      message: 'Error initiating OAuth: ' + error.message,
      iconClass: 'x-circle'
    });
  }
}

function startMigration() {
  if (!tosAccepted.value) {
    feedbackModal.value?.alert({
      title: 'Terms of Service',
      message: 'Please accept the Terms of Service before starting migration',
      iconClass: 'exclamation-triangle'
    });
    return;
  }

  const sourceUseOAuth = !isSourceGeneric.value;
  const destUseOAuth = !isDestGeneric.value;
  
  const request = {
    sourceServer: sourceServer.value,
    sourceUser: sourceUser.value,
    sourcePassword: sourceUseOAuth ? "" : sourcePassword.value,
    sourceOAuthToken: sourceUseOAuth ? sourceOAuthToken.value : null,
    sourceUseOAuth: sourceUseOAuth,
    destServer: destServer.value,
    destUser: destUser.value,
    destPassword: destUseOAuth ? "" : destPassword.value,
    destOAuthToken: destUseOAuth ? destOAuthToken.value : null,
    destUseOAuth: destUseOAuth,
    deleteSource: deleteSource.value,
    foldersOnly: foldersOnly.value,
    progressUpdates: progressUpdates.value,
    replaceExisting: replaceExisting.value
  };

  if (!request.sourceServer) {
    feedbackModal.value?.alert({
      title: 'Validation Error',
      message: 'Please fill in source server',
      iconClass: 'exclamation-triangle'
    });
    return;
  }

  if (!sourceUseOAuth && !request.sourceUser) {
    feedbackModal.value?.alert({
      title: 'Validation Error',
      message: 'Please fill in source username',
      iconClass: 'exclamation-triangle'
    });
    return;
  }

  if (!sourceUseOAuth && !request.sourcePassword) {
    feedbackModal.value?.alert({
      title: 'Validation Error',
      message: 'Please fill in source password',
      iconClass: 'exclamation-triangle'
    });
    return;
  }

  if (sourceUseOAuth && !request.sourceOAuthToken) {
    feedbackModal.value?.alert({
      title: 'Validation Error',
      message: 'Please authenticate with Google for source server',
      iconClass: 'exclamation-triangle'
    });
    return;
  }

  if (!request.destServer) {
    feedbackModal.value?.alert({
      title: 'Validation Error',
      message: 'Please fill in destination server',
      iconClass: 'exclamation-triangle'
    });
    return;
  }

  if (!destUseOAuth && !request.destUser) {
    feedbackModal.value?.alert({
      title: 'Validation Error',
      message: 'Please fill in destination username',
      iconClass: 'exclamation-triangle'
    });
    return;
  }

  if (!destUseOAuth && !request.destPassword) {
    feedbackModal.value?.alert({
      title: 'Validation Error',
      message: 'Please fill in destination password',
      iconClass: 'exclamation-triangle'
    });
    return;
  }

  if (destUseOAuth && !request.destOAuthToken) {
    feedbackModal.value?.alert({
      title: 'Validation Error',
      message: 'Please authenticate with Google for destination server',
      iconClass: 'exclamation-triangle'
    });
    return;
  }

  isStarting.value = true;

  props.connection.invoke("StartMigration", request).catch((err: Error) => {
    console.error(err.toString());
    feedbackModal.value?.alert({
      title: 'Error',
      message: 'Error starting migration: ' + err.toString(),
      iconClass: 'x-circle'
    });
    isStarting.value = false;
  });
}
</script>
