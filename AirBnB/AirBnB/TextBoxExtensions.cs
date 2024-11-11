using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using System.Runtime.InteropServices;

namespace AirBnB
{
    public static class TextBoxExtensions
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
            textBox.Location = new Point(10, 0);
            textBox.Width = container.Width - 20; 
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

        public static void ApplyRoundedCornersToAll(this Form form)
        {
            // Process all panels in the form
            foreach (Control control in form.Controls)
            {
                if (control is Panel panel)
                {
                    ApplyToPanel(panel);
                }
            }
        }

        private static void ApplyToPanel(Panel panel)
        {
            // Create a list to store textboxes
            var textBoxes = new System.Collections.Generic.List<TextBox>();

            foreach (Control control in panel.Controls)
            {
                if (control is TextBox textBox)
                {
                    textBoxes.Add(textBox);
                }
            }

            // Apply rounded corners to each textbox
            foreach (var textBox in textBoxes)
            {
                textBox.ApplyRoundedCorners();
            }
        }
    }
}