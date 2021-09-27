using Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Models
{
    public class Result<T>
    {
        public Result(T resultObject, bool success, string errorMessage = "", ErrorType errorType = ErrorType.Exception)
        {
            ResultObject = resultObject;
            Success = success;
            ErrorMessage = errorMessage;
            ErrorType = errorType;
        }

        public T ResultObject { get; set; }
        public bool Success { get; set; }
        public string ErrorMessage { get; set; }
        public ErrorType ErrorType { get; set; }
    }
}
