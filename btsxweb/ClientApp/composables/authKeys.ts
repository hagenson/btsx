import type { InjectionKey } from 'vue';
import type { AuthState, AuthMethods } from './useAuth';

export const AUTH_STATE_KEY: InjectionKey<AuthState> = Symbol('AUTH_STATE');
export const AUTH_METHODS_KEY: InjectionKey<AuthMethods> = Symbol('AUTH_METHODS');
