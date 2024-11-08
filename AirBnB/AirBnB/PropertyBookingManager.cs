using Firebase.Database;
using Firebase.Database.Query;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AirBnB
{
    public class PropertyBookingManager
    {
        private FirebaseClient firebaseClient;
        private const int PROPERTY_CARD_WIDTH = 200;
        private const int PROPERTY_CARD_HEIGHT = 300;

        // Define event for property selection
        public event EventHandler<Dictionary<string, object>> PropertySelected;

        public PropertyBookingManager(FirebaseClient client)
        {
            firebaseClient = client;
        }

        public async Task<List<Dictionary<string, object>>> GetAvailablePropertiesFromFirebase()
        {
            var properties = await firebaseClient
                .Child("Available Properties")
                .OnceSingleAsync<Dictionary<string, Dictionary<string, object>>>();

            var propertyList = new List<Dictionary<string, object>>();

            if (properties != null)
            {
                foreach (var property in properties)
                {
                    if (property.Value != null)
                    {
                        var propertyData = new Dictionary<string, object>(property.Value);
                        propertyData["Username"] = property.Key;
                        propertyList.Add(propertyData);
                    }
                }
            }

            return propertyList;
        }

        public async void DisplayAvailableProperties(List<Dictionary<string, object>> properties, FlowLayoutPanel flowPanel)
        {
            flowPanel.Controls.Clear();
            flowPanel.AutoScroll = true;
            flowPanel.WrapContents = true;
            flowPanel.FlowDirection = FlowDirection.LeftToRight;
            flowPanel.Padding = new Padding(10);

            foreach (var property in properties)
            {
                Panel propertyCard = new Panel
                {
                    Width = PROPERTY_CARD_WIDTH,
                    Height = PROPERTY_CARD_HEIGHT,
                    Margin = new Padding(10),
                    BorderStyle = BorderStyle.FixedSingle,
                    BackColor = Color.White,
                    Cursor = Cursors.Hand
                };

                // Store the property data
                propertyCard.Tag = property;

                // Add click event handler
                propertyCard.Click += PropertyCard_Click;

                // Property Image
                PictureBox propertyImage = new PictureBox
                {
                    Width = PROPERTY_CARD_WIDTH - 20,
                    Height = 150,
                    Location = new Point(10, 10),
                    SizeMode = PictureBoxSizeMode.Zoom,
                    BorderStyle = BorderStyle.FixedSingle
                };

                try
                {
                    if (property.ContainsKey("Front Image"))
                    {
                        propertyImage.Load(property["Front Image"].ToString());
                    }
                }
                catch
                {
                    // Handle image loading error if needed
                }

                // Start labels right after the image
                int yPosition = 170; 
                int spacing = 25;

                // Get city from the nested "Address" dictionary
                string city = "Unknown";

                try
                {
                    var addressData = await firebaseClient
                        .Child("Available Properties")
                        .Child(property["Username"].ToString())
                        .Child("Address")
                        .OnceSingleAsync<Dictionary<string, object>>();

                    if (addressData != null && addressData.ContainsKey("City"))
                    {
                        city = addressData["City"].ToString();
                    }
                }
                catch (Exception ex)
                {
                    // Log or handle any potential errors
                    Console.WriteLine($"Error fetching city: {ex.Message}");
                }

                // City Label
                Label cityLabel = new Label
                {
                    Location = new Point(10, yPosition),
                    AutoSize = false,
                    Width = PROPERTY_CARD_WIDTH - 20,
                    Height = 20,
                    Text = $"City: {city}"
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
                    Text = $"Price per night: £{property["PricePerNight"]}"
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
                    Text = $"Host: {property["Name"]}"
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
                    Text = $"Contact: {property["Email"]}"
                };
                propertyCard.Controls.Add(contactLabel);

                // Add image to property card
                propertyCard.Controls.Add(propertyImage);


                foreach (Control control in propertyCard.Controls)
                {
                    control.Click += (s, e) => PropertyCard_Click(propertyCard, e);
                }

                // Add property card to flow panel
                flowPanel.Controls.Add(propertyCard);
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
    }
}