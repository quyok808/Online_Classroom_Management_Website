# Online Classroom Management Website

This project is a comprehensive Online Classroom Management Website designed to streamline the 
management of virtual classrooms. It provides a robust set of features for managing users, classes,
students, attendance, grading, online learning, and real-time communication within a class using 
SignalR.

## Features

**1. Classroom Management**

- View and manage all classrooms in a centralized index.
- Create new classes with customizable settings.
- Join existing classes as a student or instructor.

**2. Assignment Management**

- Create, distribute, and manage assignments for each class.
- Grade assignments and provide feedback to students.

**3. Student Management**

- Maintain and update student rosters for each class.
- Track student participation and performance.

**4. Real-Time Communication**

- Enable in-class messaging and discussions using SignalR for seamless, real-time interaction.

**5. Grade Management**

- Maintain and update gradebooks for tracking student performance across assignments and exams.

**6. Schedule Management**

- Create and manage class schedules to keep students and instructors organized.

**7. Class Customization**

- Customize class settings, layouts, and configurations to suit specific teaching needs.

**8. Notifications**

- Send and manage class-wide announcements and notifications.

**9. Content Reuse**

- Reuse posts, lectures, and assignments from other classes to save time and maintain consistency.

## Technologies Used

- **ASP.NET Core**: Backend framework for building the web application.
- **SignalR**: For real-time communication within classes.
- **HTML/CSS/JavaScript**: Frontend technologies for building the user interface.
- **SQL Server**: Database for storing user, class, and assignment data.
- **Entity Framework Core**: ORM for database interactions.
- **Bootstrap**: For responsive and modern UI design.

## Getting Started

### Prerequisites
- [.NET SDK](https://dotnet.microsoft.com/download) (version 7.0)
- [SQL Server Management](https://www.microsoft.com/en-us/sql-server/sql-server-downloads) (Have been installed and running )
- IDE: [Visual Studio](https://visualstudio.microsoft.com/)
- A modern web browser for accessing the website.

### Installation

1. Clone the repository:

```bash
git clone https://github.com/quyok808/Online_Classroom_Management_Website.git
```

3. Navigate to the project directory

```bash
cd Online_Classroom_Management_Website
```

4. Install backend dependencies

```bash
dotnet restore
```

6. Run Migrations

```bash
dotnet ef database update
```

7. Run the application

```bash
dotnet run
```

### Usage

- Access the website via the provided URL `Access the website via the provided URL `http://localhost:5267/`
- Account:
    ```bash
    username: admin
    password: Admin_111
    ```
- Log in as an admin, instructor, or student to explore the respective functionalities.
- Use the dashboard to manage classes, assignments, grades, and more.

### Contact

For questions or feedback, please reach out via GitHub Issues.

---

© 2025 quyok808 - Nguyen Cong Quy
