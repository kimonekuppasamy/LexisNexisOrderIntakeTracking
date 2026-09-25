using System.Collections;
using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;

namespace OrderIntakeTracking.Tests.Infrastructure
{
    public static class ActionResultAssert
    {
        /// <summary>Normalises ActionResult&lt;T&gt; into the IActionResult MVC would actually execute.</summary>
        public static IActionResult Unwrap<T>(ActionResult<T>? result) => result is null
            // `return null;` from an ActionResult<T> action: MVC throws "Cannot return null from an action method" => 500.
            ? new ObjectResult("Action returned a null ActionResult<T>, which ASP.NET Core turns into 500 Internal Server Error.") { StatusCode = StatusCodes.Status500InternalServerError }
            : ((IConvertToActionResult)result).Convert();

        public static int StatusCodeOf(IActionResult result) => result switch
        {
            ObjectResult { StatusCode: { } code } => code,
            ObjectResult { Value: null } => StatusCodes.Status204NoContent, // MVC writes a null body as 204
            ObjectResult => StatusCodes.Status200OK,
            IStatusCodeActionResult { StatusCode: { } code } => code,
            _ => StatusCodes.Status200OK
        };

        public static T Success<T>(IActionResult result)
        {
            var status = StatusCodeOf(result);
            Assert.True(status is >= 200 and < 300, $"Expected a 2xx response but got {Describe(result)}.");
            var value = Assert.IsAssignableFrom<ObjectResult>(result).Value;
            return Assert.IsAssignableFrom<T>(value);
        }

        /// <summary>Asserts a 400 with a non-empty, human-readable message and returns that message.</summary>
        public static string HelpfulBadRequest(IActionResult result)
        {
            Assert.True(StatusCodeOf(result) == StatusCodes.Status400BadRequest,
                $"Expected 400 Bad Request with a helpful message but got {Describe(result)}.");

            var message = MessageOf((result as ObjectResult)?.Value);
            Assert.False(string.IsNullOrWhiteSpace(message), "400 Bad Request was returned without an explanatory message.");
            return message;
        }

        public static string MessageOf(object? value) => value switch
        {
            null => string.Empty,
            string s => s,
            ValidationProblemDetails v => string.Join(" ",
                new[] { v.Title, v.Detail }.Concat(v.Errors.SelectMany(e => e.Value.Prepend(e.Key)))),
            ProblemDetails p => $"{p.Title} {p.Detail}",
            _ => JsonSerializer.Serialize(value)
        };

        public static string Describe(IActionResult result) =>
            $"{StatusCodeOf(result)} ({result.GetType().Name}, body: {MessageOf((result as ObjectResult)?.Value) switch { "" => "<empty>", var m => m }})";
    }

    /// <summary>
    /// Unit tests call controller actions directly, so the [ApiController] automatic 400 for invalid
    /// DataAnnotations never runs. This reproduces it, meaning rules may be implemented either as
    /// attributes on the models or as explicit checks in the controller and the tests pass either way.
    /// </summary>
    public static class ApiControllerModelValidation
    {
        public static IActionResult? Validate(params object?[] models)
        {
            var errors = new Dictionary<string, string[]>();

            foreach (var model in Flatten(models))
            {
                var results = new List<ValidationResult>();
                if (Validator.TryValidateObject(model, new ValidationContext(model), results, validateAllProperties: true))
                    continue;

                foreach (var r in results)
                    foreach (var member in r.MemberNames.DefaultIfEmpty(model.GetType().Name))
                        errors[member] = errors.GetValueOrDefault(member, Array.Empty<string>())
                            .Append(r.ErrorMessage ?? "Invalid value.").ToArray();
            }

            return errors.Count == 0 ? null : new BadRequestObjectResult(new ValidationProblemDetails(errors));
        }

        private static IEnumerable<object> Flatten(IEnumerable<object?> models)
        {
            foreach (var model in models)
            {
                if (model is null or string) continue;
                if (model is IEnumerable many)
                {
                    foreach (var inner in Flatten(many.Cast<object?>())) yield return inner;
                }
                else yield return model;
            }
        }
    }
}
