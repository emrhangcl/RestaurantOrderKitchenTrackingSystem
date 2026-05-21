$ErrorActionPreference = "Stop"

$root = Split-Path -Parent $MyInvocation.MyCommand.Path
$reportPath = Join-Path $root "SoftwareReport.md"
$projectDir = Join-Path $root "RestaurantOrderKitchenTrackingSystem"

$codeFiles = @(
    "Program.cs",
    "Models.cs",
    "DatabaseService.cs",
    "DataStore.cs",
    "LoginForm.cs",
    "MainForm.cs",
    "PaymentMethodForm.cs",
    "PromptDialog.cs",
    "Properties\AssemblyInfo.cs"
)

$intro = @'
# Software Report: Restaurant Order & Kitchen Tracking System

## Project Information

- **Project name:** Restaurant Order & Kitchen Tracking System
- **Application type:** Windows Forms desktop application
- **Framework:** Microsoft .NET Framework 4.8
- **Database technology:** SQL Server LocalDB with ADO.NET
- **Repository:** https://github.com/emrhangcl/RestaurantOrderKitchenTrackingSystem

This report is prepared according to the requested structure: three main sections and one appendix. The first section explains the classes created in the project. The second section explains the database schema, tables, relationships, and diagram. The third section describes the user interface and developed functionality. The appendix contains the complete code listing.

---

# Section 1: Classes, Fields, Methods, And Functionality

## 1.1 Program

**File:** `Program.cs`

The `Program` class is the entry point of the application.

**Main functionality:**

- Enables Windows Forms visual styles.
- Opens the login form.
- Starts the main application form after successful authentication.
- Supports logout by returning to the login screen.

**Important method:**

- `Main()` starts the application loop and controls login/logout behavior.

## 1.2 LoginForm

**File:** `LoginForm.cs`

The `LoginForm` class provides the user authentication screen.

**Main fields:**

- `_waiters`: stores demo user accounts and their roles.
- `_usernameBox`: input for username.
- `_passwordBox`: input for password.
- `_messageLabel`: displays login information or errors.

**Important methods:**

- `BuildInterface()` creates the login UI.
- `TryLogin()` validates username and password.
- `PasswordBoxKeyDown()` allows login with the Enter key.

**Functionality:**

The form supports role-based login for Waiter, Kitchen, Cashier, and Manager users.

## 1.3 MainForm

**File:** `MainForm.cs`

The `MainForm` class is the main user interface of the restaurant management system.

**Main fields:**

- `_loggedInWaiter`: stores the currently logged-in account.
- `_menuItems`: stores menu items loaded from the database.
- `_orders`: stores active and paid restaurant orders.
- `_currentLines`: stores the current cart before order submission.
- `_tables`: stores table state information.
- `_clockTimer`: updates system time, order timing, and table colors.
- `_selectedTableNumber`: stores the currently selected table.
- `_ordersGrid`: displays kitchen/order tracking records.
- `_receiptBox`: displays receipt preview.
- `_salesSummaryLabel`: displays daily cash, card, and total revenue.

**Important methods:**

- `BuildInterface()` creates the main layout.
- `BuildTablePanel()` creates the 16-table restaurant layout.
- `BuildOrderPanel()` creates order-entry controls.
- `BuildKitchenPanel()` creates kitchen tracking, payment, report, and management controls.
- `LoadOrSeedData()` initializes the database and default application data.
- `RefreshMenu()` reads and displays menu items.
- `AddSelectedItem()` adds selected menu items to the cart.
- `SubmitOrder()` creates a restaurant order and decreases stock.
- `ChangeSelectedStatus()` updates kitchen/payment status.
- `SplitPaySelectedOrder()` supports split cash/card payments.
- `ShowDayReport()` displays end-of-day reporting.
- `TransferSelectedOrder()` transfers an order to another table.
- `MergeSelectedTable()` moves all active orders from one table to another.
- `SetSelectedTableState()` marks a table as Available, Reserved, or Cleaning.
- `AddMenuItem()`, `RestockSelectedMenuItem()`, `ToggleSelectedMenuItem()`, and `DeleteSelectedMenuItem()` implement menu CRUD operations.
- `SaveState()` persists runtime application state.

**Functionality:**

This class coordinates the entire restaurant workflow: table selection, order entry, kitchen preparation, payment, reporting, receipt export, role authorization, and menu management.

## 1.4 Models And Enums

**File:** `Models.cs`

This file contains the data models and enums used by the application.

### Enums

- `OrderStatus`: New, Preparing, Ready, Served, Paid, Cancelled.
- `PaymentMethod`: None, Cash, Card, Split.
- `UserRole`: Waiter, Kitchen, Cashier, Manager.
- `TableState`: Available, Reserved, Cleaning.

### MenuItem

Represents a menu item stored in the SQL database.

