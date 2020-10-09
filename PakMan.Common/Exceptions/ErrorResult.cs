using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace PakMan.Exceptions
{
    /// <summary>
    /// Represents an error result from the ADGMT server
    /// </summary>
    [JsonObject]
    public class ErrorResult
    {


        /// <summary>
        /// Default ctor
        /// </summary>
        public ErrorResult()
        {

        }

        /// <summary>
        /// Create error result from exception
        /// </summary>
        public ErrorResult(System.Exception exception, bool includeStack = true)
        {
            this.Message = exception.Message;
            if (includeStack)
                this.Stack = exception.StackTrace;
            if (exception.InnerException != null)
                this.CausedBy = new ErrorResult(exception.InnerException, includeStack);
        }

        /// <summary>
        /// Message for the event
        /// </summary>
        [JsonProperty("message")]
        public String Message { get; set; }

        /// <summary>
        /// Cause of the event
        /// </summary>
        [JsonProperty("cause")]
        public ErrorResult CausedBy { get; set; }

        /// <summary>
        /// Stack trace
        /// </summary>
#if DEBUG
        [JsonProperty("stack")]
#else
        [JsonIgnore]
#endif
        public String Stack { get; set; }
    }
}
