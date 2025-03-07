﻿using MagicVilla_VillaAPI.Models;
using MagicVilla_VillaAPI.Models.Dto;

namespace MagicVilla_VillaAPI.Repository.IRepository
{
    public interface IUserRepository 
    {
        bool IsUniqueUser(string username);
        Task<TokenDTO> Login(LoginRequestDTO loginRequestDTO);
        Task<UserDTO> Register(RegisterationRequstDTO registerationRequstDTO);
        Task<TokenDTO> RefreshAccessToken(TokenDTO tokenDTO);

        Task RevokeRefreshToken(TokenDTO tokenDTO);
    }
}
