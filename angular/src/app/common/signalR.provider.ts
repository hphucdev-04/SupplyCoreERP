import { inject, provideAppInitializer } from '@angular/core';
import { SignalRManager } from '../shared/services/signalR/signalr-manager.service'

export const APP_SIGNALR_PROVIDER = [
  provideAppInitializer(() => {
    const signalRManager = inject(SignalRManager);
    signalRManager.init();
  }),
];