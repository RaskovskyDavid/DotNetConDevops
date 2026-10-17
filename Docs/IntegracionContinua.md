# Integración Continua explicada fácil

## ¿Qué es?
Imagina que tú y tus amigos construyen juntos un castillo de LEGO. Antes de pegar una pieza nueva, un **robot revisor** comprueba que no se caiga nada. Eso es la **Integración Continua (CI)**: cada vez que alguien propone un cambio al código, un robot lo construye y corre las pruebas automáticamente.

## ¿Dónde vive el robot?
En el archivo [.github/workflows/ci.yml](../.github/workflows/ci.yml). GitHub lo lee solo; no hay que instalar nada. Los resultados se ven en la pestaña **Actions** del repositorio.

## ¿Cuándo se activa?
- Cuando abres o actualizas un **Pull Request** hacia `main` (pedir permiso para mezclar tus cambios).
- Cuando alguien sube cambios a `main`.

## ¿Qué hace, paso a paso?
| # | Paso | Analogía |
|---|------|----------|
| 1 | `checkout` | Traer los LEGO a la mesa del robot |
| 2 | `setup-dotnet` | Darle al robot las herramientas (.NET 10) |
| 3 | `dotnet restore` | Descargar las piezas que otras personas hicieron (paquetes) |
| 4 | `dotnet build` | Armar el castillo |
| 5 | `dotnet test` | Empujarlo suavemente para ver si se cae (las 9 pruebas) |
| 6 | `upload-artifact` | Guardar el informe en una carpeta para leerlo luego |

Si algún paso falla, los siguientes no corren y verás una ❌ roja en el Pull Request. Si todo sale bien, ✅ verde.

## Cambios respecto al ejemplo original
| Ejemplo | Aquí | Por qué |
|---------|------|---------|
| `dotnet-version: '8.0'` | `'10.0.x'` | Tu proyecto usa `net10.0` |
| `working-directory: src` | (quitado) | Tu solución `ContactosAPI.slnx` está en la raíz |
| `checkout@v3`, `setup-dotnet@v3` | `@v4` | Versiones más nuevas |
| Solo `pull_request` | + `push` a `main` | Revisar también lo que llega a main |
| Informe `.trx` sin guardar | `upload-artifact` | Para poder descargar el informe desde GitHub |

## Cómo probarlo
1. Haz commit de `.github/workflows/ci.yml` y súbelo (`git push`).
2. Crea una rama, cambia algo y abre un Pull Request hacia `main`.
3. Mira la pestaña **Actions**: verás el robot trabajando.
4. Para ver qué pasa cuando falla, rompe una prueba a propósito (por ejemplo cambia `32` por `33` en un `InlineData`).

## Siguiente paso recomendado
En GitHub: *Settings → Branches → Branch protection rules → main* y marca **Require status checks to pass**. Así nadie podrá mezclar código a `main` si el robot dice ❌.
