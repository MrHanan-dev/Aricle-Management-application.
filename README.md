# Article Management Application

## Overview
This repository contains a full-stack Article Management Application. It includes a backend server, a frontend web interface, and database integration.

---

## Backend
- **Language:** C# (.NET 8)
- **Main Backend Directory:** `ELDLPlatform/Core/`
- **Key Files:**
  - `Server.cs`: Main server logic, HTTP API, user and article management
  - `SVController.cs`, `SVTerminal.cs`, `Terminal.cs`: Supporting server logic
  - `Database/`: Contains database handler and controller classes
- **API:**
  - Exposes endpoints for user authentication, article CRUD, and search
  - Uses `HttpListener` for handling HTTP requests

---

## Frontend
- **Type:** Web-based (HTML/JS)
- **Location:**
  - `WebSide/index.html` (root and inside build folders)
- **Description:**
  - The frontend is a simple HTML page that interacts with the backend API via HTTP requests (AJAX/fetch)
  - Designed for user login, article management, and user management

---

## Database
- **Type:**
  - Uses flat files for user data (`Configs/TestUsers.txt`)
  - Example data in `ELDL.ASSESSMENT 2(DATABASE).json`
  - MongoDB driver is present, but main logic uses file-based storage for users
- **Config Files:**
  - `Configs/DBConfig.txt`, `Configs/WebConfig.txt`, `Configs/TestUsers.txt`

---

## How to Run
1. **Requirements:**
   - .NET 8 SDK (https://dotnet.microsoft.com/en-us/download)
2. **Build and Run:**
   - Open a terminal in the project root
   - Run: `dotnet build ELDLPlatform/ELDLPlatform.csproj`
   - Run: `dotnet run --project ELDLPlatform/ELDLPlatform.csproj`
3. **Access the Application:**
   - By default, the backend listens on the prefixes specified in `Configs/WebConfig.txt`
   - Open `WebSide/index.html` in your browser, or access the server's web interface as configured

---

## Usage
- **Login:** Use credentials from `Configs/TestUsers.txt`
- **User Management:**
  - Add, edit, delete users via the frontend (if you have admin/superadmin rights)
- **Article Management:**
  - Add, edit, delete, and search articles

---

## Notes
- The backend is modular and can be extended to use MongoDB or other databases
- For production, update the config files and secure user data
- For more details, see the `Documentation/` folder

---

## Author
- [Your Name] 