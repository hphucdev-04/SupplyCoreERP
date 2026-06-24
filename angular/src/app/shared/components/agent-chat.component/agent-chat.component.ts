import { Component, ViewChild, ElementRef, OnInit, AfterViewChecked } from '@angular/core';
import { SharedModule } from '../../shared.module';
import { AgentService } from 'src/app/proxy/agent/agent.service';
import { AgentSessionMessageDto } from 'src/app/proxy/agent/dtos/models';

@Component({
  selector: 'app-agent-chat',
  standalone: true,
  imports: [SharedModule],
  templateUrl: 'agent-chat.component.html',
  styleUrls: ['./agent-chat.component.scss']
})
export class AgentChatComponent implements OnInit, AfterViewChecked {
  @ViewChild('scrollContainer') scrollContainer!: ElementRef;
  @ViewChild('chatInput') chatInputRef!: ElementRef;

  currentSessionId: string | null = null;
  messages: Array<AgentSessionMessageDto> = [];
  totalLoadedDbMessages = 0;
  readonly maxResultCount = 50;
  hasMoreMessages = true;
  isPageLoading = false;
  userInput = '';
  isLoading = false;
  isExecutingTask = false; // Trạng thái loading khi bấm các nút phẳng trên slide panel
  private shouldScrollToBottom = false;

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
      this.hasMoreMessages = false;
      this.shouldScrollToBottom = true;
    }
  }

  ngAfterViewChecked() {
    if (this.shouldScrollToBottom && !this.isPageLoading) {
      this.scrollToBottom();
      this.shouldScrollToBottom = false;
    }
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
    this.totalLoadedDbMessages = 0;
    this.hasMoreMessages = true;
    this.agentService.getHistory({ sessionId: sessionId, skipCount: 0, maxResultCount: this.maxResultCount }).subscribe({
      next: (response: any) => {
        this.isLoading = false;
        const chatHistory: AgentSessionMessageDto[] = [];
        const steps = response?.steps || [];
        
        if (steps.length > 0) {
          for (const step of steps) {
            if (step.text) {
              chatHistory.push(step);
            }
          }
        }
        
        if (chatHistory.length > 0) {
          this.messages = chatHistory;
          this.totalLoadedDbMessages = steps.length;
          this.hasMoreMessages = steps.length >= this.maxResultCount;
        } else {
          this.messages = [
            { role: 'model', text: 'Xin chào! Tôi là Trợ lý AI của hệ thống SupplyCoreERP. Hôm nay tôi có thể hỗ trợ gì cho bạn ?' }
          ];
          this.hasMoreMessages = false;
        }

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

        this.shouldScrollToBottom = true;
      },
      error: (err) => {
        this.isLoading = false;
        console.error('Lỗi khi tải lịch sử chat từ server:', err);
      }
    });
  }

  newChat() {
    if (!this.currentSessionId || this.isLoading) return;

    this.isLoading = true;
    this.agentService.resetSession({ sessionId: this.currentSessionId }).subscribe({
      next: () => {
        this.isLoading = false;
        this.currentSessionId = null;
        this.totalLoadedDbMessages = 0;
        this.hasMoreMessages = false;
        localStorage.removeItem('rx_ai_chat_session_id');
        this.messages = [
          { role: 'model', text: 'Xin chào! Tôi là Trợ lý AI của hệ thống SupplyCoreERP. Hôm nay tôi có thể hỗ trợ gì cho bạn ?' }
        ];
        this.pendingApprovalInfo = null;
        this.pendingElicitationInfo = null;
        this.shouldScrollToBottom = true;
      },
      error: (err) => {
        this.isLoading = false;
        console.error('Lỗi khi dọn dẹp phiên chat:', err);
      }
    });
  }

  sendChatMessage() {
    const text = this.userInput.trim();
    if (!text || this.isLoading) return;

    // 1. Thêm tin nhắn của User vào khung chat
    this.messages.push({
      role: 'user',
      text: text,
      creationTime: new Date().toISOString()
    });
    this.totalLoadedDbMessages++;
    this.userInput = '';
    this.isLoading = true;
    this.shouldScrollToBottom = true;

    // Reset chiều cao textarea về mặc định sau khi gửi
    if (this.chatInputRef) {
      this.chatInputRef.nativeElement.style.height = '24px';
    }

    // 2. Gọi API gửi tin nhắn tới Agent
    this.agentService.sendMessage({
      text: text,
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
            text: '🤖 *Tác vụ yêu cầu phê duyệt...*',
            creationTime: 'pending-' + new Date().getTime()
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
            text: '🤖 *Yêu cầu cung cấp thông tin tác vụ...*',
            creationTime: 'pending-' + new Date().getTime()
          });
        } else {
          this.messages.push({
            role: 'model',
            text: response.text || '',
            creationTime: new Date().toISOString()
          });
          this.totalLoadedDbMessages++;
        }
        this.shouldScrollToBottom = true;
      },
      error: (err) => {
        this.isLoading = false;
        this.messages.push({
          role: 'model',
          text: `❌ Có lỗi xảy ra khi kết nối tới Trợ lý AI: ${err?.message || 'Không xác định'}`
        });
        this.shouldScrollToBottom = true;
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
            text: '🤖 *Tác vụ tiếp theo yêu cầu phê duyệt...*',
            creationTime: 'pending-' + new Date().getTime()
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
            text: '🤖 *Tác vụ tiếp theo yêu cầu cung cấp thông tin...*',
            creationTime: 'pending-' + new Date().getTime()
          });
        } else {
          this.messages.push({
            role: 'model',
            text: response.text || '',
            creationTime: new Date().toISOString()
          });
          this.totalLoadedDbMessages++;
        }
        this.shouldScrollToBottom = true;
      },
      error: (err) => {
        this.isExecutingTask = false;
        this.pendingApprovalInfo = null;
        this.messages.push({
          role: 'model',
          text: `❌ Lỗi thực thi phê duyệt: ${err?.message || 'Không xác định'}`,
          creationTime: 'error-' + new Date().getTime()
        });
        this.shouldScrollToBottom = true;
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
            text: '🤖 *Tác vụ tiếp theo yêu cầu phê duyệt...*',
            creationTime: 'pending-' + new Date().getTime()
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
            text: '🤖 *Tác vụ tiếp theo yêu cầu cung cấp thông tin...*',
            creationTime: 'pending-' + new Date().getTime()
          });
        } else {
          this.messages.push({
            role: 'model',
            text: response.text || '',
            creationTime: new Date().toISOString()
          });
          this.totalLoadedDbMessages++;
        }
        this.shouldScrollToBottom = true;
      },
      error: (err) => {
        this.isExecutingTask = false;
        this.pendingApprovalInfo = null;
        this.messages.push({
          role: 'model',
          text: `❌ Lỗi khi từ chối tác vụ: ${err?.message || 'Không xác định'}`,
          creationTime: 'error-' + new Date().getTime()
        });
        this.shouldScrollToBottom = true;
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
            text: '🤖 *Tác vụ yêu cầu phê duyệt...*',
            creationTime: 'pending-' + new Date().getTime()
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
            text: '🤖 *Yêu cầu thêm thông tin...*',
            creationTime: 'pending-' + new Date().getTime()
          });
        } else {
          this.messages.push({
            role: 'model',
            text: response.text || '',
            creationTime: new Date().toISOString()
          });
          this.totalLoadedDbMessages++;
        }
        this.shouldScrollToBottom = true;
      },
      error: (err) => {
        this.isExecutingTask = false;
        this.pendingElicitationInfo = null;
        this.messages.push({
          role: 'model',
          text: `❌ Lỗi nộp thông tin Form: ${err?.message || 'Không xác định'}`
        });
        this.shouldScrollToBottom = true;
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
    this.shouldScrollToBottom = true;
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

  renderMarkdown(text: string | null | undefined): string {
    if (!text) return '';
    
    // 1. Thoát HTML thô của user để chống XSS
    let html = text
      .replace(/&/g, '&amp;')
      .replace(/</g, '&lt;')
      .replace(/>/g, '&gt;');

    // 2. Trích xuất tạm thời các khối Code Blocks để không bị parse nhầm nội dung bên trong
    const codeBlocks: string[] = [];
    html = html.replace(/```([\s\S]*?)```/g, (match, code) => {
      codeBlocks.push(`<pre><code>${code}</code></pre>`);
      return `__CODE_BLOCK_${codeBlocks.length - 1}__`;
    });

    // 3. Tách dòng và xử lý parse Table Markdown thành HTML Table
    const lines = html.split('\n');
    let inTable = false;
    let tableHtml = '';
    const processedLines: string[] = [];

    for (let i = 0; i < lines.length; i++) {
      const line = lines[i].trim();
      const isTableLine = line.startsWith('|') && line.endsWith('|');

      if (isTableLine) {
        // Bỏ qua các dòng phân cách dạng |---|---| hoặc |:---|---:|
        if (line.match(/^\|[\s\-\|:]+\|$/)) {
          continue;
        }

        const cells = line.split('|')
          .slice(1, -1) // Bỏ cell rỗng ở đầu và cuối tạo ra do split('|')
          .map(c => c.trim());

        if (!inTable) {
          inTable = true;
          tableHtml = '<table class="markdown-table"><thead><tr>';
          cells.forEach(cell => {
            tableHtml += `<th>${this.renderInlineElements(cell)}</th>`;
          });
          tableHtml += '</tr></thead><tbody>';
        } else {
          tableHtml += '<tr>';
          cells.forEach(cell => {
            tableHtml += `<td>${this.renderInlineElements(cell)}</td>`;
          });
          tableHtml += '</tr>';
        }
      } else {
        if (inTable) {
          inTable = false;
          tableHtml += '</tbody></table>';
          processedLines.push(tableHtml);
          tableHtml = '';
        }
        processedLines.push(lines[i]);
      }
    }

    if (inTable) {
      tableHtml += '</tbody></table>';
      processedLines.push(tableHtml);
    }

    html = processedLines.join('\n');

    // 4. Định dạng các inline elements (bold, italic, inline code)
    html = this.renderInlineElements(html);

    // 5. Định dạng các danh sách (lists) gạch đầu dòng
    html = html.replace(/^\s*[\-\*]\s+(.+)$/gm, '<li>$1</li>');

    // 6. Khôi phục lại các khối Code Blocks ban đầu
    codeBlocks.forEach((block, index) => {
      html = html.replace(`__CODE_BLOCK_${index}__`, block);
    });

    // 7. Thay đổi các ký tự xuống dòng (\n) thành <br/>, ngoại trừ trong thẻ table hoặc pre
    const finalLines = html.split('\n');
    const finalProcessed: string[] = [];
    let insidePreOrTable = false;

    for (let i = 0; i < finalLines.length; i++) {
      const line = finalLines[i];
      if (line.includes('<table') || line.includes('<pre')) {
        insidePreOrTable = true;
      }

      if (insidePreOrTable) {
        finalProcessed.push(line);
      } else {
        finalProcessed.push(line + '<br/>');
      }

      if (line.includes('</table>') || line.includes('</pre>')) {
        insidePreOrTable = false;
      }
    }

    return finalProcessed.join('');
  }

  private renderInlineElements(text: string): string {
    let result = text;
    // Inline Code: `code`
    result = result.replace(/`([^`]+)`/g, '<code>$1</code>');
    // Bold: **text**
    result = result.replace(/\*\*([^*]+)\*\*/g, '<strong>$1</strong>');
    // Italic: *text*
    result = result.replace(/\*([^*]+)\*/g, '<em>$1</em>');
    return result;
  }

  onScroll(event: Event) {
    const element = event.target as HTMLElement;
    if (element.scrollTop <= 5 && !this.isLoading && !this.isPageLoading && this.hasMoreMessages) {
      this.loadMoreHistory();
    }
  }

  loadMoreHistory() {
    if (!this.currentSessionId || this.isPageLoading || !this.hasMoreMessages) return;

    this.isPageLoading = true;
    const container = this.scrollContainer.nativeElement;
    const oldScrollHeight = container.scrollHeight;
    const oldScrollTop = container.scrollTop;

    this.agentService.getHistory({
      sessionId: this.currentSessionId,
      skipCount: this.totalLoadedDbMessages,
      maxResultCount: this.maxResultCount
    }).subscribe({
      next: (response: any) => {
        this.isPageLoading = false;
        const steps = response?.steps || [];
        const olderHistory: AgentSessionMessageDto[] = [];

        if (steps.length > 0) {
          for (const step of steps) {
            if (step.text) {
              olderHistory.push(step);
            }
          }
        }

        if (olderHistory.length > 0) {
          const existingCreationTimes = new Set(this.messages.map(m => m.creationTime).filter(t => !!t));
          const uniqueOlderHistory = olderHistory.filter(msg => {
            if (msg.creationTime && existingCreationTimes.has(msg.creationTime)) {
              return false;
            }
            return !this.messages.some(m => m.role === msg.role && m.text === msg.text);
          });

          if (uniqueOlderHistory.length > 0) {
            this.messages = [...uniqueOlderHistory, ...this.messages];
            this.totalLoadedDbMessages += steps.length;
            this.hasMoreMessages = steps.length >= this.maxResultCount;

            setTimeout(() => {
              const newScrollHeight = container.scrollHeight;
              container.scrollTop = newScrollHeight - oldScrollHeight + oldScrollTop;
            }, 0);
          } else {
            this.hasMoreMessages = steps.length >= this.maxResultCount;
          }
        } else {
          this.hasMoreMessages = false;
        }
      },
      error: (err) => {
        this.isPageLoading = false;
        console.error('Lỗi khi tải thêm lịch sử chat:', err);
      }
    });
  }

  adjustTextareaHeight(event: Event): void {
    const textarea = event.target as HTMLTextAreaElement;
    textarea.style.height = '24px';
    const newHeight = Math.max(24, Math.min(textarea.scrollHeight, 120));
    textarea.style.height = `${newHeight}px`;
  }
}