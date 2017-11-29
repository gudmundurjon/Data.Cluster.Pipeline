namespace Data.Cluster.Pipeline.Shared.Query.Base
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;

    public abstract class BaseConnectionPattern
    {
        private readonly Dictionary<ConnectionPatternParamType, string> parameters =
            new Dictionary<ConnectionPatternParamType, string>();

        public void AddParameter(ConnectionPatternParamType paramType, string paramValue)
        {
            if (!parameters.ContainsKey(paramType))
            {
                parameters.Add(paramType, paramValue);
            }
        }

        public string GetParameterValue(ConnectionPatternParamType paramType)
        {
            string res;
            if (this.parameters.TryGetValue(paramType, out res))
            {
                return res;
            }
            throw new ArgumentOutOfRangeException("paramType");
        }

        public ReadOnlyDictionary<ConnectionPatternParamType, string> GetParameters()
        {
            return new ReadOnlyDictionary<ConnectionPatternParamType, string>(parameters);
        }
    }
}
