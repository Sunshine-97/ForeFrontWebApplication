using FluentValidation;
using MediatR;

namespace ForeFrontWebApplication.Shared.Behaviors
{

    /// <summary>
    /// MediatR pipeline behavior that runs all registered FluentValidation validators
    /// for a request before its handler executes. A failed validation short-circuits the
    /// pipeline by throwing a <see cref="ValidationException"/>, which is mapped to a
    /// 400 response in the global exception handler.
    /// </summary>
<<<<<<< HEAD
    public class ValidationBehavior<TRequest, TResponse>
=======
    public sealed class ValidationBehavior<TRequest, TResponse>
>>>>>>> 7726cb5f43fa24672b425b11376930eed481072c
        : IPipelineBehavior<TRequest, TResponse>
        where TRequest : notnull
    {
        private readonly IEnumerable<IValidator<TRequest>> _validators;

        public ValidationBehavior(IEnumerable<IValidator<TRequest>> validators)
        {
            _validators = validators;
        }

        public async Task<TResponse> Handle(
            TRequest request,
            RequestHandlerDelegate<TResponse> next,
            CancellationToken cancellationToken)
        {
            if (!_validators.Any())
                return await next();

            var context = new ValidationContext<TRequest>(request);

            var failures = (await Task.WhenAll(
                    _validators.Select(v => v.ValidateAsync(context, cancellationToken))))
                .SelectMany(result => result.Errors)
                .Where(failure => failure is not null)
                .ToList();

            if (failures.Count != 0)
                throw new ValidationException(failures);

            return await next();
        }
    }
}