using TA_API.Models.Data;
using System.Threading.Tasks;
using TA_API.Models.DTOs;

namespace TA_API.Services.Auth
{
    public interface IAuthService
    {
        /// <summary>
        /// Autentica al usuario por userName o email y devuelve token + info del usuario.
        /// Devuelve null si las credenciales no son válidas.
        /// </summary>
        Task<AuthResult?> AuthenticateAsync(string userNameOrEmail, string password);
    }
}