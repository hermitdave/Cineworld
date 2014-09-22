using Cineworld;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CineDataWorkerRole.MobileService
{
    internal class CineDataAccess
    {
        const string connectionString = "Server=tcp:gqdsbgdydw.database.windows.net,1433;Database=invokeit_db;User ID=hermitd@gqdsbgdydw;Password=hd34lon!;Trusted_Connection=False;Encrypt=True;Connection Timeout=30;";

        public DataSet GetFilmReviewData(int Id)
        {
            DataSet ds = new DataSet();

            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();
                var dataAdapter = new SqlDataAdapter(String.Format("Select * from FilmReview WHERE Id={0}", Id), connection);

                try
                {
                    dataAdapter.Fill(ds);
                }
                catch (Exception e)
                {   
                }
            }

            return ds;
        }

        public void InsertFilmReview(FilmReview review)
        {
            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();
                var command = new SqlCommand(String.Format("Select * from FilmReview WHERE Id={0}", Id), connection);

                try
                {
                    command.ExecuteNonQuery();
                }
                catch (Exception e)
                {
                }
            }
        }
    }
}
