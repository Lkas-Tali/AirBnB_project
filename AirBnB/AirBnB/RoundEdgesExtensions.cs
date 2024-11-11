using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using System.Runtime.InteropServices;

namespace AirBnB
{
    public static class RoundEdgesExtensions
    {
        [DllImport("Gdi32.dll", EntryPoint = "CreateRoundRectRgn")]
        private static extern IntPtr CreateRoundRectRgn(int nLeftRect, int nTopRect,
            int nRightRect, int nBottomRect, int nWidthEllipse, int nHeightEllipse);

        public static void ApplyRoundedCorners(this TextBox textBox, int cornerRadius = 25)
        {

            textBox.BorderStyle = BorderStyle.None;

            // Create a Panel for the background
            var container = new Panel
            {
                Size = new Size(textBox.Width, textBox.Height),
                Location = textBox.Location,
                BackColor = Color.White,
                Parent = textBox.Parent
            };

            // Set rounded region for the panel
            container.Region = Region.FromHrgn(CreateRoundRectRgn(0, 0, container.Width, container.Height,
                cornerRadius, cornerRadius));

            // Move textbox to panel
            textBox.Parent = container;
            textBox.Location = new Point(10, 0); // Add left padding
            textBox.Width = container.Width - 20; // Account for padding
            textBox.BackColor = Color.White;

            // Handle container resize
            container.Resize += (sender, e) =>
            {
                container.Region = Region.FromHrgn(CreateRoundRectRgn(0, 0, container.Width, container.Height,
                    cornerRadius, cornerRadius));
            };

            // Handle textbox resize
            textBox.Resize += (sender, e) =>
            {
                container.Size = new Size(textBox.Width + 20, textBox.Height);
                container.Region = Region.FromHrgn(CreateRoundRectRgn(0, 0, container.Width, container.Height,
                    cornerRadius, cornerRadius));
            };
        }

        public static void ApplyRoundedCorners(this Button button, int cornerRadius = 75)
        {
            button.FlatStyle = FlatStyle.Flat;
            button.FlatAppearance.BorderSize = 0;
            button.Region = Region.FromHrgn(CreateRoundRectRgn(0, 0, button.Width, button.Height,
                cornerRadius, cornerRadius));

            // Handle button resize
            button.Resize += (sender, e) =>
            {
                button.Region = Region.FromHrgn(CreateRoundRectRgn(0, 0, button.Width, button.Height,
                    cornerRadius, cornerRadius));
            };
        }

        public static void ApplyRoundedCornersToAll(this Form form)
        {
            ApplyToControlCollection(form.Controls);
        }

        private static void ApplyToControlCollection(Control.ControlCollection controls)
        {
            foreach (Control control in controls)
            {
                // If the control is a TextBox, apply rounded corners
                if (control is TextBox textBox)
                {
                    textBox.ApplyRoundedCorners();
                }
                // If the control is a Button, apply rounded corners
                else if (control is Button button)
                {
                    // Only apply to specific buttons
                    switch (button.Name)
                    {
                        case "registerButton":
                        case "clearButton":
                        case "searchButton":
                        case "buttonPay":
                        case "button_FinalBook":
                        case "buttonCancelReservation":
                        case "uploadButton":
                        case "frontImageButton":
                            button.ApplyRoundedCorners();
                            break;
                    }
                }

                // Recursively check child controls
                if (control.HasChildren)
                {
                    ApplyToControlCollection(control.Controls);
                }
            }
        }
    }
}