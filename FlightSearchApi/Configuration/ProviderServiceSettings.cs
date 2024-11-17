namespace FllightSearchApi.Configuration;
public class ProviderServiceSettings
{
    public ProviderSettings HopeAir { get; set; } = new();
    public ProviderSettings AybJet { get; set; } = new();
}

public class ProviderSettings
{
    public string BaseUrl { get; set; } = string.Empty;
} 