<template>
  <div class="modal fade" ref="modalElement" tabindex="-1" aria-labelledby="modalTitle" aria-hidden="true" data-bs-backdrop="static" data-bs-keyboard="true">
    <div class="modal-dialog modal-dialog-centered">
      <div class="modal-content">
        <div class="modal-header">
          <h5 class="modal-title" id="modalTitle">
            <i class="bi" :class="currentIconClass"></i> {{ currentTitle }}
          </h5>
          <button type="button" class="btn-close" data-bs-dismiss="modal" aria-label="Close"></button>
        </div>
        <div class="modal-body">
          {{ currentMessage }}
        </div>
        <div class="modal-footer">
          <button v-if="currentNoAction !== null" type="button" class="btn btn-secondary" data-bs-dismiss="modal" @click="handleNo">
            {{ currentNoButtonText }}
          </button>
          <button type="button" class="btn btn-primary" @click="handleYes">
            {{ currentYesButtonText }}
          </button>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted } from 'vue';

interface PromptOptions {
  title: string;
  message: string;
  yesAction: () => any;
  noAction?: () => any;
  yesButtonText?: string;
  noButtonText?: string;
  iconClass?: string;
}

interface AlertOptions {
  title: string;
  message: string;
  iconClass?: string;
}

const modalElement = ref<HTMLElement | null>(null);
let modalInstance: any = null;

const currentTitle = ref<string>('');
const currentMessage = ref<string>('');
const currentYesAction = ref<(() => any) | null>(null);
const currentNoAction = ref<(() => any) | null>(null);
const currentYesButtonText = ref<string>('Yes');
const currentNoButtonText = ref<string>('No');
const currentIconClass = ref<string>('bi-info-circle');

onMounted(() => {
  if (modalElement.value) {
    const bootstrap = (window as any).bootstrap;
    if (bootstrap && bootstrap.Modal) {
      modalInstance = new bootstrap.Modal(modalElement.value);
    }
  }
});

function prompt(options: PromptOptions): void {
  currentTitle.value = options.title;
  currentMessage.value = options.message;
  currentYesAction.value = options.yesAction;
  currentNoAction.value = options.noAction || null;
  currentYesButtonText.value = options.yesButtonText || 'Yes';
  currentNoButtonText.value = options.noButtonText || 'No';
  currentIconClass.value = options.iconClass ? `bi-${options.iconClass}` : 'bi-info-circle';
  
  if (modalInstance) {
    modalInstance.show();
  }
}

function alert(options: AlertOptions): void {
  currentTitle.value = options.title;
  currentMessage.value = options.message;
  currentYesAction.value = () => {
    if (modalInstance) {
      modalInstance.hide();
    }
  };
  currentNoAction.value = null;
  currentYesButtonText.value = 'OK';
  currentIconClass.value = options.iconClass ? `bi-${options.iconClass}` : 'bi-info-circle';
  
  if (modalInstance) {
    modalInstance.show();
  }
}

function handleYes(): void {
  if (currentYesAction.value) {
    currentYesAction.value();
  }
  if (modalInstance) {
    modalInstance.hide();
  }
}

function handleNo(): void {
  if (currentNoAction.value) {
    currentNoAction.value();
  }
  if (modalInstance) {
    modalInstance.hide();
  }
}

defineExpose({
  prompt,
  alert
});
</script>
