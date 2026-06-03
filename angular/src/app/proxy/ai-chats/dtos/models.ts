
export interface ChatMessageDto {
  role: string;
  text: string;
}

export interface ChatRequestInputDto {
  text: string;
  history?: ChatMessageDto[];
}

export interface ChatResponseOutputDto {
  text?: string;
}
