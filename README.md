
# 🏢 Tenant Management System

A simple and efficient **Tenant Management System** built using:

- ASP.NET Core MVC (.NET 6/7/8)
- Dapper ORM for lightweight data access
- SQL Server (MSSQL)
- Stored Procedures for database operations

---

## 📌 Features

- 🔐 User Authentication (Login, Register)
- 🏠 Property Management (Create, Update, Delete)
- 🛏️ Unit Management (Assign Renter, Track Vacancies)
- 💡 Utility Bills
- 🧾 Monthly Rent, Utility Bills and Payment Tracking


---

## 🛠️ Tech Stack

| Layer              | Technology                     |
|--------------------|--------------------------------|
| Backend            | ASP.NET Core MVC               |
| ORM                | Dapper                         |
| Database           | SQL Server (MSSQL)             |
| Database Access    | Stored Procedures              |
| Frontend UI        | Bootstrap 5                    |

---

## ⚙️ Database Setup

1. Create a new **SQL Server** database.
2. Execute the provided stored procedures located in `Tenent ManagementSQL/initialSQL.sql`.
3. Update your connection string in `appsettings.json`:

```json
"ConnectionStrings": {
  "DefaultConnection": "Server=YOUR_SERVER;Database=TenentMangement;Trusted_Connection=True;"
}
```

---


## 🚀 Getting Started

1. **Clone the repo**

```bash
git clone https://github.com/prabesh-stha/TenentManagement.git
cd TenentManagement
```

2. **Restore packages**

```bash
dotnet restore
```

3. **Update connection string** in `appsettings.json`

4. **Run the app**

```bash
dotnet run
```

Visit [https://localhost:5001](https://localhost:5001)

---

## 🙋‍♂️ Author

**Prabesh Shrestha**  
📧 shresthaprabesh155@gmail.com  
📍 Nepal  
