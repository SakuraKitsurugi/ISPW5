namespace ISPW5;

public class PasswordManagerHub {
    private string _username;
    private string _password;

    public PasswordManagerHub(string username, string password)
    {
        _username = username;
        _password = password;
    }

    public void Start() {
        Console.Clear();
        Console.WriteLine($"Welcome back, {_username}!");
        
    }
}