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
        private SelectedProperty selectedPropertyManager;

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
            selectedPropertyManager = new SelectedProperty(firebaseClient);

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
            // Show panel before starting to load
            ShowPanel(panelBook);

            // Disable the book button while loading to prevent double-clicks
            bookButton.Enabled = false;

            try
            {
                var properties = await propertyBookingManager.GetAvailablePropertiesFromFirebase();

                if (properties != null && properties.Count > 0)
                {
                    await propertyBookingManager.DisplayAvailableProperties(properties, flowPanelBook);
                }
                else
                {
                    MessageBox.Show("No available properties found.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            finally
            {
                // Re-enable the button whether the operation succeeded or failed
                bookButton.Enabled = true;
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
                await propertyBookingManager.DisplayAvailableProperties(properties, flowPanelSearch);
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
                selectedPropertyData = propertyData;

                var addressData = await selectedPropertyManager.GetPropertyAddress(propertyData["Username"].ToString());

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

                ShowPanel(panelPropertyDetails);

                await selectedPropertyManager.DisplayPropertyDetails(propertyData, flowLayoutPanelImages);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading property details: {ex.Message}", "Error",
                              MessageBoxButtons.OK, MessageBoxIcon.Error);
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
                await propertyReservationManager.DisplayUserReservations(reservations, flowPanelReservations);
            }
            else
            {
                MessageBox.Show("No reservations found", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async void PropertyReservationManager_ReservationSelected(object sender, Dictionary<string, object> reservationData)
        {
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
            ShowPanel(panelSelectedReservation);

            // Use the new method instead of DisplaySelectedPropertyImages
            await selectedPropertyManager.DisplayPropertyDetails(reservationData, flowPanelSelectedResevation);
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

                selectedPropertyManager.AddReservationToDatabase(username, strCheckOutDate, totalNights, strCheckInDate, propertyData, addressData);

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