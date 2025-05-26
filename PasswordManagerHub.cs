using System.Security.Cryptography;
using System.Text;

namespace ISPW5;

public class PasswordManagerHub {
    private string _username;
    private string _password;
    private string _filePath;
    private List<Passwords> _passwords = new();

    private byte[] _aesKey;
    private byte[] _aesIV;

    public PasswordManagerHub(string username, string password) {
        _username = username;
        _password = password;
        _filePath = $"{_username}.csv";

        if (File.Exists(_filePath)) {
            LoadPasswords();
        }

        byte[] staticSalt = Encoding.UTF8.GetBytes("STATICSALT"); // fixed salt for all users
        using var pbkdf2 = new Rfc2898DeriveBytes(_password, staticSalt, 100_000);
        _aesKey = pbkdf2.GetBytes(32);
        _aesIV = pbkdf2.GetBytes(16);
    }

    public void Start() {
        while (true) {
            Console.Clear();
            ShowPasswords();
            Console.WriteLine($"<<<<<<<< Choose a function: >>>>>>>>");
            Console.WriteLine(
                "Select function:\n1. Add\n2. Remove\n3. Update\n4. Show Password\n5. Generate Password\n0. Quit");
            switch (Console.ReadKey(intercept: true).Key) {
                case ConsoleKey.D1:
                case ConsoleKey.NumPad1:
                    AddPassword();
                    break;
                case ConsoleKey.D2:
                case ConsoleKey.NumPad2:
                    RemovePassword();
                    break;
                case ConsoleKey.D3:
                case ConsoleKey.NumPad3:
                    UpdatePassword();
                    break;
                case ConsoleKey.D4:
                case ConsoleKey.NumPad4:
                    ShowDecryptedPassword();
                    break;
                case ConsoleKey.D5:
                case ConsoleKey.NumPad5:
                    GeneratePassword();
                    break;
                case ConsoleKey.D0:
                case ConsoleKey.NumPad0:
                    return;
            }
        }
    }

    private void LoadPasswords() {
        foreach (var line in File.ReadLines(_filePath)) {
            var parts = line.Split(';');
            if (parts.Length >= 4) {
                _passwords.Add(new Passwords {
                    title = parts[0],
                    encryptedPassword = parts[1],
                    url = parts[2],
                    notes = parts[3]
                });
            }
        }
    }

    private void ShowPasswords() {
        string separatorTop = $"╔{new string('═', 6)}╦{new string('═', 22)}╦" +
                              $"{new string('═', 42)}╦{new string('═', 27)}╦" +
                              $"{new string('═', 32)}╗";
        string separatorMid = $"╠{new string('═', 6)}╬{new string('═', 22)}╬" +
                              $"{new string('═', 42)}╬{new string('═', 27)}╬" +
                              $"{new string('═', 32)}╣";
        string separatorBot = $"╚{new string('═', 6)}╩{new string('═', 22)}╩" +
                              $"{new string('═', 42)}╩{new string('═', 27)}╩" +
                              $"{new string('═', 32)}╝";

        Console.WriteLine(separatorTop);
        Console.WriteLine($"║ {"ID",-4} ║ {"Title",-20} ║ {"Encrypted Password",-40} ║ {"URL",-25} ║ {"Notes",-30} ║");
        Console.WriteLine(separatorMid);

        for (int i = 0; i < _passwords.Count; i++) {
            Console.WriteLine($"║ {i,-4} ║ {_passwords[i]} ║");
        }

        Console.WriteLine(separatorBot);
    }

    private void AddPassword() {
        Passwords newPassword = new();

        Console.Clear();
        Console.WriteLine($"<<<<<<<< Add new password entry: >>>>>>>");
        Console.Write("Enter title: ");
        newPassword.title = Console.ReadLine();
        Console.Write("\nEnter password: ");
        newPassword.encryptedPassword = Encrypt(Console.ReadLine());
        Console.Write("\nEnter url: ");
        newPassword.url = Console.ReadLine();
        Console.Write("\nEnter notes: ");
        newPassword.notes = Console.ReadLine();

        _passwords.Add(newPassword);
        SavePasswords();
    }

    private void RemovePassword() {
        while (true) {
            Console.Clear();
            ShowPasswords();
            Console.Write("Enter the ID of the entry to delete (or Q to quit): ");
            string input = Console.ReadLine();
            int id;

            if (input == "q") {
                break;
            }

            if (int.TryParse(input, out id) && id >= 0 && id < _passwords.Count) {
                _passwords.RemoveAt(id);
                SavePasswords();
                Console.WriteLine("Password deleted! (Press any key to continue)");
                Console.ReadKey();
                break;
            }

            Console.WriteLine("Invalid input. Please enter a valid numeric ID! (Press any key to continue)");
            Console.ReadKey();
        }
    }

