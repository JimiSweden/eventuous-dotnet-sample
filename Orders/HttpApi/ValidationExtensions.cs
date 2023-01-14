using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

//namespace Orders.HttpApi
// ReSharper disable once CheckNamespace
namespace System.ComponentModel.DataAnnotations;

public class ValidationFailure
{
    public string PropertyName { get; set; }
    public string ErrorMessage { get; set; }

}
public static class ValidationExtensions
{
    
    public static IEnumerable<ValidationFailure> Errors(this ValidationException ex)
    {
        var length = ex.Data.Count;
        var enumerator = ex.Data.GetEnumerator();

        var validationFailures = new List<ValidationFailure>();
        for (int i = 0; i < length; i++)
        {
            var validationFailure = new ValidationFailure
            {
                PropertyName = enumerator.Key.ToString(),
                ErrorMessage = enumerator.Value.ToString()
            };
            validationFailures.Add(validationFailure);
        }

        return validationFailures;

    }

    //inspiration, https://www.youtube.com/watch?v=a1ye9eGTB98 
    public static ValidationProblemDetails ToProblemDetails(this ValidationException ex)
    {
        var error = new ValidationProblemDetails
        {
            Type = "https://tools.ietf.org/html/rfc7231#section-6.5.1", //default type 
            Status = 400
        };

        foreach (var validationFailure in ex.Errors()) //ex.Errors (extension? , chapsas.
        {
            if (error.Errors.ContainsKey(validationFailure.PropertyName))
            {
                error.Errors[validationFailure.PropertyName] =
                    error.Errors[validationFailure.PropertyName]
                        .Concat(new[] { validationFailure.ErrorMessage }).ToArray();
                continue;
            }

            error.Errors.Add(new KeyValuePair<string, string[]>(
                validationFailure.PropertyName,
                new [] {validationFailure.ErrorMessage}));
        }

        return error;
    }
}