
# E-CommerceAPI

## Overview

E-CommerceAPI is a backend API designed for a pizza ordering website. Built with .NET Core, it provides endpoints for managing products, orders, and user authentication, facilitating seamless integration with frontend applications.

## Features

- **Product Management**: Create, read, update, and delete pizza products.
- **Order Processing**: Handle customer orders with functionalities to create, view, and update order statuses.
- **User Authentication**: Secure user registration and login using JWT tokens.

## Project Structure

The repository is organized as follows:

- **Controllers/**: Contains API controllers for handling HTTP requests.
- **DTO/**: Data Transfer Objects for managing data flow between layers.
- **Data/**: Database context and configuration files.
- **Migrations/**: Entity Framework Core migration files for database schema management.
- **Models/**: Defines the data models used in the application.
- **Properties/**: Project metadata, including assembly information.

## Getting Started

To set up and run the project locally:

1. **Clone the Repository**:
   ```bash
   git clone https://github.com/brunobusaala/E-CommerceAPI.git
   cd E-CommerceAPI
   ```

2. **Restore Dependencies**:
   ```bash
   dotnet restore
   ```

3. **Apply Migrations**:
   ```bash
   dotnet ef database update
   ```

4. **Run the Application**:
   ```bash
   dotnet run
   ```

## Prerequisites

- **.NET SDK**: Ensure the .NET SDK is installed. [Download here](https://dotnet.microsoft.com/download).
- **Entity Framework Core Tools**: For database migrations. Install using:
  ```bash
  dotnet tool install --global dotnet-ef
  ```

## Configuration

Configuration settings, such as database connection strings, are located in the `appsettings.json` file. Update this file to match your local environment settings.

## Contributing

Contributions are welcome! Feel free to open issues or submit pull requests for enhancements or bug fixes.

## License

This project is licensed under the MIT License. See the [LICENSE](LICENSE) file for details.

## Acknowledgments

Special thanks to all contributors and the open-source community for their support.
