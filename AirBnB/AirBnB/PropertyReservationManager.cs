using Firebase.Database;
using Firebase.Database.Query;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AirBnB
{
    internal class PropertyReservationManager
    {
        private FirebaseClient firebaseClient;
        private const int PROPERTY_CARD_WIDTH = 200;
        private const int PROPERTY_CARD_HEIGHT = 300;
        public event EventHandler<Dictionary<string, object>> ReservationSelected;

        public PropertyReservationManager(FirebaseClient client)
        {
            firebaseClient = client;
        }

        public async Task<List<Dictionary<string, object>>> RetrieveUserReservationDetails(string username)
        {
            var reservationData = await firebaseClient
                .Child("Reservations")
                .OnceAsync<Dictionary<string, object>>();

            var reservations = new List<Dictionary<string, object>>();

            foreach (var reservation in reservationData)
            {
                var reservationDetails = reservation.Object;

                // Check if the customerName matches
                if (reservationDetails != null &&
                    reservationDetails["customerName"].ToString() == username)
                {
                    // Add the Firebase key to the reservation details
                    reservationDetails["firebaseKey"] = reservation.Key;
                    reservations.Add(reservationDetails);
                }
            }

            return reservations;
        }

        public void DisplayUserReservations(List<Dictionary<string, object>> reservations, FlowLayoutPanel flowPanel)
        {
            flowPanel.Controls.Clear();
            flowPanel.AutoScroll = true;
            flowPanel.WrapContents = true;
            flowPanel.FlowDirection = FlowDirection.LeftToRight;
            flowPanel.Padding = new Padding(10);

            foreach (var reservation in reservations)
            {
                Panel reservationCard = new Panel
                {
                    Width = PROPERTY_CARD_WIDTH,
                    Height = PROPERTY_CARD_HEIGHT,
                    Margin = new Padding(10),
                    BackColor = Color.White,
                    Cursor = Cursors.Hand
                };

                // Add rounded corners
                int radius = 20;
                System.Drawing.Drawing2D.GraphicsPath path = new System.Drawing.Drawing2D.GraphicsPath();
                path.AddArc(0, 0, radius, radius, 180, 90);
                path.AddArc(reservationCard.Width - radius, 0, radius, radius, 270, 90);
                path.AddArc(reservationCard.Width - radius, reservationCard.Height - radius, radius, radius, 0, 90);
                path.AddArc(0, reservationCard.Height - radius, radius, radius, 90, 90);
                reservationCard.Region = new Region(path);

                // Store the property data
                reservationCard.Tag = reservation;

                // Add click event handler
                reservationCard.Click += reservationCard_Click;

                // Property Image
                PictureBox propertyImage = new PictureBox
                {
                    Width = PROPERTY_CARD_WIDTH - 20,
                    Height = 150,
                    Location = new Point(10, 10),
                    SizeMode = PictureBoxSizeMode.Zoom
                };

                // Add rounded corners
                int picRadius = 10; // Adjust this value to change how rounded the corners are
                System.Drawing.Drawing2D.GraphicsPath picPath = new System.Drawing.Drawing2D.GraphicsPath();
                picPath.AddArc(0, 0, picRadius, picRadius, 180, 90);
                picPath.AddArc(propertyImage.Width - picRadius, 0, picRadius, picRadius, 270, 90);
                picPath.AddArc(propertyImage.Width - picRadius, propertyImage.Height - picRadius, picRadius, picRadius, 0, 90);
                picPath.AddArc(0, propertyImage.Height - picRadius, picRadius, picRadius, 90, 90);
                propertyImage.Region = new Region(picPath);

                try
                {
                    if (reservation.ContainsKey("mainImage"))
                    {
                        propertyImage.Load(reservation["mainImage"].ToString());
                    }
                }
                catch
                {
                    // Handle image loading error if needed
                }

                // Start labels right after the image
                int yPosition = 170;
                int spacing = 25;

                // City Label
                Label cityLabel = new Label
                {
                    Location = new Point(10, yPosition),
                    AutoSize = false,
                    Width = PROPERTY_CARD_WIDTH - 20,
                    Height = 20,
                    Text = $"City: {reservation["city"]}"
                };
                reservationCard.Controls.Add(cityLabel);
                yPosition += spacing;

                // Conversion to decimal and int
                decimal pricePerNight = decimal.Parse(reservation["pricePerNight"].ToString());
                int nights = int.Parse(reservation["nights"].ToString());
                decimal totalPrice = pricePerNight * nights;

                // Price Label
                Label totalPriceLabel = new Label
                {
                    Location = new Point(10, yPosition),
                    AutoSize = false,
                    Width = PROPERTY_CARD_WIDTH - 20,
                    Height = 20,
                    Text = $"Total price: £{totalPrice:F2}"
                };
                reservationCard.Controls.Add(totalPriceLabel);
                yPosition += spacing;

                // Host Label
                Label hostLabel = new Label
                {
                    Location = new Point(10, yPosition),
                    AutoSize = false,
                    Width = PROPERTY_CARD_WIDTH - 20,
                    Height = 20,
                    Text = $"Host: {reservation["owner"]}"
                };
                reservationCard.Controls.Add(hostLabel);
                yPosition += spacing;

                // Contact Label
                Label contactLabel = new Label
                {
                    Location = new Point(10, yPosition),
                    AutoSize = false,
                    Width = PROPERTY_CARD_WIDTH - 20,
                    Height = 20,
                    Text = $"Contact: {reservation["email"]}"
                };
                reservationCard.Controls.Add(contactLabel);

                // Add image to property card
                reservationCard.Controls.Add(propertyImage);


                foreach (Control control in reservationCard.Controls)
                {
                    control.Click += (s, e) => reservationCard_Click(reservationCard, e);
                }

                // Add property card to flow panel
                flowPanel.Controls.Add(reservationCard);
            }
        }

        private void reservationCard_Click(object sender, EventArgs e)
        {
            if (sender is Panel propertyCard && propertyCard.Tag is Dictionary<string, object> propertyData)
            {
                // Trigger the event with the selected property data
                ReservationSelected?.Invoke(this, propertyData);
            }
        }

        public async Task CancelReservation(Dictionary<string, object> reservation)
        {
            try
            {
                if (reservation.ContainsKey("firebaseKey"))
                {
                    string reservationId = reservation["firebaseKey"].ToString();

                    await firebaseClient
                        .Child("Reservations")
                        .Child(reservationId)
                        .DeleteAsync();

                    MessageBox.Show("Reservation cancelled successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    MessageBox.Show("Could not find the reservation ID.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error cancelling reservation: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
