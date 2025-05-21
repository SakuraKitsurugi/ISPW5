namespace ISPW5;

public class Passwords {
    public string title { get; set; }
    public string encryptedPassword { get; set; }
    public string url { get; set; }
    public string notes { get; set; }
    
    public override string ToString() {
        return $"{title, -20} ║ {encryptedPassword, -40} ║ {url, -25} ║ {notes, -30}";
    }
}