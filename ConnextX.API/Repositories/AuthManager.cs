using AutoMapper;
using ConnextX.API.Contracts;
using ConnextX.API.Data.Models;
using ConnextX.API.Data.Models.DbContext;
using ConnextX.API.Data.Models.Users;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace ConnextX.API.Repositories
{
    public class AuthManager : IAuthManager
    {
        private readonly IMapper _mapper;
        private readonly UserManager<ApiUser> _userManager;
        private readonly IConfiguration _configuration;
        private ApiUser _user;
        private ApplicationDbContext _dbContext;

        private const string _loginProvider = "ConnectXAPI";
        private const string _refreshToken = "RefreshToken";
        public AuthManager(IMapper mapper,
            UserManager<ApiUser> userManager, 
            IConfiguration configuration,
            ApplicationDbContext dbContext,
            Helper helper)
        {
            this._mapper = mapper;
            this._userManager = userManager;
            this._configuration = configuration;
            this._dbContext = dbContext;
        }

        public async Task<IEnumerable<IdentityError>> Register(ApiUserDto userDto)
        {
            _user = _mapper.Map<ApiUser>(userDto);
            var result = await _userManager.CreateAsync(_user, userDto.Password);
            if (result.Succeeded)
            {
                await _userManager.AddToRolesAsync(_user, new[] { "User" });
            }
            return result.Errors;
        }

        public async Task<AuthResponseDto> Login(LoginDto loginDto)
        {
            _user = _dbContext.Users.Where(X => X.UserName == loginDto.UserName).FirstOrDefault();
            bool isValidUser = await _userManager.CheckPasswordAsync(_user, loginDto.Password);
            if (_user == null || isValidUser == false)
            {
                return null;
            }
            //Generating Token After Credentials are Valid
            var token = await GenerateToken();
            return new AuthResponseDto
            {
                FirstName = _user.FirstName,
                LastName = _user.LastName,
                UserName = _user.UserName,
                Email = _user.Email,
                Token = token,
            };
        }

        private async Task<string> GenerateToken()
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JwtSettings:Key"]));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
            var roles = await _userManager.GetRolesAsync(_user);
            var roleClaims = roles.Select(r => new Claim(ClaimTypes.Role, r)).ToList();
            var UserClaims = await _userManager.GetClaimsAsync(_user);
            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, _user.UserName),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(JwtRegisteredClaimNames.Email, _user.Email),
                new Claim("uid", _user.Id)
            }.Union(roleClaims).Union(UserClaims);

            var token = new JwtSecurityToken(
                issuer: _configuration["JwtSettings:Issuer"],
                audience: _configuration["JwtSettings:Audience"],
                claims: claims,
                expires: DateTime.Now.AddMinutes(Convert.ToDouble(_configuration["JwtSettings:DurationInMinutes"])),
                signingCredentials: credentials
                );
            return new JwtSecurityTokenHandler().WriteToken(token);
        }


        string IAuthManager.VerifyJwt(string jwtToken)
        {
            try
            {
                var handler = new JwtSecurityTokenHandler();
                string Key = _configuration["JwtSettings:Key"];
                var tokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(Key)),
                    ValidateIssuer = false,
                    ValidateAudience = false
                };

                SecurityToken validatedToken;
                handler.ValidateToken(jwtToken, tokenValidationParameters, out validatedToken);

                // Extract expiration time from the validated token
                var decodedJwtToken = (JwtSecurityToken)validatedToken;
                var expirationTime = decodedJwtToken.ValidTo;

                // Compare expiration time with current time
                if (expirationTime > DateTime.UtcNow)
                {
                    return decodedJwtToken.Claims.Where(X => X.Type == "uid").FirstOrDefault().Value;
                }
                else
                {
                    return "Not A Valid Token , Please Re-Login";
                }
            }
            catch (SecurityTokenExpiredException)
            {
                return ("Token has expired");
            }
            catch (Exception ex)
            {
                return ($"Invalid token: {ex.Message}");
            }
        }
    }
}
