resource "azurerm_monitor_action_group" "metering_point" {
  name                = "ag-metering-point-${lower(var.environment_short)}-${lower(var.environment_instance)}"
  resource_group_name = azurerm_resource_group.this.name
  short_name          = "ag-mp-${lower(var.environment_short)}-${lower(var.environment_instance)}"

  email_receiver {
    name                    = "Alerts-MarketRoles-${lower(var.domain_name_short)}-${lower(var.environment_short)}-${lower(var.environment_instance)}"
    email_address           = "0a494d0d.energinet.onmicrosoft.com@emea.teams.ms"
    use_common_alert_schema = true
  }
}


resource "azurerm_monitor_scheduled_query_rules_alert" "marketroles_outbox" {
  name                = "alert-outbox-${lower(var.domain_name_short)}-${lower(var.environment_short)}-${lower(var.environment_instance)}"
  location            = azurerm_resource_group.this.location
  resource_group_name = var.shared_resources_resource_group_name

  action {
    action_group           = azurerm_monitor_action_group.metering_point.id
  }
  data_source_id = data.azurerm_key_vault_secret.appi_shared_id.value
  description    = "Alert when total results cross threshold"
  enabled        = true
  # Count all requests with server error result code grouped into 5-minute bins
  query       = <<-QUERY
  requests
| where timestamp > ago(1h) and  success == false
| join kind= inner (
exceptions
| where timestamp > ago(1h)  and cloud_RoleName == 'func-outbox-${lower(var.domain_name_short)}-${lower(var.environment_short)}-${lower(var.environment_instance)}'
) on operation_Id
| project exceptionType = type, failedMethod = method, requestName = name, requestDuration = duration, function = cloud_RoleName
  QUERY
  severity    = 1
  frequency   = 5
  time_window = 30
  trigger {
    operator  = "GreaterThan"
    threshold = 0
  }
}