import type { PagedResultRequestDto } from '@abp/ng.core';

export interface AgentElicitationInputDto {
  sessionId?: string;
  formValues?: Record<string, string>;
}

export interface AgentHistoryDto {
  steps?: AgentSessionMessageDto[];
  pendingTask?: object;
}

export interface AgentRequestInputDto {
  text: string;
  sessionId?: string;
}

export interface AgentSessionInputDto {
  sessionId: string;
}

export interface AgentSessionMessageDto {
  role?: string;
  text?: string;
  toolCalls?: AgentToolCallMessageDto[];
  toolResponses?: AgentToolResponseMessageDto[];
  creationTime?: string;
}

export interface AgentSessionPagedInputDto extends PagedResultRequestDto {
  sessionId?: string;
}

export interface AgentToolCallMessageDto {
  name?: string;
  arguments?: Record<string, any>;
  thoughtSignature?: string;
}

export interface AgentToolResponseMessageDto {
  name?: string;
  content?: string;
}
