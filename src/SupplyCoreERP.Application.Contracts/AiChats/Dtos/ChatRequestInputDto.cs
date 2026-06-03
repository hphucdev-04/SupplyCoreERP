using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SupplyCoreERP.AiChats.Dtos;

public class ChatRequestInputDto
{
    [Required]
    public string Text { get; set; }

    public List<ChatMessageDto> History { get; set; } = new();
}
