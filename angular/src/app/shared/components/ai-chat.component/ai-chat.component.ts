import { Component, CUSTOM_ELEMENTS_SCHEMA } from '@angular/core';
import { trigger, transition, style, animate } from '@angular/animations';
import 'deep-chat';
import { SharedModule } from '../../shared.module';
import { AiChatService } from '../../../proxy/ai-chats/ai-chat.service';

@Component({
  selector: 'app-ai-chat',
  standalone: true,
  imports: [SharedModule],
  schemas: [CUSTOM_ELEMENTS_SCHEMA],
  templateUrl: 'ai-chat.component.html',
  styleUrls: ['./ai-chat.component.scss'],
  animations: [
    trigger('chatAnimation', [
      // Mở ra: mượt mà trượt lên + phóng to
      transition(':enter', [
        style({ opacity: 0, transform: 'translateY(20px) scale(0.95)' }),
        animate('0.3s cubic-bezier(0.2, 0.9, 0.4, 1.1)',
          style({ opacity: 1, transform: 'translateY(0) scale(1)' }))
      ]),
      // Đóng lại: mờ dần + thu nhỏ nhanh
      transition(':leave', [
        animate('0.2s ease-in',
          style({ opacity: 0, transform: 'translateY(15px) scale(0.97)' }))
      ])
    ])
  ]
})
export class AiChatComponent {
  isOpen = false;

  chatInitialMessages = [
    { role: 'ai', text: 'Xin chào! Tôi là Trợ lý AI của hệ thống RxLogistics. Hôm nay tôi có thể hỗ trợ gì cho bạn ?' }
  ];

  chatRequest = {
    handler: (body: any, signals: any) => {
      const messages = body.messages || [];
      if (messages.length === 0) {
        signals.onResponse({ error: 'Không có tin nhắn gửi đi!' });
        return;
      }

      const currentMessage = messages[messages.length - 1];
      const history = messages.slice(0, messages.length - 1).map((m: any) => ({
        role: m.role === 'ai' ? 'model' : 'user',
        text: m.text
      }));

      this.aiChatService.sendMessage({
        text: currentMessage.text,
        history: history
      }).subscribe({
        next: (response) => {
          signals.onResponse({ text: response.text });
        },
        error: (err) => {
          signals.onResponse({ error: err?.message || 'Có lỗi xảy ra khi kết nối tới Trợ lý AI!' });
        }
      });
    }
  };

  constructor(private aiChatService: AiChatService) {}

  toggleChat() {
    this.isOpen = !this.isOpen;
  }

  // Giữ hover effect bằng function (không cần thiết nếu đã có CSS :hover, nhưng để giữ code gốc)
  onMouseEnter(event: MouseEvent) {
    (event.currentTarget as HTMLElement).style.transform = 'scale(1.05)';
  }

  onMouseLeave(event: MouseEvent) {
    (event.currentTarget as HTMLElement).style.transform = 'scale(1)';
  }
}