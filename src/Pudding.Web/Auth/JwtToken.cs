using Pudding.Core.Tool;
using Pudding.Web.Api;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;

namespace Pudding.Web.Auth
{
    public class JwtToken : IToken
    {
        private readonly IEnumerable<DefaultRequirement> _defaultRequirement;

        public JwtToken(IEnumerable<DefaultRequirement> defaultRequirement)
        {
            _defaultRequirement = defaultRequirement;
        }

        public TokenInfo BuildToken(string authName, Claim[] claims)
        {
            var requirment = _defaultRequirement.FirstOrDefault(o => o.AuthName == authName);
            return buildToken(requirment, claims);
        }

        public TokenInfo BuildRefreashToken(string authName, Claim[] claims)
        {
            var requirment = _defaultRequirement.FirstOrDefault(o => o.AuthName == authName);
            var now = DateTime.UtcNow;
            var jwt = new JwtSecurityToken(
                claims: claims,
                notBefore: now,
                expires: now.Add(requirment.RefreashExpiration),
                signingCredentials: requirment.RefreashSigningCredentials
            );
            var encodedJwt = new JwtSecurityTokenHandler().WriteToken(jwt);
            return new TokenInfo
            {
                Status = true,
                AccessToken = encodedJwt,
                ExpiresIn = requirment.RefreashExpiration.TotalMilliseconds,
                TokenType = "Refreash"
            };
        }

        public MessageResult RefreashToken(string authName, string token, string refreashToken, Func<ClaimsPrincipal, ClaimsPrincipal, AuthResult> condition = null)
        {
            var requirment = _defaultRequirement.FirstOrDefault(o => o.AuthName == authName);
            var handler = new JwtSecurityTokenHandler();
            var principal = handler.ValidateToken(refreashToken, requirment.RefreashTokenParameters, out var refreashsecurityToken);
            if (principal != null)
            {
                if (long.Parse(principal.Claims.SingleOrDefault(s => s.Type == "exp").Value) < DateTime.Now.ToIntS())
                {
                    return new MessageResult
                    {
                        Status = false,
                        Msg = ConifgMessage.REFRESHTOKEN_TIMEOUT
                    };
                }
                else
                {
                    if (principal.Claims.Count() <= 2)
                    {
                        return new MessageResult
                        {
                            Status = false,
                            Msg = ConifgMessage.REFRESHTOKEN_NOTRIGHT
                        };
                    }
                    var tokenprincipal = handler.ValidateToken(token, requirment.TokenParameters, out var securityToken);
                    if (tokenprincipal != null)
                    {
                        bool ischange = false;
                        foreach (var claim in principal.Claims)
                        {
                            if (claim.Type == "exp" || claim.Type == "nbf")
                            {
                                continue;
                            }
                            if (claim.Type == "aud" || claim.Type == "iss" || tokenprincipal.Claims.Count(o => o.Type == claim.Type && o.Value == claim.Value) == 0)
                            {
                                ischange = true;
                            }
                        }
                        if (ischange)
                        {
                            return new MessageResult
                            {
                                Status = false,
                                Msg = ConifgMessage.REFRESHTOKEN_NOTMATCH
                            };
                        }
                        else
                        {
                            if (condition != null)
                            {
                                var result = condition(tokenprincipal, principal);
                                if (!result.IsValid)
                                {
                                    return new MessageResult
                                    {
                                        Status = false,
                                        Msg = result.Msg
                                    };
                                }
                            }

                            return new MessageResult<TokenInfo>
                            {
                                Status = true,
                                Msg = ConifgMessage.TOKEN_NEW,
                                Data = buildToken(requirment, tokenprincipal.Claims.ToArray()),
                            };
                        }
                    }
                    else
                    {
                        return new MessageResult
                        {
                            Status = false,
                            Msg = ConifgMessage.NOTRIGHT
                        };
                    }
                }
            }
            else
            {
                return new MessageResult
                {
                    Status = false,
                    Msg = ConifgMessage.REFRESHTOKEN_NOTRIGHT
                };
            }
        }

        /// <summary>
        /// 生成token方法
        /// </summary>
        /// <param name="requirment"></param>
        /// <param name="claims"></param>
        /// <returns></returns>
        private TokenInfo buildToken(DefaultRequirement requirment, Claim[] claims)
        {
            var now = DateTime.UtcNow;
            var jwt = new JwtSecurityToken(
                issuer: requirment.Issuer,
                audience: requirment.Audience,
                claims: claims,
                notBefore: now,
                expires: now.Add(requirment.Expiration),
                signingCredentials: requirment.SigningCredentials
            );
            var encodedJwt = new JwtSecurityTokenHandler().WriteToken(jwt);
            return new TokenInfo
            {
                Status = true,
                AccessToken = encodedJwt,
                ExpiresIn = requirment.Expiration.TotalMilliseconds,
                TokenType = "Bearer"
            };
        }
    }
}
