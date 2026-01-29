<template>
    <div class="container mt-4">
        <div v-if="jobNotFound" class="text-center mt-5">
            <h1 class="mb-3">Job Not Found</h1>
            <p class="text-muted">No Migration Job with the specified ID was found.</p>
        </div>

        <div v-else>
            <div class="d-flex justify-content-between align-items-center mb-4">
                <h1 class="mb-0">Migration Status</h1>
                <button v-if="hasJobId" class="btn btn-outline-primary" @click="bookmarkPage" title="Bookmark this page">
                    <i class="bi bi-bookmark"></i> Bookmark this page
                </button>
            </div>

            <FeedbackModal ref="feedbackModal" />

            <div id="progressSection">
                <div class="card">
                    <div class="card-header">
                        <h5 class="mb-0">Migration Progress</h5>
                    </div>
                    <div class="card-body">
                        <div v-show="showProgressBar" class="mb-3">
                            <div class="progress" style="height: 30px;">
                                <div class="progress-bar progress-bar-striped progress-bar-animated"
                                     role="progressbar"
                                     :style="{ width: progress + '%' }"
                                     :aria-valuenow="progress"
                                     aria-valuemin="0"
                                     aria-valuemax="100">
                                    {{ progress }}%
                                </div>
                            </div>
                        </div>

                        <div class="border rounded p-3" style="max-height: 400px; overflow-y: auto; background-color: #f8f9fa;">
                            <div ref="statusContainer" class="font-monospace small">
                                <div v-for="(message, index) in statusMessages" :key="index" :class="message.className">
                                    [{{ message.timestamp }}] {{ message.text }}
                                </div>
                            </div>
                        </div>

                        <div v-show="showStatistics" class="mt-3">
                            <h6>Migration Statistics:</h6>
                            <table class="table table-sm">
                                <tbody>
                                    <tr>
                                        <td><strong>Total Messages:</strong></td>
                                        <td>{{ statistics.total }}</td>
                                    </tr>
                                    <tr>
                                        <td><strong>Successfully Migrated:</strong></td>
                                        <td>{{ statistics.success }}</td>
                                    </tr>
                                    <tr>
                                        <td><strong>Skipped:</strong></td>
                                        <td>{{ statistics.skipped }}</td>
                                    </tr>
                                    <tr>
                                        <td><strong>Failed:</strong></td>
                                        <td>{{ statistics.failed }}</td>
                                    </tr>
                                </tbody>
                            </table>
                        </div>

                        <div v-if="!isJobCompleted" class="mt-3">
                            <button class="btn btn-danger" @click="cancelMigration" :disabled="isCancelling">Cancel</button>
                        </div>

                        <div v-if="isJobCompleted && jobInfo" class="mt-3">
                            <div class="alert alert-info">
                                <h6>Job Information:</h6>
                                <p><strong>Start Time:</strong> {{ formatDateTime(jobInfo.startTime) }}</p>
                                <p v-if="jobInfo.endTime"><strong>End Time:</strong> {{ formatDateTime(jobInfo.endTime) }}</p>
                                <p><strong>Status:</strong> {{ jobInfo.status }}</p>
                            </div>
                            <button class="btn btn-danger" @click="deleteJob" :disabled="isDeleting">
                                <span v-if="isDeleting">Deleting...</span>
                                <span v-else>Delete Job</span>
                            </button>
                        </div>
                    </div>
                </div>
            </div>
        </div>
    </div>
</template>

