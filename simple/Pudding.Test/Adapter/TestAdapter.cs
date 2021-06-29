using Pudding.Core.Entity;
using Pudding.Core.Policy;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Pudding.Test.Adapter
{
    public class TestAdapter : PolicyAdapter<int>
    {
        protected override IEnumerable<PolicyEntity<int>> LoadPolicy()
        {
            List<PolicyEntity<int>> list = new List<PolicyEntity<int>>()
            {
                new PolicyEntity<int>
                {
                    Id =1,
                    PType = "p",
                    V0 = "alice",
                    V1 = "data1",
                    V2 = "read",
                }
            };
            return list;
        }
    }
}
