using Pudding.Core.Data;

namespace Pudding.Core.Entity
{
    public class PagingEntity
    {
        /// <summary>
        /// 当前页
        /// </summary>
        [Ignore]
        public int PageIndex { get; set; }
        /// <summary>
        /// 当前数量
        /// </summary>
        [Ignore]
        public int PageSize { get; set; }
    }
}
