using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Pudding.Web.Api;
using System.Collections.Generic;

namespace Pudding.Web.Valid
{
    public class ValidateResponseAttribute : ActionFilterAttribute
    {
        private IValidatorFactory _validatorFactory;

        public ValidateResponseAttribute(IValidatorFactory validatorFactory)
        {
            _validatorFactory = validatorFactory;
        }

        public override void OnActionExecuting(ActionExecutingContext actionContext)
        {
            foreach (KeyValuePair<string, object> arg in actionContext.ActionArguments)
            {
                if (arg.Value == null)
                {
                    break;
                }
                IValidator valid = _validatorFactory.GetValidator(arg.Value.GetType());
                ValidationResult result = valid?.Validate(arg.Value);
                if (result != null && !result.IsValid)
                {
                    MessageResult messageResult = new MessageResult<IList<ValidationFailure>>
                    {
                        Code = 500,
                        Status = false,
                        Msg = "发生错误",
                        Data = result.Errors
                    };
                    actionContext.Result = new JsonResult(messageResult);
                    break;
                }
            }
        }
    }
}
