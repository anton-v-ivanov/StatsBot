namespace TlenBot.Entities
{
    public class UserInfo
    {
        public int Id { get; }
        public string UserName { get; }
        public string FirstName { get; }
        public string LastName { get; }

        public UserInfo(int id, string username, string firstName, string lastName)
        {
            Id = id;
            UserName = username;
            FirstName = firstName;
            LastName = lastName;
        }

        public string GetName()
        {
            if (string.IsNullOrEmpty(FirstName) && string.IsNullOrEmpty(LastName) && string.IsNullOrEmpty(UserName))
                return Id.ToString();

            if (string.IsNullOrEmpty(FirstName) && string.IsNullOrEmpty(LastName))
                return UserName;

            var result = string.Empty;

            if (!string.IsNullOrEmpty(FirstName))
                result += FirstName;

            if (!string.IsNullOrEmpty(LastName))
                result += " " + LastName;

            return result;
        }

        public override int GetHashCode()
        {
            return Id;
        }
    }
}