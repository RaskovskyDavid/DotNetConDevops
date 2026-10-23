variable "resource_group_name" {
  description = "Grupo de recursos que ya existe (creado a mano)."
  type        = string
  default     = "TestTerraform"
}

variable "docker_image" {
  description = "Imagen de Docker Hub a desplegar, ej: usuario/contactosapi:latest"
  type        = string
}
