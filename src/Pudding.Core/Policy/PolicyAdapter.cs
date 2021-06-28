using NetCasbin.Model;
using NetCasbin.Persist;
using Pudding.Core.Entity;
using System.Collections.Generic;

namespace Pudding.Core.Policy
{
    public abstract class PolicyAdapter<Tkey> : IAdapter
    {
        protected abstract IEnumerable<PolicyEntity<Tkey>> LoadPolicy();

        #region IAdapter
        public abstract void AddPolicy(string sec, string ptype, IList<string> rule);
        public void LoadPolicy(Model model)
        {
            LoadPolicyFromCasbinRules(model, LoadPolicy());
        }
        public void RemoveFilteredPolicy(string sec, string ptype, int fieldIndex, params string[] fieldValues) { }
        public void RemovePolicy(string sec, string ptype, IList<string> rule) { }
        public void SavePolicy(Model model) { }
        #endregion

        #region private
        private void LoadPolicyFromCasbinRules(Model model, IEnumerable<PolicyEntity<Tkey>> rules)
        {
            foreach (var rule in rules)
            {
                string ruleString = rule.ToString();
                Helper.LoadPolicyLine(ruleString, model);
            }
        }
        #endregion
    }
}
