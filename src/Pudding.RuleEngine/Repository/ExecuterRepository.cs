using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using NRules.Fluent;
using NRules.RuleModel;
using NRules.RuleModel.Builders;
using Pudding.RuleEngine.Util;

namespace Pudding.RuleEngine.Repository
{
    public class ExecuterRepository : IRuleRepository, IExecuterRepository
    {
        private readonly IRuleSet _ruleSet;
        private readonly List<Assembly> _assemblys;
        private readonly RuleRepository _internalRuleRepository;

        public ExecuterRepository()
        {
            _ruleSet = new RuleSet("default");
            _assemblys = new List<Assembly>();
            _internalRuleRepository = new RuleRepository();
        }

        public void AddAssembly(IEnumerable<Assembly> assemblys)
        {
            _assemblys.AddRange(assemblys);
        }

        public void AddAssembly(Assembly assembly)
        {
            _assemblys.Add(assembly);
        }

        public void AddRule(RuleDefinition definition)
        {
            RuleBuilder builder = new RuleBuilder();
            builder.Name(definition.Name);
            definition.Conditions.ForEach(o =>
            {
                ParameterExpression parameter = o.Parameters.FirstOrDefault();
                Type type = parameter.Type;
                PatternBuilder customerPattern = builder.LeftHandSide().Pattern(type, parameter.Name);
                customerPattern.Condition(o);
            });
            definition.Actions.ForEach(o =>
            {
                ParameterExpression param = o.Parameters.FirstOrDefault();
                dynamic obj = getObject(param.Type);
                builder.RightHandSide().Action(parseAction(obj, o, param.Name));
            });
            _ruleSet.Add(new[] { builder.Build() });
        }

        private LambdaExpression parseAction<TEntity>(TEntity entity, LambdaExpression action, string param) where TEntity : class, new()
        {
            return NRulesHelper.AddContext(action as Expression<Action<TEntity>>);
        }

        internal void LoadAssemblys()
        {
            _internalRuleRepository.Load(x => x.From(_assemblys));
        }

        public IEnumerable<IRuleSet> GetRuleSets()
        {
            List<IRuleSet> sets = new List<IRuleSet>
            {
                _ruleSet
            };
            sets.AddRange(_internalRuleRepository.GetRuleSets());
            return sets;
        }

        private dynamic getObject(Type type)
        {
            return Activator.CreateInstance(type);
        }
    }
}
