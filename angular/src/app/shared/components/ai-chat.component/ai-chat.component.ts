import { Component, ViewChild, ElementRef, OnInit, AfterViewChecked } from '@angular/core';
import { SharedModule } from '../../shared.module';
import { AgentService } from 'src/app/proxy/agent/agent.service';

@Component({
  selector: 'app-ai-chat',
  standalone: true,
  imports: [SharedModule],
  templateUrl: 'ai-chat.component.html',
  styleUrls: ['./ai-chat.component.scss']
})
export class AiChatComponent implements OnInit, AfterViewChecked {
  @ViewChild('scrollContainer') scrollContainer!: ElementRef;

  currentSessionId: string | null = null;
  messages: Array<{ role: 'user' | 'model'; text: string }> = [];
  userInput = '';
  isLoading = false;
  isExecutingTask = false; // Trạng thái loading khi bấm các nút phẳng trên slide panel

  pendingApprovalInfo: {
    sessionId: string;
    toolName: string;
    arguments: any;
  } | null = null;

  pendingElicitationInfo: {
    sessionId: string;
    elicitationForm: {
      type: string;
      action: string;
      title: string;
      description: string;
      fields: Array<{
        name: string;
        label: string;
        type: string;
        required: boolean;
        secret: boolean;
        placeholder?: string;
      }>
    };
  } | null = null;

  elicitationFormValues: { [key: string]: string } = {};

  constructor(private agentService: AgentService) {}

  ngOnInit() {
    const savedSessionId = localStorage.getItem('rx_ai_chat_session_id');
    if (savedSessionId) {
      this.currentSessionId = savedSessionId;
      this.loadChatHistory(savedSessionId);
    } else {
      // Tin nhắn chào mừng mặc định nếu không có session
      this.messages = [
        { role: 'model', text: 'Xin chào! Tôi là Trợ lý AI của hệ thống SupplyCoreERP. Hôm nay tôi có thể hỗ trợ gì cho bạn ?' }
      ];
    }
  }

  ngAfterViewChecked() {
    this.scrollToBottom();
  }

  setSessionId(sessionId: string) {
    this.currentSessionId = sessionId;
    localStorage.setItem('rx_ai_chat_session_id', sessionId);
  }

  scrollToBottom(): void {
    try {
      if (this.scrollContainer) {
        this.scrollContainer.nativeElement.scrollTop = this.scrollContainer.nativeElement.scrollHeight;
      }
    } catch (err) {
      // Bỏ qua lỗi scroll
    }
  }

  loadChatHistory(sessionId: string) {
    this.isLoading = true;
    this.agentService.getHistory({ sessionId: sessionId }).subscribe({
      next: (response: any) => {
        this.isLoading = false;
        const chatHistory: any[] = [];
        const steps = response?.steps || [];
        
        if (steps.length > 0) {
          for (const step of steps) {
            if (step.text) {
              chatHistory.push({ role: step.role, text: step.text });
            }
          }
        }
        
        if (chatHistory.length > 0) {
          this.messages = chatHistory;
        } else {
          this.messages = [
            { role: 'model', text: 'Xin chào! Tôi là Trợ lý AI của hệ thống SupplyCoreERP. Hôm nay tôi có thể hỗ trợ gì cho bạn ?' }
          ];
        }

        // Tự động khôi phục trạng thái tác vụ pending (nếu có) khi F5
        const pendingTask = response?.pendingTask;
        if (pendingTask) {
          if (pendingTask.status === 'PendingApproval') {
            this.pendingApprovalInfo = {
              sessionId: pendingTask.sessionId,
              toolName: pendingTask.toolName,
              arguments: pendingTask.arguments
            };
          } else if (pendingTask.status === 'PendingElicitation') {
            const transformedForm = this.transformJsonSchemaToForm(pendingTask.elicitationForm);
            this.pendingElicitationInfo = {
              sessionId: pendingTask.sessionId,
              elicitationForm: transformedForm
            };
            this.elicitationFormValues = {};
            if (transformedForm?.fields) {
              transformedForm.fields.forEach((f: any) => {
                this.elicitationFormValues[f.name] = pendingTask.arguments?.[f.name] || '';
              });
            }
          }
        }
      },
      error: (err) => {
        this.isLoading = false;
        console.error('Lỗi khi tải lịch sử chat từ server:', err);
      }
    });
  }

