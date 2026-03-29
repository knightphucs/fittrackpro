namespace FitTrackPro.Application.Common.Models
{
    public class JwtSettings
    {
        public string Secret { get; set; } = default!;
        public string Issuer { get; set; } = default!;
        public string Audience { get; set; } = default!;
        public int AccessTokenExpirationMinutes { get; set; } = 60;
        public int RefreshTokenExpirationDays { get; set; } = 7;
    }
}