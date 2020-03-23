using Pudding.Core.Tool;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Text;

namespace Pudding.Web.Auth
{
    public class DefaultRequirement : IAuthorizationRequirement
    {
        /// <summary>
        /// 验证名称
        /// </summary>
        public string AuthName { get; set; }
        /// <summary>
        /// 使用的架构
        /// </summary>
        public string AuthScheme { get; set; }
        /// <summary>
        /// 发行人
        /// </summary>
        public string Issuer { get; set; }
        /// <summary>
        /// 订阅人
        /// </summary>
        public string Audience { get; set; }
        /// <summary>
        /// 过期时间
        /// </summary>
        public TimeSpan Expiration { get; set; }
        /// <summary>
        /// Refreash过期时间
        /// </summary>
        public TimeSpan RefreashExpiration { get; set; }
        /// <summary>
        /// 签名验证
        /// </summary>
        public SigningCredentials SigningCredentials { get; set; }
        /// <summary>
        /// Refreash签名验证
        /// </summary>
        public SigningCredentials RefreashSigningCredentials { get; set; }
        /// <summary>
        /// 验证权限
        /// </summary>
        public Func<HttpContext, AuthResult> Validation { get; set; }

        public TokenValidationParameters TokenParameters { get; set; }

        public TokenValidationParameters RefreashTokenParameters { get; set; }

        /// <summary>
        /// 构造
        /// </summary>
        /// <param name="deniedAction">拒约请求的url</param> 
        /// <param name="claimType">声明类型</param>
        /// <param name="issuer">发行人</param>
        /// <param name="audience">订阅人</param>
        /// <param name="signingCredentials">签名验证实体</param>
        public DefaultRequirement(string issuer, string audience, string secret, TimeSpan expiration, string authName, string authScheme, Func<HttpContext, AuthResult> validation = null)
        {
            BuildRequirement(issuer, audience, secret, expiration, authName, authScheme, validation);
        }

        public DefaultRequirement(string issuer, string audience, string secret, TimeSpan expiration, string authName, string authScheme, TimeSpan refreashExpiration, Func<HttpContext, AuthResult> validation = null)
        {
            BuildRequirement(issuer, audience, secret, expiration, authName, authScheme, validation);
            var refreashsecret = secret.GetMd5();
            var keyByteArray = Encoding.ASCII.GetBytes(refreashsecret);
            var signingKey = new SymmetricSecurityKey(keyByteArray);
            RefreashSigningCredentials = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);
            RefreashExpiration = refreashExpiration;
            RefreashTokenParameters = new TokenValidationParameters
            {
                ValidateAudience = false,
                ValidateIssuer = false,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = signingKey,
                ValidateLifetime = false,
                ClockSkew = TimeSpan.Zero,
                RequireExpirationTime = false,
            };
        }

        private void BuildRequirement(string issuer, string audience, string secret, TimeSpan expiration, string authName, string authScheme, Func<HttpContext, AuthResult> validation = null)
        {
            Issuer = issuer;
            Audience = audience;
            Expiration = expiration;
            var keyByteArray = Encoding.ASCII.GetBytes(secret);
            var signingKey = new SymmetricSecurityKey(keyByteArray);
            SigningCredentials = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);
            AuthName = authName;
            AuthScheme = authScheme;
            Validation = validation;
            TokenParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = signingKey,
                ValidateIssuer = true,
                ValidIssuer = issuer,//发行人
                ValidateAudience = true,
                ValidAudience = audience,//订阅人
                ValidateLifetime = false,
                ClockSkew = TimeSpan.Zero,
                RequireExpirationTime = false,
            };
        }
    }
}
