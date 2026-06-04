import type { AgentRequestInputDto, AgentSessionInputDto } from './dtos/models';
import { RestService, Rest } from '@abp/ng.core';
import { Injectable, inject } from '@angular/core';

@Injectable({
  providedIn: 'root',
})
export class AgentService {
  private restService = inject(RestService);
  apiName = 'Default';
  

  approve = (input: AgentSessionInputDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, object>({
      method: 'POST',
      url: '/api/app/agent/approve',
      body: input,
    },
    { apiName: this.apiName,...config });
  

  reject = (input: AgentSessionInputDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, object>({
      method: 'POST',
      url: '/api/app/agent/reject',
      body: input,
    },
    { apiName: this.apiName,...config });
  

  sendMessage = (input: AgentRequestInputDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, object>({
      method: 'POST',
      url: '/api/app/agent/send-message',
      body: input,
    },
    { apiName: this.apiName,...config });
}