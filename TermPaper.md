# Restaurant Order & Kitchen Tracking System Term Paper

## 1. Introduction

Restaurants need fast communication between waiters, kitchen staff, and cashiers. Paper-based order tracking can cause lost tickets, delayed preparation, incorrect totals, and poor service quality. This project proposes a Windows Forms desktop application named Restaurant Order & Kitchen Tracking System.

The selected topic follows the course requirement because it is not a car rental application and it is not related to rental operations.

## 2. Purpose Of The Project

The purpose of the application is to manage restaurant orders from the moment a waiter creates an order until the payment is completed. The system helps staff see active orders, kitchen preparation status, table information, item quantities, estimated preparation time, and paid revenue.

## 3. Target Users

The target users are:

- Waiters who create table orders.
- Kitchen staff who update preparation status.
- Cashiers who mark served orders as paid.
- Managers who review active orders and revenue.

## 4. Main Features

The application includes these main features:

- Menu filtering by category.
- Role-based login with waiter, kitchen, cashier, and manager accounts.
- Fixed table layout for restaurant floor monitoring.
- Table-based order filtering in the kitchen tracking screen.
- New order creation with table number and server name.
- System-clock order time recording.
- Elapsed time and idle table time tracking.
- Table color changes after 30 and 45 minutes without a new order.
- Ingredient removal and extra ingredient notes for menu items.
- Cart management with quantity and item totals.
- Kitchen queue with color-coded order status.
- Status updates for New, Preparing, Ready, Served, and Paid.
- Order cancellation for incorrect or unwanted orders.
- Status filtering for kitchen and cashier workflows.
- Receipt preview for the selected order.
- Payment method selection with Cash and Card options.
- Split payment support for cash and card together.
- Daily sales tracking by cash total, card total, and grand total.
- End-of-day reporting with best-selling item and waiter performance.
- Receipt export to text file.
- Table transfer and table merge operations.
- Table states for Available, Reserved, and Cleaning.
- Menu management with stock tracking and item availability.
- Local persistent storage between application runs.
- Summary panel for active orders, ready orders, and paid revenue.

## 5. Technologies Used

The project is developed as a C# Windows Forms application. It uses .NET Framework 4.8 and System.Windows.Forms for the user interface. The application stores data locally by serializing the application state, so menu items, table states, orders, payments, and stock values can remain available between application runs.

## 6. System Design

The system uses several model classes:

- MenuItem stores menu information such as name, category, price, and preparation time.
- OrderLine stores a selected menu item and its quantity.
- RestaurantOrder stores table number, server name, order lines, total amount, estimated preparation time, payment records, and current status.
- RestaurantTable stores table number, active order state, reservation/cleaning state, and timing information.
- WaiterAccount stores login identity and role information.
- PaymentRecord stores cash/card payment history.

The application starts with a waiter login form. After successful login, the main form contains three workflow areas. The left side is used for table layout monitoring. The center is used for order entry and ingredient customization. The right side is used for kitchen tracking, filtering, status management, elapsed time monitoring, and receipt preview.

## 7. Future Work

Future versions can add SQL Server or SQLite storage, graphical charts, PDF receipt export, barcode-based inventory updates, and networked multi-device kitchen screens.

## 8. Conclusion

The Restaurant Order & Kitchen Tracking System provides a practical Windows Forms project topic for the term assignment. It demonstrates form design, event handling, model classes, collection management, data binding, and workflow tracking in a real restaurant environment.
