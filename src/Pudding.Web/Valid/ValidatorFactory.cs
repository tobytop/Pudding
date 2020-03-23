using Autofac;
using FluentValidation;
using System;

namespace Pudding.Web.Valid
{
    public class ValidatorFactory : IValidatorFactory
    {
        private readonly IComponentContext context;

        public ValidatorFactory(IComponentContext context)
        {
            this.context = context;
        }

        public IValidator<T> GetValidator<T>()
        {
            return context.Resolve<IValidator<T>>();
        }

        public IValidator GetValidator(Type type)
        {
            Type validatorType = typeof(IValidator<>).MakeGenericType(type);
            if (context.IsRegistered(validatorType))
            {
                return context.Resolve(typeof(IValidator<>).MakeGenericType(type)) as IValidator;
            }
            else
            {
                return null;
            }

        }
    }
}
