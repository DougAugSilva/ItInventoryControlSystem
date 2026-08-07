# IT Inventory Control System - 1.0v
> Made by: Douglas Augusto da Silva.

> This project is an adapted, generalized version of a real e-commerce application, built for portfolio purposes. Data, branding, and specific integrations have been replaced with generic/fictional versions.

## 1. Project goals
This project aims to improve the current spreadsheet-based process for IT inventory control, targeting better organization and productivity gains by replacing the current spreadsheet-based inventory system.

## 2. Features

### 2.1. Registering new items

The following information is available and can be edited when registering new items in the system.

- Upload an item photo (`.png` or `.jpg`, up to 10MB).
- Select the type of item being registered, among:
    - Phone
    - Phone Base
    - Mono Headset
    - Stereo Headset
    - Mobile Phone
    - HDMI Cables
    - VGA Cables
    - Wired Keyboard
    - Wireless Keyboard
    - Wired Mouse
    - Wireless Mouse
    - Wired Earphones
    - Wireless Earphones
    - HDMI-to-VGA Adapters
    - DisplayPort Adapters
    - Desktop Workstation
    - Monitors
    - Monitor Stand
    - Laptop Stand
    - Headphone Foam Pads
    - Laptops
- Model and brand of the item (e.g., Dell Extreme Laptop).
- Additional information (e.g., model with a cracked screen).
- Availability status of the item, one of:
    - Available
    - Loaned (enables the due date, which technician, and who it was loaned to)
    - Unavailable
- Condition of the item, one of:
    - New
    - Used
    - Defective
    - Broken

### 2.2. Editing already-registered items
Allows editing all the parameters described above for items already registered in the inventory system, such as updating the asset number or correcting incorrectly entered information.

### 2.3. Searching items in the inventory

The system includes a search mechanism, allowing items to be found by asset number, item name, or a keyword within the item record, and also filtered by the criteria described in section *2.1*, making it easier to find items across the site.

### 2.4. User management
An administrator account named `admin.besttechti` is created, with its password managed outside the system (never in plain text in the code — see `docs/doc_backend.md` and `docs/doc_security.md`). This account can:
- Create new users.
- Remove existing users.
- Edit existing users' information, such as usernames, passwords, and so on.

An extra tab is enabled to perform these tasks, accessible only to the admin account.

> For security reasons, the admin account's information cannot be edited through the frontend, nor can another account with the same name be created, nor can the admin account be deleted. Every password set or changed must be at least 12 characters long, with an uppercase letter, a lowercase letter, and a number. Login attempts are also rate-limited per minute to make brute-force attacks harder.

## 3. System sections
For better organization, the system's features are split across different tabs, as follows.

### 3.1. Login screen
Where the system user signs in with a username and password.

### 3.2. Register
Allows registering and editing items in the inventory.

### 3.3. Dashboard
Shows a chart of item quantities in the inventory, filterable by **item availability status** or **item condition**.

### 3.4. Loans
Shows which items have been loaned out, with the checkout date and return date, and who took and received the item.

### 3.5. Items
Shows which items were recently added to the system.

### 3.6. User Management
Allows creating new users and editing existing users' information.

> Available only to the admin.besttechti account.

## 4. Frameworks and technologies used
Development uses a Windows environment with WSL, along with ASDF to install and manage the programming language toolchains. The system was built with the following technologies:

### 4.1. Frontend
- HTML
- CSS
- JavaScript - 26.5.0v
- React - 19v

### 4.2. Backend
- C# and .NET project structure - 10.0v
- SQL Server
- End-to-end HTTPS (real certificate in production; local development certificate during development)
- Docker — `docker-compose.yml`/`Dockerfile.api` ready for the database and API packaging; the development environment currently runs SQL Server natively on Windows rather than the container (details in `docs/doc_docker.md`)

## 5. Running the project
For a complete step-by-step guide on how to execute the project, see `docs\execution_manual.md`.
the base users I created for testing are:
- User: `john.doe` | Password: `TestPassword1234!`
- User: `admin.besttechti` | Password: `StrongAdminPassword123!`

> Make sure all languages and frameworks are installed in their respective versions.

### 5.1 Windows environment
```shell
## Running the backend:
wsl
cd ~/your_folder/ItInventoryControl\backend\Inventory.Api && dotnet run

## Running the frontend
wsl
cd ~/your_folder/ItInventoryControl\frontend && npm run dev
```
### 5.2. Linux environment
```bash
## Running the backend:
cd ~/your_folder/ItInventoryControl\backend\Inventory.Api && dotnet run

## Running the frontend
cd ~/your_folder/ItInventoryControl\frontend && npm run dev
```

## 6. Final notes
- For more information on each part of the project, see the `docs/` folder — it includes `doc_backend.md`, `doc_database.md`, `doc_docker.md`, `doc_frontend.md`, `doc_security.md`, and `project_structure.md`.
- This project follows the [MIT](LICENSE) license.
