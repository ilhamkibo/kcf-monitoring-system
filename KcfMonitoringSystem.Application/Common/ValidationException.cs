using System;
using System.Collections.Generic;

namespace KcfMonitoringSystem.Application.Common;

public class ValidationException : Exception
{
    public Dictionary<string, string[]> Errors { get; }

    public ValidationException(string message) : base(message)
    {
        Errors = new Dictionary<string, string[]>();
    }

    public ValidationException(string propertyName, string errorMessage) : base("Error Validations")
    {
        Errors = new Dictionary<string, string[]>
        {
            { propertyName, new[] { errorMessage } }
        };
    }

    public ValidationException(Dictionary<string, string[]> errors) : base("Error Validations")
    {
        Errors = errors;
    }
}
