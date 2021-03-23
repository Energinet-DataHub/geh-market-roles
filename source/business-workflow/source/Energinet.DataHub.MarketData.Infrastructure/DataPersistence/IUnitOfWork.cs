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

using System.Threading.Tasks;

namespace Energinet.DataHub.MarketData.Infrastructure.DataPersistence
{
    /// <summary>
    /// Service for transaction management
    /// </summary>
    public interface IUnitOfWork
    {
        // /// <summary>
        // /// Current transaction
        // /// </summary>
        // IDbTransaction Transaction { get; }

        /// <summary>
        /// Commits the transaction
        /// </summary>
        /// <returns><see cref="Task"/></returns>
        Task CommitAsync();

        /// <summary>
        /// Register updated or changed entity
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="repository"></param>
        void RegisterAmended(IDataModel entity, ICanUpdateDataModel repository);

        /// <summary>
        /// Register new entity
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="repository"></param>
        void RegisterNew(IDataModel entity, ICanInsertDataModel repository);
    }
}
