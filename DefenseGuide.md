# Defense Guide

This document is a short guide for the live defense of the Restaurant Order & Kitchen Tracking System.

## 1. Opening Explanation

The project is a Windows Forms desktop application developed with the Microsoft .NET Framework. The selected topic is a restaurant order and kitchen tracking system. It is not related to car rental or any rental operation.

The main goal is to manage the workflow between waiter, kitchen, cashier, and manager users. The application supports table selection, order creation, kitchen status tracking, payment, daily sales reporting, receipt export, menu management, and ADO.NET CRUD operations.

## 2. Demo Accounts

Use these accounts during the defense:

| Username | Password | Role |
|---|---|---|
| `ayse` | `1234` | Waiter |
| `mehmet` | `1234` | Waiter |
| `chef` | `1234` | Kitchen |
| `cashier` | `1234` | Cashier |
| `manager` | `1234` | Manager |

Recommended demo account: `manager / 1234`, because it can access all features.

## 3. Suggested Live Demo Flow

1. Open the application.
2. Log in with `manager / 1234`.
3. Show the 16-table layout.
4. Click a table and show that the right panel filters orders for that table.
5. Add menu items to the cart.
6. Remove one ingredient or add an extra note.
7. Send the order to the kitchen.
8. Change order status to `Preparing`, then `Ready`, then `Served`.
9. Click `Paid`.
10. Select `Cash` or `Card` from the custom payment screen.
11. Show that the table becomes available after payment.
12. Show the `Daily Sales` totals.
13. Click `Day Report`.
14. Show `Add Menu`, `Restock`, `Toggle Item`, and `Delete Item` as MenuItems CRUD actions.
15. Open the GitHub repository and show the source code files.

## 4. Where ADO.NET Is Used

ADO.NET is implemented in:

`RestaurantOrderKitchenTrackingSystem/DatabaseService.cs`

Important methods:

- `Initialize()` creates the LocalDB database and `MenuItems` table.
- `GetMenuItems()` performs the Read operation.
- `InsertMenuItem()` performs the Create operation.
- `UpdateMenuItem()` performs the Update operation.
- `DeleteMenuItem()` performs the Delete operation.

The database is:

`RestaurantOrderKitchenTrackingSystemDb`

The table is:

`MenuItems`

## 5. CRUD Explanation

The required CRUD entity is `MenuItems`.

- Create: The manager uses `Add Menu`.
- Read: The menu list loads from SQL Server LocalDB when the application starts.
- Update: The manager uses `Restock` or `Toggle Item`; stock is also updated when orders are submitted or cancelled.
- Delete: The manager uses `Delete Item`.

## 6. Authorization Explanation

The application has role-based login:

- Waiter can create orders, use tables, transfer/merge tables, and use split payment.
- Kitchen can update kitchen preparation statuses.
- Cashier can mark orders as paid.
- Manager can access all management features.

This satisfies the optional authorization requirement mentioned in the assignment.

## 7. Database Diagram Explanation

The database diagram is documented in:

`DatabaseDiagram.md`

The diagram contains the `MenuItems` table, its columns, primary key, CRUD mapping, relationship explanation, and trigger explanation.

## 8. Closing Statement

This application satisfies the assignment because it is a .NET Framework Windows Forms application with ADO.NET database connectivity and full CRUD operations for at least one database entity. It also includes a login/authorization mechanism, which supports the bonus/maximum grade requirement.
