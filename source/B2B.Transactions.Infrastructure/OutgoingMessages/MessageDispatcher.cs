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
using System.IO;
using System.Threading.Tasks;
using B2B.Transactions.OutgoingMessages;
using Energinet.DataHub.MessageHub.Client.Storage;
using Energinet.DataHub.MessageHub.Model.Model;

namespace B2B.Transactions.Infrastructure.OutgoingMessages
{
    public class MessageDispatcher : IMessageDispatcher
    {
        private readonly IStorageHandler _storageHandler;
        private readonly MessageRequestContext _messageRequestContext;

        public MessageDispatcher(IStorageHandler storageHandler, MessageRequestContext messageRequestContext)
        {
            _storageHandler = storageHandler;
            _messageRequestContext = messageRequestContext;
        }

        public async Task<Uri> DispatchAsync(Stream message)
        {
            return await _storageHandler.AddStreamToStorageAsync(
                message,
                _messageRequestContext.DataBundleRequestDto ?? throw new InvalidOperationException())
                .ConfigureAwait(false);
        }
    }
}