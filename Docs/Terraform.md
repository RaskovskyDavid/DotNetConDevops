# Terraform en Azure: preparación y uso

## Archivos (carpeta `infra/`)
| Archivo | Para qué sirve |
|---|---|
| `backend.tf` | Dónde guarda Terraform su estado: storage account `tfstatecontactos1984`, contenedor `tfstate` |
| `providers.tf` | Le dice a Terraform que hable con Azure (`azurerm`) |
| `variables.tf` | Valores que se pueden cambiar (grupo de recursos, imagen) |
| `main.tf` | La infraestructura: Log Analytics + Container Apps Environment + Container App con tu imagen |
| `outputs.tf` | Muestra la URL pública al terminar |

Workflow: `.github/workflows/terraform.yml` (plan en Pull Request, apply al mezclar en `main`).

## Preparación única en Azure (portal)
1. Grupo de recursos `TestTerraform` (East US) ✔
2. App registration `terraform-contactos` con rol **Contributor** sobre el grupo ✔
3. Storage account `tfstatecontactos1984` con contenedor `tfstate` ✔
4. Rol **Storage Blob Data Contributor** de `terraform-contactos` sobre la storage account
5. **Registrar proveedores** (el Service Principal no puede hacerlo): *Subscriptions → Azure subscription 1 → Settings → Resource providers*, buscar y pulsar **Register** en:
   - `Microsoft.App`
   - `Microsoft.OperationalInsights`

## Secretos de GitHub
`ARM_CLIENT_ID`, `ARM_CLIENT_SECRET`, `ARM_TENANT_ID`, `ARM_SUBSCRIPTION_ID`, `DOCKERHUB_USERNAME`.

## Probar localmente (opcional)
```powershell
az login
cd infra
terraform init
terraform plan -var "docker_image=<usuario>/contactosapi:latest"
```
