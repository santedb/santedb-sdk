using Newtonsoft.Json;
using PakMan.Exceptions;
using RestSrvr;
using RestSrvr.Message;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Security;
using System.Text;

namespace PakSrv
{
    public class PakSrvErrorHandler : IServiceBehavior, IServiceErrorHandler
    {
        /// <summary>
        /// Add the service behavior
        /// </summary>
        public void ApplyServiceBehavior(RestService service, ServiceDispatcher dispatcher)
        {
            dispatcher.ErrorHandlers.Clear();
            dispatcher.ErrorHandlers.Add(this);
        }

        /// <summary>
        /// Handle the specified error
        /// </summary>
        public bool HandleError(Exception error) => true;

        /// <summary>
        /// Provide the actual fault message
        /// </summary>
        public bool ProvideFault(Exception error, RestResponseMessage response)
        {
            if (error is SecurityException)
                response.StatusCode = 403;
            else if (error is FileNotFoundException || error is KeyNotFoundException)
                response.StatusCode = 404;
            else if (error is DuplicateNameException)
                response.StatusCode = 409;
            else
                response.StatusCode = 500;

            var errorResponse = new ErrorResult(error);
            response.Body = new MemoryStream(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(errorResponse)));
            response.ContentType = "application/json";
            return true;
        }
    }
}
