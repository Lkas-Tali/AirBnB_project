using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using FirebaseAdmin;
using Google.Apis.Auth.OAuth2;
using System.IO;
using System.Collections;
using Google.Cloud.Firestore;
using Firebase.Database;
using Firebase.Database.Query;
using System.Linq;
using System.Text.RegularExpressions;

namespace AirBnB
{

    public partial class frmRegister : Form
    {

        private FirebaseClient firebaseClient;

        public void InitializeFirebase()
        {
            string basePath = Directory.GetCurrentDirectory();
            string pathToJson = Path.Combine(basePath, "FireBase", "airbnb-d4964-firebase-adminsdk-cbzm0-e68a4950cf.json");

            // Initialize Firebase Realtime Database client
            firebaseClient = new FirebaseClient("https://airbnb-d4964-default-rtdb.europe-west1.firebasedatabase.app/");
        }

        public frmRegister()
        {
            InitializeComponent();
            InitializeFirebase(); // Call to initialize Firebase on form load

            foreach (Control control in this.Controls)
            {
                if (control is TextBox textBox)
                {
                    textBox.ApplyRoundedCorners(25); // Increased corner radius
                }
            }
            txtEmail?.ApplyRoundedCorners(25);
            registerButton?.ApplyRoundedCorners(45);
            clearButton?.ApplyRoundedCorners(45);
        }

        //Going back to the Login form
        private void label5_Click(object sender, EventArgs e)
        {
            frmLogin loginForm = new frmLogin();
            loginForm.Show();
            this.Hide();
        }

        private void label6_Click(object sender, EventArgs e)
        {

        }

        private void clearButton_Click(object sender, EventArgs e)
        {
            EmptyFields();
        }

        private void checkbxShowPas_CheckedChanged(object sender, EventArgs e)
        {
            if (checkbxShowPas.Checked)
            {
                txtPassword.PasswordChar = '\0';
                txtConPassword.PasswordChar = '\0';
            }
            else
            {
                txtPassword.PasswordChar = '•';
                txtConPassword.PasswordChar = '•';
            }
        }

        private async void registerButton_Click(object sender, EventArgs e)
        {
            bool usernameExists = await CheckUsernameExists(txtUsername.Text);

            if (txtEmail.Text == "" || txtPassword.Text == "" || txtConPassword.Text == "" || txtUsername.Text == "")
            {
                MessageBox.Show("Make sure you fill up all fields and try again", "Registration Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
                EmptyFields();
                new frmRegister().Show();
                this.Hide();
            }
            else if (usernameExists)
            {
                MessageBox.Show("This username is already taken, choose another one.", "Registration Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
                EmptyFields();
                new frmRegister().Show();
                this.Hide();
            }
            else if (txtPassword.Text != txtConPassword.Text)
            {
                MessageBox.Show("Your password doesn't match!", "Registration Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
                EmptyFields();
                new frmRegister().Show();
                this.Hide();
            }
            // Validating the email
            else if (!IsValidEmail(txtEmail.Text))
            {
                MessageBox.Show("Please eneter a valid Email address", "Registration Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
                EmptyFields();
                new frmRegister().Show();
                this.Hide();
            }
            // Checking whether or not the password is secure
            else if (!IsValidPassword(txtPassword.Text) )
            {
                MessageBox.Show("Password must have at least 8 characters, one uppercase letter, and one number", "Registration Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
                EmptyFields();
                new frmRegister().Show();
                this.Hide();
            }
            else
            {
                // Populate user data to the database
                var userData = UserData(); // Gather user data
                await AddUserToDatabase(userData); // Add the data to Firebase

                MessageBox.Show("Registration successful!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                
                new frmLogin().Show();
                this.Hide();
            }
        }

        //Setting the text fields to empty
        public void EmptyFields()
        {
            txtUsername.Text = "";
            txtEmail.Text = "";
            txtPassword.Text = "";
            txtConPassword.Text = "";
            txtUsername.Focus();
        }

        // Collecting user data into a dictionary
        public Dictionary<string, string> UserData()
        {
            Dictionary<string, string> userData = new Dictionary<string, string>();

            userData.Add("Email", txtEmail.Text); // Store email
            userData.Add("Password", txtPassword.Text); // Store password

            return userData;
        }

        // Add user data to Firebase Realtime Database with the username as the key
        public async Task AddUserToDatabase(Dictionary<string, string> userData)
        {
            // Add user data to the "Users" node in the Realtime Database
            await firebaseClient
                .Child("Users")
                .Child(txtUsername.Text)
                .Child("Credentials")
                .PutAsync(userData); // Use PostAsync to add data

            await firebaseClient
                .Child("Users")
                .Child(txtUsername.Text)
                .Child("isListing")
                .PutAsync(false); // Use PostAsync to add data
        }

        // Method to validate the password
        public static bool IsValidPassword(string password)
        {
            // Check if the password is longer than 8 characters
            if (password.Length <= 7)
            {
                return false;
            }

            // Check if the password contains at least one uppercase letter
            if (!password.Any(char.IsUpper))
            {
                return false;
            }

            // Check if the password contains at least one number
            if (!password.Any(char.IsDigit))
            {
                return false;
            }

            return true;
        }

        // Method to validate the Email
        public static bool IsValidEmail(string email)
        {
            // Regular expression pattern to validate email
            string emailPattern = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";

            // Use Regex to check if the email matches the pattern
            return Regex.IsMatch(email, emailPattern);
        }

        // Method to check if a username already exists in the Firebase Realtime Database
        private async Task<bool> CheckUsernameExists(string username)
        {
            try
            {
                // Attempt to retrieve user data from the Realtime Database
                var userSnapshot = await firebaseClient
                    .Child("Users")
                    .Child(username) // Use the username as the key
                    .OnceSingleAsync<object>(); // Use object type since we only care about existence

                // If userSnapshot is not null, the username exists
                return userSnapshot != null;
            }
            catch (Exception ex)
            {
                return false; // Return false in case of error (e.g., username not found)
            }
        }

        private void frmRegister_Load(object sender, EventArgs e)
        {

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
