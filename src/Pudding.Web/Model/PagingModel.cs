using Pudding.Core.Entity;
using System.Collections;

namespace Pudding.Web.Model
{
    public class PagingModel<T> : PagingEntity where T : IEnumerable
    {
        public long TotalCount { get; set; }

        public T List { get; set; }
    }
}
