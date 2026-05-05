using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace RestaurantOrderKitchenTrackingSystem
{
    public sealed class LoginForm : Form
    {
        private readonly List<WaiterAccount> _waiters = new List<WaiterAccount>
        {
            new WaiterAccount("ayse", "1234", "Ayse", UserRole.Waiter),
            new WaiterAccount("mehmet", "1234", "Mehmet", UserRole.Waiter),
            new WaiterAccount("chef", "1234", "Kitchen Staff", UserRole.Kitchen),
            new WaiterAccount("cashier", "1234", "Cashier", UserRole.Cashier),
            new WaiterAccount("manager", "1234", "Manager", UserRole.Manager)
        };

        private TextBox _usernameBox;
        private TextBox _passwordBox;
        private Label _messageLabel;

        public LoginForm()
        {
            Text = "Waiter Login";
            StartPosition = FormStartPosition.CenterScreen;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            ClientSize = new Size(380, 260);
            BackColor = Color.FromArgb(247, 248, 250);

            BuildInterface();
        }

        public WaiterAccount LoggedInWaiter { get; private set; }

        private void BuildInterface()
        {
            var layout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 7,
                Padding = new Padding(24)
            };
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 44));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 28));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 38));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 28));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 38));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 42));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 34));

            layout.Controls.Add(new Label
            {
                Text = "Waiter Login",
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI", 18, FontStyle.Bold),
                ForeColor = Color.FromArgb(28, 35, 45)
            }, 0, 0);

            layout.Controls.Add(FieldLabel("Username"), 0, 1);
            _usernameBox = new TextBox { Dock = DockStyle.Fill, Font = BodyFont(), Text = "ayse" };
            layout.Controls.Add(_usernameBox, 0, 2);

            layout.Controls.Add(FieldLabel("Password"), 0, 3);
            _passwordBox = new TextBox { Dock = DockStyle.Fill, Font = BodyFont(), PasswordChar = '*', Text = "1234" };
            _passwordBox.KeyDown += PasswordBoxKeyDown;
            layout.Controls.Add(_passwordBox, 0, 4);

            var loginButton = new Button
            {
                Text = "Log In",
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(22, 101, 52),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10, FontStyle.Bold)
            };
            loginButton.Click += delegate { TryLogin(); };
            layout.Controls.Add(loginButton, 0, 5);

            _messageLabel = new Label
            {
                Text = "Demo: ayse, chef, cashier, manager. Password: 1234",
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleLeft,
                Font = new Font("Segoe UI", 9, FontStyle.Regular),
                ForeColor = Color.FromArgb(100, 116, 139)
            };
            layout.Controls.Add(_messageLabel, 0, 6);

            Controls.Add(layout);
        }

        private void PasswordBoxKeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                TryLogin();
            }
        }

        private void TryLogin()
        {
            var waiter = _waiters.FirstOrDefault(account =>
                account.Username.Equals(_usernameBox.Text.Trim(), StringComparison.OrdinalIgnoreCase)
                && account.Password == _passwordBox.Text);

            if (waiter == null)
            {
                _messageLabel.Text = "Username or password is incorrect.";
                _messageLabel.ForeColor = Color.FromArgb(185, 28, 28);
                return;
            }

            LoggedInWaiter = waiter;
            DialogResult = DialogResult.OK;
            Close();
        }

        private static Label FieldLabel(string text)
        {
            return new Label
            {
                Text = text,
                Dock = DockStyle.Fill,
                Font = BodyFont(),
                TextAlign = ContentAlignment.MiddleLeft
            };
        }

        private static Font BodyFont()
        {
            return new Font("Segoe UI", 10, FontStyle.Regular);
        }
    }
}
