using System.ComponentModel.DataAnnotations;

namespace R3Polska.Sse.Mercure;

public class MercurePublisherOptions
{
    [Required, Url]
    public required string Host { get; set; }

    [Required, MinLength(1)]
    public required string Token { get; set; }
}