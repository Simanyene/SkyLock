using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SQLite;

namespace SkyLock.Models
{
        public class PlayerAccount
        {
            [PrimaryKey, AutoIncrement]
            public int Id { get; set; }

            public string GamerName { get; set; } = "";

            [Unique]
            public string Email { get; set; } = "";

            public string PasswordHash { get; set; } = "";

            public string PasswordSalt { get; set; } = "";
        }
    }