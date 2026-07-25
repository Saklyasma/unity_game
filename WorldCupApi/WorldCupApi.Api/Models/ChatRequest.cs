using System.ComponentModel.DataAnnotations;

namespace WorldCupApi.Api.Models;

public class ChatRequest
{
    [Required]
    public int PlayerId { get; set; }

    [Required]
    [MaxLength(2000)]
    public string Message { get; set; } = string.Empty;
}
