using System.Collections.Generic;

namespace UserFormApp.Models
{
    public static class FakeUserDB
    {
        public static List<UserModel> RegisteredUsers { get; set; } = new List<UserModel>();
    }
}
