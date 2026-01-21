<template>
  <div class="container mt-4">
    <h1>Processing OAuth...</h1>
    <p>Please wait while we authenticate with Google...</p>
    <FeedbackModal ref="feedbackModal" />
  </div>
</template>

<script setup lang="ts">
import { onMounted, ref } from 'vue';
import FeedbackModal from './FeedbackModal.vue';

interface Props {
  success: boolean;
  token: string;
  email: string;
  serverType: string;
  error: string;
}

const props = defineProps<Props>();
const feedbackModal = ref<InstanceType<typeof FeedbackModal> | null>(null);

onMounted(() => {
  if (props.success) {
    if (window.opener) {
      window.opener.postMessage({
        type: 'oauth-success',
        token: props.token,
        email: props.email,
        serverType: props.serverType
      }, window.location.origin);
      window.close();
    } else {
      feedbackModal.value?.alert({
        title: 'Authentication Successful',
        message: 'Authentication successful! Please close this window and return to the main window.',
        iconClass: 'check-circle'
      });
    }
  } else {
    if (window.opener) {
      window.opener.postMessage({
        type: 'oauth-error',
        error: props.error
      }, window.location.origin);
      window.close();
    } else {
      feedbackModal.value?.alert({
        title: 'Authentication Failed',
        message: 'Authentication failed: ' + props.error,
        iconClass: 'x-circle'
      });
    }
  }
});
</script>
