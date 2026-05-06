# Restaurant Order & Kitchen Tracking System

This is a Windows Forms term project for managing restaurant orders and kitchen workflow.

## Project Scope

The application is not a car rental or rental-related system. It focuses on a restaurant scenario:

- Create orders by table number and waiter/server name.
- Sign in with role-based accounts for waiter, kitchen staff, cashier, and manager.
- Log out and return to the login screen without restarting the application.
- Select tables from a fixed 16-table restaurant layout.
- Click a table to show only that table's orders in the tracking panel.
- Use the All Tables button to return to the full kitchen queue.
- Select menu items by category: Drinks, Main Courses, Desserts, and Salads.
- Remove default ingredients or add extra ingredient notes before adding an item.
- Add item quantities to a cart.
- Send the order to the kitchen queue.
- Record order time from the live system clock.
- Track elapsed order time and idle table time.
- Change table colors when no new order is placed after 30 and 45 minutes.
- Track kitchen statuses: New, Preparing, Ready, Served, Paid.
- Cancel incorrect orders when needed.
- Filter kitchen orders by status.
- Preview a receipt-style order detail panel.
- Clear cancelled orders from the active screen while paid orders remain in daily sales.
- Choose Cash or Card when marking an order as paid.
- Paid orders use an in-app payment screen with large Cash and Card buttons.
- Split a payment between Cash and Card.
- Waiter, cashier, and manager accounts can use Split Pay.
- Track daily sales totals by Cash, Card, and grand total.
- View order totals, estimated preparation time, and daily paid revenue.
- Save receipt previews as text files.
- Transfer an order to another table.
- Merge all active orders from one table into another table.
- Mark tables as Available, Reserved, or Cleaning.
- Add menu items, restock items, and toggle item availability.
- Persist application data locally between runs.
- Use ADO.NET with SQL Server LocalDB for MenuItems CRUD operations.

## How To Run

1. Open `RestaurantOrderKitchenTrackingSystem.sln` in Visual Studio.
2. Make sure the .NET Framework 4.8 Developer Pack is installed.
3. Build the solution.
4. Run the project.

Demo login accounts:

- `ayse` / `1234`
- `mehmet` / `1234`
- `chef` / `1234`
- `cashier` / `1234`
- `manager` / `1234`

## Implemented Advanced Features

- SQL Server LocalDB database: `RestaurantOrderKitchenTrackingSystemDb`
- ADO.NET table: `MenuItems`
- Menu CRUD:
  - Create: `Add Menu`
  - Read: menu list loads from SQL
  - Update: `Restock` and `Toggle Item`
  - Delete: `Delete Item`
- Local persistence is stored under the user's application data folder.
- Receipts are exported to text files.
- Day report shows paid order count, cancelled order count, cash total, card total, grand total, best-selling item, and waiter order counts.
- Manager-only menu management supports adding menu items, restocking, and toggling availability.
- Role guards prevent waiter/kitchen/cashier accounts from using manager-only or cashier-only actions.

## GitHub Submission

Upload this repository to GitHub and submit the repository link with the term paper text.

## Submission Files

- `TermPaper.md`: report on the developed software.
- `DatabaseDiagram.md`: database diagram, table description, CRUD mapping, relationships, and trigger explanation.
- `DefenseGuide.md`: live defense guide and recommended demo flow.
