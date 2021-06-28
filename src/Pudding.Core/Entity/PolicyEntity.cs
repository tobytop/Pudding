using System;
using System.Collections.Generic;
using System.Text;

namespace Pudding.Core.Entity
{
    public class PolicyEntity<TKey>
    {
        public TKey Id { get; set; }
        public string PType { get; set; }
        public string V0 { get; set; }
        public string V1 { get; set; }
        public string V2 { get; set; }
        public string V3 { get; set; }
        public string V4 { get; set; }
        public string V5 { get; set; }

        public override string ToString()
        {
            var stringBuilder = new StringBuilder(PType);
            AppendValue(stringBuilder, V0);
            AppendValue(stringBuilder, V1);
            AppendValue(stringBuilder, V2);
            AppendValue(stringBuilder, V3);
            AppendValue(stringBuilder, V4);
            AppendValue(stringBuilder, V5);
            return stringBuilder.ToString();
        }

        private void AppendValue(StringBuilder stringBuilder, string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return;
            }
            stringBuilder.Append(", ");
            stringBuilder.Append(value);
        }
    }
}
