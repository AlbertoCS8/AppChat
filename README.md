# AppChat

**Aplicación de chat en tiempo real** desarrollada con **Blazor WebAssembly (Hosted)** y base de datos **MongoDB** local.

---

## 🚀 Requisitos previos

### 1. Instalar [.NET 9.0 SDK (Preview)](https://dotnet.microsoft.com/en-us/download/dotnet/9.0)
- Asegúrate de que esté en el `PATH` y disponible desde la terminal:
  ```bash
  dotnet --version
  ```

### 2. Instalar MongoDB
- Puedes usar:
  - MongoDB **local** (recomendado con MongoDB Compass)
  - **MongoDB Atlas** (opcional, en la nube)

> Si usas Atlas, edita el archivo de configuración para establecer tu cadena de conexión personalizada:

```jsonc
// pruebaMudBlazor/pruebaMudBlazor/appsettings.json
"ConnectionStrings": {
  "MongoDb": "TU_CADENA_DE_CONEXIÓN"
}
```

---

## 🛠️ Cómo ejecutar la app

1. Abre una terminal y navega al proyecto del servidor:

   ```bash
   cd ./pruebaMudBlazor/
   ```

2. Las dependencias estan en el proyecto pero si tuviste problemas con dependencias, ejecuta:

   ```bash
   dotnet build
   ```

3. Inicia la aplicación:

   ```bash
   dotnet run
   ```

4. Abre tu navegador en:

   ```
   http://localhost:5235
   ```

---

## 📁 Estructura del Proyecto

- `pruebaMudBlazor/`  
  Contiene el **servidor (API)** y el **cliente (Blazor WebAssembly)**.
  Este es un proyecto **Blazor Hosted**, por lo que el front y el back se ejecutan desde el mismo comando.

---

## 📦 Dependencias

Ya están incluidas en el repositorio.  
Si por alguna razón falla algo, ejecuta:

```bash
dotnet restore
```

---

## ✨ Créditos

Proyecto realizado como sistema de mensajería en tiempo real para prácticas personales con tecnologías modernas (.NET, Blazor, MongoDB).

---
