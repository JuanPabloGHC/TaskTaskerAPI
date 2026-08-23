# TaskTasker API

API REST para **gestionar las tareas del hogar**: casas, miembros, roles, tareas, asignaciones, estados y logros. Construida en **C# / .NET 8** con el patrón *repository* sobre **PostgreSQL** (Neon).

---

## Índice

- [Los dos clientes: Web vs App](#los-dos-clientes-web-vs-app)
- [Stack](#stack)
- [Puesta en marcha](#puesta-en-marcha)
- [Arquitectura](#arquitectura)
- [Convenciones de respuesta](#convenciones-de-respuesta)
- [Autenticación](#autenticación)
- [Autorización y roles](#autorización-y-roles)
- [Flujo de aprobación de tareas](#flujo-de-aprobación-de-tareas)
- [Referencia de endpoints](#referencia-de-endpoints)
- [Qué consume cada cliente](#qué-consume-cada-cliente)
- [Semilla inicial (obligatoria)](#semilla-inicial-obligatoria)

---

## Los dos clientes: Web vs App

La API sirve a **dos frontends distintos**, con dos identidades y dos ámbitos de permisos. Entender esta separación es lo más importante antes de consumir la API.

| | 🌐 **Plataforma Web** | 📱 **App móvil** |
|---|---|---|
| **Quién** | Staff / administrador de plataforma (externo) | Personas normales (usuarios de casas) |
| **Login** | `POST /api/admin/login` | `POST /api/auth/login` |
| **Identidad en el token** | claim `role: platform_admin` | `sub` = id de la persona (sin rol de plataforma) |
| **Sesión** | Solo *access token* (~2 h). Al expirar, re-login. | *Access token* (15 min) + *refresh token* (7 días) con rotación. |
| **Qué gestiona** | **Catálogo global**: Tasks, Achievements, Status, Roles (CRUD completo) | **Su hogar**: casas, miembros, asignaciones, logros |
| **Sobre el catálogo** | Lee y **escribe** (crear/editar/eliminar) | **Solo lee** (para poder asignar tareas, ver estados, etc.) |
| **Alcance de permisos** | Global (toda la plataforma) | Por casa: se valida su rol (Owner/Admin/Member) en cada operación |

> **Regla mental:** la Web administra *los conceptos* (qué tareas y logros existen en la plataforma). La App usa esos conceptos *dentro de cada casa* (asigna tareas a miembros, marca avances, gana logros).

---

## Stack

- **.NET 8** (ASP.NET Core Web API)
- **Entity Framework Core 9** + **Npgsql** (PostgreSQL)
- **PostgreSQL 17** en Neon (AWS us-east-2)
- **JWT** (`Microsoft.AspNetCore.Authentication.JwtBearer`) + refresh tokens propios
- **Swagger / Swashbuckle** para documentación interactiva
- **DotNetEnv** para variables de entorno

---

## Puesta en marcha

### 1. Variables de entorno (`.env`)

Crea un archivo `.env` en la raíz (está en `.gitignore`, **no se sube**):

```env
DATABASE=Host=<host>;Database=<db>;Username=<user>;Password=<pass>;SSL Mode=VerifyFull;Channel Binding=Require;
JWT_KEY=<clave-secreta-larga-y-aleatoria>
```

- `DATABASE`: cadena de conexión en **formato .NET/Npgsql** (no la URL `postgresql://`).
- `JWT_KEY`: clave para firmar los JWT (usa una cadena larga y aleatoria).

Los tiempos de token y el emisor están en `appsettings.json` (no son secretos):

```json
"Jwt": {
  "Issuer": "http://localhost:5235",
  "Audience": "http://localhost:5235",
  "AccessTokenMinutes": 15,
  "RefreshTokenDays": 7,
  "AdminAccessTokenMinutes": 120
}
```

### 2. Base de datos (EF Migrations)

El esquema es administrado por **EF Migrations** (fuente de verdad). Para crear/actualizar las tablas:

```bash
dotnet ef database update
```

Para añadir un cambio de esquema:

```bash
dotnet ef migrations add <NombreDelCambio>
dotnet ef database update
```

> La herramienta `dotnet-ef` está fijada en `.config/dotnet-tools.json`. Si hiciera falta: `dotnet tool restore`.

### 3. Ejecutar

```bash
dotnet run --launch-profile http
```

- API: `http://localhost:5235`
- Swagger UI: `http://localhost:5235/swagger`
- Perfil HTTPS disponible: `https://localhost:7120`

### 4. Swagger

En `/swagger` puedes probar todo. Para endpoints protegidos:
1. Haz login (`/api/auth/login` o `/api/admin/login`) y copia el `accessToken`.
2. Botón **Authorize** → pega el token (sin `Bearer`) → Authorize.

---

## Arquitectura

```
Controllers/        Endpoints HTTP. Los de la app heredan de AppControllerBase.
DAL/
  Context/          TaskTaskerContext (DbContext) + configuración de relaciones.
  Entities/         Entidades EF (tablas).
  DTOs/             Objetos de entrada/salida de la API.
  Interfaces/       Contratos de repositorios y servicios.
  Repositories/     Acceso a datos (patrón repository) + validaciones de dominio.
Services/           TokenService (emisión de JWT / refresh tokens).
Middleware/         ExceptionHandlingMiddleware (manejo central de errores).
Utilities/          ApiResponse, ApiException, PagedResult, Validate, helpers.
Migrations/         Migraciones EF.
```

- **Inyección de dependencias** en todo el árbol de repositorios (sin `new` manual).
- **Errores centralizados**: los controladores no llevan `try/catch`; lanzan `ApiException` y el middleware los traduce.

---

## Convenciones de respuesta

### Envoltorio estándar

Casi todas las respuestas usan `ApiResponse<T>` (JSON en camelCase):

```json
{
  "statusCode": 200,
  "message": "Login successful",
  "data": { }
}
```

### Errores

- **Errores de negocio** (validaciones, permisos, no encontrado): mismo envoltorio, con `data: ""`.
  ```json
  { "statusCode": 404, "message": "Home not found", "data": "" }
  ```
- **Error inesperado**: `500` con `message: "Internal server error"` (sin filtrar detalles internos).
- **Validación de modelo** (campos requeridos en el body de auth): **no** usa el envoltorio; devuelve el formato estándar de ASP.NET `ValidationProblemDetails`:
  ```json
  { "type": "...", "title": "One or more validation errors occurred.", "status": 400, "errors": { "name": ["The name field is required."] } }
  ```
- **401 / 403 por token** (falta token o rol): los genera el framework, con **cuerpo vacío** (solo el status).

> **Para el frontend:** maneja tres formas de error → (a) envoltorio `ApiResponse` con `message`, (b) `ValidationProblemDetails` con `errors`, (c) 401/403 sin cuerpo.

### Paginación

Los endpoints de **listas que crecen** devuelven `PagedResult<T>` dentro de `data`:

```json
{
  "statusCode": 200,
  "message": "",
  "data": {
    "items": [ ],
    "page": 1,
    "pageSize": 20,
    "total": 57,
    "totalPages": 3
  }
}
```

Aceptan `?page=1&pageSize=20` (pageSize se limita a 1–100). Ordenados por fecha descendente.

---

## Autenticación

### Persona (App)

```
POST /api/auth/login   { "name": "...", "password": "..." }
→ { accessToken, refreshToken, person }
```

- Usa el `accessToken` en el header: `Authorization: Bearer <accessToken>`.
- Cuando expire (15 min), pide uno nuevo:
  ```
  POST /api/auth/refresh   { "refreshToken": "..." }
  → { accessToken, refreshToken, person }   // nuevo par; el refresh viejo queda revocado (rotación)
  ```
- Cerrar sesión:
  ```
  POST /api/auth/logout   { "refreshToken": "..." }
  ```
- Registro:
  ```
  POST /api/person/signup   { PersonDTO }   // anónimo
  ```

### Admin de plataforma (Web)

```
POST /api/admin/login   { "username": "...", "password": "..." }
→ { accessToken, username }   // token con claim platform_admin, ~2 h, SIN refresh
```

**Bootstrap del primer admin:** como la tabla de admins arranca vacía, el **primer** `POST /api/admin/create` funciona **sin token**. A partir de ahí, crear más admins exige un token con `platform_admin`.

```
POST /api/admin/create   { "username": "...", "password": "..." }
```

- `username` ≥ 3 caracteres, `password` ≥ 6.

---

## Autorización y roles

Hay **dos niveles** de autorización:

1. **Rol de plataforma** (claim `platform_admin` en el token del admin) → protege el catálogo global.
2. **Rol dentro de la casa** (`Owner` / `Admin` / `Member`) → guardado en la tabla `Member` y validado **en cada request** (una persona puede ser Owner en una casa y Member en otra). La identidad del que llama **siempre sale del token**, nunca de un id en la URL.

### Matriz de permisos (App)

| Acción | Owner | Admin | Member |
|---|:---:|:---:|:---:|
| Ver miembros / historial / logros de la casa | ✅ | ✅ | ✅ |
| Crear asignaciones, añadir miembros | ✅ | ✅ | ❌ |
| **Aprobar** una tarea a "Done" | ✅ | ✅ | ❌ |
| Editar/eliminar miembros, editar/eliminar la casa | ✅ | ❌ | ❌ |
| Cambiar el estado de **su propia** asignación (excepto "Done") | — | — | ✅ (solo el asignado) |
| Editar / eliminar su propio perfil | cualquier persona autenticada, solo sobre sí misma |

> El creador de una casa (`POST /api/home/create`) se convierte automáticamente en **Owner**.

---

## Flujo de aprobación de tareas

El estado de una asignación vive en el catálogo `Status`. El ciclo esperado:

1. El **miembro** realiza la tarea y cambia su asignación a **"En revisión"** (`PATCH /api/assignment/change-status/{id}`). No puede ponerla en **"Done"** directamente → recibe `403`.
2. El **Owner/Admin** aprueba (`PATCH /api/assignment/approve/{id}`): el estado pasa a **"Done"** y se otorgan automáticamente los logros (`Attainment`) que el miembro haya alcanzado para esa tarea.

> Requiere que existan en el catálogo los estados **"En revisión"** y **"Done"** (los crea la Web). Los logros se otorgan según `Achievement.days` (nº de veces que la tarea se completó) vs. las tareas ya hechas.

---

## Referencia de endpoints

**Base:** `http://localhost:5235`
Acceso: 🔓 anónimo · 🔒 autenticado (cualquier token) · 👤 dueño del recurso · 🏠 rol de casa · 🛡️ `platform_admin`

### Auth — `/api/auth` (personas / App)

| Método | Ruta | Acceso | Descripción |
|---|---|---|---|
| POST | `/login` | 🔓 | Devuelve access + refresh + persona |
| POST | `/refresh` | 🔓 | Rota el refresh token y devuelve un par nuevo |
| POST | `/logout` | 🔓 | Revoca el refresh token |

### Admin — `/api/admin` (plataforma / Web)

| Método | Ruta | Acceso | Descripción |
|---|---|---|---|
| POST | `/login` | 🔓 | Devuelve access token con `platform_admin` |
| POST | `/create` | 🔓 primero, luego 🛡️ | Crea admin (bootstrap si no hay admins) |
| GET | `/get-all` | 🛡️ | Lista admins (`{ id, username }`, sin password) |
| DELETE | `/delete/{id}` | 🛡️ | Elimina un admin |

### Person — `/api/person` (App)

| Método | Ruta | Acceso | Descripción |
|---|---|---|---|
| POST | `/signup` | 🔓 | Registrar persona |
| GET | `/me` | 🔒 | Perfil propio (del token) |
| PATCH | `/update/{id}` | 👤 | Editar el propio perfil |
| DELETE | `/delete/{id}` | 👤 | Eliminar la propia cuenta |

### Home — `/api/home` (App)

| Método | Ruta | Acceso | Descripción |
|---|---|---|---|
| GET | `/mine` | 🔒 | Casas del usuario **con su rol en cada una** |
| GET | `/get/{id}` | 🏠 miembro | Detalle de una casa |
| POST | `/create` | 🔒 | Crear casa (el creador queda como **Owner**) |
| PATCH | `/update/{id}` | 🏠 Owner/Admin | Editar casa |
| DELETE | `/delete/{id}` | 🏠 Owner | Eliminar casa |

### Member — `/api/member` (App)

| Método | Ruta | Acceso | Descripción |
|---|---|---|---|
| GET | `/get-all/{homeId}` | 🏠 miembro | Miembros de la casa |
| GET | `/get/{memberId}` | 🏠 mismo hogar / sí mismo | Detalle del miembro (incluye sus logros y asignaciones) |
| POST | `/create` | 🏠 Owner/Admin | Añadir miembro (`person{id}`, `home{id}`, `role{id}`) |
| PATCH | `/update/{memberId}` | 🏠 Owner | Cambiar el rol de un miembro |
| DELETE | `/remove/{memberId}` | 🏠 Owner | Quitar miembro |

### Assignment — `/api/assignment` (App)

| Método | Ruta | Acceso | Descripción |
|---|---|---|---|
| GET | `/get-all/member/{memberId}` | 🏠 sí mismo / Owner/Admin | Asignaciones de un miembro · **paginado** · `?undone=true` filtra las no terminadas |
| GET | `/get-all/home/{homeId}` | 🏠 miembro | Historial de la casa · **paginado** |
| POST | `/create` | 🏠 Owner/Admin | Crear asignación (`member{id}`, `task{id}`, `status{id}`, `date`) |
| PATCH | `/update/{assignmentId}` | 🏠 Owner/Admin | Editar asignación |
| PATCH | `/change-status/{assignmentId}` | 🏠 el asignado | Cambiar estado (no puede ser "Done") |
| PATCH | `/approve/{assignmentId}` | 🏠 Owner/Admin | Aprobar → "Done" + otorga logros |
| DELETE | `/delete/{assignmentId}` | 🏠 Owner/Admin | Eliminar (no si está "Done") |

### Attainment — `/api/attainment` (App)

| Método | Ruta | Acceso | Descripción |
|---|---|---|---|
| GET | `/member/{memberId}` | 🏠 sí mismo / Owner/Admin | Logros de un miembro |
| GET | `/home/{homeId}` | 🏠 miembro | Logros de toda la casa (con quién los ganó) · **paginado** |

### Catálogo — `/api/task`, `/api/achievement`, `/api/status`, `/api/role` (Web escribe, App lee)

Los cuatro comparten la misma forma:

| Método | Ruta | Acceso | Descripción |
|---|---|---|---|
| GET | `/get-all` | 🔒 | Listar |
| GET | `/get/{id}` | 🔒 | Obtener por id |
| POST | `/create` | 🛡️ | Crear |
| PATCH | `/update/{id}` | 🛡️ | Editar |
| DELETE | `/delete/{id}` | 🛡️ | Eliminar (**409 si está en uso**, ver abajo) |

> **Borrado con referencias:** eliminar un elemento de catálogo que ya está en uso devuelve **`409`** y no borra nada (protege el historial):
> - `Task` en uso por asignaciones o logros · `Status` en uso por asignaciones · `Role` en uso por miembros · `Achievement` ya obtenido por algún miembro.
>
> Mensaje: `"<Recurso> is in use and cannot be deleted"`. En la UI, muestra ese mensaje y bloquea el borrado.

---

## Qué consume cada cliente

**🌐 Web (panel de administración)**
- `POST /api/admin/login` (y `POST /api/admin/create` para el primer admin).
- CRUD de **catálogo**: `/api/task`, `/api/achievement`, `/api/status`, `/api/role`.
- Gestión de admins (`/api/admin/delete/{id}`).
- Sesión: solo access token; al expirar (~2 h), re-login.

**📱 App (móvil)**
- `POST /api/person/signup`, `POST /api/auth/login`, `/refresh`, `/logout`.
- `/api/home`, `/api/member`, `/api/assignment`, `/api/attainment`.
- **Lectura** del catálogo (`GET /get-all`, `GET /get/{id}`) para poder asignar tareas, elegir estados, mostrar logros.
- Sesión: guarda `refreshToken` de forma segura y renueva el `accessToken` de forma silenciosa.

---

## Semilla inicial

El catálogo base se **siembra automáticamente al arrancar** la API (`DbSeeder`, idempotente: solo inserta lo que falta, es seguro en cada arranque). Incluye:

- **Roles**: `Owner`, `Admin`, `Member`.
- **Estados**: `Pending`, `En revisión`, `Done`.
- **Tasks** de ejemplo: Barrer, Trapear, Lavar trastes, Sacar basura, Tender cama, Lavar ropa.
- **Achievements** de ejemplo: Limpiador (5× Barrer), Trapeador experto (5× Trapear), Rey de la cocina (10× Lavar trastes).

Las imágenes se generan como **placeholders base64 (SVG)**; reemplázalas desde el panel web.

Lo **único manual** es crear el **primer admin de plataforma** (bootstrap), ya que el seed no crea admins por seguridad:

```bash
curl -X POST http://localhost:5235/api/admin/create \
  -H "Content-Type: application/json" \
  -d '{"username":"TU_USUARIO","password":"TU_PASSWORD"}'
```

> Los nombres de roles (`Owner`/`Admin`/`Member`) y estados (`En revisión`/`Done`) son sensibles: la lógica de permisos, la creación de casas y el flujo de aprobación dependen de ellos.
