import { ProtocolError } from "@modelcontextprotocol/server";
/**
 * Helper hỗ trợ Elicitation Stateless (không treo kết nối) theo chuẩn MCP.
 * Nếu thiếu các trường bắt buộc, helper sẽ ném lỗi -32042 để ngắt kết nối POST ngay lập tức.
 * Nếu đã có đầy đủ, helper trả về kết quả chứa các đối số để tool tiếp tục thực thi.
 */
export async function elicitInput(server, // Sử dụng kiểu any để tránh xung đột kiểu dữ liệu
params, currentArguments) {
    const requiredFields = params.requestedSchema.required || [];
    // Kiểm tra xem tất cả các trường bắt buộc của Elicitation đã có trong arguments hiện tại chưa
    const hasAllRequired = requiredFields.every((field) => {
        const value = currentArguments[field];
        return value !== undefined && value !== null && value !== "";
    });
    if (!hasAllRequired) {
        // Ngắt luồng lập tức bằng cách ném lỗi JSON-RPC -32042 (ELICITATION_REQUIRED)
        throw new ProtocolError(-32042, // ELICITATION_REQUIRED
        params.message, {
            mode: params.mode || 'form',
            requestedSchema: params.requestedSchema
        });
    }
    // Trả về dữ liệu để tool chạy tiếp
    return {
        action: 'accept',
        content: currentArguments
    };
}
