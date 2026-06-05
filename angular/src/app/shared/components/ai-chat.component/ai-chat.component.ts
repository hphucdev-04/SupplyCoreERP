import { Component, CUSTOM_ELEMENTS_SCHEMA, ViewChild, ElementRef, OnInit } from '@angular/core';
import { trigger, transition, style, animate } from '@angular/animations';
import 'deep-chat';
import { SharedModule } from '../../shared.module';
import { AgentService } from 'src/app/proxy/agent/agent.service';

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
export class AiChatComponent implements OnInit {
  @ViewChild('chatElement') chatElementRef!: ElementRef;

  isOpen = false;
  currentSessionId: string | null = null;

  pendingApprovalInfo: {
    sessionId: string;
    toolName: string;
    arguments: any;
  } | null = null;

  isExecutingApproval = false;

  chatInitialMessages: any[] = [
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

      this.agentService.sendMessage({
        text: currentMessage.text,
        history: history,
        sessionId: this.currentSessionId || undefined
      }).subscribe({
        next: (response: any) => {
          if (response && response.sessionId) {
            this.setSessionId(response.sessionId);
          }

          if (response && response.status === 'PendingApproval') {
            this.pendingApprovalInfo = {
              sessionId: response.sessionId,
              toolName: response.toolName,
              arguments: response.arguments
            };
            signals.onResponse({ text: '🤖 *Tác vụ yêu cầu phê duyệt...*' });
          } else {
            signals.onResponse({ text: response.text || '' });
          }
        },
        error: (err) => {
          signals.onResponse({ error: err?.message || 'Có lỗi xảy ra khi kết nối tới Trợ lý AI!' });
        }
      });
    }
  };

  constructor(private agentService: AgentService) {}

  ngOnInit() {
    const savedSessionId = localStorage.getItem('rx_ai_chat_session_id');
    if (savedSessionId) {
      this.currentSessionId = savedSessionId;
      this.loadChatHistory(savedSessionId);
    }
  }

  setSessionId(sessionId: string) {
    this.currentSessionId = sessionId;
    localStorage.setItem('rx_ai_chat_session_id', sessionId);
  }

  loadChatHistory(sessionId: string) {
    this.agentService.getHistory({ sessionId: sessionId }).subscribe({
      next: (history: any[]) => {
        const deepChatHistory: any[] = [];
        if (history && history.length > 0) {
          for (const step of history) {
            // Chỉ hiển thị tin nhắn dạng text của user và model lên UI, loại bỏ tool call trung gian
            if (step.role === 'user' && step.text) {
              deepChatHistory.push({ role: 'user', text: step.text });
            } else if (step.role === 'model' && step.text) {
              deepChatHistory.push({ role: 'ai', text: step.text });
            }
          }
        }
        
        // Nếu có lịch sử thực tế trong DB, gán đè lên tin nhắn chào mặc định
        if (deepChatHistory.length > 0) {
          this.chatInitialMessages = deepChatHistory;
        }
      },
      error: (err) => {
        console.error('Lỗi khi tải lịch sử chat từ server:', err);
      }
    });
  }

  toggleChat() {
    this.isOpen = !this.isOpen;
  }

  onApprove() {
    if (!this.pendingApprovalInfo) return;
    this.isExecutingApproval = true;

    this.agentService.approve({
      sessionId: this.pendingApprovalInfo.sessionId
    }).subscribe({
      next: (response: any) => {
        this.isExecutingApproval = false;
        
        if (response && response.sessionId) {
          this.setSessionId(response.sessionId);
        }
        this.pendingApprovalInfo = null;

        if (response && response.status === 'PendingApproval') {
          this.pendingApprovalInfo = {
            sessionId: response.sessionId,
            toolName: response.toolName,
            arguments: response.arguments
          };
          this.chatElementRef.nativeElement.addMessage({
            text: '🤖 *Tác vụ tiếp theo yêu cầu phê duyệt...*',
            role: 'ai'
          });
        } else {
          const finalText = response.text || '';
          this.chatElementRef.nativeElement.addMessage({
            text: finalText,
            role: 'ai'
          });
        }
      },
      error: (err) => {
        this.isExecutingApproval = false;
        this.pendingApprovalInfo = null;
        this.chatElementRef.nativeElement.addMessage({
          text: `❌ Lỗi thực thi phê duyệt: ${err?.message || 'Không xác định'}`,
          role: 'ai'
        });
      }
    });
  }

  onReject() {
    if (!this.pendingApprovalInfo) return;
    this.isExecutingApproval = true;

    this.agentService.reject({
      sessionId: this.pendingApprovalInfo.sessionId
    }).subscribe({
      next: (response: any) => {
        this.isExecutingApproval = false;

        if (response && response.sessionId) {
          this.setSessionId(response.sessionId);
        }
        this.pendingApprovalInfo = null;

        if (response && response.status === 'PendingApproval') {
          this.pendingApprovalInfo = {
            sessionId: response.sessionId,
            toolName: response.toolName,
            arguments: response.arguments
          };
          this.chatElementRef.nativeElement.addMessage({
            text: '🤖 *Tác vụ tiếp theo yêu cầu phê duyệt...*',
            role: 'ai'
          });
        } else {
          const finalText = response.text || '';
          this.chatElementRef.nativeElement.addMessage({
            text: finalText,
            role: 'ai'
          });
        }
      },
      error: (err) => {
        this.isExecutingApproval = false;
        this.pendingApprovalInfo = null;
        this.chatElementRef.nativeElement.addMessage({
          text: `❌ Lỗi khi từ chối tác vụ: ${err?.message || 'Không xác định'}`,
          role: 'ai'
        });
      }
    });
  }

  // Giữ hover effect bằng function (để giữ tương thích ngược CSS cũ nếu cần)
  onMouseEnter(event: MouseEvent) {
    (event.currentTarget as HTMLElement).style.transform = 'scale(1.05)';
  }

  onMouseLeave(event: MouseEvent) {
    (event.currentTarget as HTMLElement).style.transform = 'scale(1)';
  }
}