  sendChatMessage() {
    const text = this.userInput.trim();
    if (!text || this.isLoading) return;

    // 1. Thêm tin nhắn của User vào khung chat
    this.messages.push({ role: 'user', text: text });
    this.userInput = '';
    this.isLoading = true;

    // Chuẩn bị history DTO
    const history = this.messages.slice(0, this.messages.length - 1).map(m => ({
      role: m.role,
      text: m.text
    }));

    // 2. Gọi API gửi tin nhắn tới Agent
    this.agentService.sendMessage({
      text: text,
      history: history,
      sessionId: this.currentSessionId || undefined
    }).subscribe({
      next: (response: any) => {
        this.isLoading = false;

        if (response && response.sessionId) {
          this.setSessionId(response.sessionId);
        }

        if (response && response.status === 'PendingApproval') {
          this.pendingApprovalInfo = {
            sessionId: response.sessionId,
            toolName: response.toolName,
            arguments: response.arguments
          };
          this.messages.push({
            role: 'model',
            text: '🤖 *Tác vụ yêu cầu phê duyệt...*'
          });
        } else if (response && response.status === 'PendingElicitation') {
          const transformedForm = this.transformJsonSchemaToForm(response.elicitationForm);
          this.pendingElicitationInfo = {
            sessionId: response.sessionId,
            elicitationForm: transformedForm
          };
          this.elicitationFormValues = {};
          if (transformedForm?.fields) {
            transformedForm.fields.forEach((f: any) => {
              this.elicitationFormValues[f.name] = '';
            });
          }
          this.messages.push({
            role: 'model',
            text: '🤖 *Yêu cầu cung cấp thông tin tác vụ...*'
          });
        } else {
          this.messages.push({
            role: 'model',
            text: response.text || ''
          });
        }
      },
      error: (err) => {
        this.isLoading = false;
        this.messages.push({
          role: 'model',
          text: `❌ Có lỗi xảy ra khi kết nối tới Trợ lý AI: ${err?.message || 'Không xác định'}`
        });
      }
    });
  }

  handleEnterKey(event: Event) {
    const keyboardEvent = event as KeyboardEvent;
    if (keyboardEvent.key === 'Enter' && !keyboardEvent.shiftKey) {
      keyboardEvent.preventDefault();
      this.sendChatMessage();
    }
  }

