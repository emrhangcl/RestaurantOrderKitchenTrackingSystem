using System.Drawing;
using System.Windows.Forms;

namespace RestaurantOrderKitchenTrackingSystem
{
    public static class PromptDialog
    {
        public static string Ask(string title, string label, string defaultValue)
        {
            using (var form = new Form())
            using (var input = new TextBox())
            using (var okButton = new Button())
            using (var cancelButton = new Button())
            using (var promptLabel = new Label())
            {
                form.Text = title;
                form.StartPosition = FormStartPosition.CenterParent;
                form.FormBorderStyle = FormBorderStyle.FixedDialog;
                form.MaximizeBox = false;
                form.MinimizeBox = false;
                form.ClientSize = new Size(430, 150);

                promptLabel.Text = label;
                promptLabel.SetBounds(12, 12, 400, 26);
                promptLabel.Font = new Font("Segoe UI", 10, FontStyle.Regular);

                input.Text = defaultValue;
                input.SetBounds(12, 42, 400, 28);
                input.Font = new Font("Segoe UI", 10, FontStyle.Regular);

                okButton.Text = "OK";
                okButton.DialogResult = DialogResult.OK;
                okButton.SetBounds(236, 92, 84, 32);

                cancelButton.Text = "Cancel";
                cancelButton.DialogResult = DialogResult.Cancel;
                cancelButton.SetBounds(328, 92, 84, 32);

                form.Controls.AddRange(new Control[] { promptLabel, input, okButton, cancelButton });
                form.AcceptButton = okButton;
                form.CancelButton = cancelButton;

                return form.ShowDialog() == DialogResult.OK ? input.Text.Trim() : null;
            }
        }
    }
}