**Fields/properties:**

- `Id`
- `Name`
- `Category`
- `Price`
- `PrepMinutes`
- `Ingredients`
- `StockQuantity`
- `IsActive`

**Important method:**

- `ToString()` displays item name, price, and stock status in the UI.

### OrderLine

Represents one item in an order.

**Fields/properties:**

- `Item`
- `Quantity`
- `Customization`
- `Total`
- `PrepMinutes`
- `DisplayName`

### PaymentRecord

Represents a payment transaction.

**Fields/properties:**

- `Method`
- `Amount`
- `PaidAt`

### RestaurantOrder

Represents a full restaurant order.

**Fields/properties:**

- `Id`
- `TableNumber`
- `ServerName`
- `Notes`
- `CreatedAt`
- `LastActivityAt`
- `Status`
- `PaymentMethod`
- `Lines`
- `Payments`
- `Total`
- `PaidAmount`
- `Balance`
- `EstimatedPrepMinutes`
- `ItemsSummary`
- `ReceiptText`

### WaiterAccount

Represents a login account.

**Fields/properties:**

- `Username`
- `Password`
- `DisplayName`
- `Role`

### RestaurantTable

Represents table status and timing.

**Fields/properties:**

- `Number`
- `LastOrderAt`
- `HasActiveOrder`
- `State`
- `MinutesSinceLastOrder`

### AppState

Represents serialized runtime state.

**Fields/properties:**

- `MenuItems`
- `Orders`
- `Tables`
- `NextOrderId`

## 1.5 DatabaseService

**File:** `DatabaseService.cs`

This class implements ADO.NET database operations.

**Main fields/constants:**

- `DatabaseName`
- `MasterConnectionString`
- `AppConnectionString`

**Important methods:**

- `Initialize()`: creates the LocalDB database and `MenuItems` table if they do not exist.
- `GetMenuItems()`: reads menu records from SQL Server.
- `InsertMenuItem()`: inserts a new menu item.
- `UpdateMenuItem()`: updates an existing menu item.
- `DeleteMenuItem()`: deletes a menu item.
- `ExecuteMenuItemCommand()`: shared helper for parameterized insert/update operations.

**Functionality:**

This class satisfies the ADO.NET CRUD requirement using `SqlConnection`, `SqlCommand`, and `SqlDataReader`.

## 1.6 DataStore

**File:** `DataStore.cs`

This class serializes runtime state and exports receipts.

**Important methods:**

- `Load()`: loads saved application state.
- `Save()`: saves application state.
- `SaveReceipt()`: writes selected order receipt text to a file.

## 1.7 PaymentMethodForm

**File:** `PaymentMethodForm.cs`

This form provides an in-application payment method selection screen.

**Functionality:**

- Displays selected order number, table number, and total.
- Provides two large buttons: Cash and Card.
- Returns the selected payment method to `MainForm`.

## 1.8 PromptDialog

**File:** `PromptDialog.cs`

This helper class displays small input dialogs.

**Important method:**

- `Ask()` opens a modal text input dialog and returns the entered value.

## 1.9 AssemblyInfo

**File:** `Properties/AssemblyInfo.cs`

This file contains assembly metadata such as title, description, product name, GUID, and version.

---

# Section 2: Database Schema, Diagram, Tables, Relationships, And Triggers

## 2.1 Database Overview

The application uses SQL Server LocalDB with ADO.NET.

- **Database name:** `RestaurantOrderKitchenTrackingSystemDb`
- **Main CRUD table:** `MenuItems`
- **Database access class:** `DatabaseService`
- **Connection provider:** `System.Data.SqlClient`

The database and table are created automatically when the application starts.

## 2.2 Database Diagram

```mermaid
erDiagram
    MENU_ITEMS {
        int Id PK
        nvarchar Name
        nvarchar Category
        decimal Price
        int PrepMinutes
        nvarchar Ingredients
        int StockQuantity
        bit IsActive
    }
```

## 2.3 MenuItems Table

| Column | Type | Key | Nullable | Description |
|---|---:|---|---|---|
| `Id` | `INT` | Primary Key | No | Unique menu item identifier. |
| `Name` | `NVARCHAR(100)` |  | No | Name of the menu item. |
| `Category` | `NVARCHAR(60)` |  | No | Menu category. |
| `Price` | `DECIMAL(18,2)` |  | No | Item price. |
| `PrepMinutes` | `INT` |  | No | Estimated preparation time. |
| `Ingredients` | `NVARCHAR(MAX)` |  | No | Comma-separated ingredient list. |
| `StockQuantity` | `INT` |  | No | Available stock count. |
| `IsActive` | `BIT` |  | No | Availability flag. |

## 2.4 SQL Creation Script

