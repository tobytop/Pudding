namespace Pudding.Web.Api
{
    /// <summary>
    /// api统一输出模式
    /// </summary>
    public class MessageResult
    {
        /// <summary>
        /// 指示是否成功
        /// </summary>
        public bool Status { get; set; }
        /// <summary>
        /// 自定义错误代码
        /// </summary>
        public int Code { get; set; }
        /// <summary>
        /// 返回信息
        /// </summary>
        public string Msg { get; set; }
    }

    public class MessageResult<T> : MessageResult
    {
        /// <summary>
        /// 装载数据
        /// </summary>
        public T Data { get; set; }
    }
}
