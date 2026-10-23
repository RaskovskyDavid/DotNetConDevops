# Los 3 comandos principales de Terraform

Imagina que quieres construir una casa de LEGO siguiendo un plano:

1. **`terraform init`**: preparar la mesa y traer las piezas.
2. **`terraform plan`**: mirar el plano y ver qué piezas faltan, sin tocar nada.
3. **`terraform apply`**: construir de verdad lo que dice el plano.

Siempre se usan en ese orden. Se ejecutan dentro de la carpeta `infra/`.

---

## 1. `terraform init` — Preparar

**Qué hace**
- Descarga el *proveedor* de Azure (`azurerm`), el "traductor" que permite a Terraform hablar con Azure.
- Se conecta al *backend*: la storage account `tfstatecontactos1984`, donde Terraform guarda su memoria (el estado).
- Crea una carpeta local `.terraform/` con lo descargado.

**Cuándo se usa**
- La primera vez en una carpeta o máquina nueva.
- Cada vez que cambias de proveedor, de versión o de `backend.tf`.
- En GitHub Actions corre siempre al inicio, porque cada ejecución empieza en una computadora vacía.

**Qué NO hace:** no crea ni cambia nada en Azure. Es seguro repetirlo.

```powershell
cd infra
terraform init
```

Éxito: `Terraform has been successfully initialized!`

**Errores comunes**
| Mensaje | Causa |
|---|---|
| `AuthorizationFailed` / `403` | Falta el rol *Storage Blob Data Contributor* en la storage account |
| `storage account not found` | El nombre en `backend.tf` no coincide con el real |
| `Failed to query available provider packages` | Sin internet |

---

## 2. `terraform plan` — Ver qué pasaría

**Qué hace**
Compara tres cosas:
1. Lo que **escribiste** en los archivos `.tf` (lo que quieres).
2. Lo que Terraform **recuerda** en el estado.
3. Lo que **existe de verdad** en Azure.

Y te muestra la diferencia, con símbolos:

| Símbolo | Significado |
|---|---|
| `+` verde | Se va a **crear** |
| `~` amarillo | Se va a **modificar** |
| `-` rojo | Se va a **borrar** |
| `-/+` | Se va a **borrar y volver a crear** (¡atención!) |

Al final resume: `Plan: 3 to add, 0 to change, 0 to destroy.`

**Qué NO hace:** no modifica nada en Azure. Es el comando de "mirar antes de saltar" y es seguro ejecutarlo cuantas veces quieras.

```powershell
terraform plan -var "docker_image=<usuario>/contactosapi:latest"
```

**Buena costumbre:** lee el resumen. Si esperabas crear 3 recursos y dice `5 to destroy`, algo anda mal: detente antes del `apply`.

**En este proyecto:** el workflow `terraform.yml` ejecuta `plan` en cada Pull Request, para que veas el efecto del cambio **antes** de mezclarlo.

---

## 3. `terraform apply` — Construir

**Qué hace**
- Ejecuta el plan: crea, modifica o borra recursos en Azure.
- Actualiza el estado para recordar lo que hizo.
- Muestra los *outputs* al final (en nuestro caso, `api_url`).

**Cómo se usa**
```powershell
terraform apply -var "docker_image=<usuario>/contactosapi:latest"
```
Terraform muestra el plan otra vez y pide escribir `yes`. Si dices otra cosa, no hace nada.

En GitHub Actions no hay quien escriba `yes`, así que se guarda el plan en un archivo y se aplica exactamente ese:
```bash
terraform plan -out=tfplan
terraform apply -auto-approve tfplan
```
Así lo que se aplica es **lo mismo que se revisó**, sin sorpresas.

**Es idempotente:** si lo ejecutas dos veces seguidas sin cambiar nada, la segunda dice `No changes` y no hace nada. Terraform solo toca lo que difiere.

**Éxito:**
```
Apply complete! Resources: 3 added, 0 changed, 0 destroyed.
Outputs:
api_url = "https://ca-contactos....azurecontainerapps.io/weatherforecast"
```

**Errores comunes**
| Mensaje | Causa |
|---|---|
| `MissingSubscriptionRegistration` | Falta registrar `Microsoft.App` o `Microsoft.OperationalInsights` en la suscripción |
| `AuthorizationFailed` | El Service Principal no tiene rol *Contributor* sobre el grupo |
| Imagen no encontrada | `<usuario>/contactosapi:latest` no existe aún en Docker Hub |

---

## Resumen visual

```
 terraform init    →  prepara (proveedor + estado)        no toca Azure
 terraform plan    →  muestra qué cambiaría               no toca Azure
 terraform apply   →  lo hace de verdad                   SÍ cambia Azure
 terraform destroy →  lo borra (ver DestruirInfraestructura.md)
```

## Comandos de apoyo útiles

| Comando | Para qué |
|---|---|
| `terraform fmt` | Ordena y alinea el código de los `.tf` |
| `terraform validate` | Revisa que la sintaxis esté bien (sin conectar a Azure) |
| `terraform output` | Muestra de nuevo los valores de salida, como la URL |
| `terraform state list` | Lista lo que Terraform recuerda haber creado |

## Cómo se ve en el flujo de este proyecto

```
Pull Request  →  fmt → init → validate → plan        (solo mirar)
Merge a main  →  fmt → init → validate → plan → apply (construir)
Manual        →  workflow "Destruir" → init → destroy (recoger)
```
