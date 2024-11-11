using Firebase.Database;
using Firebase.Database.Query;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Management;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.StartPanel;

namespace AirBnB
{
    public partial class frmDashboard : Form
    {
        private FirebaseClient firebaseClient;
        private ListPropertyManager listPropertyManager;
        private ListedPropertyViewer listedPropertyViewer;
        private PropertyBookingManager propertyBookingManager;
        private Dictionary<string, object> selectedPropertyData;
        private Dictionary<string, object> selectedReservationData;
        private PropertyReservationManager propertyReservationManager;

        const int IMAGE_PADDING = 10;
        private const int PROPERTY_CARD_WIDTH = 350;
        private const int PROPERTY_CARD_HEIGHT = 300;

        public frmDashboard()
        {
            InitializeComponent();
            InitializeFirebase();

            listPropertyManager = new ListPropertyManager(firebaseClient);
            listedPropertyViewer = new ListedPropertyViewer(firebaseClient);
            propertyBookingManager = new PropertyBookingManager(firebaseClient);
            propertyReservationManager = new PropertyReservationManager(firebaseClient);

            this.ApplyRoundedCornersToAll();
            txtCardNumber?.ApplyRoundedCorners(25);
            txtAddressLine2?.ApplyRoundedCorners(25);
            button_ConfirmBooking?.ApplyRoundedCorners(75);
            txtPrice?.ApplyRoundedCorners(25);
            txtDescription?.ApplyRoundedCorners(25);
            uploadButton?.ApplyRoundedCorners(75);
            searchButton?.ApplyRoundedCorners(25);

            // Subscribe to the PropertySelected event
            propertyBookingManager.PropertySelected += PropertyBookingManager_PropertySelected;

            propertyReservationManager.ReservationSelected += PropertyReservationManager_ReservationSelected;
        }

        public void InitializeFirebase()
        {
            firebaseClient = new FirebaseClient("https://airbnb-d4964-default-rtdb.europe-west1.firebasedatabase.app/");
        }

        private void frmListed_Load(object sender, EventArgs e)
        {
            usernameLabel.Text = GlobalData.Username;
            ShowPanel(panelHome);
        }

        private void listButton_Click(object sender, EventArgs e)
        {
            ShowPanel(panelList);
        }

