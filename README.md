💻 Laptop Store (ASP.NET Core MVC)

Welcome to Laptop Store — a modern e-commerce web application for buying and managing laptops online.
This project demonstrates a fully functional laptop e-commerce system built using ASP.NET Core MVC and MySQL.

🚀 Features
🛍️ Customer Side

Browse laptops by category

View detailed product descriptions

Add to cart and checkout

Search and filter products

Responsive and user-friendly interface

🧑‍💼 Admin Side

Secure admin login (created from DB)

Add, edit, delete products and categories

Manage stock and featured items

Dashboard with key statistics

🧱 Tech Stack
Layer	Technology
Frontend	HTML, CSS, Bootstrap, Razor Views
Backend	ASP.NET Core MVC (C#)
Database	MySQL
Authentication	ASP.NET Identity / Custom Admin Login

⚙️ Setup Guide
1️⃣ Clone the repository
git clone https://github.com/LIMO2001
cd laptopstore

2️⃣ Configure the database

Open appsettings.json

Update your MySQL connection string:

"ConnectionStrings": {
  "DefaultConnection": "Server=localhost;Database=LaptopStore;User ID=root;Password=yourpassword;"
}

3️⃣ Apply migrations (if using EF Core)
dotnet ef database update

4️⃣ Run the project
dotnet run


Then visit:
👉 https://localhost:5001 or http://localhost:5000

🧩 Database Structure

Tables

Products

Categories

Users

Orders

OrderDetails

Each product includes details like name, price, description, image, stock quantity, specs, and category.

🖼️ Screenshots
Home Page	Product Details	
<img width="959" height="476" alt="image" src="https://github.com/user-attachments/assets/e5614779-740c-4c1d-b8fd-035a350a50c4" />

Login
<img width="956" height="473" alt="image" src="https://github.com/user-attachments/assets/11a8f60e-b2c2-49bc-818c-7d04ca37af5d" />





Admin Dashboard
<img width="959" height="475" alt="image" src="https://github.com/user-attachments/assets/d2e34d55-9d6c-48cc-a536-e12619370f1d" />

<img width="950" height="474" alt="image" src="https://github.com/user-attachments/assets/be0427c7-0a0f-408f-a399-25a72fb10a29" />

	
🪄 Extras

Fully responsive design

Image upload for products

Real-time category updates

Reusable partial views

Custom favicon support

🧠 Developer Info

Project by: DKL EMPIRE TECH
Motto: Tech is our concern

For contributions or issues, please open a pull request or contact the maintainer.

📜 License

This project is licensed under the MIT License — free to use and modify.
