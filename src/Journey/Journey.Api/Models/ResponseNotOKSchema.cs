using System.Text.Json;

namespace Journey.Api.Models;

public class ResponseNotOKSchema
{
 
        /// <summary>
        /// The result.
        /// </summary>
        public object? Result { get; set; }

        /// <summary>
        /// The HTTP Status Code.
        /// </summary>
        public int StatusCode { get; set; }

        /// <summary>
        /// Has the request failed flag.
        /// </summary>
        public bool IsError { get; set; }

        /// <summary>
        /// Internal Error Code of the failed operation.
        /// </summary>
        public string? ErrorCode { get; set; }

        /// <summary>
        /// Descriptive message of the failure.
        /// </summary>
        public string? Message { get; set; }
    public override string ToString()
    {
        return JsonSerializer.Serialize(this);
    }
}
