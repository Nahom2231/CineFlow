namespace Infrastructure.Configuration;

public class ChapaOptions
{
    public const string SectionName = "Chapa";

    public string SecretKey { get; set; } = string.Empty;

    public string PublicKey { get; set; } = string.Empty;

    public string BaseUrl { get; set; } = "https://api.chapa.co";

    public string CallbackUrl { get; set; } = string.Empty;
}