using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Navi_Server.Models;

namespace Navi_Server.Services
{
    public class JwtService
    {
        private readonly string _securityKey;
        private readonly string _tokenIssuer;
        private readonly string _tokenAudience;

        private readonly TokenValidationParameters _tokenValidationParameters;


        public JwtService(IConfiguration configuration)
        {
            // Initialize Security-Related strings
            _securityKey = configuration.GetSection("SecurityInfo")["TokenSecurityKey"];
            _tokenIssuer = configuration.GetSection("SecurityInfo")["TokenIssuer"];
            _tokenAudience = configuration.GetSection("SecurityInfo")["TokenAudience"];
            _tokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = _tokenIssuer,
                ValidAudience = _tokenAudience,
                IssuerSigningKey =
                    new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_securityKey)),
                ClockSkew = TimeSpan.Zero
            };
        }
        
        /// <summary>
        /// Generate JWT Token from User Information+
        /// </summary>
        /// <param name="userInfo">Owner of JWT Token.</param>
        /// <returns>JWT Token as string.</returns>
        public string GenerateJwtToken(User userInfo)
        {
            var securityKey =
                new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_securityKey));

            // Set Credential to SHA-512 with security key 
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha512Signature);

            // Create JWT Claims[key : val]
            Claim[] claims =
            {
                new(JwtRegisteredClaimNames.Sub, userInfo.UserEmail),
                new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            };

            // Generate JWT Token
            var token = new JwtSecurityToken(
                _tokenIssuer,
                _tokenAudience,
                claims,
                expires: DateTime.Now.AddHours(2),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
        
        /// <summary>
        /// <para>Get Validated Token</para>
        /// <para>Validate plain token first, and return security token.</para>
        /// </summary>
        /// <param name="userToken">Plain JWT Token</param>
        /// <returns>Validated JwtSecurityToken</returns>
        public JwtSecurityToken GetValidatedToken(string userToken)
        {
            JwtSecurityToken validatedSecurityToken = null;
            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                tokenHandler.ValidateToken(userToken, _tokenValidationParameters, out var validatedToken);

                validatedSecurityToken = (JwtSecurityToken) validatedToken;
            }
            catch
            {
                // Do Nothing
            }

            return validatedSecurityToken;
        }
        
        /// <summary>
        /// Get User Email[ID] from JWT TOken
        /// </summary>
        /// <param name="securityToken">JWT Security Token from <see cref="GetValidatedToken"/>.</param>
        /// <returns>User Email[ID]</returns>
        public string GetUserEmailFromToken(JwtSecurityToken securityToken)
        {
            return securityToken?.Claims.First(token => token.Type == JwtRegisteredClaimNames.Sub).Value;
        }
    }
}