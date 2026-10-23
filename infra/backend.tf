# Aquí Terraform guarda su "memoria" (el estado): qué recursos ya creó.
# Vive en la storage account de Azure que creamos a mano, en el contenedor "tfstate".
terraform {
  backend "azurerm" {
    resource_group_name  = "TestTerraform"
    storage_account_name = "tfstatecontactos1984"
    container_name       = "tfstate"
    key                  = "contactos.tfstate"
    use_azuread_auth     = true # usa el Service Principal (rol Storage Blob Data Contributor)
  }
}
