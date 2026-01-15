using System;
using System.Data.SqlClient;
using System.Configuration;

namespace CandlePatternML
{
    /// <summary>
    /// Model class for the RSI4Long table
    /// </summary>
    public class RSI4LongModel
    {
        public int ID { get; set; }
        public int SetupInstanceID { get; set; }
        public decimal? RSI4Value { get; set; }

        /// <summary>
        /// Constructor for RSI4LongModel
        /// </summary>
        /// <param name="setupInstance">The SetupInstancesModel instance</param>
        /// <param name="rsi4Value">The RSI4 value</param>
        public RSI4LongModel(SetupInstancesModel setupInstance, decimal rsi4Value)
        {
            SetupInstanceID = setupInstance.ID;
            RSI4Value = rsi4Value;
        }

        /// <summary>
        /// Saves this RSI4LongModel to the RSI4Long table in the database
        /// </summary>
        public void Save()
        {
            using (SqlConnection connection = new SqlConnection(ConfigurationManager.ConnectionStrings["StockSetup"].ConnectionString))
            {
                connection.Open();
                string insertQuery = @"INSERT INTO [RSI4Long] ([SetupInstanceID], [RSI4Value])
                                      OUTPUT INSERTED.ID
                                      VALUES (@SetupInstanceID, @RSI4Value)";
                
                using (SqlCommand command = new SqlCommand(insertQuery, connection))
                {
                    command.Parameters.AddWithValue("@SetupInstanceID", SetupInstanceID);
                    command.Parameters.AddWithValue("@RSI4Value", RSI4Value);
                    
                    ID = (int)command.ExecuteScalar();
                }
            }
        }
    }
}