```sql
CREATE TABLE dbo.MenuItems
(
    Id INT NOT NULL PRIMARY KEY,
    Name NVARCHAR(100) NOT NULL,
    Category NVARCHAR(60) NOT NULL,
    Price DECIMAL(18,2) NOT NULL,
    PrepMinutes INT NOT NULL,
    Ingredients NVARCHAR(MAX) NOT NULL,
    StockQuantity INT NOT NULL,
    IsActive BIT NOT NULL
);
```

## 2.5 CRUD Mapping

| CRUD Operation | User Interface Action | ADO.NET Method | SQL Operation |
|---|---|---|---|
| Create | Add Menu | `InsertMenuItem()` | `INSERT` |
| Read | Application startup / menu list | `GetMenuItems()` | `SELECT` |
| Update | Restock / Toggle Item / Stock update | `UpdateMenuItem()` | `UPDATE` |
| Delete | Delete Item | `DeleteMenuItem()` | `DELETE` |

## 2.6 Relationships

The submitted database contains one main required entity, `MenuItems`. The assignment requires CRUD operations for at least one database entity, and `MenuItems` satisfies this requirement.

Order, payment, and table workflow information is represented by application model classes and local state. In the application model, `RestaurantOrder` contains `OrderLine` objects, and each `OrderLine` references a `MenuItem`. This creates a logical application-level relationship between orders and menu records.

## 2.7 Triggers

No database triggers are used. Business rules are handled in the application layer:

- Submitting an order decreases item stock.
- Cancelling an order restores item stock.
- Both operations update the `MenuItems` table through ADO.NET.

---

# Section 3: User Interface And Developed Functionality

## 3.1 Login Screen

The first screen is the login form. It supports role-based login with username and password.

Demo accounts:

| Username | Password | Role |
|---|---|---|
| `ayse` | `1234` | Waiter |
| `mehmet` | `1234` | Waiter |
| `chef` | `1234` | Kitchen |
| `cashier` | `1234` | Cashier |
| `manager` | `1234` | Manager |

![Login screen](Screenshots/login-screen.png)

## 3.2 Main Form Layout

The main form contains three main areas:

1. Table layout panel
2. Order entry panel
3. Kitchen tracking and payment panel

![Main form](Screenshots/main-form.png)

## 3.3 Table Layout

The table layout displays 16 restaurant tables. Clicking a table filters the right-side tracking panel to that table's active orders.

Table colors:

- Green: active under 30 minutes
- Orange: active without a new order after 30 minutes
- Red: active without a new order after 45 minutes
- Gray: no active order
- Blue: reserved
- Light gray: cleaning

## 3.4 Order Entry

The order panel allows the user to:

- Select a table.
- Select menu category.
- Select menu item.
- Remove ingredients.
- Add extra ingredient notes.
- Select quantity.
- Add items to cart.
- Send the order to the kitchen.

When an order is submitted, item stock is decreased and the `MenuItems` table is updated through ADO.NET.

## 3.5 Kitchen Tracking

The kitchen tracking panel displays orders and status values:

- New
- Preparing
- Ready
- Served
- Paid
- Cancelled

Kitchen users can update preparation statuses. The table can also be filtered by status.

## 3.6 Payment

The payment workflow supports:

- Cash payment
- Card payment
- Split payment

When `Paid` is selected, an in-application payment screen opens with large Cash and Card buttons. Paid orders are removed from the active table view and included in daily sales totals.

## 3.7 Daily Sales And Report

The system displays:

- Cash total
- Card total
- Grand total
- Paid order count
- Cancelled order count
- Best-selling item
- Waiter performance

## 3.8 Menu Management

Manager users can manage the `MenuItems` database entity:

- Add Menu: creates a new database record.
- Restock: updates stock quantity.
- Toggle Item: updates active/passive status.
- Delete Item: deletes a database record.

These actions demonstrate full CRUD functionality.

## 3.9 Receipt Export

The selected order can be exported as a text receipt. The receipt includes table number, server, items, payment details, total, and balance.

## 3.10 Table Operations

The application supports:

- Transfer order to another table
- Merge active orders from one table to another
- Mark table as Available
- Mark table as Reserved
- Mark table as Cleaning

---

# Appendix: Complete Code Listing

The following appendix contains the complete source code listing for the project.

'@

Set-Content -Path $reportPath -Value $intro -Encoding UTF8

foreach ($relativePath in $codeFiles) {
    $path = Join-Path $projectDir $relativePath
    Add-Content -Path $reportPath -Value ""
    Add-Content -Path $reportPath -Value "## $relativePath"
    Add-Content -Path $reportPath -Value ""
    Add-Content -Path $reportPath -Value '```csharp'
    Add-Content -Path $reportPath -Value (Get-Content -Path $path -Raw)
    Add-Content -Path $reportPath -Value '```'
}

Write-Host "Generated $reportPath"
