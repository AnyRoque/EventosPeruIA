namespace EventosPeruIA.CORE.Core.Settings
{
    public class JWTSettings
    {
        public string SecretKey { get; set; } = string.Empty;
        public string Issuer { get; set; } = string.Empty;
        public string Audience { get; set; } = string.Empty;
        public double ExpirationInMinutes { get; set; }
    }
}
