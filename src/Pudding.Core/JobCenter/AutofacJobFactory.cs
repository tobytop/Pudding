using Autofac;
using Quartz;
using Quartz.Spi;

namespace Pudding.Core.JobCenter
{
    public class AutofacJobFactory : IJobFactory
    {
        private readonly IComponentContext _context;

        public AutofacJobFactory(IComponentContext context)
        {
            _context = context;
        }

        public IJob NewJob(TriggerFiredBundle bundle, IScheduler scheduler)
        {
            return (IJob)_context.Resolve(bundle.JobDetail.JobType);
        }

        public void ReturnJob(IJob job)
        {
        }
    }
}
