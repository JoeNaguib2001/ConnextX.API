using ConnextX.API.Data.Models.Users;
using Microsoft.AspNetCore.Identity;

namespace ConnextX.API.Contracts
{
    public interface IAuthManager
    {
        Task<IEnumerable<IdentityError>> Register(ApiUserDto userDto);
        Task<AuthResponseDto> Login(LoginDto loginDto);
        string VerifyJwt(string token);
    }
}
