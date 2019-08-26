namespace StatsBot.Entities
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

        public override string ToString()
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

        protected bool Equals(UserInfo other)
        {
            return Id == other.Id;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((UserInfo)obj);
        }

        public override int GetHashCode()
        {
            return Id;
        }
    }
}
