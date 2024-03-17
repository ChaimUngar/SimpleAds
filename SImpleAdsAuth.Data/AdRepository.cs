using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SqlClient;
using System.Reflection.Metadata;

namespace SimpleAdsAuth.Data
{
    public class AdRepository
    {
        private readonly string _connectionString;
        public AdRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        public List<Ad> GetAds()
        {
            using var connection = new SqlConnection(_connectionString);
            using var cmd = connection.CreateCommand();
            cmd.CommandText = "SELECT * FROM Ads";
            List<Ad> ads = new();
            connection.Open();
            using var reader = cmd.ExecuteReader();
            while(reader.Read())
            {
                ads.Add(new()
                {
                    Id = (int)reader["Id"],
                    Number = (string)reader["PhoneNumber"],
                    Details = (string)reader["Text"],
                    DateCreated = (DateTime)reader["DateCreated"],
                    ListerId = (int)reader["ListerId"],
                    ListerName = (string)reader["ListerName"]
                });
            }
            return ads;
        }

        public void AddAd(Ad ad, string email)
        {
            using var connection = new SqlConnection(_connectionString);

            var lister = GetByEmail(email);

            using var cmd = connection.CreateCommand();
            cmd.CommandText = @"INSERT INTO Ads (PhoneNumber, Text, DateCreated, ListerId, ListerName)
                                VALUES (@number, @text, @date, @listerid, @listername)";

            cmd.Parameters.AddWithValue("@number", ad.Number);
            cmd.Parameters.AddWithValue("@text", ad.Details);
            cmd.Parameters.AddWithValue("@date", DateTime.Today);
            cmd.Parameters.AddWithValue("@listerid", lister.Id);
            cmd.Parameters.AddWithValue("@listername", lister.Name);


            connection.Open();

            cmd.ExecuteNonQuery();
        }

        public void AddLister(Lister lister, string password)
        {
            var hash = BCrypt.Net.BCrypt.HashPassword(password);
            using var connection = new SqlConnection(_connectionString);
            using var cmd = connection.CreateCommand();
            cmd.CommandText = @"INSERT INTO Listers (Name, Email, Password)
                                VALUES (@name, @email, @password)";

            cmd.Parameters.AddWithValue("@name", lister.Name);
            cmd.Parameters.AddWithValue("@email", lister.Email);
            cmd.Parameters.AddWithValue("@password", hash);

            connection.Open();

            cmd.ExecuteNonQuery();
        }

        public Lister Login(string email, string password)
        {
            var lister = GetByEmail(email);
            if (lister == null)
            {
                return null;
            }

            var isMatch = BCrypt.Net.BCrypt.Verify(password, lister.PasswordHash);
            if (!isMatch)
            {
                return null;
            }

            return lister;
        }

        public Lister GetByEmail(string email)
        {
            using var connection = new SqlConnection(_connectionString);
            using var cmd = connection.CreateCommand();
            cmd.CommandText = "SELECT TOP 1 * FROM Listers WHERE Email = @email";
            cmd.Parameters.AddWithValue("@email", email);
            connection.Open();
            var reader = cmd.ExecuteReader();
            if (!reader.Read())
            {
                return null;
            }

            return new Lister
            {
                Id = (int)reader["Id"],
                Email = (string)reader["Email"],
                Name = (string)reader["Name"],
                PasswordHash = (string)reader["Password"]
            };
        }

        public List<Ad> GetAdsByLister(string  email)
        {
            using var connection = new SqlConnection(_connectionString);
            using var cmd = connection.CreateCommand();

            var lister = GetByEmail(email);

            cmd.CommandText = @"SELECT * FROM Ads
                                WHERE ListerId = @id";

            cmd.Parameters.AddWithValue("@id", lister.Id);

            List<Ad> ads = new();
            connection.Open();
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                ads.Add(new()
                {
                    Id = (int)reader["Id"],
                    Number = (string)reader["PhoneNumber"],
                    Details = (string)reader["Text"],
                    DateCreated = (DateTime)reader["DateCreated"],
                    ListerId = (int)reader["ListerId"]
                });
            }
            return ads;
        }

        public void DeleteAd(int id)
        {
            using var connection = new SqlConnection(_connectionString);
            using var cmd = connection.CreateCommand();
            cmd.CommandText = @"DELETE FROM Ads
                                WHERE Id = @id";

            cmd.Parameters.AddWithValue("@id", id);

            connection.Open();

            cmd.ExecuteNonQuery();
        }
    }
}