  onApprove() {
    if (!this.pendingApprovalInfo) return;
    this.isExecutingTask = true;

    this.agentService.approve({
      sessionId: this.pendingApprovalInfo.sessionId
    }).subscribe({
      next: (response: any) => {
        this.isExecutingTask = false;
        
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
          this.messages.push({
            role: 'model',
            text: '🤖 *Tác vụ tiếp theo yêu cầu phê duyệt...*'
          });
        } else if (response && response.status === 'PendingElicitation') {
          const transformedForm = this.transformJsonSchemaToForm(response.elicitationForm);
          this.pendingElicitationInfo = {
            sessionId: response.sessionId,
            elicitationForm: transformedForm
          };
          this.elicitationFormValues = {};
          if (transformedForm?.fields) {
            transformedForm.fields.forEach((f: any) => {
              this.elicitationFormValues[f.name] = '';
            });
          }
          this.messages.push({
            role: 'model',
            text: '🤖 *Tác vụ tiếp theo yêu cầu cung cấp thông tin...*'
          });
        } else {
          this.messages.push({
            role: 'model',
            text: response.text || ''
          });
        }
      },
      error: (err) => {
        this.isExecutingTask = false;
        this.pendingApprovalInfo = null;
        this.messages.push({
          role: 'model',
          text: `❌ Lỗi thực thi phê duyệt: ${err?.message || 'Không xác định'}`
        });
      }
    });
  }

  onReject() {
    if (!this.pendingApprovalInfo) return;
    this.isExecutingTask = true;

    this.agentService.reject({
      sessionId: this.pendingApprovalInfo.sessionId
    }).subscribe({
      next: (response: any) => {
        this.isExecutingTask = false;
        
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
          this.messages.push({
            role: 'model',
            text: '🤖 *Tác vụ tiếp theo yêu cầu phê duyệt...*'
          });
        } else if (response && response.status === 'PendingElicitation') {
          const transformedForm = this.transformJsonSchemaToForm(response.elicitationForm);
          this.pendingElicitationInfo = {
            sessionId: response.sessionId,
            elicitationForm: transformedForm
          };
          this.elicitationFormValues = {};
          if (transformedForm?.fields) {
            transformedForm.fields.forEach((f: any) => {
              this.elicitationFormValues[f.name] = '';
            });
          }
          this.messages.push({
            role: 'model',
            text: '🤖 *Tác vụ tiếp theo yêu cầu cung cấp thông tin...*'
          });
        } else {
          this.messages.push({
            role: 'model',
            text: response.text || ''
          });
        }
      },
      error: (err) => {
        this.isExecutingTask = false;
        this.pendingApprovalInfo = null;
        this.messages.push({
          role: 'model',
          text: `❌ Lỗi khi từ chối tác vụ: ${err?.message || 'Không xác định'}`
        });
      }
    });
  }

  onSubmitElicitation() {
    if (!this.pendingElicitationInfo) return;
    this.isExecutingTask = true;

    this.agentService.submitElicitation({
      sessionId: this.pendingElicitationInfo.sessionId,
      formValues: this.elicitationFormValues
    }).subscribe({
      next: (response: any) => {
        this.isExecutingTask = false;
        
        if (response && response.sessionId) {
          this.setSessionId(response.sessionId);
        }
        this.pendingElicitationInfo = null;

        if (response && response.status === 'PendingApproval') {
          this.pendingApprovalInfo = {
            sessionId: response.sessionId,
            toolName: response.toolName,
            arguments: response.arguments
          };
          this.messages.push({
            role: 'model',
            text: '🤖 *Tác vụ yêu cầu phê duyệt...*'
          });
        } else if (response && response.status === 'PendingElicitation') {
          const transformedForm = this.transformJsonSchemaToForm(response.elicitationForm);
          this.pendingElicitationInfo = {
            sessionId: response.sessionId,
            elicitationForm: transformedForm
          };
          this.elicitationFormValues = {};
          if (transformedForm?.fields) {
            transformedForm.fields.forEach((f: any) => {
              this.elicitationFormValues[f.name] = '';
            });
          }
          this.messages.push({
            role: 'model',
            text: '🤖 *Yêu cầu thêm thông tin...*'
          });
        } else {
          this.messages.push({
            role: 'model',
            text: response.text || ''
          });
        }
      },
      error: (err) => {
        this.isExecutingTask = false;
        this.pendingElicitationInfo = null;
        this.messages.push({
          role: 'model',
          text: `❌ Lỗi nộp thông tin Form: ${err?.message || 'Không xác định'}`
        });
      }
    });
  }

  onCancelElicitation() {
    this.pendingElicitationInfo = null;
    this.elicitationFormValues = {};
    this.messages.push({
      role: 'model',
      text: '❌ Đã hủy cung cấp thông tin tác vụ.'
    });
  }

  transformJsonSchemaToForm(schema: any): any {
    if (!schema) return null;
    
    if (schema.fields && Array.isArray(schema.fields)) {
      return schema;
    }

    const fields: any[] = [];
    if (schema.properties) {
      const requiredList = schema.required || [];
      for (const key in schema.properties) {
        if (schema.properties.hasOwnProperty(key)) {
          const prop = schema.properties[key];
          fields.push({
            name: key,
            label: prop.title || key,
            type: prop.type || 'string',
            required: requiredList.includes(key),
            secret: prop.secret || false,
            placeholder: prop.description || ''
          });
        }
      }
    }

    return {
      type: schema.type || 'object',
      action: 'accept',
      title: schema.title || 'Yêu cầu cung cấp thông tin',
      description: schema.description || 'Vui lòng điền các thông tin bắt buộc dưới đây để hệ thống tiếp tục thực thi tác vụ.',
      fields: fields
    };
  }

  renderMarkdown(text: string): string {
    if (!text) return '';
    
    // 1. Thoát HTML thô của user để chống XSS
    let html = text
      .replace(/&/g, '&amp;')
      .replace(/</g, '&lt;')
      .replace(/>/g, '&gt;');

    // 2. Chuyển đổi Code Blocks: ```code```
    html = html.replace(/```([\s\S]*?)```/g, '<pre><code>$1</code></pre>');

    // 3. Chuyển đổi Inline Code: `code`
    html = html.replace(/`([^`]+)`/g, '<code>$1</code>');

    // 4. Chuyển đổi Bold: **text**
    html = html.replace(/\*\*([^*]+)\*\*/g, '<strong>$1</strong>');

    // 5. Chuyển đổi Italic: *text*
    html = html.replace(/\*([^*]+)\*/g, '<em>$1</em>');

    // 6. Chuyển đổi Lists dạng dấu gạch đầu dòng
    html = html.replace(/^\s*[\-\*]\s+(.+)$/gm, '<li>$1</li>');

    // 7. Chuyển đổi xuống dòng
    html = html.replace(/\n/g, '<br/>');

    return html;
  }
}