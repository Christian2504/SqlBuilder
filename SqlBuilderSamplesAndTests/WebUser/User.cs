using SqlBuilderFramework;
using System.Text.Json.Serialization;

namespace SqlBuilderSamplesAndTests
{
    public class User
    {
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Username { get; set; }
        public string Role { get; set; }

        [JsonIgnore]
        public string Password { get; set; }

        public static DbMapper<User> Mapper =>
            new DbMapper<User>()
                .Map(Tables.Users.ColId, (user, value) => user.Id = value)
                .Map(Tables.Users.ColFirstName, (user, value) => user.FirstName = value)
                .Map(Tables.Users.ColLastName, (user, value) => user.LastName = value)
                .Map(Tables.Users.ColUsername, (user, value) => user.Username = value)
                .Map(Tables.Users.ColRole, (user, value) => user.Role = value)
                .Map(Tables.Users.ColPassword, (user, value) => user.Password = value);
    }

    public class ChangeUserPassword
    {
        public int Id { get; set; }
        public string Password { get; set; }
        public string NewPassword { get; set; }
    }

    public class UserNew
    {
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName  { get; set; }
        public string Username { get; set; }
        public string Role { get; set; }
    }
}