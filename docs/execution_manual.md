# Step-by-Step Guide to Run the Project - IT Inventory Control

This document contains the complete guide with all the steps needed to set up and run the **IT Inventory Control** project (ASP.NET Core 10 Backend + React Vite Frontend + SQL Server Database).

---

## 1. Prerequisites

Make sure you have the following components installed in your environment (Linux or WSL2):

- **.NET 10 SDK**
- **Node.js** (version 20+ recommended) and **npm**
- **Docker** and **Docker Compose** (to run SQL Server)

---

## 2. Step 1: Start the Database (SQL Server Docker)

1. Go to the `docker/` directory at the project root:
   
   ```bash
   cd docker
   ```

2. Create the `.env` file if it hasn't been created yet, or edit the existing one:
   
   ```bash
   # creating the file
   touch .env
   
   # opening the file in the nano text editor
   nano .env
   ```

3. Make sure the `sa` user password is set in the `docker/.env` file:
   
   ```env
   MSSQL_SA_PASSWORD=StrongDatabasePassword13!
   ```

4. Start the SQL Server container in the background:
   
   ```bash
   docker compose up -d
   ```

---

## 3. Step 2: Configure the Backend User Secrets

The backend does not store credentials and JWT keys in plain text in the repository. Therefore, the variables must be registered in the .NET secrets manager (`user-secrets`).

1. Go to the API project folder:
   
   ```bash
   cd backend/Inventory.Api
   ```

2. Run the commands below to set the connection string, JWT keys, and seed user passwords:
   
   ```bash
   dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Server=localhost,1433;Database=InventoryDb;User Id=sa;Password=StrongDatabasePassword13!;TrustServerCertificate=True"
   dotnet user-secrets set "Jwt:Key" "YourSuperSecureJWTSecretKey123456!"
   dotnet user-secrets set "Jwt:Issuer" "InventoryApi"
   dotnet user-secrets set "Jwt:Audience" "InventoryFrontend"
   dotnet user-secrets set "Seed:SenhaAdmin" "StrongAdminPassword123!"
   dotnet user-secrets set "Seed:SenhaUsuarioTeste" "TestPassword1234!"
   ```

3. (Optional) To check the registered keys:
   
   ```bash
   dotnet user-secrets list
   ```

---

## 4. Step 3: Install the Frontend Dependencies

1. Go to the frontend directory at the project root:
   
   ```bash
   cd frontend
   ```

2. Install the dependencies with npm:
   
   ```bash
   npm install
   ```

---

## 5. Step 4: Handle the HTTPS Certificate in the Browser (Firefox / Chrome)

The backend runs on **HTTPS** by default on port **5443** (`https://localhost:5443`), using a self-signed .NET SSL certificate.

To prevent the browser from blocking API (`fetch`) requests coming from the frontend (`http://localhost:5173`):

1. Open the browser (Firefox) and go directly to:
   
   ```text
   https://localhost:5443
   ```

2. When the security warning screen loads (*"Warning: Potential Security Risk Ahead"*), click **Advanced...** and then **Accept the Risk and Continue**.

3. *Alternative*: If you'd rather not use HTTPS in local development, change the `frontend/.env` file to point to the HTTP port:
   
   ```env
   VITE_API_URL=http://localhost:5080
   ```

---

## 6. Step 5: Running the Application (2 Separate Terminals)

The Backend and Frontend must be run simultaneously in two separate terminals:

### Terminal 1: Backend (ASP.NET Core API)

```bash
cd backend/Inventory.Api
dotnet run
```

*On first startup, the API will automatically create the `InventoryDb` database, apply the migrations, and seed the initial data.*

### Terminal 2: Frontend (React + Vite)

```bash
cd frontend
npm run dev
```

---

## Common Troubleshooting Summary

| Symptom | Cause | Solution |
|:------------------------------------------------------------------------------------------ |:----------------------------------------------------------------------------- |:------------------------------------------------------------------------------------------------------------- |
| `System.InvalidOperationException: ConnectionString property is not initialized` | The `ConnectionStrings:DefaultConnection` key was not saved in `user-secrets`. | Run `dotnet user-secrets set "ConnectionStrings:DefaultConnection" "..."` in the `backend/Inventory.Api` folder. |
| `vite` commands or packages not found in the frontend | Missing `node_modules` folder. | Run `npm install` in the `frontend` folder. |
| CORS Errors/Connection Failure in Firefox | Backend's self-signed SSL certificate not accepted by the browser. | Visit `https://localhost:5443` in Firefox once and accept the security exception. |
| HTTP Status `304 Not Modified` in DevTools | Normal browser response indicating cache usage. | **Not an error.** No action required. |
