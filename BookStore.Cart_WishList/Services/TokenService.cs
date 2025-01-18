using BookStore.Cart_WishList.Interfaces;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;

public class TokenService:ITokenService
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IConfiguration _configuration;
    private readonly ILogger<TokenService> _logger;

    public TokenService(IConfiguration configuration, IHttpContextAccessor httpContextAccessor, ILogger<TokenService> logger)
    {
        _configuration = configuration;
        _httpContextAccessor = httpContextAccessor;
        _logger = logger;
    }

    public virtual int GetUserIdFromToken()
    {
        try
        {
            var userIdClaim = _httpContextAccessor.HttpContext.User.FindFirst(claim => claim.Type == ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userIdClaim))
            {
                _logger.LogError("UserId claim is missing or empty.");
                throw new UnauthorizedAccessException();
            }

            _logger.LogInformation("UserId claim found: " + userIdClaim);
            return int.Parse(userIdClaim);
        }
        catch (SecurityTokenExpiredException ex)
        {
            _logger.LogError("Token expired: " + ex.Message);
            throw new SecurityTokenExpiredException("Token has expired.");
        }
        catch (Exception ex)
        {
            _logger.LogError("Error extracting user ID from token: " + ex.Message);
            throw;
        }
    }
}
