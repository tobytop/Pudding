using System;
using NRules;

namespace Pudding.RuleEngine.Container
{
    public interface IExecuterContainer
    {
        ISession CreateSession();

        ISession CreateSession(Action<ISession> initializationAction);
    }
}
