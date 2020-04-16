using System.Data.SqlClient;
using System.Data;

namespace PizzaBot
{
    class DB
    {
        private readonly SqlConnection connection = new SqlConnection(@"Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename=C:\Users\User\source\repos\PizzaBot\PizzaBot\Database1.mdf;Integrated Security=True");
        public async void OpenAsync()
        {
            if(connection.State == ConnectionState.Closed)
                 await connection.OpenAsync();
        }
        public async void Close()
        {
            if (connection.State != ConnectionState.Closed)
                await connection.CloseAsync();
        }
        public SqlConnection GetConnection()
        {
           
            return connection;
        }
    }
}