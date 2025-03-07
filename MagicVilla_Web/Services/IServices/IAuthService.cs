﻿using MagicVilla_Web.Models.Dto;

namespace MagicVilla_Web.Services.IServices
{
    public interface IAuthService
    {
        Task<T> LoginAsync<T>(LoginRequestDTO obj);

        Task<T> RegisterAsync<T> (RegisterationRequestDTO obj);

        Task<T> LogoutAsync<T>(TokenDTO obj);
    }
}
