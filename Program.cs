using System.Net.Mime;
using System.Security.Cryptography;

namespace ISPW5;

/// <summary>
/// Mandatory and optional tasks:
/// </summary>
/// <param name="1.">Secure File Handling</param>
/// <param name="├">When the application is started for the first time, it must create a new .csv or .txt file to store password data.</param>
/// <param name="├">When the application is closed, this file must be encrypted using the AES algorithm.</param>
/// <param name="└">When the application is launched again, it must decrypt the file to allow access to the stored data.</param>
/// <param name="2.">Add a New Password Entry</param>
/// <param name="├">Provide a form to input the following fields: (Title, Password, URL/Application, Notes)</param>
/// <param name="└">The password itself must be encrypted before saving using AES, DES, or RSA (you choose).</param>
/// <param name="3.">Search for a Password</param>
/// <param name="├">Allow searching for a password entry using its title.</param>
/// <param name="└">Display the associated data (e.g., URL, notes), but do not display the password immediately.</param>
/// <param name="4.">Update a Password</param>
/// <param name="├">Allow updating an existing password entry by its title.</param>
/// <param name="└">The updated password must be encrypted using the same algorithm as originally selected.</param>
/// <param name="5.">Delete a Password Entry</param>
/// <param name="├">Implement a function to delete an existing password entry by title.</param>
/// <param name="└">Remove the associated data from the file.</param>
/// <param name="(+1.5)">User Registration and Login</param>
/// <param name="├">Add a registration form that stores username and hashed/encrypted password.</param>
/// <param name="├">Add a login form to verify the user.</param>
/// <param name="├">Recommended methods for password hashing: PBKDF2, Bcrypt, Scrypt, or Argon2.</param>
/// <param name="├">After successful registration, a unique .csv or .txt file should be created and assigned to the user.</param>
/// <param name="└">Modify the mandatory file encryption feature to support user-specific files.</param>
/// <param name="(+1.0)">File Encryption on Login/Logout</param>
/// <param name="├">After login, the user's file should be decrypted automatically.</param>
/// <param name="└">When the user logs out or the application closes unexpectedly, the file should be encrypted.</param>
/// <param name="(+0.5)">Random Password Generator</param>
/// <param name="├">Implement a feature to generate strong, random passwords.</param>
/// <param name="└">Allow the user to select password length and character types (e.g., symbols, numbers, uppercase, lowercase).</param>
/// <param name="(+0.5)">Secure Reveal</param>
/// <param name="├">After searching for a password, do not show it immediately.</param>
/// <param name="└">Instead, provide a "Show" button that decrypts and displays the password when clicked.</param>
/// <param name="(+0.5)">Copy to Clipboard</param>
/// <param name="└">Add a button that copies the decrypted password directly to the clipboard for easy use.</param>
class Program {
    private static Dictionary<string, string> users = new();
    private const string UsersFile = "users.csv";
    
    // PBKDF2 parameters (can be adjusted)
    private const int SaltSize = 16; // 128 bits
    private const int HashSize = 20; // 160 bits
    private const int Iterations = 10000; // Adjust based on performance needs

    static void Main(string[] args) {
        UserFileInitialization();

        while (true) {
            Console.Clear();
            Console.WriteLine("Select function:\n1. Login\n2. Register\n0. Quit");
            switch (Console.ReadKey(intercept: true).Key) {
                case ConsoleKey.D1:
                case ConsoleKey.NumPad1:
                    Login();
                    break;
                case ConsoleKey.D2:
                case ConsoleKey.NumPad2:
                    Registration();
                    break;
                case ConsoleKey.D0:
                case ConsoleKey.NumPad0:
                    Environment.Exit(0);
                    break;
            }
        }
    }

    public static void UserFileInitialization() {
        if (!File.Exists(UsersFile)) {
            File.Create(UsersFile).Dispose();
        }

        var lines = File.ReadAllLines(UsersFile);
        foreach (var line in lines) {
            var parts = line.Split(';');
            users[parts[0]] = parts[1] + ';' + parts[2];
        }
    }

    public static void Registration() {
        string username, password, encryptedPassword;

        while (true) {
            Console.Clear();
            Console.WriteLine("<<<< Registration form >>>>");
            Console.WriteLine("Enter your username:");
            username = Console.ReadLine();
            if (!users.ContainsKey(username)) {
                while (true) {
                    Console.Clear();
                    Console.WriteLine("<<<< Registration form >>>>");
                    Console.WriteLine("Enter your password:");
                    password = Console.ReadLine();
                    if (password.Length < 8) {
                        Console.WriteLine("Password is too short! (Press any key to continue)");
                        Console.ReadKey();
                    }
                    else {
                        break;
                    }
                }
                break;
            }

            Console.WriteLine("User with same username already exists! (Press any key to continue)");
            Console.ReadKey();
        }

        encryptedPassword = PBKDF2Encryption(password);

        users[username] = encryptedPassword;
        File.AppendAllText(UsersFile, $"{username};{encryptedPassword}\n");
        
        File.Create($"{username}.csv").Dispose();
        
        Console.WriteLine("Registration successful! (Press any key to continue)");
        Console.ReadKey();
    }

    public static void Login() {
        string username, password;

        while (true) {
            Console.Clear();
            Console.WriteLine("<<<< Login form >>>>");
            Console.WriteLine("Enter your username:");
            username = Console.ReadLine();
            Console.WriteLine("Enter your password:");
            password = Console.ReadLine();

            if (users.ContainsKey(username)) {
                // Get stored hash and salt
                var parts = users[username].Split(';');
                string storedHash = parts[0];
                byte[] salt = Convert.FromBase64String(parts[1]);

                if (PBKDF2Validation(password, storedHash, salt)) {
                    Console.WriteLine("Password is correct! (Press any key to continue)");
                    Console.ReadKey();
                    PasswordManagerHub hub = new PasswordManagerHub(username, password);
                    hub.Start();
                    break;
                }
            }
                
            Console.WriteLine("Invalid username or password! (Press any key to continue)");
            Console.ReadKey();
        }
    }

    public static string PBKDF2Encryption(string text) {
        // Generate salt
        byte[] salt = new byte[SaltSize];
        using (var rng = RandomNumberGenerator.Create())
        {
            rng.GetBytes(salt);
        }

        // Hash password
        string hash = HashPassword(text, salt);
        
        return $"{hash};{Convert.ToBase64String(salt)}";
    }
    
    static string HashPassword(string password, byte[] salt) {
        using (var pbkdf2 = new Rfc2898DeriveBytes(password, salt, Iterations, HashAlgorithmName.SHA256))
        {
            byte[] hash = pbkdf2.GetBytes(HashSize);
            return Convert.ToBase64String(hash);
        }
    }

    public static bool PBKDF2Validation(string enteredPassword, string storedHash, byte[] salt) {
        string newHash = HashPassword(enteredPassword, salt);
        return newHash == storedHash;
    }
}