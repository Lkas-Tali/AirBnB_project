using Firebase.Database;
using Firebase.Database.Query;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Linq;
using System.Net.Http;
using System.Collections.Concurrent;

namespace AirBnB
{
    public class PropertyBookingManager
    {
        private FirebaseClient firebaseClient;
        private const int PROPERTY_CARD_WIDTH = 200;
        private const int PROPERTY_CARD_HEIGHT = 300;
        private static readonly HttpClient httpClient = new HttpClient();

        public event EventHandler<Dictionary<string, object>> PropertySelected;

        public PropertyBookingManager(FirebaseClient client)
        {
            firebaseClient = client;
            // Set timeout for faster image loading
            httpClient.Timeout = TimeSpan.FromSeconds(3);
        }

        public async Task<List<Dictionary<string, object>>> GetAvailablePropertiesFromFirebase()
        {
            var properties = await firebaseClient
                .Child("Available Properties")
                .OnceSingleAsync<Dictionary<string, Dictionary<string, object>>>();

            return properties?.Select(property =>
            {
                var propertyData = new Dictionary<string, object>(property.Value);
                propertyData["Username"] = property.Key;
                return propertyData;
            }).ToList() ?? new List<Dictionary<string, object>>();
        }

        public async void DisplayAvailableProperties(List<Dictionary<string, object>> properties, FlowLayoutPanel flowPanel)
        {
            try
            {
                flowPanel.SuspendLayout();
                flowPanel.Controls.Clear();
                flowPanel.AutoScroll = true;
                flowPanel.WrapContents = true;
                flowPanel.FlowDirection = FlowDirection.LeftToRight;
                flowPanel.Padding = new Padding(10);

                var loadingLabel = new Label
                {
                    Text = "Loading properties...",
                    Font = new Font("Nirmala UI", 20, FontStyle.Bold),
                    ForeColor = Color.FromArgb(255, 56, 92),
                    AutoSize = true,
                    Location = new Point(flowPanel.Width / 2 - 100, flowPanel.Height / 2 - 15),
                };
                flowPanel.Controls.Add(loadingLabel);
                flowPanel.ResumeLayout();

                for (int i = 0; i < properties.Count; i += 3)
                {
                    var batch = properties.Skip(i).Take(3);
                    var tasks = new List<Task>();

                    foreach (var property in batch)
                    {
                        var propertyCard = CreatePropertyCard(property);
                        flowPanel.Controls.Add(propertyCard);

                        if (property.ContainsKey("Front Image"))
                        {
                            tasks.Add(LoadPropertyImageAsync(property["Front Image"].ToString(), propertyCard));
                        }
                        tasks.Add(LoadPropertyAddressAsync(property, propertyCard));
                    }

                    if (i == 0)
                    {
                        flowPanel.Controls.Remove(loadingLabel);
                    }

                    await Task.WhenAll(tasks);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading properties: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                flowPanel.ResumeLayout();
            }
        }

        private Panel CreatePropertyCard(Dictionary<string, object> property, bool isLoading = false)
        {
            var card = new Panel
            {
                Width = PROPERTY_CARD_WIDTH,
                Height = PROPERTY_CARD_HEIGHT,
                Margin = new Padding(10),
                BackColor = Color.White,
                Cursor = Cursors.Hand,
                Tag = property
            };

            // Add rounded corners
            int radius = 20;
            System.Drawing.Drawing2D.GraphicsPath path = new System.Drawing.Drawing2D.GraphicsPath();
            path.AddArc(0, 0, radius, radius, 180, 90);
            path.AddArc(card.Width - radius, 0, radius, radius, 270, 90);
            path.AddArc(card.Width - radius, card.Height - radius, radius, radius, 0, 90);
            path.AddArc(0, card.Height - radius, radius, radius, 90, 90);
            card.Region = new Region(path);

            if (isLoading)
            {
                // Add a loading indicator instead of placeholder text
                var loadingLabel = new Label
                {
                    Text = "Loading...",
                    Font = new Font("Nirmala UI", 12, FontStyle.Bold),
                    ForeColor = Color.FromArgb(255, 56, 92),
                    AutoSize = true,
                    Location = new Point(card.Width / 2 - 30, card.Height / 2 - 10)
                };
                card.Controls.Add(loadingLabel);
            }
            else
            {
                // Add the regular controls
                AddPropertyControls(card, property);

                // Add click handlers to all controls including the card itself
                card.Click += (s, e) => PropertyCard_Click(card, e);

                // Make all child controls trigger the parent card's click
                foreach (Control control in card.Controls)
                {
                    control.Click += (s, e) => PropertyCard_Click(card, e);

                    // If the control is a PictureBox, make sure it's clickable
                    if (control is PictureBox pictureBox)
                    {
                        pictureBox.Cursor = Cursors.Hand;
                        pictureBox.Click += (s, e) => PropertyCard_Click(card, e);
                    }
                }
            }

            return card;
        }

        private void AddPropertyControls(Panel card, Dictionary<string, object> property)
        {
            // Add your existing control creation code here
            PictureBox propertyImage = CreatePropertyImageBox();
            card.Controls.Add(propertyImage);
            AddPropertyLabels(card, property);
        }

        private async Task LoadPropertyAddressAsync(Dictionary<string, object> property, Panel propertyCard)
        {
            try
            {
                var addressData = await firebaseClient
                    .Child("Available Properties")
                    .Child(property["Username"].ToString())
                    .Child("Address")
                    .OnceSingleAsync<Dictionary<string, object>>();

                if (!propertyCard.IsDisposed && addressData != null && addressData.ContainsKey("City"))
                {
                    if (propertyCard.InvokeRequired)
                    {
                        propertyCard.Invoke(new Action(() =>
                        {
                            var cityLabel = propertyCard.Controls.OfType<Label>().FirstOrDefault();
                            if (cityLabel != null && !cityLabel.IsDisposed)
                            {
                                cityLabel.Text = $"City: {addressData["City"]}";
                            }
                        }));
                    }
                    else
                    {
                        var cityLabel = propertyCard.Controls.OfType<Label>().FirstOrDefault();
                        if (cityLabel != null && !cityLabel.IsDisposed)
                        {
                            cityLabel.Text = $"City: {addressData["City"]}";
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading address: {ex.Message}");
            }
        }

        private async Task LoadPropertyImageAsync(string imageUrl, Panel propertyCard)
{
    try
    {
        using (var client = new HttpClient())
        {
            client.Timeout = TimeSpan.FromSeconds(10);
            var imageBytes = await client.GetByteArrayAsync(imageUrl);

            if (!propertyCard.IsDisposed)
            {
                using (var ms = new System.IO.MemoryStream(imageBytes))
                {
                    var image = Image.FromStream(ms);
                    if (propertyCard.InvokeRequired)
                    {
                        propertyCard.Invoke(new Action(() =>
                        {
                            var pictureBox = propertyCard.Controls.OfType<PictureBox>().FirstOrDefault();
                            if (pictureBox != null && !pictureBox.IsDisposed)
                            {
                                pictureBox.Image = image;
                            }
                        }));
                    }
                    else
                    {
                        var pictureBox = propertyCard.Controls.OfType<PictureBox>().FirstOrDefault();
                        if (pictureBox != null && !pictureBox.IsDisposed)
                        {
                            pictureBox.Image = image;
                        }
                    }
                }
            }
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error loading image: {ex.Message}");
    }
}

        private PictureBox CreatePropertyImageBox()
        {
            PictureBox propertyImage = new PictureBox
            {
                Width = PROPERTY_CARD_WIDTH - 20,
                Height = 150,
                Location = new Point(10, 10),
                SizeMode = PictureBoxSizeMode.Zoom,
                BackColor = Color.White,
                Cursor = Cursors.Hand  // Add cursor to indicate clickable
            };

            // Add rounded corners
            int picRadius = 10;
            System.Drawing.Drawing2D.GraphicsPath picPath = new System.Drawing.Drawing2D.GraphicsPath();
            picPath.AddArc(0, 0, picRadius, picRadius, 180, 90);
            picPath.AddArc(propertyImage.Width - picRadius, 0, picRadius, picRadius, 270, 90);
            picPath.AddArc(propertyImage.Width - picRadius, propertyImage.Height - picRadius, picRadius, picRadius, 0, 90);
            picPath.AddArc(0, propertyImage.Height - picRadius, picRadius, picRadius, 90, 90);
            propertyImage.Region = new Region(picPath);

            return propertyImage;
        }

        private void AddPropertyLabels(Panel propertyCard, Dictionary<string, object> property)
        {
            int yPosition = 170;
            int spacing = 25;

            // City Label (placeholder)
            Label cityLabel = new Label
            {
                Location = new Point(10, yPosition),
                AutoSize = false,
                Width = PROPERTY_CARD_WIDTH - 20,
                Height = 20,
                Text = "Loading city...",
                Font = new Font("Nirmala UI", 9.75f, FontStyle.Bold)
            };
            propertyCard.Controls.Add(cityLabel);
            yPosition += spacing;

            // Price Label
            Label priceLabel = new Label
            {
                Location = new Point(10, yPosition),
                AutoSize = false,
                Width = PROPERTY_CARD_WIDTH - 20,
                Height = 20,
                Text = $"Price per night: £{property["PricePerNight"]}",
                Font = new Font("Nirmala UI", 9.75f, FontStyle.Bold)
            };
            propertyCard.Controls.Add(priceLabel);
            yPosition += spacing;

            // Host Label
            Label hostLabel = new Label
            {
                Location = new Point(10, yPosition),
                AutoSize = false,
                Width = PROPERTY_CARD_WIDTH - 20,
                Height = 20,
                Text = $"Host: {property["Name"]}",
                Font = new Font("Nirmala UI", 9.75f, FontStyle.Bold)
            };
            propertyCard.Controls.Add(hostLabel);
            yPosition += spacing;

            // Contact Label
            Label contactLabel = new Label
            {
                Location = new Point(10, yPosition),
                AutoSize = false,
                Width = PROPERTY_CARD_WIDTH - 20,
                Height = 20,
                Text = $"Contact: {property["Email"]}",
                Font = new Font("Nirmala UI", 9.75f, FontStyle.Bold)
            };
            propertyCard.Controls.Add(contactLabel);

            // Add click handlers to all labels
            foreach (Control control in propertyCard.Controls)
            {
                if (control is Label)
                {
                    control.Click += (s, e) => PropertyCard_Click(propertyCard, e);
                }
            }
        }

        public async Task<List<Dictionary<string, object>>> SearchPropertiesByCity(string city)
        {
            if (string.IsNullOrWhiteSpace(city)) return await GetAvailablePropertiesFromFirebase();

            var allProperties = await GetAvailablePropertiesFromFirebase();
            var matchingProperties = new List<Dictionary<string, object>>();

            //Capitalise city
            city = char.ToUpper(city[0]) + city.Substring(1).ToLower();

            foreach (var property in allProperties)
            {
                var addressData = await firebaseClient
                        .Child("Available Properties")
                        .Child(property["Username"].ToString())
                        .Child("Address")
                        .OnceSingleAsync<Dictionary<string, object>>();

                if (addressData != null && addressData["City"].ToString() == city)
                {
                    matchingProperties.Add(property);
                }
            }

            return matchingProperties;
        }

        private void PropertyCard_Click(object sender, EventArgs e)
        {
            if (sender is Panel propertyCard && propertyCard.Tag is Dictionary<string, object> propertyData)
            {
                // Trigger the event with the selected property data
                PropertySelected?.Invoke(this, propertyData);
            }
        }

        public async void AddReservationToDatabase(string customerName, string endDate, int nights, string startDate, Dictionary<string, object> propertyData, Dictionary<string, object> propertyAddress)
        {

            //debug:
            // Debug: Print all keys in the dictionaries
            Console.WriteLine("Available propertyAddress:");
            foreach (var key in propertyAddress.Keys)
            {
                Console.WriteLine($"Key: '{key}'");
            }

            foreach (var key in propertyData.Keys)
            {
                Console.WriteLine($"Key: '{key}'");
            }

            // Initialize reservation data dictionary with booking details
            var reservationData = new Dictionary<string, object>
            {
                { "address", propertyAddress["Address"]},
                { "city", propertyAddress["City"] },
                { "customerName", customerName },
                { "description", propertyAddress["Description"] },
                { "email", propertyData["Email"] },
                { "endDate", endDate },
                { "mainImage", propertyData["Front Image"] },
                { "nights", nights },
                { "owner", propertyData["Name"] },
                { "pricePerNight", propertyData["PricePerNight"]},
                { "startDate", startDate },
                { "title", propertyAddress["Title"] }
            };

            await firebaseClient
                .Child("Reservations")
                .PostAsync(reservationData);
        }
    }
}