using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Firebase.Database;
using Firebase.Database.Query;
using FirebaseAdmin;
using Google.Apis.Auth.OAuth2;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.StartPanel;

namespace AirBnB
{
    public partial class frmLogin : Form
    {
        private FirebaseClient firebaseClient; // Declare FirebaseClient to interact with the database

        public frmLogin()
        {
            InitializeComponent();
            InitializeFirebase(); // Initialize Firebase when the form loads

            foreach (Control control in this.Controls)
            {
                if (control is TextBox textBox)
                {
                    textBox.ApplyRoundedCorners(25); // Increased corner radius
                }
            }
        }

        public void InitializeFirebase()
        {
            string basePath = Directory.GetCurrentDirectory();
            string pathToJson = Path.Combine(basePath, "FireBase", "airbnb-d4964-firebase-adminsdk-cbzm0-e68a4950cf.json");

            // Initialize Firebase Realtime Database client
            firebaseClient = new FirebaseClient("https://airbnb-d4964-default-rtdb.europe-west1.firebasedatabase.app/");
        }

        private void txtConPassword_TextChanged(object sender, EventArgs e)
        {

        }

        private void checkbxShowPas_CheckedChanged(object sender, EventArgs e)
        {
            if (checkbxShowPas.Checked)
            {
                txtPassword.PasswordChar = '\0';
            }
            else
            {
                txtPassword.PasswordChar = '•';
            }
        }

        private void frmLogin_Load(object sender, EventArgs e)
        {

        }

        private async void loginButton_Click(object sender, EventArgs e)
        {
            // Check if the username and password fields are not empty
            if (txtUsername.Text == "" || txtPassword.Text == "")
            {
                MessageBox.Show("Please fill in both username and password", "Login Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            // Try to retrieve the user data from Firebase Realtime Database
            var userData = await GetUserFromDatabase(txtUsername.Text);

            if (userData != null)
            {
                // Validate if the entered password matches the stored password
                if (userData["Password"] == txtPassword.Text)
                {
                    // Set the global username
                    GlobalData.Username = txtUsername.Text;

                    // Retrieve the user email
                    var userCredsRef = firebaseClient
                        .Child("Users")
                        .Child(txtUsername.Text)
                        .Child("Credentials")
                        .Child("Email");

                    var email = await userCredsRef.OnceSingleAsync<string>();

                    //Set the global email
                    GlobalData.email = email;

                    new frmDashboard().Show();
                    this.Hide();
                }
                else
                {
                    MessageBox.Show("Invalid password", "Login Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            else
            {
                MessageBox.Show("User not found", "Login Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // Retrieve user data from Firebase Realtime Database
        public async Task<Dictionary<string, string>> GetUserFromDatabase(string username)
        {
            try
            {
                // Retrieve the user data based on the username
                var userSnapshot = await firebaseClient
                    .Child("Users")
                    .Child(username) // Use the username as the key
                    .Child("Credentials")
                    .OnceSingleAsync<Dictionary<string, string>>(); // Deserialize the data into a dictionary

                return userSnapshot; // Return the user data
            }
            catch (Exception ex)
            {
                // Handle any errors (e.g., if the user does not exist)
                Console.WriteLine("Error: " + ex.Message);
                return null; // Return null if there's an issue (e.g., user not found)
            }
        }

        private void label5_Click(object sender, EventArgs e)
        {
            new frmRegister().Show();
            this.Hide();
        }

        //Setting the text fields to empty
        public void EmptyFields()
        {
            txtUsername.Text = "";
            txtPassword.Text = "";
            txtUsername.Focus();
        }

        private void clearButton_Click(object sender, EventArgs e)
        {
            EmptyFields();
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

    public static class GlobalData
    {
        public static string Username { get; set; }
        public static string email { get; set; }
    }

}
