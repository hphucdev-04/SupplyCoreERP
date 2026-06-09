
export interface AgentElicitationInputDto {
  sessionId?: string;
  formValues?: Record<string, string>;
}

export interface AgentMessageDto {
  role: string;
  text?: string;
  toolCalls?: AgentToolCallMessageDto[];
  toolResponses?: AgentToolResponseMessageDto[];
}

export interface AgentRequestInputDto {
  text: string;
  history?: AgentMessageDto[];
  sessionId?: string;
}

export interface AgentSessionInputDto {
  sessionId: string;
}

export interface AgentToolCallMessageDto {
  name?: string;
  arguments?: Record<string, any>;
}

export interface AgentToolResponseMessageDto {
  name?: string;
  content?: string;
}