    private void UpdatePassword() {
        while (true) {
            Console.Clear();
            ShowPasswords();
            Console.Write("Enter the ID of the entry to update (or Q to quit): ");
            string input = Console.ReadLine();
            int id;

            if (input == "q") {
                break;
            }

            if (int.TryParse(input, out id) && id >= 0 && id < _passwords.Count) {
                Console.Write("New Password: ");
                string newPassword = Console.ReadLine();
                _passwords[id].encryptedPassword = Encrypt(newPassword);
                SavePasswords();
                Console.WriteLine("Password updated. (Press any key to continue)");
                Console.ReadKey();
                break;
            }

            Console.WriteLine("Invalid input. Please enter a valid numeric ID! (Press any key to continue)");
            Console.ReadKey();
        }
    }

    private void ShowDecryptedPassword() {
        while (true) {
            Console.Clear();
            ShowPasswords();
            Console.Write("Enter the ID of the entry you want to decrypt (or Q to quit): ");
            string input = Console.ReadLine();
            int id;

            if (input == "q") {
                break;
            }

            if (int.TryParse(input, out id) && id >= 0 && id < _passwords.Count) {
                var entry = _passwords[id];

                string decrypted = Decrypt(entry.encryptedPassword);
                Console.WriteLine($"Password: {decrypted}");

                Console.WriteLine("Do you want to copy the password? (y/n)");
                switch (Console.ReadKey(intercept: true).Key) {
                    case ConsoleKey.Y:
                        Console.WriteLine("Password copied to clipboard! (Press any key to continue)");
                        Console.ReadKey();
                        break;
                    case ConsoleKey.N:
                        Console.WriteLine("(Press any key to continue");
                        Console.ReadKey();
                        break;
                }

                break;
            }

            Console.WriteLine("Invalid input. Please enter a valid numeric ID! (Press any key to continue)");
            Console.ReadKey();
        }
    }

    private void SavePasswords() {
        using var writer = new StreamWriter(_filePath, false);
        foreach (var password in _passwords) {
            writer.WriteLine($"{password.title};{password.encryptedPassword};{password.url};{password.notes}");
        }
    }

    private void GeneratePassword() {
        Console.Clear();
        ShowPasswords();
        const string validChars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890!@#$%^&*()";
        StringBuilder password = new StringBuilder();
        byte[] randomBytes = new byte[1];

        using (RandomNumberGenerator random = RandomNumberGenerator.Create()) {
            while (password.Length < 32) {
                random.GetBytes(randomBytes);
                int value = randomBytes[0] % validChars.Length;
                
                if (randomBytes[0] < validChars.Length * (256 / validChars.Length)) {
                    password.Append(validChars[value]);
                }
            }
        }

        Console.WriteLine($"Generated password: {password}");

        Console.WriteLine("Do you want to copy the password? (y/n)");
        switch (Console.ReadKey(intercept: true).Key) {
            case ConsoleKey.Y:
                Console.WriteLine("Password copied to clipboard! (Press any key to continue)");
                Console.ReadKey();
                break;
            case ConsoleKey.N:
                Console.WriteLine("(Press any key to continue");
                Console.ReadKey();
                break;
        }
    }

    private string Encrypt(string plainText) {
        using var aes = Aes.Create();
        aes.Key = _aesKey;
        aes.IV = _aesIV;
        aes.Mode = CipherMode.CBC;
        aes.Padding = PaddingMode.PKCS7;

        using var encryptor = aes.CreateEncryptor();
        byte[] inputBytes = Encoding.UTF8.GetBytes(plainText);
        byte[] encryptedBytes = encryptor.TransformFinalBlock(inputBytes, 0, inputBytes.Length);
        return Convert.ToBase64String(encryptedBytes);
    }

    private string Decrypt(string encryptedText) {
        using var aes = Aes.Create();
        aes.Key = _aesKey;
        aes.IV = _aesIV;
        aes.Mode = CipherMode.CBC;
        aes.Padding = PaddingMode.PKCS7;

        using var decryptor = aes.CreateDecryptor();
        byte[] encryptedBytes = Convert.FromBase64String(encryptedText);
        byte[] decryptedBytes = decryptor.TransformFinalBlock(encryptedBytes, 0, encryptedBytes.Length);
        return Encoding.UTF8.GetString(decryptedBytes);
    }
}