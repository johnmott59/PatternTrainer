using System;
using System.Data.SqlClient;
using System.Configuration;

namespace CandlePatternML
{
    /// <summary>
    /// Model class for the SetupInstances table
    /// </summary>
    public class SetupInstancesModel
    {
        public int ID { get; set; }
        public string Ticker { get; set; }
        public DateTime DateCreated { get; set; }

        /// <summary>
        /// Constructor for SetupInstancesModel
        /// </summary>
        /// <param name="ticker">The ticker symbol</param>
        /// <param name="dateCreated">The date when the setup instance was created</param>
        public SetupInstancesModel(string ticker, DateTime dateCreated)
        {
            Ticker = ticker;
            DateCreated = dateCreated;
        }

        /// <summary>
        /// Saves this SetupInstancesModel to the SetupInstances table in the database
        /// </summary>
        public void Save()
        {
            using (SqlConnection connection = new SqlConnection(ConfigurationManager.ConnectionStrings["StockSetup"].ConnectionString))
            {
                connection.Open();
                string insertQuery = @"INSERT INTO [SetupInstances] ([Ticker], [DateCreated])
                                      OUTPUT INSERTED.ID
                                      VALUES (@Ticker, @DateCreated)";
                
                using (SqlCommand command = new SqlCommand(insertQuery, connection))
                {
                    command.Parameters.AddWithValue("@Ticker", Ticker);
                    command.Parameters.AddWithValue("@DateCreated", DateCreated);
                    
                    ID = (int)command.ExecuteScalar();
                }
            }
        }
    }
}

