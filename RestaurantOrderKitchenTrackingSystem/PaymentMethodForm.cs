using System.Drawing;
using System.Windows.Forms;

namespace RestaurantOrderKitchenTrackingSystem
{
    public sealed class PaymentMethodForm : Form
    {
        public PaymentMethodForm(RestaurantOrder order)
        {
            Text = "Payment";
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            ClientSize = new Size(460, 250);
            BackColor = Color.FromArgb(247, 248, 250);

            BuildInterface(order);
        }

        public PaymentMethod SelectedPaymentMethod { get; private set; }

        private void BuildInterface(RestaurantOrder order)
        {
            var layout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 4,
                Padding = new Padding(22)
            };
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 42));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 38));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 38));

            layout.Controls.Add(new Label
            {
                Text = "Select Payment Method",
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleLeft,
                Font = new Font("Segoe UI", 18, FontStyle.Bold),
                ForeColor = Color.FromArgb(28, 35, 45)
            }, 0, 0);

            layout.Controls.Add(new Label
            {
                Text = "Order #" + order.Id + "     Table " + order.TableNumber + "     Total: " + order.Total.ToString("C"),
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleLeft,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                ForeColor = Color.FromArgb(71, 85, 105)
            }, 0, 1);

            var buttonLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 1
            };
            buttonLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
            buttonLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));

            var cashButton = PaymentButton("CASH", Color.FromArgb(22, 101, 52));
            cashButton.Click += delegate { Choose(PaymentMethod.Cash); };
            var cardButton = PaymentButton("CARD", Color.FromArgb(29, 78, 216));
            cardButton.Click += delegate { Choose(PaymentMethod.Card); };
            buttonLayout.Controls.Add(cashButton, 0, 0);
            buttonLayout.Controls.Add(cardButton, 1, 0);
            layout.Controls.Add(buttonLayout, 0, 2);

            var cancelButton = new Button
            {
                Text = "Cancel Payment",
                Dock = DockStyle.Right,
                Width = 140,
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(226, 232, 240),
                ForeColor = Color.FromArgb(28, 35, 45),
                Font = new Font("Segoe UI", 10, FontStyle.Regular)
            };
            cancelButton.Click += delegate { DialogResult = DialogResult.Cancel; Close(); };
            layout.Controls.Add(cancelButton, 0, 3);

            Controls.Add(layout);
        }

        private void Choose(PaymentMethod paymentMethod)
        {
            SelectedPaymentMethod = paymentMethod;
            DialogResult = DialogResult.OK;
            Close();
        }

        private static Button PaymentButton(string text, Color color)
        {
            return new Button
            {
                Text = text,
                Dock = DockStyle.Fill,
                Margin = new Padding(8),
                FlatStyle = FlatStyle.Flat,
                BackColor = color,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 20, FontStyle.Bold)
            };
        }
    }
}
