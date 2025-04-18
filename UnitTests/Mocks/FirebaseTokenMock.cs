public class FirebaseTokenMock
{
    public string Uid { get; set; }
    public Dictionary<string, object> Claims { get; set; }

    public FirebaseTokenMock(string uid, Dictionary<string, object>? claims = null)
    {
        Uid = uid;
        Claims = claims ?? new Dictionary<string, object>();
    }
}