        private async void bookButton_Click(object sender, EventArgs e)
        {
            ShowPanel(panelBook);

            var properties = await propertyBookingManager.GetAvailablePropertiesFromFirebase();

            if (properties != null && properties.Count > 0)
            {
                propertyBookingManager.DisplayAvailableProperties(properties, flowPanelBook);
            }
            else
            {
                MessageBox.Show("No available properties found.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private async void listedButton_Click(object sender, EventArgs e)
        {
            ShowPanel(panelListed);

            string username = GlobalData.Username;

            var imageUrls = await listedPropertyViewer.GetImageUrlsFromFirebase(username);

            if (imageUrls != null && imageUrls.Count > 0)
            {
                listedPropertyViewer.DisplayImages(imageUrls, flowPanelImages);
            }
            else
            {
                MessageBox.Show("No listed property images found.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            new frmLogin().Show();
            this.Hide();
        }

        private void ShowPanel(Panel panel)
        {
            panel.BringToFront();
        }

        private async void uploadButton_Click(object sender, EventArgs e)
        {
            try
            {
                listPropertyManager.SelectFiles();
                await listPropertyManager.UploadProperty(
                    GlobalData.Username,
                    txtAddress.Text,
                    txtCity.Text,
                    txtTitle.Text,
                    txtPrice.Text,
                    txtDescription.Text
                );
                MessageBox.Show("Property details and images uploaded successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (ArgumentException ex)
            {
                MessageBox.Show(ex.Message, "Action Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async void frontImageButton_Click(object sender, EventArgs e)
        {
            await listPropertyManager.UploadFrontImage(GlobalData.Username, GlobalData.email);
            MessageBox.Show("Front image uploaded successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void searchLabel_Click(object sender, EventArgs e)
        {
            ShowPanel(searchPanel);
        }

        private async void searchButton_Click(object sender, EventArgs e)
        {
            string city = txtSearch.Text;
            var properties = await propertyBookingManager.SearchPropertiesByCity(city);

            if (properties != null && properties.Count > 0)
            {
                propertyBookingManager.DisplayAvailableProperties(properties, flowPanelSearch);
            }
            else
            {
                MessageBox.Show($"No properties found in {city}.", "Search Result", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private async void PropertyBookingManager_PropertySelected(object sender, Dictionary<string, object> propertyData)
        {
            try
            {
                // Store the selected property data
                selectedPropertyData = propertyData;

                // Get the complete address data
                var addressData = await firebaseClient
                    .Child("Available Properties")
                    .Child(propertyData["Username"].ToString())
                    .Child("Address")
                    .OnceSingleAsync<Dictionary<string, object>>();

                if (labelAddress != null)
                {
                    labelAddress.Text = $"Address: {addressData["Address"]}";
                }

                if (labelPricePerNight != null)
                {
                    labelPricePerNight.Text = $"Price per night: £{propertyData["PricePerNight"]}";
                }

                if (labelContact != null)
                {
                    labelContact.Text = $"Contact: {propertyData["Email"]}";
                }

                // Show details panel
                ShowPanel(panelPropertyDetails);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading property details: {ex.Message}", "Error",
                              MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            DisplaySelectedPropertyImages(sender, propertyData, flowLayoutPanelImages);
        }

        private async void DisplaySelectedPropertyImages(object sender, Dictionary<string, object> propertyData, FlowLayoutPanel flowPanel)
        {
            flowPanel.Controls.Clear();
            flowPanel.AutoScroll = true;
            flowPanel.WrapContents = true;
            flowPanel.FlowDirection = FlowDirection.LeftToRight;
            flowPanel.Padding = new Padding(10);

            if (propertyData.ContainsKey("Username"))
            {
                var images = await firebaseClient
                    .Child("Available Properties")
                    .Child(propertyData["Username"].ToString())
                    .Child("ImageUrls")
                    .OnceSingleAsync<List<string>>();

                foreach (var imageUrl in images)
                {
                    // Create card panel with rounded corners
                    Panel imageCard = new Panel
                    {
                        Width = PROPERTY_CARD_WIDTH,
                        Height = PROPERTY_CARD_HEIGHT,
                        Margin = new Padding(10),
                        BackColor = Color.White
                    };

                    // Add rounded corners to card
                    int cardRadius = 20;
                    System.Drawing.Drawing2D.GraphicsPath cardPath = new System.Drawing.Drawing2D.GraphicsPath();
                    cardPath.AddArc(0, 0, cardRadius, cardRadius, 180, 90);
                    cardPath.AddArc(imageCard.Width - cardRadius, 0, cardRadius, cardRadius, 270, 90);
                    cardPath.AddArc(imageCard.Width - cardRadius, imageCard.Height - cardRadius, cardRadius, cardRadius, 0, 90);
                    cardPath.AddArc(0, imageCard.Height - cardRadius, cardRadius, cardRadius, 90, 90);
                    imageCard.Region = new Region(cardPath);

                    // Create PictureBox with rounded corners
                    PictureBox propertyImage = new PictureBox
                    {
                        Width = PROPERTY_CARD_WIDTH - (IMAGE_PADDING * 2),
                        Height = PROPERTY_CARD_HEIGHT - (IMAGE_PADDING * 2),
                        Location = new Point(IMAGE_PADDING, IMAGE_PADDING),
                        SizeMode = PictureBoxSizeMode.Zoom
                    };

                    // Add rounded corners to image
                    int imageRadius = 10;
                    System.Drawing.Drawing2D.GraphicsPath imagePath = new System.Drawing.Drawing2D.GraphicsPath();
                    imagePath.AddArc(0, 0, imageRadius, imageRadius, 180, 90);
                    imagePath.AddArc(propertyImage.Width - imageRadius, 0, imageRadius, imageRadius, 270, 90);
                    imagePath.AddArc(propertyImage.Width - imageRadius, propertyImage.Height - imageRadius, imageRadius, imageRadius, 0, 90);
                    imagePath.AddArc(0, propertyImage.Height - imageRadius, imageRadius, imageRadius, 90, 90);
                    propertyImage.Region = new Region(imagePath);

                    propertyImage.Load(imageUrl);
                    imageCard.Controls.Add(propertyImage);
                    flowPanel.Controls.Add(imageCard);
                }
            }
            else
            {
                var imageUrl = propertyData["mainImage"];

                // Create card panel with rounded corners
                Panel imageCard = new Panel
                {
                    Width = PROPERTY_CARD_WIDTH,
                    Height = PROPERTY_CARD_HEIGHT,
                    Margin = new Padding(10),
                    BackColor = Color.White
                };

                // Add rounded corners to card
                int cardRadius = 20;
                System.Drawing.Drawing2D.GraphicsPath cardPath = new System.Drawing.Drawing2D.GraphicsPath();
                cardPath.AddArc(0, 0, cardRadius, cardRadius, 180, 90);
                cardPath.AddArc(imageCard.Width - cardRadius, 0, cardRadius, cardRadius, 270, 90);
                cardPath.AddArc(imageCard.Width - cardRadius, imageCard.Height - cardRadius, cardRadius, cardRadius, 0, 90);
                cardPath.AddArc(0, imageCard.Height - cardRadius, cardRadius, cardRadius, 90, 90);
                imageCard.Region = new Region(cardPath);

                // Create PictureBox with rounded corners
                PictureBox propertyImage = new PictureBox
                {
                    Width = PROPERTY_CARD_WIDTH - (IMAGE_PADDING * 2),
                    Height = PROPERTY_CARD_HEIGHT - (IMAGE_PADDING * 2),
                    Location = new Point(IMAGE_PADDING, IMAGE_PADDING),
                    SizeMode = PictureBoxSizeMode.Zoom
                };

                // Add rounded corners to image
                int imageRadius = 10;
                System.Drawing.Drawing2D.GraphicsPath imagePath = new System.Drawing.Drawing2D.GraphicsPath();
                imagePath.AddArc(0, 0, imageRadius, imageRadius, 180, 90);
                imagePath.AddArc(propertyImage.Width - imageRadius, 0, imageRadius, imageRadius, 270, 90);
                imagePath.AddArc(propertyImage.Width - imageRadius, propertyImage.Height - imageRadius, imageRadius, imageRadius, 0, 90);
                imagePath.AddArc(0, propertyImage.Height - imageRadius, imageRadius, imageRadius, 90, 90);
                propertyImage.Region = new Region(imagePath);

                propertyImage.Load(imageUrl.ToString());
                imageCard.Controls.Add(propertyImage);
                flowPanel.Controls.Add(imageCard);
            }
        }

        private void button_FinalBook_Click(object sender, EventArgs e)
        {
            ShowPanel(panelFinalBook);
        }

        private void bookingCalendar_DateSelected(object sender, DateRangeEventArgs e)
        {
            DateTime checkInDate = bookingCalendar.SelectionStart;
            DateTime checkOutDate = bookingCalendar.SelectionEnd;

            // Update labels
            labelCheckIn.Text = $"Check-in Date: {checkInDate:d}";
            labelCheckOut.Text = $"Check-out Date: {checkOutDate:d}";

            // Calculate total nights and price
            int totalNights = (checkOutDate - checkInDate).Days;

            // Get price from the previous screen
            decimal pricePerNight = 0;
            if (labelPricePerNight != null)
            {
                string priceText = labelPricePerNight.Text.Replace("Price per night: £", "");
                decimal.TryParse(priceText, out pricePerNight);
            }

            decimal totalPrice = totalNights * pricePerNight;

            labelTotalNights.Text = $"Total Nights: {totalNights}";
            labelTotalPrice.Text = $"Total Price: £{totalPrice:N2}";

            // Enable confirm button if dates are valid
            button_ConfirmBooking.Enabled = totalNights > 0;
        }

        private void button_ConfirmBooking_Click(object sender, EventArgs e)
        {
            ShowPanel(panelPayment);
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {
            ShowPanel(panelHome);
        }

        private void usernameLabel_Click(object sender, EventArgs e)
        {
            ShowPanel(panelHome);
        }

        private async void buttonReservations_Click(object sender, EventArgs e)
        {
            string username = GlobalData.Username;
            ShowPanel(panelReservations);

            var reservations = await propertyReservationManager.RetrieveUserReservationDetails(username);

            if (reservations.Count != 0)
            {
                propertyReservationManager.DisplayUserReservations(reservations, flowPanelReservations);
            }
            else
            {
                MessageBox.Show("No reservations found", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void PropertyReservationManager_ReservationSelected(object sender, Dictionary<string, object> reservationData)
        {
            ShowPanel(panelSelectedReservation);

            // Store the selected reservation data
            selectedReservationData = reservationData;

            if (labelResAddress != null)
            {
                labelResAddress.Text = $"Address: {reservationData["address"]}";
            }
            if (labelResCheckIn != null)
            {
                labelResCheckIn.Text = $"Check-in Date: {reservationData["startDate"]}";
            }
            if (labelResCheckOut != null)
            {
                labelResCheckOut.Text = $"Check-out Date: {reservationData["endDate"]}";
            }
            if (labelResTotalNights != null)
            {
                labelResTotalNights.Text = $"Total Nights: {reservationData["nights"]}";
            }
            if (labelResTotalPrice != null)
            {
                decimal pricePerNight = decimal.Parse(reservationData["pricePerNight"].ToString());
                int nights = int.Parse(reservationData["nights"].ToString());
                decimal totalPrice = pricePerNight * nights;

                labelResTotalPrice.Text = $"Total Price: £{totalPrice}";
            }

            DisplaySelectedPropertyImages(sender, reservationData, flowPanelSelectedResevation);

            // Show details panel
            ShowPanel(panelSelectedReservation);
        }

        private async void buttonCancelReservation_Click(object sender, EventArgs e)
        {
            DialogResult result = MessageBox.Show(
                "Are you sure you want to cancel this reservation?",
                "Confirm Cancellation",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question
            );

            if (result == DialogResult.Yes)
            {
                await propertyReservationManager.CancelReservation(selectedReservationData);

                // Refresh the reservations panel
                buttonReservations_Click(sender, e);
            }
        }

        private async void buttonPay_Click(object sender, EventArgs e)
        {
            try
            {
                var paymentManager = new PaymentManager();
                var paymentDetails = new PaymentDetails
                {
                    FullName = txtFullName.Text,
                    CardNumber = txtCardNumber.Text.Replace(" ", ""),  // Remove any spaces
                    ExpiryDate = txtExpiryDate.Text,
                    CVV = txtCVV.Text,
                    AddressLine1 = txtAddressLine1.Text,
                    AddressLine2 = txtAddressLine2.Text,
                    City = txtCiti.Text,
                    PostCode = txtPostCode.Text
                };

                // Calculate total price from labels
                string priceText = labelTotalPrice.Text.Replace("Total Price: £", "");
                if (!decimal.TryParse(priceText, out decimal totalAmount))
                {
                    MessageBox.Show("Invalid price amount.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                // Process the payment
                paymentManager.ProcessPayment(paymentDetails, totalAmount);

                // If we get here, payment was successful
                MessageBox.Show("Payment processed successfully. Reservation successful!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);

                // Continue with the existing reservation process
                if (selectedPropertyData == null)
                {
                    MessageBox.Show("Property data is missing.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                DateTime checkInDate = bookingCalendar.SelectionStart.Date;
                DateTime checkOutDate = bookingCalendar.SelectionEnd.Date;

                string strCheckInDate = checkInDate.ToString("dd/MM/yyyy");
                string strCheckOutDate = checkOutDate.ToString("dd/MM/yyyy");
                string username = GlobalData.Username;
                int totalNights = (checkOutDate - checkInDate).Days;

                var propertyData = await firebaseClient
                    .Child("Available Properties")
                    .Child(selectedPropertyData["Username"].ToString())
                    .OnceSingleAsync<Dictionary<string, object>>();

                var addressData = await firebaseClient
                    .Child("Available Properties")
                    .Child(selectedPropertyData["Username"].ToString())
                    .Child("Address")
                    .OnceSingleAsync<Dictionary<string, object>>();

                propertyBookingManager.AddReservationToDatabase(username, strCheckOutDate, totalNights, strCheckInDate, propertyData, addressData);

                // Return to home panel or another appropriate panel
                ShowPanel(panelHome);
            }
            catch (ArgumentException ex)
            {
                MessageBox.Show(ex.Message, "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            catch (Exception ex)
            {
                MessageBox.Show("An error occurred while processing the payment: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            base.OnFormClosing(e);
            if (e.CloseReason == CloseReason.UserClosing)
            {
                Application.Exit();
            }
        }
    }
}