namespace TA_API.Helpers
{
    public class TokenReaderHelper
    {

        public static string? GetUserIdFromToken(string token)
        {
            if (string.IsNullOrEmpty(token))
            {
                return null;
            }
            try
            {
                var handler = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler();
                var jwtToken = handler.ReadJwtToken(token);
                var userIdClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == "nameid" || c.Type == "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier");
                return userIdClaim?.Value;
            }
            catch
            {
                return null;
            }
        }

        public static string? GetUserNameFromToken(string token)
        {
            if (string.IsNullOrEmpty(token))
            {
                return null;
            }
            try
            {
                var handler = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler();
                var jwtToken = handler.ReadJwtToken(token);
                var userNameClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == "unique_name" || c.Type == "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name");
                return userNameClaim?.Value;
            }
            catch
            {
                return null;
            }
        }

        public static string? GetRoleFromToken(string token)
        {
            if (string.IsNullOrEmpty(token))
            {
                return null;
            }
            try
            {
                var handler = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler();
                var jwtToken = handler.ReadJwtToken(token);
                var roleClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == "role" || c.Type == "http://schemas.microsoft.com/ws/2008/06/identity/claims/role");
                return roleClaim?.Value;
            }
            catch
            {
                return null;
            }
        }
    }
}
