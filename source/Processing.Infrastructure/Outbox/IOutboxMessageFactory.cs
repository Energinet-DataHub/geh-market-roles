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

namespace Processing.Infrastructure.Outbox
{
    /// <summary>
    ///  Creates an outbox message containing the serialized message/payload
    /// </summary>
    public interface IOutboxMessageFactory
    {
        /// <summary>
        /// Creates outbox message
        /// </summary>
        /// <param name="message">Message payload</param>
        /// <param name="category">Message category <see cref="OutboxMessageCategory"/></param>
        /// <typeparam name="T">Type of message payload to wrap in outbox message</typeparam>
        /// <returns><see cref="OutboxMessage"/></returns>
        OutboxMessage CreateFrom<T>(T message, OutboxMessageCategory category);
    }
}