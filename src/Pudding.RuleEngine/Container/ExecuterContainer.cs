using System;
using NRules;
using Pudding.RuleEngine.Repository;

namespace Pudding.RuleEngine.Container
{
    public class ExecuterContainer : IExecuterContainer
    {
        private readonly IExecuterRepository _executerRepository;
        public ExecuterContainer(IExecuterRepository executerRepository)
        {
            _executerRepository = executerRepository;
        }
        public ISession CreateSession()
        {
            return CreateSession(null);
        }

        public ISession CreateSession(Action<ISession> initializationAction)
        {
            ExecuterRepository repository = _executerRepository as ExecuterRepository;
            repository.LoadAssemblys();
            ISessionFactory factory = repository.Compile();
            ISession session = factory.CreateSession();
            initializationAction?.Invoke(session);
            return session;
        }
    }
}
