using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace RestaurantOrderKitchenTrackingSystem
{
    public sealed class MainForm : Form
    {
        private readonly WaiterAccount _loggedInWaiter;
        private readonly List<MenuItem> _menuItems = new List<MenuItem>();
        private readonly List<RestaurantOrder> _orders = new List<RestaurantOrder>();
        private readonly List<OrderLine> _currentLines = new List<OrderLine>();
        private readonly List<RestaurantTable> _tables = new List<RestaurantTable>();
        private readonly Dictionary<int, Button> _tableButtons = new Dictionary<int, Button>();
        private readonly Timer _clockTimer = new Timer();
        private int _nextOrderId = 1001;
        private int? _selectedTableNumber;

        private ComboBox _categoryCombo;
        private ListBox _menuList;
        private CheckedListBox _removeIngredientList;
        private TextBox _extraIngredientBox;
        private NumericUpDown _quantityInput;
        private NumericUpDown _tableInput;
        private TextBox _serverInput;
        private ListView _cartList;
        private DataGridView _ordersGrid;
        private ComboBox _statusFilterCombo;
        private Label _cartTotalLabel;
        private Label _summaryLabel;
        private Label _clockLabel;
        private Label _kitchenTitleLabel;
        private Label _salesSummaryLabel;
        private TextBox _notesBox;
        private TextBox _receiptBox;

        public bool IsLoggingOut { get; private set; }

        public MainForm(WaiterAccount loggedInWaiter)
        {
            _loggedInWaiter = loggedInWaiter;
            Text = "Restaurant Order & Kitchen Tracking System";
            StartPosition = FormStartPosition.CenterScreen;
            MinimumSize = new Size(1380, 820);
            Size = new Size(1480, 880);
            BackColor = Color.FromArgb(247, 248, 250);

            LoadOrSeedData();
            BuildInterface();
            RefreshMenu();
            RefreshCart();
            RefreshOrders();
            RefreshTables();

            _clockTimer.Interval = 1000;
            _clockTimer.Tick += delegate
            {
                RefreshClock();
                RefreshTables();
                RefreshOrders();
            };
            _clockTimer.Start();
        }

        private void BuildInterface()
        {
            var header = new TableLayoutPanel
            {
                Dock = DockStyle.Top,
                Height = 62,
                ColumnCount = 3,
                BackColor = Color.White,
                Padding = new Padding(18, 0, 18, 0)
            };
            header.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 56));
            header.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 34));
            header.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 112));
            header.Controls.Add(new Label
            {
                Text = "Restaurant Order & Kitchen Tracking System",
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleLeft,
                Font = new Font("Segoe UI", 18, FontStyle.Bold),
                ForeColor = Color.FromArgb(28, 35, 45)
            }, 0, 0);
            _clockLabel = new Label
            {
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleRight,
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                ForeColor = Color.FromArgb(28, 35, 45)
            };
            header.Controls.Add(_clockLabel, 1, 0);

            var logoutButton = SecondaryButton("Log Out");
            logoutButton.Dock = DockStyle.Fill;
            logoutButton.Click += delegate { LogOut(); };
            header.Controls.Add(logoutButton, 2, 0);

            var mainLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 3,
                RowCount = 1,
                Padding = new Padding(16),
                BackColor = Color.FromArgb(247, 248, 250)
            };
            mainLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25));
            mainLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 35));
            mainLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 40));

            mainLayout.Controls.Add(BuildTablePanel(), 0, 0);
            mainLayout.Controls.Add(BuildOrderPanel(), 1, 0);
            mainLayout.Controls.Add(BuildKitchenPanel(), 2, 0);

            Controls.Add(mainLayout);
            Controls.Add(header);
            RefreshClock();
        }

        private Control BuildTablePanel()
        {
            var panel = CreatePanel();
            var layout = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 3 };
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 42));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 132));
            layout.Controls.Add(SectionTitle("Table Layout"), 0, 0);

            var grid = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 4, RowCount = 4 };
            for (var i = 0; i < 4; i++)
            {
                grid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25));
                grid.RowStyles.Add(new RowStyle(SizeType.Percent, 25));
            }

            foreach (var table in _tables)
            {
                var button = new Button
                {
                    Dock = DockStyle.Fill,
                    Margin = new Padding(6),
                    FlatStyle = FlatStyle.Flat,
                    Font = new Font("Segoe UI", 10, FontStyle.Bold),
                    Tag = table.Number
                };
                button.Click += delegate { SelectTable((int)button.Tag); };
                _tableButtons[table.Number] = button;
                grid.Controls.Add(button, (table.Number - 1) % 4, (table.Number - 1) / 4);
            }

            layout.Controls.Add(grid, 0, 1);
            var tableFooter = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 2 };
            tableFooter.RowStyles.Add(new RowStyle(SizeType.Absolute, 38));
            tableFooter.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            var allTablesButton = SecondaryButton("All Tables");
            allTablesButton.Dock = DockStyle.Fill;
            allTablesButton.Click += delegate { ClearTableSelection(); };
            tableFooter.Controls.Add(allTablesButton, 0, 0);
            tableFooter.Controls.Add(new Label
            {
                Text = "Green: available or active under 30 min\r\nOrange: no new order after 30 min\r\nRed: no new order after 45 min",
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI", 9, FontStyle.Regular),
                ForeColor = Color.FromArgb(71, 85, 105),
                TextAlign = ContentAlignment.MiddleLeft
            }, 0, 1);
            layout.Controls.Add(tableFooter, 0, 2);

            panel.Controls.Add(layout);
            return panel;
        }

        private Control BuildOrderPanel()
        {
            var panel = CreatePanel();
            var layout = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 12 };
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 42));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 44));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 26));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 34));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 86));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 42));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 24));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 44));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 78));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 56));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 46));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 1));

            layout.Controls.Add(SectionTitle("New Order"), 0, 0);

            var orderInfo = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.LeftToRight };
            _tableInput = new NumericUpDown { Minimum = 1, Maximum = 16, Width = 70, Font = BodyFont() };
            _serverInput = new TextBox { Width = 140, Font = BodyFont(), ReadOnly = true, Text = _loggedInWaiter.DisplayName };
            orderInfo.Controls.Add(FieldLabel("Table"));
            orderInfo.Controls.Add(_tableInput);
            orderInfo.Controls.Add(FieldLabel("Waiter"));
            orderInfo.Controls.Add(_serverInput);
            layout.Controls.Add(orderInfo, 0, 1);

            _categoryCombo = new ComboBox { Dock = DockStyle.Top, DropDownStyle = ComboBoxStyle.DropDownList, Font = BodyFont() };
            _categoryCombo.SelectedIndexChanged += delegate { RefreshMenu(); };

            _menuList = new ListBox { Dock = DockStyle.Fill, Font = BodyFont(), IntegralHeight = false };
            _menuList.SelectedIndexChanged += delegate { RefreshIngredientOptions(); };
            var menuHost = new Panel { Dock = DockStyle.Fill };
            menuHost.Controls.Add(_menuList);
            menuHost.Controls.Add(_categoryCombo);
            layout.Controls.Add(menuHost, 0, 2);

            layout.Controls.Add(new Label
            {
                Text = "Ingredient changes",
                Dock = DockStyle.Fill,
                Font = BodyFont(),
                TextAlign = ContentAlignment.MiddleLeft
            }, 0, 3);

            var ingredientLayout = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 1 };
            ingredientLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 54));
            ingredientLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 46));
            _removeIngredientList = new CheckedListBox { Dock = DockStyle.Fill, CheckOnClick = true, Font = new Font("Segoe UI", 9, FontStyle.Regular) };
            _extraIngredientBox = new TextBox { Dock = DockStyle.Fill, Multiline = true, Font = new Font("Segoe UI", 9, FontStyle.Regular) };
            ingredientLayout.Controls.Add(_removeIngredientList, 0, 0);
            ingredientLayout.Controls.Add(_extraIngredientBox, 1, 0);
            layout.Controls.Add(ingredientLayout, 0, 4);

            var addRow = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.LeftToRight };
            _quantityInput = new NumericUpDown { Minimum = 1, Maximum = 20, Width = 70, Font = BodyFont(), Value = 1 };
            var addButton = ActionButton("Add Item");
            addButton.Click += delegate { AddSelectedItem(); };
            var removeButton = SecondaryButton("Remove");
            removeButton.Click += delegate { RemoveSelectedCartLine(); };
            addRow.Controls.Add(FieldLabel("Qty"));
            addRow.Controls.Add(_quantityInput);
            addRow.Controls.Add(addButton);
            addRow.Controls.Add(removeButton);
            layout.Controls.Add(addRow, 0, 5);

            _cartList = new ListView { Dock = DockStyle.Fill, View = View.Details, FullRowSelect = true, GridLines = true, Font = BodyFont() };
            _cartList.Columns.Add("Item", 230);
            _cartList.Columns.Add("Qty", 55);
            _cartList.Columns.Add("Total", 90);
            layout.Controls.Add(_cartList, 0, 6);

            _cartTotalLabel = new Label { Dock = DockStyle.Fill, Font = new Font("Segoe UI", 12, FontStyle.Bold), TextAlign = ContentAlignment.MiddleRight };
            layout.Controls.Add(_cartTotalLabel, 0, 7);

            var notesHost = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 2 };
            notesHost.RowStyles.Add(new RowStyle(SizeType.Absolute, 24));
            notesHost.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            notesHost.Controls.Add(new Label { Text = "Kitchen Notes", Dock = DockStyle.Fill, Font = BodyFont(), TextAlign = ContentAlignment.MiddleLeft }, 0, 0);
            _notesBox = new TextBox { Dock = DockStyle.Fill, Multiline = true, Font = BodyFont() };
            notesHost.Controls.Add(_notesBox, 0, 1);
            layout.Controls.Add(notesHost, 0, 8);

            var submitButton = ActionButton("Send To Kitchen");
            submitButton.Dock = DockStyle.Fill;
            submitButton.Click += delegate { SubmitOrder(); };
            layout.Controls.Add(submitButton, 0, 9);

            var menuAdminRow = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.LeftToRight };
            var addMenuButton = SecondaryButton("Add Menu");
            addMenuButton.Click += delegate { AddMenuItem(); };
            var restockButton = SecondaryButton("Restock");
            restockButton.Click += delegate { RestockSelectedMenuItem(); };
            var toggleItemButton = SecondaryButton("Toggle Item");
            toggleItemButton.Click += delegate { ToggleSelectedMenuItem(); };
            menuAdminRow.Controls.Add(addMenuButton);
            menuAdminRow.Controls.Add(restockButton);
            menuAdminRow.Controls.Add(toggleItemButton);
            layout.Controls.Add(menuAdminRow, 0, 10);

            panel.Controls.Add(layout);
            return panel;
        }

        private Control BuildKitchenPanel()
        {
            var panel = CreatePanel();
            var layout = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 7 };
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 42));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 42));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 126));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 190));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 70));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 76));

            _kitchenTitleLabel = SectionTitle("Kitchen Tracking");
            layout.Controls.Add(_kitchenTitleLabel, 0, 0);

            var filterRow = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.LeftToRight };
            filterRow.Controls.Add(FieldLabel("Filter"));
            _statusFilterCombo = new ComboBox { Width = 150, DropDownStyle = ComboBoxStyle.DropDownList, Font = BodyFont() };
            _statusFilterCombo.Items.AddRange(new object[] { "All", "New", "Preparing", "Ready", "Served", "Paid", "Cancelled" });
            _statusFilterCombo.SelectedIndex = 0;
            _statusFilterCombo.SelectedIndexChanged += delegate { RefreshOrders(); };
            filterRow.Controls.Add(_statusFilterCombo);
            layout.Controls.Add(filterRow, 0, 1);

            _ordersGrid = new DataGridView
            {
                Dock = DockStyle.Fill,
                ReadOnly = true,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.DisplayedCells,
                ScrollBars = ScrollBars.Both,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.None,
                Font = new Font("Segoe UI", 9, FontStyle.Regular),
                RowHeadersWidth = 28
            };
            _ordersGrid.SelectionChanged += delegate { RefreshReceiptPreview(); };
            layout.Controls.Add(_ordersGrid, 0, 2);

            var statusHost = new Panel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true,
                BackColor = Color.White
            };
            var statusRow = new FlowLayoutPanel
            {
                Dock = DockStyle.Top,
                AutoSize = true,
                WrapContents = true,
                FlowDirection = FlowDirection.LeftToRight,
                Padding = new Padding(0, 0, 8, 8)
            };
            var preparingButton = SecondaryButton("Start Preparing");
            preparingButton.Click += delegate { ChangeSelectedStatus(OrderStatus.Preparing); };
            var readyButton = SecondaryButton("Mark Ready");
            readyButton.Click += delegate { ChangeSelectedStatus(OrderStatus.Ready); };
            var servedButton = SecondaryButton("Served");
            servedButton.Click += delegate { ChangeSelectedStatus(OrderStatus.Served); };
            var paidButton = ActionButton("Paid");
            paidButton.Click += delegate { ChangeSelectedStatus(OrderStatus.Paid); };
            var splitPayButton = SecondaryButton("Split Pay");
            splitPayButton.Click += delegate { SplitPaySelectedOrder(); };
            var cancelButton = DangerButton("Cancel");
            cancelButton.Click += delegate { ChangeSelectedStatus(OrderStatus.Cancelled); };
            var clearButton = SecondaryButton("Clear Cancelled");
            clearButton.Click += delegate { ClearClosedOrders(); };
            var receiptButton = SecondaryButton("Save Receipt");
            receiptButton.Click += delegate { SaveSelectedReceipt(); };
            var reportButton = SecondaryButton("Day Report");
            reportButton.Click += delegate { ShowDayReport(); };
            var transferButton = SecondaryButton("Transfer");
            transferButton.Click += delegate { TransferSelectedOrder(); };
            var mergeButton = SecondaryButton("Merge Table");
            mergeButton.Click += delegate { MergeSelectedTable(); };
            var reserveButton = SecondaryButton("Reserve");
            reserveButton.Click += delegate { SetSelectedTableState(TableState.Reserved); };
            var cleanButton = SecondaryButton("Cleaning");
            cleanButton.Click += delegate { SetSelectedTableState(TableState.Cleaning); };
            var availableButton = SecondaryButton("Available");
            availableButton.Click += delegate { SetSelectedTableState(TableState.Available); };
            statusRow.Controls.Add(preparingButton);
            statusRow.Controls.Add(readyButton);
            statusRow.Controls.Add(servedButton);
            statusRow.Controls.Add(paidButton);
            statusRow.Controls.Add(splitPayButton);
            statusRow.Controls.Add(cancelButton);
            statusRow.Controls.Add(clearButton);
            statusRow.Controls.Add(receiptButton);
            statusRow.Controls.Add(reportButton);
            statusRow.Controls.Add(transferButton);
            statusRow.Controls.Add(mergeButton);
            statusRow.Controls.Add(reserveButton);
            statusRow.Controls.Add(cleanButton);
            statusRow.Controls.Add(availableButton);
            statusHost.Controls.Add(statusRow);
            layout.Controls.Add(statusHost, 0, 3);

            _receiptBox = new TextBox
            {
                Dock = DockStyle.Fill,
                Multiline = true,
                ReadOnly = true,
                ScrollBars = ScrollBars.Vertical,
                Font = new Font("Consolas", 9, FontStyle.Regular),
                BackColor = Color.FromArgb(250, 250, 250)
            };
            layout.Controls.Add(_receiptBox, 0, 4);

            _salesSummaryLabel = new Label
            {
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleLeft,
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                ForeColor = Color.FromArgb(15, 23, 42),
                BackColor = Color.FromArgb(240, 253, 244),
                Padding = new Padding(10, 0, 0, 0),
                AutoEllipsis = false
            };
            layout.Controls.Add(_salesSummaryLabel, 0, 5);

            _summaryLabel = new Label
            {
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleLeft,
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                ForeColor = Color.FromArgb(28, 35, 45),
                Padding = new Padding(4, 0, 0, 0),
                AutoEllipsis = false
            };
            layout.Controls.Add(_summaryLabel, 0, 6);

            panel.Controls.Add(layout);
            return panel;
        }

        private void SeedTables()
        {
            for (var number = 1; number <= 16; number++)
            {
                _tables.Add(new RestaurantTable(number));
            }
        }

        private void LoadOrSeedData()
        {
            var state = DataStore.Load();
            if (state != null && state.MenuItems != null && state.Orders != null && state.Tables != null)
            {
                _menuItems.AddRange(state.MenuItems);
                _orders.AddRange(state.Orders);
                _tables.AddRange(state.Tables);
                _nextOrderId = Math.Max(state.NextOrderId, _orders.Count == 0 ? 1001 : _orders.Max(order => order.Id) + 1);
                return;
            }

            SeedTables();
            SeedMenu();
            SeedDemoOrders();
            SaveState();
        }

        private void SeedMenu()
        {
            _menuItems.Add(new MenuItem(1, "Grilled Chicken", "Main Courses", 260, 18, new[] { "chicken", "rice", "pepper", "sauce" }, 40));
            _menuItems.Add(new MenuItem(2, "Beef Burger", "Main Courses", 245, 14, new[] { "bun", "beef patty", "cheese", "lettuce", "tomato", "onion" }, 35));
            _menuItems.Add(new MenuItem(3, "Penne Alfredo", "Main Courses", 220, 12, new[] { "pasta", "cream", "mushroom", "parmesan" }, 30));
            _menuItems.Add(new MenuItem(4, "Margherita Pizza", "Main Courses", 235, 16, new[] { "dough", "mozzarella", "tomato sauce", "basil" }, 25));
            _menuItems.Add(new MenuItem(5, "Shepherd Salad", "Salads", 115, 6, new[] { "tomato", "cucumber", "pepper", "onion", "parsley" }, 45));
            _menuItems.Add(new MenuItem(6, "Caesar Salad", "Salads", 145, 8, new[] { "lettuce", "chicken", "croutons", "parmesan", "caesar sauce" }, 35));
            _menuItems.Add(new MenuItem(7, "Cheesecake", "Desserts", 130, 5, new[] { "biscuit", "cream cheese", "cream", "berries" }, 24));
            _menuItems.Add(new MenuItem(8, "Chocolate Souffle", "Desserts", 155, 9, new[] { "chocolate", "egg", "flour", "butter" }, 22));
            _menuItems.Add(new MenuItem(9, "Lemonade", "Drinks", 75, 2, new[] { "lemon", "mint", "sugar", "ice" }, 60));
            _menuItems.Add(new MenuItem(10, "Turkish Coffee", "Drinks", 65, 4, new[] { "coffee", "water", "sugar" }, 80));
        }

        private void SeedDemoOrders()
        {
            var orderOne = new RestaurantOrder(_nextOrderId++, 4, "Ayse", "No onion", new[]
            {
                new OrderLine(_menuItems[0], 2, "remove: pepper"),
                new OrderLine(_menuItems[8], 2, "add/note: less sugar")
            });
            orderOne.Status = OrderStatus.Preparing;
            orderOne.LastActivityAt = DateTime.Now.AddMinutes(-32);

            var orderTwo = new RestaurantOrder(_nextOrderId++, 7, "Mehmet", "Dessert after main course", new[]
            {
                new OrderLine(_menuItems[3], 1, ""),
                new OrderLine(_menuItems[7], 1, ""),
                new OrderLine(_menuItems[9], 1, "medium sugar")
            });
            orderTwo.Status = OrderStatus.Ready;
            orderTwo.LastActivityAt = DateTime.Now.AddMinutes(-47);

            _orders.Add(orderOne);
            _orders.Add(orderTwo);
            UpdateTableFromOrder(orderOne);
            UpdateTableFromOrder(orderTwo);
        }

        private void RefreshClock()
        {
            _clockLabel.Text = _loggedInWaiter.Role + ": " + _loggedInWaiter.DisplayName + "     System time: " + DateTime.Now.ToString("HH:mm:ss");
        }

        private void LogOut()
        {
            IsLoggingOut = true;
            Close();
        }

        private void RefreshMenu()
        {
            if (_categoryCombo.Items.Count == 0)
            {
                _categoryCombo.Items.Add("All");
                foreach (var category in new[] { "Drinks", "Main Courses", "Desserts", "Salads" })
                {
                    _categoryCombo.Items.Add(category);
                }
                _categoryCombo.SelectedIndex = 0;
            }

            var selectedCategory = _categoryCombo.SelectedItem == null ? "All" : _categoryCombo.SelectedItem.ToString();
            _menuList.Items.Clear();
            foreach (var item in _menuItems.Where(item => selectedCategory == "All" || item.Category == selectedCategory)
                .Where(item => item.IsActive || _loggedInWaiter.Role == UserRole.Manager))
            {
                _menuList.Items.Add(item);
            }

            if (_menuList.Items.Count > 0)
            {
                _menuList.SelectedIndex = 0;
            }

            RefreshIngredientOptions();
        }

        private void RefreshIngredientOptions()
        {
            if (_removeIngredientList == null)
            {
                return;
            }

            _removeIngredientList.Items.Clear();
            _extraIngredientBox.Clear();
            var item = _menuList.SelectedItem as MenuItem;
            if (item == null)
            {
                return;
            }

            foreach (var ingredient in item.Ingredients)
            {
                _removeIngredientList.Items.Add(ingredient, false);
            }
        }

        private void AddSelectedItem()
        {
            var item = _menuList.SelectedItem as MenuItem;
            if (item == null)
            {
                MessageBox.Show("Please select a menu item.", "Missing item", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            if (!item.IsActive || item.StockQuantity < (int)_quantityInput.Value)
            {
                MessageBox.Show("This item is unavailable or does not have enough stock.", "Stock warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var customization = BuildCustomizationText();
            var existingLine = _currentLines.FirstOrDefault(line => line.Item.Id == item.Id && line.Customization == customization);
            if (existingLine == null)
            {
                _currentLines.Add(new OrderLine(item, (int)_quantityInput.Value, customization));
            }
            else
            {
                existingLine.Quantity += (int)_quantityInput.Value;
            }

            RefreshCart();
        }

        private string BuildCustomizationText()
        {
            var removed = _removeIngredientList.CheckedItems.Cast<string>().ToList();
            var parts = new List<string>();
            if (removed.Count > 0)
            {
                parts.Add("remove: " + string.Join(", ", removed));
            }

            if (!string.IsNullOrWhiteSpace(_extraIngredientBox.Text))
            {
                parts.Add("add/note: " + _extraIngredientBox.Text.Trim());
            }

            return string.Join("; ", parts);
        }

        private void RemoveSelectedCartLine()
        {
            if (_cartList.SelectedItems.Count == 0)
            {
                return;
            }

            var index = (int)_cartList.SelectedItems[0].Tag;
            if (index >= 0 && index < _currentLines.Count)
            {
                _currentLines.RemoveAt(index);
                RefreshCart();
            }
        }

        private void SubmitOrder()
        {
            if (!EnsureRole(UserRole.Waiter, UserRole.Manager))
            {
                return;
            }

            if (_currentLines.Count == 0)
            {
                MessageBox.Show("Add at least one item before sending the order.", "Empty order", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            foreach (var line in _currentLines)
            {
                if (!line.Item.IsActive || line.Item.StockQuantity < line.Quantity)
                {
                    MessageBox.Show(line.Item.Name + " does not have enough stock.", "Stock warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
            }

            foreach (var line in _currentLines)
            {
                line.Item.StockQuantity -= line.Quantity;
            }

            var tableNumber = (int)_tableInput.Value;
            var notes = _notesBox.Text.Trim();
            var order = new RestaurantOrder(_nextOrderId++, tableNumber, _loggedInWaiter.DisplayName, notes, _currentLines.Select(line => new OrderLine(line.Item, line.Quantity, line.Customization)));
            _orders.Add(order);
            _selectedTableNumber = tableNumber;
            UpdateTableFromOrder(order);
            _currentLines.Clear();
            _notesBox.Clear();
            RefreshCart();
            RefreshMenu();
            RefreshOrders();
            RefreshTables();
            SaveState();
        }

        private void SelectTable(int tableNumber)
        {
            _tableInput.Value = tableNumber;
            _selectedTableNumber = tableNumber;
            _statusFilterCombo.SelectedIndex = 0;
            RefreshOrders();
            RefreshTables();
        }

        private void ClearTableSelection()
        {
            _selectedTableNumber = null;
            RefreshOrders();
            RefreshTables();
        }

        private void ChangeSelectedStatus(OrderStatus status)
        {
            if (status == OrderStatus.Paid && !EnsureRole(UserRole.Cashier, UserRole.Manager))
            {
                return;
            }

            if ((status == OrderStatus.Preparing || status == OrderStatus.Ready) && !EnsureRole(UserRole.Kitchen, UserRole.Manager))
            {
                return;
            }

            var selected = GetSelectedOrder();
            if (selected == null)
            {
                MessageBox.Show("Please select an order first.", "No order selected", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var previousStatus = selected.Status;

            if (status == OrderStatus.Paid)
            {
                var paymentMethod = AskPaymentMethod(selected);
                if (paymentMethod == PaymentMethod.None)
                {
                    return;
                }

                selected.PaymentMethod = paymentMethod;
                selected.Payments.Clear();
                selected.Payments.Add(new PaymentRecord(paymentMethod, selected.Total));
            }

            selected.Status = status;
            selected.LastActivityAt = DateTime.Now;
            if (status == OrderStatus.Cancelled && previousStatus != OrderStatus.Cancelled)
            {
                RestoreStock(selected);
            }
            UpdateTableStates();
            RefreshOrders();
            RefreshTables();
            RefreshMenu();
            SaveState();
        }

        private void ClearClosedOrders()
        {
            if (!EnsureRole(UserRole.Manager))
            {
                return;
            }

            _orders.RemoveAll(order => order.Status == OrderStatus.Cancelled);
            UpdateTableStates();
            RefreshOrders();
            RefreshTables();
            SaveState();
        }

        private RestaurantOrder GetSelectedOrder()
        {
            if (_ordersGrid.SelectedRows.Count == 0)
            {
                return null;
            }

            var id = (int)_ordersGrid.SelectedRows[0].Cells["Id"].Value;
            return _orders.FirstOrDefault(order => order.Id == id);
        }

        private void RefreshCart()
        {
            _cartList.Items.Clear();
            for (var index = 0; index < _currentLines.Count; index++)
            {
                var line = _currentLines[index];
                var row = new ListViewItem(line.DisplayName) { Tag = index };
                row.SubItems.Add(line.Quantity.ToString());
                row.SubItems.Add(line.Total.ToString("C"));
                _cartList.Items.Add(row);
            }

            _cartTotalLabel.Text = "Order total: " + _currentLines.Sum(line => line.Total).ToString("C");
        }

        private void RefreshOrders()
        {
            var previous = GetSelectedOrder();
            int? previousId = previous == null ? (int?)null : previous.Id;
            var selectedFilter = _statusFilterCombo == null || _statusFilterCombo.SelectedItem == null
                ? "All"
                : _statusFilterCombo.SelectedItem.ToString();

            _ordersGrid.DataSource = _orders
                .Where(order => selectedFilter == "All" || order.Status.ToString() == selectedFilter)
                .Where(order => _selectedTableNumber == null || order.TableNumber == _selectedTableNumber.Value)
                .Where(order => _selectedTableNumber == null || (order.Status != OrderStatus.Paid && order.Status != OrderStatus.Cancelled))
                .OrderBy(order => order.Status == OrderStatus.Paid)
                .ThenBy(order => order.Status == OrderStatus.Cancelled)
                .ThenBy(order => order.CreatedAt)
                .Select(order => new
                {
                    order.Id,
                    Table = order.TableNumber,
                    Server = order.ServerName,
                    Time = order.CreatedAt.ToString("HH:mm:ss"),
                    Elapsed = FormatMinutes(DateTime.Now - order.CreatedAt),
                    Idle = FormatMinutes(DateTime.Now - order.LastActivityAt),
                    Status = order.Status.ToString(),
                    Payment = order.PaymentMethod == PaymentMethod.None ? "" : order.PaymentMethod.ToString(),
                    Items = order.ItemsSummary,
                    Notes = order.Notes,
                    ETA = order.EstimatedPrepMinutes + " min",
                    Total = order.Total.ToString("C")
                })
                .ToList();

            foreach (DataGridViewRow row in _ordersGrid.Rows)
            {
                var status = row.Cells["Status"].Value.ToString();
                row.DefaultCellStyle.BackColor = StatusColor(status);
                if (previousId != null && (int)row.Cells["Id"].Value == previousId.Value)
                {
                    row.Selected = true;
                }
            }

            var activeCount = _orders.Count(order => order.Status != OrderStatus.Paid && order.Status != OrderStatus.Cancelled);
            var readyCount = _orders.Count(order => order.Status == OrderStatus.Ready);
            var todaysOrders = _orders.Where(order => order.CreatedAt.Date == DateTime.Today).ToList();
            var revenue = todaysOrders.Where(order => order.Status == OrderStatus.Paid).Sum(order => order.Total);
            var cashRevenue = todaysOrders.SelectMany(order => order.Payments).Where(payment => payment.Method == PaymentMethod.Cash).Sum(payment => payment.Amount);
            var cardRevenue = todaysOrders.SelectMany(order => order.Payments).Where(payment => payment.Method == PaymentMethod.Card).Sum(payment => payment.Amount);
            var cancelledCount = _orders.Count(order => order.Status == OrderStatus.Cancelled);
            var visibleOrders = _orders.Count(order => (_selectedTableNumber == null || order.TableNumber == _selectedTableNumber.Value)
                && (_selectedTableNumber == null || (order.Status != OrderStatus.Paid && order.Status != OrderStatus.Cancelled))
                && (selectedFilter == "All" || order.Status.ToString() == selectedFilter));
            _kitchenTitleLabel.Text = _selectedTableNumber == null ? "Kitchen Tracking" : "Table " + _selectedTableNumber.Value + " Orders";
            _salesSummaryLabel.Text = $"Daily Sales     Cash: {cashRevenue:C}     Card: {cardRevenue:C}     Total: {revenue:C}";
            _summaryLabel.Text = $"Visible: {visibleOrders}     Active orders: {activeCount}     Ready: {readyCount}     Cancelled: {cancelledCount}     Paid revenue: {revenue:C}";
            RefreshReceiptPreview();
        }

        private PaymentMethod AskPaymentMethod(RestaurantOrder order)
        {
            using (var paymentForm = new PaymentMethodForm(order))
            {
                return paymentForm.ShowDialog(this) == DialogResult.OK
                    ? paymentForm.SelectedPaymentMethod
                    : PaymentMethod.None;
            }
        }

        private void SplitPaySelectedOrder()
        {
            if (!EnsureRole(UserRole.Waiter, UserRole.Cashier, UserRole.Manager))
            {
                return;
            }

            var selected = GetSelectedOrder();
            if (selected == null)
            {
                MessageBox.Show("Please select an order first.", "No order selected", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var cashText = PromptDialog.Ask("Split Payment", "Cash amount. The remaining balance will be paid by card.", "0");
            if (cashText == null)
            {
                return;
            }

            decimal cashAmount;
            if (!decimal.TryParse(cashText, out cashAmount) || cashAmount < 0 || cashAmount > selected.Total)
            {
                MessageBox.Show("Please enter a valid cash amount.", "Invalid amount", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var cardAmount = selected.Total - cashAmount;
            selected.Payments.Clear();
            if (cashAmount > 0)
            {
                selected.Payments.Add(new PaymentRecord(PaymentMethod.Cash, cashAmount));
            }

            if (cardAmount > 0)
            {
                selected.Payments.Add(new PaymentRecord(PaymentMethod.Card, cardAmount));
            }

            selected.PaymentMethod = cashAmount > 0 && cardAmount > 0
                ? PaymentMethod.Split
                : (cashAmount > 0 ? PaymentMethod.Cash : PaymentMethod.Card);
            selected.Status = OrderStatus.Paid;
            selected.LastActivityAt = DateTime.Now;
            UpdateTableStates();
            RefreshOrders();
            RefreshTables();
            SaveState();
        }

        private void SaveSelectedReceipt()
        {
            var selected = GetSelectedOrder();
            if (selected == null)
            {
                MessageBox.Show("Please select an order first.", "No order selected", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var path = DataStore.SaveReceipt(selected);
            MessageBox.Show("Receipt saved:\r\n" + path, "Receipt saved", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void ShowDayReport()
        {
            var todaysOrders = _orders.Where(order => order.CreatedAt.Date == DateTime.Today).ToList();
            var paidOrders = todaysOrders.Where(order => order.Status == OrderStatus.Paid).ToList();
            var cashRevenue = todaysOrders.SelectMany(order => order.Payments).Where(payment => payment.Method == PaymentMethod.Cash).Sum(payment => payment.Amount);
            var cardRevenue = todaysOrders.SelectMany(order => order.Payments).Where(payment => payment.Method == PaymentMethod.Card).Sum(payment => payment.Amount);
            var totalRevenue = paidOrders.Sum(order => order.Total);
            var bestSeller = todaysOrders.SelectMany(order => order.Lines)
                .GroupBy(line => line.Item.Name)
                .Select(group => new { Name = group.Key, Quantity = group.Sum(line => line.Quantity) })
                .OrderByDescending(item => item.Quantity)
                .FirstOrDefault();
            var waiterStats = todaysOrders
                .GroupBy(order => order.ServerName)
                .Select(group => group.Key + ": " + group.Count() + " orders")
                .ToList();

            var report = "End Of Day Report" + Environment.NewLine
                + "Paid orders: " + paidOrders.Count + Environment.NewLine
                + "Cancelled orders: " + todaysOrders.Count(order => order.Status == OrderStatus.Cancelled) + Environment.NewLine
                + "Cash total: " + cashRevenue.ToString("C") + Environment.NewLine
                + "Card total: " + cardRevenue.ToString("C") + Environment.NewLine
                + "Grand total: " + totalRevenue.ToString("C") + Environment.NewLine
                + "Best-selling item: " + (bestSeller == null ? "-" : bestSeller.Name + " (" + bestSeller.Quantity + ")") + Environment.NewLine
                + Environment.NewLine
                + "Waiter performance:" + Environment.NewLine
                + string.Join(Environment.NewLine, waiterStats);

            MessageBox.Show(report, "End Of Day Report", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void TransferSelectedOrder()
        {
            if (!EnsureRole(UserRole.Waiter, UserRole.Manager))
            {
                return;
            }

            var selected = GetSelectedOrder();
            if (selected == null)
            {
                MessageBox.Show("Please select an order first.", "No order selected", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var tableText = PromptDialog.Ask("Transfer Order", "Target table number", selected.TableNumber.ToString());
            int targetTable;
            if (tableText == null || !int.TryParse(tableText, out targetTable) || targetTable < 1 || targetTable > 16)
            {
                return;
            }

            selected.TableNumber = targetTable;
            selected.LastActivityAt = DateTime.Now;
            _selectedTableNumber = targetTable;
            UpdateTableStates();
            RefreshOrders();
            RefreshTables();
            SaveState();
        }

        private void MergeSelectedTable()
        {
            if (!EnsureRole(UserRole.Waiter, UserRole.Manager))
            {
                return;
            }

            if (_selectedTableNumber == null)
            {
                MessageBox.Show("Select the source table first.", "No table selected", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var tableText = PromptDialog.Ask("Merge Table", "Move all active orders to table number", _selectedTableNumber.Value.ToString());
            int targetTable;
            if (tableText == null || !int.TryParse(tableText, out targetTable) || targetTable < 1 || targetTable > 16)
            {
                return;
            }

            foreach (var order in _orders.Where(order => order.TableNumber == _selectedTableNumber.Value && order.Status != OrderStatus.Paid && order.Status != OrderStatus.Cancelled))
            {
                order.TableNumber = targetTable;
                order.LastActivityAt = DateTime.Now;
            }

            _selectedTableNumber = targetTable;
            UpdateTableStates();
            RefreshOrders();
            RefreshTables();
            SaveState();
        }

        private void SetSelectedTableState(TableState state)
        {
            if (!EnsureRole(UserRole.Waiter, UserRole.Manager))
            {
                return;
            }

            if (_selectedTableNumber == null)
            {
                MessageBox.Show("Select a table first.", "No table selected", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var table = _tables.FirstOrDefault(current => current.Number == _selectedTableNumber.Value);
            if (table == null)
            {
                return;
            }

            table.State = state;
            RefreshTables();
            SaveState();
        }

        private void AddMenuItem()
        {
            if (!EnsureRole(UserRole.Manager))
            {
                return;
            }

            var text = PromptDialog.Ask("Add Menu Item", "name|category|price|prep minutes|stock|ingredient,ingredient", "New Item|Main Courses|100|10|20|ingredient");
            if (text == null)
            {
                return;
            }

            var parts = text.Split('|');
            if (parts.Length < 6)
            {
                MessageBox.Show("Use this format: name|category|price|prep minutes|stock|ingredients", "Invalid menu item", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            decimal price;
            int prepMinutes;
            int stock;
            if (!decimal.TryParse(parts[2], out price) || !int.TryParse(parts[3], out prepMinutes) || !int.TryParse(parts[4], out stock))
            {
                MessageBox.Show("Price, prep minutes, and stock must be numeric.", "Invalid menu item", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var nextId = _menuItems.Count == 0 ? 1 : _menuItems.Max(item => item.Id) + 1;
            _menuItems.Add(new MenuItem(nextId, parts[0].Trim(), parts[1].Trim(), price, prepMinutes, parts[5].Split(',').Select(item => item.Trim()), stock));
            RefreshMenu();
            SaveState();
        }

        private void RestockSelectedMenuItem()
        {
            if (!EnsureRole(UserRole.Manager))
            {
                return;
            }

            var item = _menuList.SelectedItem as MenuItem;
            if (item == null)
            {
                return;
            }

            var stockText = PromptDialog.Ask("Restock Item", "Add stock quantity", "10");
            int stock;
            if (stockText == null || !int.TryParse(stockText, out stock) || stock < 0)
            {
                return;
            }

            item.StockQuantity += stock;
            item.IsActive = true;
            RefreshMenu();
            SaveState();
        }

        private void ToggleSelectedMenuItem()
        {
            if (!EnsureRole(UserRole.Manager))
            {
                return;
            }

            var item = _menuList.SelectedItem as MenuItem;
            if (item == null)
            {
                return;
            }

            item.IsActive = !item.IsActive;
            RefreshMenu();
            SaveState();
        }

        private void RefreshReceiptPreview()
        {
            if (_receiptBox == null)
            {
                return;
            }

            var selected = GetSelectedOrder();
            _receiptBox.Text = selected == null ? "Select an order to preview receipt details." : selected.ReceiptText;
        }

        private void UpdateTableFromOrder(RestaurantOrder order)
        {
            var table = _tables.FirstOrDefault(current => current.Number == order.TableNumber);
            if (table == null)
            {
                return;
            }

            table.LastOrderAt = order.CreatedAt;
            table.HasActiveOrder = order.Status != OrderStatus.Paid && order.Status != OrderStatus.Cancelled;
        }

        private void UpdateTableStates()
        {
            foreach (var table in _tables)
            {
                var latestActiveOrder = _orders
                    .Where(order => order.TableNumber == table.Number && order.Status != OrderStatus.Paid && order.Status != OrderStatus.Cancelled)
                    .OrderByDescending(order => order.CreatedAt)
                    .FirstOrDefault();

                table.LastOrderAt = latestActiveOrder == null ? (DateTime?)null : latestActiveOrder.CreatedAt;
                table.HasActiveOrder = latestActiveOrder != null;
            }
        }

        private void RefreshTables()
        {
            UpdateTableStates();
            foreach (var table in _tables)
            {
                var button = _tableButtons[table.Number];
                button.BackColor = TableColor(table);
                button.ForeColor = Color.FromArgb(15, 23, 42);
                button.FlatAppearance.BorderSize = _selectedTableNumber == table.Number ? 3 : 1;
                button.FlatAppearance.BorderColor = _selectedTableNumber == table.Number ? Color.FromArgb(15, 23, 42) : Color.FromArgb(148, 163, 184);
                button.Text = "Table " + table.Number + Environment.NewLine
                    + table.State + Environment.NewLine
                    + (table.LastOrderAt == null ? "No order" : table.MinutesSinceLastOrder + " min idle");
            }
        }

        private static Color TableColor(RestaurantTable table)
        {
            if (!table.HasActiveOrder)
            {
                if (table.State == TableState.Reserved)
                {
                    return Color.FromArgb(191, 219, 254);
                }

                if (table.State == TableState.Cleaning)
                {
                    return Color.FromArgb(203, 213, 225);
                }

                return Color.FromArgb(226, 232, 240);
            }

            if (table.MinutesSinceLastOrder >= 45)
            {
                return Color.FromArgb(248, 113, 113);
            }

            if (table.MinutesSinceLastOrder >= 30)
            {
                return Color.FromArgb(251, 191, 36);
            }

            return Color.FromArgb(134, 239, 172);
        }

        private static string FormatMinutes(TimeSpan value)
        {
            return Math.Floor(value.TotalMinutes) + " min";
        }

        private static Color StatusColor(string status)
        {
            switch (status)
            {
                case "New":
                    return Color.FromArgb(255, 249, 219);
                case "Preparing":
                    return Color.FromArgb(222, 235, 255);
                case "Ready":
                    return Color.FromArgb(220, 252, 231);
                case "Served":
                    return Color.FromArgb(238, 242, 255);
                case "Paid":
                    return Color.FromArgb(241, 245, 249);
                case "Cancelled":
                    return Color.FromArgb(254, 226, 226);
                default:
                    return Color.White;
            }
        }

        private bool EnsureRole(params UserRole[] roles)
        {
            if (roles.Contains(_loggedInWaiter.Role))
            {
                return true;
            }

            MessageBox.Show("This action is not available for the " + _loggedInWaiter.Role + " role.", "Permission denied", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return false;
        }

        private static void RestoreStock(RestaurantOrder order)
        {
            foreach (var line in order.Lines)
            {
                line.Item.StockQuantity += line.Quantity;
            }
        }

        private void SaveState()
        {
            DataStore.Save(new AppState
            {
                MenuItems = _menuItems,
                Orders = _orders,
                Tables = _tables,
                NextOrderId = _nextOrderId
            });
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            SaveState();
            base.OnFormClosing(e);
        }

        private static Panel CreatePanel()
        {
            return new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.White,
                Padding = new Padding(14),
                Margin = new Padding(8)
            };
        }

        private static Label SectionTitle(string text)
        {
            return new Label
            {
                Text = text,
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                ForeColor = Color.FromArgb(28, 35, 45),
                TextAlign = ContentAlignment.MiddleLeft
            };
        }

        private static Label FieldLabel(string text)
        {
            return new Label
            {
                Text = text,
                AutoSize = false,
                Width = 56,
                Height = 30,
                TextAlign = ContentAlignment.MiddleLeft,
                Font = BodyFont()
            };
        }

        private static Button ActionButton(string text)
        {
            return new Button
            {
                Text = text,
                AutoSize = true,
                MinimumSize = new Size(82, 34),
                Height = 34,
                BackColor = Color.FromArgb(22, 101, 52),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Margin = new Padding(6, 3, 6, 3),
                Padding = new Padding(12, 0, 12, 0)
            };
        }

        private static Button SecondaryButton(string text)
        {
            return new Button
            {
                Text = text,
                AutoSize = true,
                MinimumSize = new Size(82, 34),
                Height = 34,
                BackColor = Color.FromArgb(226, 232, 240),
                ForeColor = Color.FromArgb(28, 35, 45),
                FlatStyle = FlatStyle.Flat,
                Font = BodyFont(),
                Margin = new Padding(6, 3, 6, 3),
                Padding = new Padding(10, 0, 10, 0)
            };
        }

        private static Button DangerButton(string text)
        {
            return new Button
            {
                Text = text,
                AutoSize = true,
                MinimumSize = new Size(82, 34),
                Height = 34,
                BackColor = Color.FromArgb(185, 28, 28),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = BodyFont(),
                Margin = new Padding(6, 3, 6, 3),
                Padding = new Padding(10, 0, 10, 0)
            };
        }

        private static Font BodyFont()
        {
            return new Font("Segoe UI", 10, FontStyle.Regular);
        }
    }
}
