﻿using System.Threading.Tasks;
using Car.Data.Entities;
using Car.Domain.Services.Interfaces;
using Car.WebApi.JwtConfiguration;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Car.WebApi.Controllers
{
    [Route("api/login")]
    [ApiController]
    public class LoginController : ControllerBase
    {
        private readonly ILoginService loginService;
        private readonly IWebTokenGenerator webTokenGenerator;

        public LoginController(ILoginService loginService, IWebTokenGenerator webTokenGenerator)
        {
            this.loginService = loginService;
            this.webTokenGenerator = webTokenGenerator;
        }

        /// <summary>
        /// ensures the user and returns a User for client app,
        /// if user doesn't exist in DB it creates a user and saves them to DB
        /// </summary>
        /// <param name="user">Sender model params</param>
        /// <returns>User for a client app</returns>
        [AllowAnonymous]
        [HttpPost]
        public async Task<IActionResult> Login([FromBody] User user)
        {
            var loginUser = await loginService.LoginAsync(user);
            loginUser.Token = webTokenGenerator.GenerateWebToken(loginUser);

            return Ok(loginUser);
        }
    }
}
