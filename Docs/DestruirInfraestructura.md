# Cómo destruir la infraestructura de Azure sin problemas

Idea central: **lo que Terraform creó, lo borra Terraform; lo que creaste a mano, lo borras a mano, y en ese orden.** Si mezclas los dos mundos, el "estado" de Terraform (su memoria) queda desincronizado y aparecen errores.

## 1. Quién creó qué

| Recurso | Lo creó | Cómo se borra |
|---|---|---|
| Container App `ca-contactos` | Terraform | `destroy` |
| Container Apps Environment `cae-contactos` | Terraform | `destroy` |
| Log Analytics `log-contactos` | Terraform | `destroy` |
| Grupo de recursos `TestTerraform` | Tú, en el portal | A mano (al final) |
| Storage account `tfstatecontactos1984` (contiene el estado) | Tú, en el portal | A mano (al final) |
| App registration `terraform-contactos` | Tú, en el portal | A mano (opcional) |

## 2. Orden recomendado (de adentro hacia afuera)

### Paso 1 — Destruir lo de Terraform
En GitHub: **Actions → Infraestructura - Destruir (Terraform) → Run workflow** y escribe `destruir`.
Tarda unos minutos (el Container Apps Environment es lento). Espera a que termine en ✅.

### Paso 2 — Comprobar en el portal
Entra a `TestTerraform → Overview`. Deben haber desaparecido el Container App, el Environment y el Log Analytics. Debe quedar solo la storage account.

### Paso 3 — Decidir qué hacer con lo manual
- **Vas a volver a probar pronto:** deja el grupo, la storage account y la app registration. Cuestan centavos y evitas rehacer la preparación.
- **Terminaste del todo:** borra el grupo `TestTerraform` desde el portal (**Resource groups → TestTerraform → Delete resource group**, escribe el nombre para confirmar). Eso elimina también la storage account y el estado. Después puedes borrar la app registration (**Entra ID → App registrations → terraform-contactos → Delete**) y los secretos `ARM_*` de GitHub.

> Nunca borres la storage account **antes** de hacer `destroy`: Terraform perdería su memoria y no sabría qué borrar.

## 3. Problemas típicos y cómo resolverlos

| Síntoma | Causa | Solución |
|---|---|---|
| `destroy` termina, pero los recursos siguen en el portal | El workflow no corrió (escribiste mal la confirmación) | Revisa que el trabajo no diga *skipped*; vuelve a lanzarlo escribiendo exactamente `destruir` |
| Error `AuthorizationFailed` al hacer `init` o `destroy` | Falta el rol *Storage Blob Data Contributor* o *Contributor* | Revisa **Access control (IAM)** del grupo y de la storage account |
| Error de que un recurso "no se encontró" (`ResourceNotFound`) | Alguien lo borró a mano en el portal | Ver sección 4 |
| Al volver a hacer `apply`, falla el Log Analytics con "workspace is in soft-delete" | Azure conserva 14 días los workspaces borrados y no deja reusar el nombre | Borrado definitivo: `az monitor log-analytics workspace delete -g TestTerraform -n log-contactos --force true --yes`, o cambia el nombre en `main.tf` |
| `destroy` falla con un *lock* | Hay un bloqueo (Delete lock) en el grupo | Quita el bloqueo en **Locks** del grupo y repite |
| `destroy` tarda mucho | Normal con Container Apps Environment (5–15 min) | Espera; no canceles el workflow a la mitad |
| `Error acquiring the state lock` | Un workflow anterior se cortó y dejó el estado bloqueado | Espera unos minutos; si persiste, rompe el bloqueo en el blob del estado (**Container tfstate → contactos.tfstate → Break lease**) |

## 4. Si ya borraste algo a mano (estado desincronizado)

1. Lanza de nuevo el workflow de destroy. Terraform hace un *refresh* y reconoce que ya no existe lo que borraste; en general termina bien.
2. Si insiste en fallar por un recurso que no existe, sácalo de la memoria de Terraform (necesita Terraform y `az login` en tu máquina):
   ```powershell
   cd infra
   terraform init
   terraform state list
   terraform state rm azurerm_container_app.api   # ejemplo
   ```
3. Repite el destroy.

## 5. Lista de verificación final (para no pagar de más)

- [ ] El workflow de destroy terminó en ✅
- [ ] En `TestTerraform` no queda nada salvo lo que decidiste conservar
- [ ] En **Cost Management → Cost analysis** el costo diario baja a cero (puede tardar hasta 24 h en reflejarse)
- [ ] Tienes un **Budget** con alerta (por ejemplo 5 USD) como red de seguridad
- [ ] Si borraste todo: secretos `ARM_*` eliminados en GitHub y app registration borrada

## 6. Costumbre sana

Cada sesión de pruebas sigue el mismo ciclo: **apply → probar la URL → destroy → verificar**. Así el costo es de minutos, no de días.