<script setup lang="ts">
    import { ref, onMounted, nextTick, computed } from 'vue';
    import type * as signalR from '@microsoft/signalr';
    import FeedbackModal from './FeedbackModal.vue';

    interface Props {
        connection: signalR.HubConnection;
        jobId: string;
    }

    interface StatusMessage {
        timestamp: string;
        text: string;
        className: string;
    }

    interface Statistics {
        total: number;
        success: number;
        skipped: number;
        failed: number;
    }


    interface JobInfo {
        jobId: string;
        status: string;
        progress: number;
        progressUpdates: boolean;
        statusType: string;
        startTime: string;
        endTime?: string;
        isCompleted: boolean;
        sourceServer?: string;
        totalMessages: number;
        successfulMessages: number;
        skippedMessages: number;
        failedMessages: number;
    }

    const props = defineProps<Props>();

    const hasJobId = computed(() => Boolean(props.jobId));

    const progress = ref<number>(0);
    const showProgressBar = ref<boolean>(false);
    const statusMessages = ref<StatusMessage[]>([]);
    const showStatistics = ref<boolean>(false);
    const statistics = ref<Statistics>({
        total: 0,
        success: 0,
        skipped: 0,
        failed: 0
    });
    const isCancelling = ref<boolean>(false);
    const isDeleting = ref<boolean>(false);
    const statusContainer = ref<HTMLElement | null>(null);
    const isJobCompleted = ref<boolean>(false);
    const jobInfo = ref<JobInfo | null>(null);
    const jobNotFound = ref<boolean>(false);
    const feedbackModal = ref<InstanceType<typeof FeedbackModal> | null>(null);

    async function loadJobInfo() {
        try {
            const info = await props.connection.invoke("GetJobInfo", props.jobId) as JobInfo;
            if (info) {
                jobInfo.value = info;
                isJobCompleted.value = info.isCompleted;
                jobNotFound.value = false;

                if (info.isCompleted) {
                    const timestamp = new Date(info.endTime ?? info.startTime).toLocaleTimeString();
                    statusMessages.value.push({
                        timestamp,
                        text: info.status,
                        className: info.statusType === "Error" ? "text-danger" :
                            info.statusType === "Warning" ? "text-warning" : "text-success"
                    });

                    if (info.totalMessages || info.successfulMessages || info.skippedMessages || info.failedMessages) {
                        showStatistics.value = true;
                        statistics.value = {
                            total: info.totalMessages,
                            success: info.successfulMessages,
                            skipped: info.skippedMessages,
                            failed: info.failedMessages
                        };
                    }
                }
            } else {
                jobNotFound.value = true;
            }
        } catch (error) {
            console.error("Error loading job info:", error);
            jobNotFound.value = true;
        }
    }

    function formatDateTime(dateStr: string): string {
        const date = new Date(dateStr);
        return date.toLocaleString();
    }

    onMounted(async () => {
        props.connection.on("StatusUpdate", (job: JobInfo) => {
            if (job.progressUpdates) {
                showProgressBar.value = true;
                progress.value = job.progress ?? 1;
            }

            const messageColor = job.statusType === "Error" ? "text-danger" :
                job.statusType === "Warning" ? "text-warning" : "text-dark";

            const timestamp = new Date().toLocaleTimeString();
            statusMessages.value.push({
                timestamp,
                text: job.status,
                className: messageColor
            });

            jobInfo.value = job;
            isJobCompleted.value = job.isCompleted;

            nextTick(() => {
                if (statusContainer.value && statusContainer.value.parentElement) {
                    statusContainer.value.parentElement.scrollTop = statusContainer.value.parentElement.scrollHeight;
                }
            });

            if (job.totalMessages) {
                showStatistics.value = true;
                statistics.value = {
                    total: job.totalMessages,
                    success: job.successfulMessages || 0,
                    skipped: job.skippedMessages || 0,
                    failed: job.failedMessages || 0
                };
            }
        });
    });

    function cancelMigration() {
        feedbackModal.value?.prompt({
            title: 'Cancel Migration',
            message: 'Are you sure you want to cancel this migration?',
            iconClass: 'exclamation-triangle',
            yesAction: () => {
                isCancelling.value = true;
                props.connection.invoke("CancelMigration", props.jobId)
                    .catch((err: Error) => {
                        console.error(err.toString());
                        isCancelling.value = false;
                    });
            }
        });
    }

    function bookmarkPage() {
        const url = window.location.href;
        const title = jobInfo.value?.sourceServer
            ? `Migration Status - ${jobInfo.value.sourceServer}`
            : `Migration Status - Job ${props.jobId}`;

        if ((window as any).sidebar && (window as any).sidebar.addPanel) {
            (window as any).sidebar.addPanel(title, url, '');
        } else if ((window as any).external && 'AddFavorite' in (window as any).external) {
            (window as any).external.AddFavorite(url, title);
        } else if ((window as any).opera && (window as any).opera.hotkeys) {
            return true;
        } else {
            const keyCombo = navigator.userAgent.toLowerCase().indexOf('mac') !== -1 ? 'Cmd' : 'Ctrl';
            feedbackModal.value?.alert({
                title: 'Bookmark Page',
                message: `Press ${keyCombo}+D to bookmark this page.`,
                iconClass: 'bookmark'
            });
        }
    }

    async function deleteJob() {
        feedbackModal.value?.prompt({
            title: 'Delete Job',
            message: 'Are you sure you want to delete this job? This action cannot be undone.',
            iconClass: 'trash',
            yesButtonText: 'Delete',
            noButtonText: 'Cancel',
            yesAction: async () => {
                isDeleting.value = true;
                try {
                    await props.connection.invoke("DeleteJob", props.jobId);
                    window.location.href = "/";
                } catch (err) {
                    console.error(err);
                    feedbackModal.value?.alert({
                        title: 'Error',
                        message: 'Error deleting job: ' + (err as Error).toString(),
                        iconClass: 'x-circle'
                    });
                    isDeleting.value = false;
                }
            }
        });
    }

    defineExpose({
        loadJobInfo
    });
</script>
