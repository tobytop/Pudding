using Pudding.Web.Api;
using System;
using System.Security.Claims;

namespace Pudding.Web.Auth
{
    public interface IToken
    {
        /// <summary>
        /// 生产token方法
        /// </summary>
        /// <returns></returns>
        TokenInfo BuildToken(string authName, Claim[] claims);

        /// <summary>
        /// 生产refreashtoken方法
        /// </summary>
        /// <param name="authName"></param>
        /// <returns></returns>
        TokenInfo BuildRefreashToken(string authName, Claim[] claims);

        /// <summary>
        /// 刷新Token
        /// </summary>
        /// <param name="authName"></param>
        /// <param name="token"></param>
        /// <param name="refreashToken"></param>
        /// <returns></returns>
        MessageResult RefreashToken(string authName, string token, string refreashToken, Func<ClaimsPrincipal, ClaimsPrincipal, AuthResult> condition = null);
    }
    public class TokenInfo
    {
        public bool Status { get; set; }

        public string AccessToken { get; set; }

        public double ExpiresIn { get; set; }

        public string TokenType { get; set; }
    }

    public class AuthResult
    {
        /// <summary>
        /// 是否验证通过
        /// </summary>
        public bool IsValid { get; set; }
        /// <summary>
        /// 返回消息
        /// </summary>
        public string Msg { get; set; }
    }

    public class ConifgMessage
    {
        /// <summary>
        /// token不正确消息
        /// </summary>
        public const string NOTRIGHT = "Token不正确";
        /// <summary>
        /// Token过期消息
        /// </summary>
        public const string TIMEOUT = "凭证过期";
        /// <summary>
        /// refreshtoken过期消息
        /// </summary>
        public const string REFRESHTOKEN_TIMEOUT = "refreshtoken已经过期";
        /// <summary>
        /// refreshtoken不正确消息
        /// </summary>
        public const string REFRESHTOKEN_NOTRIGHT = "refreshtoken不正确";
        /// <summary>
        /// token与refreashtoken不匹配
        /// </summary>
        public const string REFRESHTOKEN_NOTMATCH = "token与refreashtoken不匹配";
        /// <summary>
        /// token生成消息
        /// </summary>
        public const string TOKEN_NEW = "新的token生成";
    }
}
