<template>
  <div class="container mt-4">
    <h1 class="mb-4">Contact Transfer Tool</h1>
    
    <div id="contactTransferForm">
      <div class="row">
        <div class="col-md-6">
          <div class="card mb-3">
            <div class="card-header bg-primary text-white">
              <h5 class="mb-0">Source Account (Google)</h5>
            </div>
            <div class="card-body">
              <div class="mb-3">
                <label for="sourceServerType" class="form-label">Server Type</label>
                <input type="text" class="form-control bg-secondary bg-opacity-25" id="sourceServerType" value="Google" readonly>
              </div>
              <div class="mb-3">
                <label for="sourceUser" class="form-label">Email</label>
                <input type="text" class="form-control bg-secondary bg-opacity-25" id="sourceUser" v-model="sourceUser" readonly>
              </div>
              <div class="mb-3">
                <button type="button" class="btn btn-outline-primary" @click="authenticateWithGoogle">
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
              <h5 class="mb-0">Destination Account (NextCloud)</h5>
            </div>
            <div class="card-body">
              <div class="mb-3">
                <label for="destServerType" class="form-label">Server Type</label>
                <input type="text" class="form-control bg-secondary bg-opacity-25" id="destServerType" value="NextCloud" readonly>
              </div>
              <div class="mb-3">
                <label for="destServer" class="form-label">Server</label>
                <input type="text" class="form-control" id="destServer" v-model="destServer" placeholder="https://cloud.example.com" required>
              </div>
              <div class="mb-3">
                <label for="destUser" class="form-label">Username</label>
                <input type="text" class="form-control" id="destUser" v-model="destUser" required>
              </div>
              <div class="mb-3">
                <label for="destPassword" class="form-label">Password</label>
                <div class="input-group">
                  <input :type="showDestPassword ? 'text' : 'password'" class="form-control" id="destPassword" v-model="destPassword" required>
                  <button class="btn btn-outline-secondary" type="button" @click="showDestPassword = !showDestPassword">
                    <i :class="showDestPassword ? 'bi bi-eye-slash' : 'bi bi-eye'"></i>
                  </button>
                </div>
              </div>
            </div>
          </div>
        </div>
      </div>
      
      <div class="card mb-3">
        <div class="card-header">
          <h5 class="mb-0">Transfer Options</h5>
        </div>
        <div class="card-body">
          <div class="row">
            <div class="col-md-6">
              <div class="form-check">
                <input class="form-check-input" type="checkbox" id="progressUpdates" v-model="progressUpdates">
                <label class="form-check-label" for="progressUpdates">
                  Progress Updates
                </label>
              </div>
            </div>
            <div class="col-md-6">
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
        <button id="startBtn" class="btn btn-primary btn-lg" @click="startContactTransfer" :disabled="!canStartTransfer">
          <i class="bi bi-play-fill"></i> Start Contact Transfer
        </button>
      </div>
    </div>
    
    <FeedbackModal ref="feedbackModal" />
  </div>
</template>

<script setup lang="ts">
import { ref, computed } from 'vue';
import type * as signalR from '@microsoft/signalr';
import FeedbackModal from './FeedbackModal.vue';

interface Props {
  connection: signalR.HubConnection;
}

const props = defineProps<Props>();

const sourceUser = ref<string>('');
const sourceOAuthToken = ref<string>('');
const sourceOAuthStatus = ref<string>('');

const destServer = ref<string>('');
const destUser = ref<string>('');
const destPassword = ref<string>('');

const progressUpdates = ref<boolean>(true);
const replaceExisting = ref<boolean>(false);

const tosAccepted = ref<boolean>(false);

const isStarting = ref<boolean>(false);

const showDestPassword = ref<boolean>(false);

const feedbackModal = ref<InstanceType<typeof FeedbackModal> | null>(null);

const isSourceAuthenticated = computed(() => {
  return !!sourceOAuthToken.value;
});

const isDestConfigured = computed(() => {
  return !!destServer.value && !!destUser.value && !!destPassword.value;
});

const canStartTransfer = computed(() => {
  return !isStarting.value && isSourceAuthenticated.value && isDestConfigured.value && tosAccepted.value;
});

async function authenticateWithGoogle() {
  try {
    const response = await fetch('/?handler=GoogleAuth&type=source');
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
        if (event.data.serverType === 'source') {
          sourceOAuthToken.value = event.data.token;
          sourceUser.value = event.data.email || '';
          sourceOAuthStatus.value = 'Authenticated';
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

function startContactTransfer() {
  if (!tosAccepted.value) {
    feedbackModal.value?.alert({
      title: 'Terms of Service',
      message: 'Please accept the Terms of Service before starting transfer',
      iconClass: 'exclamation-triangle'
    });
    return;
  }

  if (!sourceOAuthToken.value) {
    feedbackModal.value?.alert({
      title: 'Validation Error',
      message: 'Please authenticate with Google for source account',
      iconClass: 'exclamation-triangle'
    });
    return;
  }

  if (!destServer.value) {
    feedbackModal.value?.alert({
      title: 'Validation Error',
      message: 'Please fill in destination server',
      iconClass: 'exclamation-triangle'
    });
    return;
  }

  if (!destUser.value) {
    feedbackModal.value?.alert({
      title: 'Validation Error',
      message: 'Please fill in destination username',
      iconClass: 'exclamation-triangle'
    });
    return;
  }

  if (!destPassword.value) {
    feedbackModal.value?.alert({
      title: 'Validation Error',
      message: 'Please fill in destination password',
      iconClass: 'exclamation-triangle'
    });
    return;
  }

  const request = {
    sourceServiceType: 'google',
    sourceServer: '',
    sourceUser: sourceUser.value,
    sourcePassword: '',
    sourceOAuthToken: sourceOAuthToken.value,
    sourceUseOAuth: true,
    destServiceType: 'nextcloud',
    destServer: destServer.value,
    destUser: destUser.value,
    destPassword: destPassword.value,
    destOAuthToken: null,
    destUseOAuth: false,
    progressUpdates: progressUpdates.value,
    replaceExisting: replaceExisting.value
  };

  isStarting.value = true;

  props.connection.invoke("StartContactTransfer", request).catch((err: Error) => {
    console.error(err.toString());
    feedbackModal.value?.alert({
      title: 'Error',
      message: 'Error starting contact transfer: ' + err.toString(),
      iconClass: 'x-circle'
    });
    isStarting.value = false;
  });
}
</script>
