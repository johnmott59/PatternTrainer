using System;
using System.Data.SqlClient;
using System.Configuration;

namespace CandlePatternML
{
    /// <summary>
    /// Model class for the ThreeBar table
    /// </summary>
    public class ThreeBarModel
    {
        public int ID { get; set; }
        public int SetupInstanceID { get; set; }
        public decimal? Confidence { get; set; }
        public string Note { get; set; }

        /// <summary>
        /// Constructor for ThreeBarModel
        /// </summary>
        /// <param name="setupInstance">The SetupInstancesModel instance</param>
        /// <param name="confidence">The confidence value</param>
        /// <param name="note">The note value</param>
        public ThreeBarModel(SetupInstancesModel setupInstance, decimal confidence, string note)
        {
            SetupInstanceID = setupInstance.ID;
            Confidence = confidence;
            Note = note;
        }

        /// <summary>
        /// Saves this ThreeBarModel to the ThreeBar table in the database
        /// </summary>
        public void Save()
        {
            using (SqlConnection connection = new SqlConnection(ConfigurationManager.ConnectionStrings["StockSetup"].ConnectionString))
            {
                connection.Open();
                string insertQuery = @"INSERT INTO [ThreeBar] ([SetupInstanceID], [Confidence], [Note])
                                      OUTPUT INSERTED.ID
                                      VALUES (@SetupInstanceID, @Confidence, @Note)";
                
                using (SqlCommand command = new SqlCommand(insertQuery, connection))
                {
                    command.Parameters.AddWithValue("@SetupInstanceID", SetupInstanceID);
                    command.Parameters.AddWithValue("@Confidence", Confidence);
                    command.Parameters.AddWithValue("@Note", Note ?? (object)DBNull.Value);
                    
                    ID = (int)command.ExecuteScalar();
                }
            }
        }
    }
}

