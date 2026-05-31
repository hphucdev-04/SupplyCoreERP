import { Component, CUSTOM_ELEMENTS_SCHEMA } from '@angular/core';
import 'deep-chat';
import { SharedModule } from '../../shared.module';

@Component({
  selector: 'app-ai-chat',
  standalone: true,
  imports: [SharedModule],
  schemas: [CUSTOM_ELEMENTS_SCHEMA],
  templateUrl: 'ai-chat.component.html',
  styleUrls: ['./ai-chat.component.scss']
})
export class AiChatComponent {
  isOpen = false;

  // Cấu hình UI cho Deep Chat
  chatInitialMessages = [
    { role: 'ai', text: 'Xin chào! Tôi là Trợ lý AI của hệ thống RxLogistics. Hôm nay tôi có thể hỗ trợ gì cho bạn ?' }
  ];

  toggleChat() {
    this.isOpen = !this.isOpen;
  }
}