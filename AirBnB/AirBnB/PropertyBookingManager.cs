using Firebase.Database;
using System.Collections.Generic;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AirBnB
{
    public class PropertyBookingManager
    {
        private FirebaseClient firebaseClient;

        public PropertyBookingManager(FirebaseClient client)
        {
            firebaseClient = client;
        }

        public async Task<List<Property>> GetAvailablePropertiesFromFirebase()
        {
            List<Property> properties = new List<Property>();
            var availablePropertiesRef = firebaseClient.Child("Available Properties");
            var availableProperties = await availablePropertiesRef.OnceAsync<Dictionary<string, object>>();

            foreach (var property in availableProperties)
            {
                var data = property.Object;

                if (data.ContainsKey("Front Image") && data.ContainsKey("Address"))
                {
                    string frontImage = data["Front Image"].ToString();
                    string address = "";

                    if (data["Address"] is Dictionary<string, object> addressData)
                    {
                        if (addressData.ContainsKey("Address"))
                        {
                            address = addressData["Address"].ToString();
                        }
                    }
                    else
                    {
                        address = data["Address"].ToString();
                    }

                    Property newProperty = new Property
                    {
                        FrontImageUrl = frontImage,
                        Address = address
                    };

                    properties.Add(newProperty);
                }
            }

            return properties;
        }

        public void DisplayAvailableProperties(List<Property> properties, FlowLayoutPanel flowPanelBook)
        {
            flowPanelBook.Controls.Clear();

            foreach (var property in properties)
            {
                PictureBox pictureBox = new PictureBox
                {
                    SizeMode = PictureBoxSizeMode.Zoom,
                    Size = new Size(200, 200)
                };
                pictureBox.Load(property.FrontImageUrl);

                Label addressLabel = new Label
                {
                    Text = property.Address,
                    AutoSize = true,
                    TextAlign = ContentAlignment.MiddleCenter
                };

                Panel propertyPanel = new Panel
                {
                    Size = new Size(200, 250)
                };
                propertyPanel.Controls.Add(pictureBox);
                propertyPanel.Controls.Add(addressLabel);

                addressLabel.Location = new Point(0, pictureBox.Bottom);

                flowPanelBook.Controls.Add(propertyPanel);
            }
        }

        public async Task<List<Property>> SearchPropertiesByCity(string city)
        {
            List<Property> properties = new List<Property>();
            var availablePropertiesRef = firebaseClient.Child("Available Properties");
            var availableProperties = await availablePropertiesRef.OnceAsync<Dictionary<string, object>>();

            foreach (var property in availableProperties)
            {
                var data = property.Object;

                if (data.ContainsKey("Address") && data["Address"] is Dictionary<string, object> addressData)
                {
                    if (addressData.ContainsKey("City") && addressData["City"].ToString().ToLower() == city.ToLower())
                    {
                        string frontImage = data.ContainsKey("Front Image") ? data["Front Image"].ToString() : "";
                        string address = addressData.ContainsKey("Address") ? addressData["Address"].ToString() : "";

                        Property newProperty = new Property
                        {
                            FrontImageUrl = frontImage,
                            Address = address
                        };

                        properties.Add(newProperty);
                    }
                }
            }

            return properties;
        }
    }

    public class Property
    {
        public string FrontImageUrl { get; set; }
        public string Address { get; set; }
    }
}