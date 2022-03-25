# Copyright 2020 Energinet DataHub A/S
#
# Licensed under the Apache License, Version 2.0 (the "License2");
# you may not use this file except in compliance with the License.
# You may obtain a copy of the License at
#
#     http://www.apache.org/licenses/LICENSE-2.0
#
# Unless required by applicable law or agreed to in writing, software
# distributed under the License is distributed on an "AS IS" BASIS,
# WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
# See the License for the specific language governing permissions and
# limitations under the License.
module "func_receiver" {
  source                                    = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/function-app?ref=5.1.0"

  name                                      = "cimmessagereceiver"
  project_name                              = var.domain_name_short
  environment_short                         = var.environment_short
  environment_instance                      = var.environment_instance
  resource_group_name                       = azurerm_resource_group.this.name
  location                                  = azurerm_resource_group.this.location
  app_service_plan_id                       = data.azurerm_key_vault_secret.plan_shared_id.value
  application_insights_instrumentation_key  = data.azurerm_key_vault_secret.appi_instrumentation_key.value
  always_on                                 = true
  app_settings                              = {
    # Region: Default Values
    WEBSITE_ENABLE_SYNC_UPDATE_SITE                               = true
    WEBSITE_RUN_FROM_PACKAGE                                      = 1
    WEBSITES_ENABLE_APP_SERVICE_STORAGE                           = true
    FUNCTIONS_WORKER_RUNTIME                                      = "dotnet-isolated"
    # Endregion: Default Values
    # Shared resources logging
    REQUEST_RESPONSE_LOGGING_CONNECTION_STRING   = data.azurerm_key_vault_secret.st_market_operator_logs_primary_connection_string.value
    REQUEST_RESPONSE_LOGGING_CONTAINER_NAME      = data.azurerm_key_vault_secret.st_market_operator_logs_container_name.value
    B2C_TENANT_ID                         = data.azurerm_key_vault_secret.b2c_tenant_id.value
    BACKEND_SERVICE_APP_ID                = data.azurerm_key_vault_secret.backend_service_app_id.value
    # Endregion: Default Values
    MARKET_DATA_QUEUE_URL                   = "${module.sb_marketroles.name}.servicebus.windows.net:9093"
    MARKET_DATA_QUEUE_CONNECTION_STRING     = module.sb_marketroles.primary_connection_strings["send"]
    MARKET_DATA_QUEUE_TOPIC_NAME            = module.sbq_marketroles.name
  }
  
  tags                                      = azurerm_resource_group.this.tags
}