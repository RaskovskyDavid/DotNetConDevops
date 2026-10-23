terraform {
  required_version = ">= 1.6"

  required_providers {
    azurerm = {
      source  = "hashicorp/azurerm"
      version = "~> 4.0"
    }
  }
}

provider "azurerm" {
  features {}

  # El Service Principal solo tiene permisos sobre el grupo TestTerraform,
  # así que Terraform no puede registrar proveedores a nivel de suscripción.
  # Se registran a mano una sola vez (ver Docs/Terraform.md).
  resource_provider_registrations = "none"

  # Las credenciales llegan por las variables de entorno ARM_CLIENT_ID,
  # ARM_CLIENT_SECRET, ARM_TENANT_ID y ARM_SUBSCRIPTION_ID.
}
