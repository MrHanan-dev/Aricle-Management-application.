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


## User Roles & Functionality

After logging in, users experience different functionalities based on their assigned role:

### Superadmin
- Full access to all features
- Can add, edit, and delete any user (including Admins)
- Can manage all articles
- Can view and search all users and articles

### Admin
- Can add, edit, and delete users (except Superadmins)
- Can manage all articles
- Can view and search all users and articles

### Tutor
- Can view and search articles
- Can add and edit articles
- Cannot manage users

### Student
- Can view and search articles
- Cannot add, edit, or delete articles
- Cannot manage users

**Note:** The actual permissions are enforced in the backend logic (see `Server.cs`). The frontend adapts its interface based on the logged-in user's role, showing or hiding features accordingly.

--- 
<img width="953" alt="image" src="https://github.com/user-attachments/assets/02540969-696c-4905-8579-2c98d03cab66" />
<img width="955" alt="image" src="https://github.com/user-attachments/assets/64f8021c-c0f3-46ac-906c-7ba8576585bc" />
<img width="958" alt="image" src="https://github.com/user-attachments/assets/8ecbf415-81fc-42cf-8d8f-97b606364e0a" />



