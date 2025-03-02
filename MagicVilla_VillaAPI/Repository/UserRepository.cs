using AutoMapper;
using MagicVilla_VillaAPI.Data;
using MagicVilla_VillaAPI.Models;
using MagicVilla_VillaAPI.Models.Dto;
using MagicVilla_VillaAPI.Repository.IRepository;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace MagicVilla_VillaAPI.Repository
{
    public class UserRepository : IUserRepository
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IMapper _mapper;
        private string secretKey;
        public UserRepository(ApplicationDbContext db, UserManager<ApplicationUser> userManager,
            IConfiguration configuration, RoleManager<IdentityRole> roleManager, IMapper mapper)
        {
            _db = db;
            _userManager = userManager;
            _roleManager = roleManager;
            _mapper = mapper;
            secretKey = configuration.GetValue<string>("ApiSettings:Secret");
        }
        public bool IsUniqueUser(string username)
        {
            var user = _db.ApplicationUsers.FirstOrDefault(x => x.UserName == username);
            if (user == null)
            {
                return true;
            }
            return false;
        }

        public async Task<TokenDTO> Login(LoginRequestDTO loginRequestDTO)
        {
            var user = _db.ApplicationUsers.FirstOrDefault(u=>u.UserName.ToLower() == loginRequestDTO.UserName.ToLower());

            bool isValid = await _userManager.CheckPasswordAsync(user, loginRequestDTO.Password);
            if ( user ==null || isValid == false)
            {
                return new TokenDTO()
                {
                    AccessToken = "",
                };
            }
            
            var jwtTokenId = $"JTI{Guid.NewGuid()}";
            var accessToken = await GetAccessToken(user,jwtTokenId);
            var refreshToken = await CreateNewRefreshToken(jwtTokenId, accessToken);

            TokenDTO tokenDto = new TokenDTO()
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                //Role = roles.FirstOrDefault()
            };
            return tokenDto;

        }

        public async Task<UserDTO> Register(RegisterationRequstDTO registerationRequstDTO)
        {
            ApplicationUser user = new()
            {
                UserName = registerationRequstDTO.UserName,
                Email = registerationRequstDTO.UserName,
                NormalizedEmail=registerationRequstDTO.UserName.ToUpper(),
                Name = registerationRequstDTO.Name,
            };
            try
            {
                var  result = await _userManager.CreateAsync(user,registerationRequstDTO.Password);
                if (result.Succeeded)
                {
                    if(!_roleManager.RoleExistsAsync(registerationRequstDTO.Role).GetAwaiter().GetResult())
                    {
                        await _roleManager.CreateAsync(new IdentityRole(registerationRequstDTO.Role));
                    }
                    await _userManager.AddToRoleAsync(user, registerationRequstDTO.Role);
                    var userToReturn = _db.ApplicationUsers
                        .FirstOrDefault(u => u.UserName == registerationRequstDTO.UserName);
                    return _mapper.Map<UserDTO>(userToReturn);
                }
            }
            catch
            {

            }
            return new UserDTO();
            
        }

        public async Task<TokenDTO> RefreshAccessToken(TokenDTO tokenDTO)
        {
            //Find an existing refresh token
            var existingRefreshToken = await _db.RefreashTokens.FirstOrDefaultAsync(u=>u.Refresh_Token == tokenDTO.RefreshToken);
            if (existingRefreshToken == null)
            {
                return new TokenDTO();
            }

            //compare data from existing refresh and access token provided and if there is any missmatch then consider it as fraud
            var isTokenValid = GetAccessTokenData(tokenDTO.AccessToken,existingRefreshToken.UserId,existingRefreshToken.JwtTokenId);
            if (!isTokenValid)
            {
                await MarkTokenAsInvalid(existingRefreshToken);
                return new TokenDTO();
            }

            //When someone tries to use not valid refresh token, fraud possible
            if (!existingRefreshToken.IsValid) {
               
                await MarkAllTokenInChainAsInvalid(existingRefreshToken.UserId,existingRefreshToken.JwtTokenId);
            }

            //if just expired then mark as invalid and return empty
            if(existingRefreshToken.ExpiresAt<DateTime.UtcNow)
            {
                existingRefreshToken.IsValid = false;
                _db.SaveChanges();
                return new TokenDTO();
            }

            //replace old refresh with a new one with updated expire date
            var newRefreshToken = await CreateNewRefreshToken(existingRefreshToken.UserId,existingRefreshToken.JwtTokenId);

            //revoke existing refresh token
            existingRefreshToken.IsValid = false;
            _db.SaveChanges();

            //generate new access token 
            var applicationUser = _db.ApplicationUsers.FirstOrDefault(u=>u.Id == existingRefreshToken.UserId);
            if (applicationUser == null) return new TokenDTO();

            var newAccessToken = await GetAccessToken(applicationUser, existingRefreshToken.JwtTokenId);

            return new TokenDTO()
            {
                AccessToken = newAccessToken,
                RefreshToken = newRefreshToken,
            };



        }

        public async Task RevokeRefreshToken(TokenDTO tokenDTO)
        {
            var existingRefreshToken = await _db.RefreashTokens.FirstOrDefaultAsync
                (_=>_.Refresh_Token == tokenDTO.RefreshToken);
            if (existingRefreshToken == null) return;

            var isTokenValid = GetAccessTokenData(tokenDTO.AccessToken,existingRefreshToken.UserId, existingRefreshToken.JwtTokenId);
            if (!isTokenValid) return;

            await MarkAllTokenInChainAsInvalid(existingRefreshToken.UserId, existingRefreshToken.JwtTokenId);

        }

        private async Task<string> GetAccessToken(ApplicationUser user,string jwtTokenId)
        {
            var roles = await _userManager.GetRolesAsync(user);
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(secretKey);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new Claim[]
                {
                    new Claim(ClaimTypes.Name,user.UserName.ToString()),
                    new Claim(ClaimTypes.Role,roles.FirstOrDefault()),
                    new Claim(JwtRegisteredClaimNames.Jti,jwtTokenId),
                    new Claim(JwtRegisteredClaimNames.Sub,user.Id),
                }),
                Expires = DateTime.UtcNow.AddMinutes(60),
                SigningCredentials = new(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);

            var tokenStr = tokenHandler.WriteToken(token);
            return tokenStr;
        }

        private async Task<string> CreateNewRefreshToken(string userId, string tokenId)
        {
            RefreshToken refreshToken = new()
            {
                IsValid = true,
                UserId = userId,
                JwtTokenId = tokenId,
                ExpiresAt = DateTime.UtcNow.AddDays(30),
                Refresh_Token = Guid.NewGuid()+"-"+Guid.NewGuid()
            };
            await _db.RefreashTokens.AddAsync(refreshToken);
            await _db.SaveChangesAsync();
            
            return refreshToken.Refresh_Token;
        }

        private bool GetAccessTokenData(string accessToken, string expectedUserId, string expectedTokenId)
        {
            try
            {
                var tokenHanlder = new JwtSecurityTokenHandler();
                var jwt = tokenHanlder.ReadJwtToken(accessToken);
                var jwtTokenId = jwt.Claims.FirstOrDefault(u => u.Type == JwtRegisteredClaimNames.Jti).Value;
                var userId = jwt.Claims.FirstOrDefault(u => u.Type == JwtRegisteredClaimNames.Sub).Value;
                return userId == expectedUserId && jwtTokenId == expectedTokenId;
            }
            catch
            {
                return false;
            }
        }

        private Task MarkTokenAsInvalid(RefreshToken refreshToken)
        {
            refreshToken.IsValid = false;
            return _db.SaveChangesAsync();
        }

        private async Task MarkAllTokenInChainAsInvalid(string userId,string tokenId)
        {
             await _db.RefreashTokens.Where(u => u.UserId == userId
               && u.JwtTokenId == tokenId)
               .ExecuteUpdateAsync(u => u.SetProperty(refreshToken => refreshToken.IsValid, false));

            _db.SaveChangesAsync();
        }

        
    }
}
