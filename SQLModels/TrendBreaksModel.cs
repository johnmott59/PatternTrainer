using System;
using System.Data.SqlClient;
using System.Configuration;

namespace CandlePatternML
{
    /// <summary>
    /// Model class for the TrendBreaks table
    /// </summary>
    public class TrendBreaksModel
    {
        public int ID { get; set; }
        public int SetupInstanceID { get; set; }
        public DateTime? TrendBreakDate { get; set; }
        public decimal? ProjectedBreakValue { get; set; }

        /// <summary>
        /// Constructor for TrendBreaksModel
        /// </summary>
        /// <param name="setupInstance">The SetupInstancesModel instance</param>
        /// <param name="trendBreakDate">The trend break date</param>
        /// <param name="projectedBreakValue">The projected break value</param>
        public TrendBreaksModel(SetupInstancesModel setupInstance, DateTime trendBreakDate, decimal projectedBreakValue)
        {
            SetupInstanceID = setupInstance.ID;
            TrendBreakDate = trendBreakDate;
            ProjectedBreakValue = projectedBreakValue;
        }

        /// <summary>
        /// Saves this TrendBreaksModel to the TrendBreaks table in the database
        /// </summary>
        public void Save()
        {
            using (SqlConnection connection = new SqlConnection(ConfigurationManager.ConnectionStrings["StockSetup"].ConnectionString))
            {
                connection.Open();
                string insertQuery = @"INSERT INTO [TrendBreaks] ([SetupInstanceID], [TrendBreakDate], [ProjectedBreakValue])
                                      OUTPUT INSERTED.ID
                                      VALUES (@SetupInstanceID, @TrendBreakDate, @ProjectedBreakValue)";
                
                using (SqlCommand command = new SqlCommand(insertQuery, connection))
                {
                    command.Parameters.AddWithValue("@SetupInstanceID", SetupInstanceID);
                    command.Parameters.AddWithValue("@TrendBreakDate", TrendBreakDate);
                    command.Parameters.AddWithValue("@ProjectedBreakValue", ProjectedBreakValue);
                    
                    ID = (int)command.ExecuteScalar();
                }
            }
        }
    }
}

