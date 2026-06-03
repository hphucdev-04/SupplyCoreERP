import type { ChatRequestInputDto, ChatResponseOutputDto } from './dtos/models';
import { RestService, Rest } from '@abp/ng.core';
import { Injectable, inject } from '@angular/core';

@Injectable({
  providedIn: 'root',
})
export class AiChatService {
  private restService = inject(RestService);
  apiName = 'Default';
  

  sendMessage = (input: ChatRequestInputDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, ChatResponseOutputDto>({
      method: 'POST',
      url: '/api/app/ai-chat/send-message',
      body: input,
    },
    { apiName: this.apiName,...config });
}