output "api_url" {
  description = "Dirección pública de la API."
  value       = "https://${azurerm_container_app.api.ingress[0].fqdn}/weatherforecast"
}
