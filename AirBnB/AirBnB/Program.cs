using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using Firebase.Database;
using Firebase.Database.Query;
using FirebaseAdmin;
using Google.Apis.Auth.OAuth2;
using System.IO;

namespace AirBnB
{
    internal static class Program
    {
        private static FirebaseClient firebaseClient; // Declare FirebaseClient to interact with the database

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            InitializeFirebase(); // Call to initialize Firebase on program start
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new frmRegister());
        }

        public static void InitializeFirebase()
        {
            string basePath = Directory.GetCurrentDirectory();
            string pathToJson = Path.Combine(basePath, "FireBase", "airbnb-d4964-firebase-adminsdk-cbzm0-e68a4950cf.json");

            // Initialize the Firebase app using the JSON key file
            FirebaseApp.Create(new AppOptions()
            {
                Credential = GoogleCredential.FromFile(pathToJson)
            });

            // Initialize Firebase Realtime Database client
            firebaseClient = new FirebaseClient("https://airbnb-d4964-default-rtdb.europe-west1.firebasedatabase.app/");
        }
    }
}
