using System.ComponentModel.DataAnnotations;

namespace SupplyCoreERP.AiChats.Dtos;

public class ChatMessageDto
{
    [Required]
    public string Role { get; set; } // "user" hoặc "model" (Gemini dùng "model" cho AI)

    [Required]
    public string Text { get; set; }
}
