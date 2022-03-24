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
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using B2B.CimMessageAdapter.Response;
using B2B.CimMessageAdapter.Schema;
using B2B.Transactions.CimMessageAdapter;
using Energinet.DataHub.MarketRoles.Infrastructure.Correlation;
using MarketRoles.B2B.CimMessageAdapter.IntegrationTests.Stubs;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;

namespace B2B.Transactions.CimMessageAdapter.Receiver
{
    public class B2BCimHttpTrigger
    {
        private readonly ILogger _logger;
        private readonly TransactionIdsStub _transactionIdsStub;
        private readonly MessageIdsStub _messageIdsStub;
        private readonly MarketActivityRecordForwarderStub _marketActivityRecordForwarderSpy;
        private readonly SchemaProvider _schemaProvider;
        private readonly ICorrelationContext _correlationContext;

        public B2BCimHttpTrigger(
            ILogger logger,
            ICorrelationContext correlationContext,
            TransactionIdsStub transactionIdsStub,
            MessageIdsStub messageIdsStub,
            MarketActivityRecordForwarderStub marketActivityRecordForwarderStub,
            SchemaProvider schemaprovider)
        {
            _logger = logger;
            _correlationContext = correlationContext;
            _transactionIdsStub = transactionIdsStub;
            _messageIdsStub = messageIdsStub;
            _marketActivityRecordForwarderSpy = marketActivityRecordForwarderStub;
            _schemaProvider = schemaprovider;
        }

        [Function("B2BCimHttpTrigger")]
        public async Task<HttpResponseData> RunAsync(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post")]
            HttpRequestData request)
        {
            _logger.LogInformation("Received MarketRoles request");

            if (request == null) throw new ArgumentNullException(nameof(request));

            var messageReceiver = new CimMessageAdapter.MessageReceiver(_messageIdsStub, _marketActivityRecordForwarderSpy, _transactionIdsStub, _schemaProvider);

            // TODO extract version and business process type from request
            var result = await messageReceiver.ReceiveAsync(request.Body, "requestchangeofsupplier", "1.0")
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
