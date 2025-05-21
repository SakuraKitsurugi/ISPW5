> [!IMPORTANT]
> Grade: ?/10
## Task:
**Your goal is to create a Password Manager application that allows users to securely store, update, and retrieve their passwords. You will implement encryption and basic password management functions. You may use a GUI (e.g., WinForms, WPF, JavaFX) or a console-based interface.**
> [!NOTE]
> Any decrypted information is stored only in memory and never written to disk.  
> This ensures that no sensitive data remains if the app closes, crashes, or the user logs out.  
> Some tasks are marked as complete based on this approach, as it meets the intended security goals.
## Mandatory Features
- Secure File Handling:
  - - [x] When the application is started for the first time, it must create a new .csv or .txt file to store password data.
  - - [x] When the application is closed, this file must be encrypted using the AES algorithm.
  - - [x] When the application is launched, it must decrypt the file to allow access to the stored data.
- - Add a New Password Entry:
  - - [x] Provide a form to input the following fields:
    - Title (e.g., "Facebook")
    - Password
    - URL or application name
    - Additional notes
  - - [x] The password itself must be encrypted before saving using AES, DES, or RSA.
- Search for a Password:
  - - [ ] Allow searching for a password entry using its title.
  - - [ ] Display the associated data (e.g., URL, notes), but do not display the password immediately.
- Update a Password:
  - - [x] Allow updating an existing password entry by its title.
  - - [x] The updated password must be encrypted using the same algorithm as originally selected.
- Delete a Password Entry:
  - - [x] Implement a function to delete an existing password entry by title.
  - - [x] Remove the associated data from the file.
## Optional Features
- User Registration & Login:
  - - [x] Add a registration form that stores username and hashed/encrypted password (Recommended methods for password hashing: **PBKDF2**, Bcrypt, Scrypt, or Argon2).
  - - [x] Add a login form to verify the user.     
  - - [x] After successful registration, a unique .csv or .txt file should be created and assigned to the user.
  - - [x] Modify the mandatory file encryption feature to support user-specific files.
- File Encryption on Login/Logout: 
  - - [x] After login, the user's file should be decrypted automatically.
  - - [x] When the user logs out or the application closes unexpectedly, the file should be encrypted.
- Random Password Generator:
  - - [ ] Implement a feature to generate strong, random passwords.
  - - [ ] Allow the user to select password length and character types (e.g., symbols, numbers, uppercase, lowercase).
- Secure Reveal:
  - - [x] After searching for a password, do not show it immediately.
  - - [x] Instead, provide a "Show" button that decrypts and displays the password when clicked.
- Copy to Clipboard:
  - - [ ] Add a button that copies the decrypted password directly to the clipboard for easy use.
