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
Home Page	Product Details	Admin Dashboard

	
	
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
