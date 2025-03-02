# MagicVilla
# ASP.NET Core Web API Project

## 📌 Overview
This project is a **Beginner to Advanced** level implementation of an **ASP.NET Core Web API**, designed to teach the fundamentals of building and consuming RESTful APIs. It covers essential API development concepts, security enhancements, and best practices in API design using **.NET Core 3.1+**.

By the end of this project, you will have a fully functional API with **authentication, repository patterns, versioning, exception handling, and more**.

## 🚀 Features
- **Build RESTful APIs** using **ASP.NET Core 3.1+**.
- **Repository Pattern** for data access with **Entity Framework Core**.
- **Code-First Migrations** to manage database schema changes.
- **API Versioning** to support multiple versions of the API.
- **Exception Handling** using Middleware and Filters.
- **Security Implementation** with JWT authentication and refresh tokens.
- **File Upload Support** in API and web projects.
- **Clean Code Architecture** following best practices.
- **Dynamic Base Service** for improved modularity.
- **API Documentation** using Swagger.
- **HTTP Client Consumption** of APIs in a .NET Web App.

## 🛠️ Technologies Used
- **.NET Core 3.1+**
- **ASP.NET Core Web API**
- **Entity Framework Core**
- **JWT Authentication & Refresh Tokens**
- **Swagger (API Documentation)**
- **Repository & Unit of Work Pattern**
- **MediatR for CQRS (Optional Enhancements)**

## 🏗️ Setup & Installation
### Prerequisites
- **.NET SDK 3.1+** installed on your machine.
- **SQL Server** or any preferred database provider.
- **Postman** (for API testing) or Swagger UI.

### Steps to Run the Project
1. **Clone the Repository**
   ```bash
   git clone https://github.com/your-username/your-repo.git
   cd your-repo
   ```
2. **Install Dependencies**
   ```bash
   dotnet restore
   ```
3. **Set Up Database** (Run Migrations)
   ```bash
   dotnet ef database update
   ```
4. **Run the Application**
   ```bash
   dotnet run
   ```
5. **Access API Documentation**
   Open your browser and navigate to:
   ```
   http://localhost:<port>/swagger
   ```

## 🔥 API Endpoints
| Method | Endpoint | Description |
|--------|---------|-------------|
| GET | /api/v1/resource | Fetch data |
| POST | /api/v1/resource | Create new resource |
| PUT | /api/v1/resource/{id} | Update a resource |
| DELETE | /api/v1/resource/{id} | Delete a resource |
| POST | /api/auth/login | Authenticate user |
| POST | /api/auth/refresh-token | Get a new access token |

## 🛡️ Authentication & Security
This API implements **JWT-based authentication** along with **refresh tokens** to ensure security. Follow these steps:
1. **Login to get an access token**.
2. **Use the token in the Authorization header** for protected routes.
3. **Refresh the token** when it expires using the `/refresh-token` endpoint.

## 🎯 Contribution
Feel free to contribute by submitting **pull requests** or opening **issues**. Your contributions help improve the project!

## 📜 License
This project is licensed under the **MIT License** – see the [LICENSE](LICENSE) file for details.

---
### 🌟 Happy Coding! 🚀

