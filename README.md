# PlatformOps & Product Feedback Hub — Backend API

[![.NET](https://img.shields.io/badge/.NET-10.0-512BD4?logo=dotnet&logoColor=white)](https://dotnet.microsoft.com/)
[![C#](https://img.shields.io/badge/C%23-14.0-239120?logo=csharp&logoColor=white)](https://docs.microsoft.com/dotnet/csharp/)
[![AWS Lambda](https://img.shields.io/badge/AWS-Lambda-FF9900?logo=awslambda&logoColor=white)](https://aws.amazon.com/lambda/)
[![AWS Cognito](https://img.shields.io/badge/AWS-Cognito-FF4F8B?logo=amazoncognito&logoColor=white)](https://aws.amazon.com/cognito/)
[![Amazon RDS MySQL](https://img.shields.io/badge/MySQL-8.4_LTS-4479A1?logo=mysql&logoColor=white)](https://www.mysql.com/)
[![Swagger](https://img.shields.io/badge/Swagger-OpenAPI_3.0-85EA2D?logo=swagger&logoColor=black)](https://swagger.io/)
[![Clean Architecture](https://img.shields.io/badge/Architecture-Clean_Architecture-brightgreen)](#arquitectura-clean-architecture)

Backend monolítico serverless de alta disponibilidad para **OpsHub (*TuNegocio.pe*)**, diseñado bajo **Clean Architecture** y principios **SOLID** en **.NET 10 (C#)**, desplegado en **AWS Lambda** tras **AWS API Gateway (HTTP API v2)** con base de datos relacional en **Amazon RDS MySQL 8.4 LTS** y gestión de identidades con **AWS Cognito**.

---

## 🏛 Arquitectura: Clean Architecture

El proyecto sigue estrictamente la Regla de Dependencia de Clean Architecture. Las capas de negocio (`Domain` y `Application`) son totalmente independientes de frameworks externos, bases de datos o servicios en la nube:

```text
projects/backend/
├── database/
│   └── schema_opshub_db.sql         # Script DDL completo MySQL 8.4 (30 tablas + seeds)
├── docs/
│   ├── ESPECIFICACION_BACKEND_DOTNET.md # Especificación técnica y manual de defensa
│   └── ROADMAP_BITACORA_BACKEND.md     # Bitácora de desarrollo y fases
├── src/
│   ├── OpsHub.Domain/               # Núcleo del dominio (Entidades, Enums, Excepciones)
│   ├── OpsHub.Application/          # Casos de uso (Vertical Slices, DTOs, Validadores)
│   ├── OpsHub.Infrastructure/       # Persistencia (EF Core, MySQL, AWS SDK, Cognito, S3)
│   └── OpsHub.Api/                  # Web API (Controladores REST, Middlewares, Lambda Hosting)
├── Dockerfile                       # Contenedor para despliegues en ECS / Fargate / Docker
├── deploy-lambda.ps1                # Script automatizado de empaquetado y despliegue AWS
└── OpsHub.Backend.slnx              # Solución principal en formato XML moderno .NET 10
```

```
┌────────────────────────────────────────────────────────────────────────┐
│                   4. OpsHub.Api (Presentation / Lambda)               │
│                                   │                                    │
│   ┌───────────────────────────────┴────────────────────────────────┐   │
│   │             3. OpsHub.Infrastructure (MySQL, AWS SDK)          │   │
│   │                               │                                │   │
│   │   ┌───────────────────────────┴────────────────────────────┐   │   │
│   │   │         2. OpsHub.Application (Use Cases / DTOs)        │   │   │
│   │   │                           │                            │   │   │
│   │   │   ┌───────────────────────┴────────────────────────┐   │   │   │
│   │   │   │         1. OpsHub.Domain (Entities / Enums)    │   │   │   │
│   │   │   └────────────────────────────────────────────────┘   │   │   │
│   │   └────────────────────────────────────────────────────────┘   │   │
│   └────────────────────────────────────────────────────────────────┘   │
└────────────────────────────────────────────────────────────────────────┘
```

---

## 🚀 Patrones Avanzados de Ingeniería

### 1. Transacción Compensatoria (Saga Pattern) con AWS Cognito
Al registrar un usuario, se desacopla la creación en AWS Cognito User Pool y la base de datos relacional MySQL:
1. **Pre-validación en MySQL:** Email único, departamento y rol activos.
2. **Aprovisionamiento en AWS Cognito:** Creación mediante `AdminCreateUser` con contraseña permanente, retornando el identificador `cognito_sub`.
3. **Persistencia en MySQL:** Se guarda en la tabla `usuarios_internos`.
4. **Rollback Compensatorio:** Si MySQL falla por timeout, concurrencia o caída, el bloque `catch` ejecuta automáticamente:
   ```csharp
   await _cognitoAuthService.DeleteUserAsync(request.Email, CancellationToken.None);
   ```
   Eliminando de inmediato el usuario en Cognito para garantizar **cero registros huérfanos**.

### 2. Bloqueo Optimista (Optimistic Locking) en Tickets
Para evitar sobreescritura accidental de notas o estados de incidencias (*Lost Update Problem*), la tabla `tickets` posee un token de concurrencia `version INT NOT NULL`. Si dos agentes actualizan el mismo ticket simultáneamente, la sentencia SQL atómica afecta 0 filas y el backend emite **`HTTP 409 Conflict`**, protegiendo la integridad de la información.

### 3. Carga Eficiente de Adjuntos (S3 Pre-signed URLs)
Para no saturar el payload de AWS API Gateway (límite 10 MB) ni agotar la memoria de Lambda con archivos multimedia pesados, el backend genera URLs prefirmadas de Amazon S3 firmadas criptográficamente con validez de 5 minutos, permitiendo que la App Móvil o Web suban el archivo directamente a S3 con verbo `PUT`.

### 4. Trazabilidad Inmutable (`AuditLogsInterceptor`)
Interceptor automático de Entity Framework Core (`AuditLogsInterceptor`) que captura cada operación `INSERT`, `UPDATE` o `DELETE` sobre tablas operativas, guardando en `logs_actividad` el usuario, IP, plataforma y los estados en JSON (`valores_anteriores_json` y `valores_nuevos_json`) con enmascaramiento automático de credenciales.

### 5. Onboarding Seguro Multiplataforma (OWASP Top 1: Anti-Escalación de Privilegios)
- **Catálogo Público de Departamentos:** `GET /api/v1/catalogs/departamentos` permite poblar combos de registro sin requerir token JWT previo.
- **Desacoplamiento de Roles:** En `POST /api/v1/auth/register`, el usuario solo elige su departamento; el servidor asigna automáticamente el rol base correspondiente (`DEV_INGENIERO`, `EJECUTIVO_VENTAS`, `AGENTE_SOPORTE`), evitando que usuarios anónimos se autoasignen privilegios de Administrador.
- **Endpoint de Sesión `GET /api/v1/auth/me`:** Resuelve la identidad y devuelve la matriz de permisos RBAC del usuario conectado para restaurar sesión tras recargar la página (F5) o abrir la app móvil.

---

## 🛠 Requisitos Previos

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- [MySQL 8.4 LTS](https://dev.mysql.com/downloads/) o instancia Amazon RDS MySQL
- [AWS CLI v2](https://aws.amazon.com/cli/) (opcional, para despliegues)
- [PowerShell 7+](https://github.com/PowerShell/PowerShell)

---

## ⚙️ Configuración y Ejecución Local

### 1. Clonar el Repositorio
```bash
git clone https://github.com/Eduardo25042103/backend_PlatformOps-ProductFeedbackHub.git
cd backend_PlatformOps-ProductFeedbackHub
```

### 2. Base de Datos MySQL
Ejecutar el script DDL con las 30 tablas y semillas iniciales:
```bash
mysql -u root -p < database/schema_opshub_db.sql
```

### 3. Configurar Secretos de Desarrollo
Usar `dotnet user-secrets` para no exponer contraseñas en código:
```bash
dotnet user-secrets init --project src/OpsHub.Api
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Server=localhost;Port=3306;Database=opshub_db;Uid=root;Pwd=TuPassword123!;" --project src/OpsHub.Api
dotnet user-secrets set "JwtSettings:SecretKey" "Clave_Secreta_Super_Segura_OpsHub_2026_Min_32_Chars!" --project src/OpsHub.Api
```

### 4. Compilar y Ejecutar
```bash
# Compilar la solución completa
dotnet build OpsHub.Backend.slnx

# Ejecutar el servidor API
dotnet run --project src/OpsHub.Api
```

Acceder a Swagger UI:
👉 **`http://localhost:5183/swagger`**

---

## ☁️ Despliegue en AWS Lambda (Serverless)

El proyecto incluye el script [deploy-lambda.ps1](file:///d:/react/proyecto-sexto-ciclo/projects/backend/deploy-lambda.ps1) para empaquetar de forma self-contained para Amazon Linux 2023 (`provided.al2023` / `linux-x64`):

```powershell
# 1. Compilar y generar paquete ZIP optimizado para Lambda
.\deploy-lambda.ps1 -Action Package

# 2. Cargar en S3 y actualizar la función Lambda
aws s3 cp bin/opshub-backend-lambda.zip s3://tu-bucket-deploy/opshub-backend-lambda.zip --profile idat-cli --region us-east-2
aws lambda update-function-code --function-name opshub-backend-prod --s3-bucket tu-bucket-deploy --s3-key opshub-backend-lambda.zip --profile idat-cli --region us-east-2
```

---

## 📡 Endpoints Principales

| Módulo | Método | Endpoint | Acceso | Descripción |
| :--- | :---: | :--- | :---: | :--- |
| **Diagnóstico** | `GET` | `/api/v1/health` | Público | Estado de la API y conectividad con MySQL |
| **Documentación**| `GET` | `/swagger` | Público | Interfaz interactiva Swagger / OpenAPI 3.0 |
| **Auth** | `POST` | `/api/v1/auth/login` | Público | Autenticación híbrida (Cognito / BCrypt) + JWT |
| **Auth** | `POST` | `/api/v1/auth/register` | Público | Registro de usuario con Transacción Compensatoria |
| **Auth** | `GET` | `/api/v1/auth/me` | Protegido | Identidad del usuario actual y permisos RBAC |
| **Catálogos** | `GET` | `/api/v1/catalogs/departamentos` | Público | Listado organizacional para onboarding |
| **Catálogos** | `GET` | `/api/v1/catalogs/prioridades` | Público | Catálogo de prioridades de tickets (P1..P4) |
| **Catálogos** | `GET` | `/api/v1/catalogs/estados-ticket` | Público | Estados de tickets (Abierto, En Progreso, etc.) |
| **Tickets** | `GET` | `/api/v1/tickets` | Protegido | Listado de incidencias con filtros avanzados |
| **Tickets** | `POST` | `/api/v1/tickets` | Protegido | Registro de nueva incidencia técnica |
| **Tickets** | `PATCH`| `/api/v1/tickets/{id}/status` | Protegido | Cambio de estado con Bloqueo Optimista |
| **Tickets** | `POST` | `/api/v1/tickets/{id}/escalate`| Protegido | Escalación formal a equipo de desarrollo |
| **Tickets** | `POST` | `/api/v1/tickets/{id}/attachments/presigned-url` | Protegido | Generación de URL prefirmada de Amazon S3 |
| **Feedback** | `GET` | `/api/v1/feedback/sugerencias` | Protegido | Solicitudes de mejora con puntaje RICE |
| **Feedback** | `POST` | `/api/v1/feedback/sugerencias/{id}/votar` | Protegido | Voto comercial con cálculo ARR |
| **Integraciones**| `POST` | `/api/v1/integrations/zoho/webhook` | Firma HMAC | Recepción webhook Zoho CRM |
| **Usuarios** | `GET` | `/api/v1/users` | Admin | Directorio corporativo de colaboradores |
| **Usuarios** | `GET` | `/api/v1/users/roles` | Admin | Matriz de roles y permisos RBAC |

---

## 👥 Equipo y Créditos Académicos

- **Institución:** Instituto de Educación Superior Privado **IDAT**
- **Carrera:** Desarrollo de Sistemas de Información
- **Ciclo:** VI Ciclo
- **Proyecto:** Proyecto de Aplicación Empresarial — *PlatformOps & Product Feedback Hub*
- **Empresa Caso:** *TuNegocio.pe*
