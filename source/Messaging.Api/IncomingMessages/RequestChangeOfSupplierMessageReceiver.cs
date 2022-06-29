﻿// Copyright 2020 Energinet DataHub A/S
//
// Licensed under the Apache License, Version 2.0 (the "License2");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System;
using System.Linq;
using System.Net;
using System.Net.Mime;
using System.Text;
using System.Threading.Tasks;
using Messaging.Application.Configuration;
using Messaging.CimMessageAdapter;
using Messaging.CimMessageAdapter.Response;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;

namespace Messaging.Api.IncomingMessages
{
    public class RequestChangeOfSupplierMessageReceiver
    {
        private readonly ILogger<RequestChangeOfSupplierMessageReceiver> _logger;
        private readonly ICorrelationContext _correlationContext;
        private readonly MessageReceiver _messageReceiver;

        public RequestChangeOfSupplierMessageReceiver(
            ILogger<RequestChangeOfSupplierMessageReceiver> logger,
            ICorrelationContext correlationContext,
            MessageReceiver messageReceiver)
        {
            _logger = logger;
            _correlationContext = correlationContext;
            _messageReceiver = messageReceiver ?? throw new ArgumentNullException(nameof(messageReceiver));
        }

        [Function("RequestChangeOfSupplier")]
        public async Task<HttpResponseData> RunAsync(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post")]
            HttpRequestData request)
        {
            _logger.LogInformation($"Received {nameof(RequestChangeOfSupplierMessageReceiver)} request");

            if (request == null) throw new ArgumentNullException(nameof(request));

            var contentType = request.Headers.GetValues("Content-Type").FirstOrDefault();
            var result = await _messageReceiver.ReceiveAsync(request.Body, contentType)
                .ConfigureAwait(false);

            var httpStatusCode = result.Success ? HttpStatusCode.Accepted : HttpStatusCode.BadRequest;
            return CreateResponse(request, httpStatusCode, ResponseFactory.From(result));
        }

        private HttpResponseData CreateResponse(HttpRequestData request, HttpStatusCode statusCode, ResponseMessage responseMessage)
        {
            var response = request.CreateResponse(statusCode);
            response.WriteString(responseMessage.MessageBody, Encoding.UTF8);
            response.Headers.Add("CorrelationId", _correlationContext.Id);
            return response;
        }
    }
}
