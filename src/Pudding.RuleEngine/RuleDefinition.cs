using System.Collections.Generic;
using System.Linq.Expressions;

namespace Pudding.RuleEngine
{
    /// <summary>
    /// 规则实例
    /// </summary>
    public class RuleDefinition
    {
        /// <summary>
        /// 规则名称
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 规则条件
        /// </summary>
        public List<LambdaExpression> Conditions { get; set; }

        /// <summary>
        /// 规则执行动作
        /// </summary>
        public List<LambdaExpression> Actions { get; set; }
    }
}
