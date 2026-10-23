# El grupo de recursos ya existe: lo LEEMOS (data), no lo creamos.
data "azurerm_resource_group" "rg" {
  name = var.resource_group_name
}

# Lugar donde se guardan los registros (logs) de la app.
resource "azurerm_log_analytics_workspace" "logs" {
  name                = "log-contactos"
  location            = data.azurerm_resource_group.rg.location
  resource_group_name = data.azurerm_resource_group.rg.name
  sku                 = "PerGB2018"
  retention_in_days   = 30
}

# El "terreno" donde viven los contenedores.
resource "azurerm_container_app_environment" "env" {
  name                       = "cae-contactos"
  location                   = data.azurerm_resource_group.rg.location
  resource_group_name        = data.azurerm_resource_group.rg.name
  log_analytics_workspace_id = azurerm_log_analytics_workspace.logs.id
}

# La API de contactos corriendo desde la imagen de Docker Hub.
resource "azurerm_container_app" "api" {
  name                         = "ca-contactos"
  container_app_environment_id = azurerm_container_app_environment.env.id
  resource_group_name          = data.azurerm_resource_group.rg.name
  revision_mode                = "Single"

  template {
    min_replicas = 0
    max_replicas = 1

    container {
      name   = "contactosapi"
      image  = var.docker_image
      cpu    = 0.25
      memory = "0.5Gi"
    }
  }

  ingress {
    external_enabled = true
    target_port      = 8080

    traffic_weight {
      latest_revision = true
      percentage      = 100
    }
  }
}
