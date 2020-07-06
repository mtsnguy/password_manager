using System;
using System.Threading;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
//namespace
namespace console_program
{

    //@@@@@@@@@@@Main class
    class Program
    {
        //Entry Point method
        static void Main(string[] args)
        {
            //set up sqlite connection and tables
            Boolean login_success = false;
            String User = "";
            Boolean exit = false;
            SQLiteConnection sqlite_conn;
            sqlite_conn = CreateConnection();
            //check if table exists; if not then create
            if(!TableExists("Users",sqlite_conn)){
                CreateTable(sqlite_conn);
            }
            if(!TableExists("Data",sqlite_conn)){
                CreateTable2(sqlite_conn);
            }
            // InsertData(sqlite_conn);
            // ReadData(sqlite_conn);
            Console.WriteLine("Welcome to PasswordManager v1!");
            Console.WriteLine("Hit Backspace to create a new User or Hit Enter to Login");
            ConsoleKeyInfo info = Console.ReadKey(true);
            if(info.Key == ConsoleKey.Backspace){
                CreateNewUser(sqlite_conn);
            }
            while(!login_success){
                var test = tryLogin(sqlite_conn);
                login_success = test.Item1;
                User = test.Item2;
                if(login_success == false){
                    Console.WriteLine("Username/Password Combination was not found, try again!");
                }
            }
            Console.WriteLine("Successful Login!");
            while(!exit){
                //if it gets past here then successful login!
                exit = PromptUser(sqlite_conn,User);
                //Prompt user if they want to store info or view info
            }
            sqlite_conn.Close();
        }
        //@@@@@@@@@@@@@@Functions for app login
        static void CreateNewUser(SQLiteConnection connection){
            Console.Write("Please enter a user name: ");
            String username = Console.ReadLine();
            Console.Write("Please enter a password: ");
            String password = ReadPassword();
            
            SQLiteCommand cmd = connection.CreateCommand();
            if(!TableExists("Users",connection)){
                CreateTable(connection);
            }
            //need to check if user name exists so it doesn't insert..
            cmd.CommandText = "INSERT INTO Users(username, password) VALUES (@username,@password)";
            cmd.Parameters.Add("@username",DbType.String).Value = username;
            cmd.Parameters.Add("@password",DbType.String).Value = password;
            cmd.ExecuteScalar();
        }

        static SQLiteConnection CreateConnection()
        {
    
            SQLiteConnection sqlite_conn;
            // Create a new database connection:
            sqlite_conn = new SQLiteConnection("Data Source=users.db;Version=3;New=True;Compress=True;");
            // Open the connection:
            try
            {
                sqlite_conn.Open();
            }
            catch (Exception ex)
            {
                Console.Write("there was an issue with the db." + ex);
            }
            return sqlite_conn;
        }

        static void CreateTable(SQLiteConnection connection)
        {
            SQLiteCommand sqlite_cmd;

            string Createsql = "CREATE TABLE Users(username VARCHAR(20), password VARCHAR(20))";
            sqlite_cmd = connection.CreateCommand();
            sqlite_cmd.CommandText = Createsql;
            sqlite_cmd.ExecuteNonQuery();
        }
        static Boolean CheckTable(String username,String password, SQLiteConnection connection){
            SQLiteCommand cmd = connection.CreateCommand();
            cmd.CommandText = "SELECT * FROM Users WHERE username = @username AND password = @password";
            cmd.Parameters.Add("@username",DbType.String).Value = username;
            cmd.Parameters.Add("@password",DbType.String).Value = password;
            return (cmd.ExecuteScalar()!= null);
        }  
        static Tuple<Boolean,String> tryLogin(SQLiteConnection connection){
            Console.Write("Enter your user name : ");
            string userName = Console.ReadLine();
            Console.Write("Enter your password  : ");
            string password = ReadPassword();

            //now handle the checking of valid user/password
            return Tuple.Create(CheckTable(userName,password,connection),userName);
        }
        //@@@@@@@@@@@@@@@@functions for data and storing info
        static Boolean PromptUser(SQLiteConnection connection,String User){
            Console.WriteLine("\nHit Backspace to create a new entry or Hit Enter to view information.");
            ConsoleKeyInfo info = Console.ReadKey(true);
            if(info.Key == ConsoleKey.Backspace){
                CreateNewEntry(connection,User);
            }else if(info.Key == ConsoleKey.Enter){
                ViewUserInfo(connection,User);
            }else if(info.Key == ConsoleKey.Escape){
                return true;
            }
            return false;
        }
        static void CreateNewEntry(SQLiteConnection connection,String User){
            Console.Write("Please enter a Platform(ie: Facebook, Gmail, etc.): ");
            String platform = Console.ReadLine();
            Console.Write("Please enter a user name: ");
            String username = Console.ReadLine();
            Console.Write("Please enter a password: ");
            String password = ReadPassword();

            InsertNewEntry(connection,User,platform,username,password);
        }
        static void ViewUserInfo(SQLiteConnection connection,String User){
            SQLiteDataReader reader;
            SQLiteCommand cmd = connection.CreateCommand();
            cmd.CommandText = "SELECT * FROM DATA WHERE loginusername = @User";
            cmd.Parameters.Add("@user",DbType.String).Value = User;
            reader = cmd.ExecuteReader();
            Console.WriteLine("\nUser|Platform|Username|Password");
            while(reader.Read()){
                string user = reader.GetString(0);
                string platform = reader.GetString(1);
                string username = reader.GetString(2);
                string password = reader.GetString(3);
                Console.WriteLine(user + ","+platform+","+username+","+password);
            }
        }
        static void InsertNewEntry(SQLiteConnection connection,String User,String platform,String username,String password){
            SQLiteCommand cmd = connection.CreateCommand();

            cmd.CommandText = "INSERT INTO Data(loginusername,platform,username, password) VALUES (@user,@platform,@username,@password)";
            cmd.Parameters.Add("@user",DbType.String).Value = User;
            cmd.Parameters.Add("@platform",DbType.String).Value = platform;
            cmd.Parameters.Add("@username",DbType.String).Value = username;
            cmd.Parameters.Add("@password",DbType.String).Value = password;

            cmd.ExecuteScalar();
        }
        static void CreateTable2(SQLiteConnection connection)
        {
            SQLiteCommand sqlite_cmd;

            string Createsql = "CREATE TABLE Data(loginusername VARCHAR(20), platform VARCHAR(20), username VARCHAR(20), password VARCHAR(20))";
            sqlite_cmd = connection.CreateCommand();
            sqlite_cmd.CommandText = Createsql;
            sqlite_cmd.ExecuteNonQuery();
        }
        //@@@@@@@@@@@@@@@@ universal functions
        public static Boolean TableExists(String tableName,SQLiteConnection connection){
            SQLiteCommand cmd = connection.CreateCommand();
            cmd.CommandText = "SELECT * FROM sqlite_master WHERE type = 'table' AND name = @name";
            cmd.Parameters.Add("@name",DbType.String).Value = tableName;
            return (cmd.ExecuteScalar()!= null);
        }
        static Boolean CheckTableDB(String user, String platform, String username,String password, SQLiteConnection connection){
            SQLiteCommand cmd = connection.CreateCommand();
            cmd.CommandText = "SELECT * FROM Data WHERE username = @username AND password = @password";
            cmd.Parameters.Add("@username",DbType.String).Value = username;
            cmd.Parameters.Add("@password",DbType.String).Value = password;
            return (cmd.ExecuteScalar()!= null);
        }        
        public static string ReadPassword()
        {
            string password = "";
            ConsoleKeyInfo info = Console.ReadKey(true);
            while (info.Key != ConsoleKey.Enter)
            {
                if (info.Key != ConsoleKey.Backspace)
                {
                    //add a * indicating using pressed a keydv
                    Console.Write("*");
                    password += info.KeyChar;
                }
                else if (info.Key == ConsoleKey.Backspace)
                {
                    if (!string.IsNullOrEmpty(password))
                    {
                    // remove one character from the list of password characters
                    password = password.Substring(0, password.Length - 1);
                    // get the location of the cursor
                    int pos = Console.CursorLeft;
                    // move the cursor to the left by one character
                    Console.SetCursorPosition(pos - 1, Console.CursorTop);
                    // replace it with space
                    Console.Write(" ");
                    // move the cursor to the left by one character again
                    Console.SetCursorPosition(pos - 1, Console.CursorTop);
                    }
                }
                info = Console.ReadKey(true);
            }

            // add a new line because user pressed enter at the end of their password
            Console.WriteLine();
            return password;
        }
    }
}

 
//       static void InsertData(SQLiteConnection conn)
//       {
//          SQLiteCommand sqlite_cmd;
//          sqlite_cmd = conn.CreateCommand();
//          sqlite_cmd.CommandText = "INSERT INTO SampleTable(Col1, Col2) VALUES ('Test Text ', 1);";
//          sqlite_cmd.ExecuteNonQuery();
//          sqlite_cmd.CommandText = "INSERT INTO SampleTable(Col1, Col2) VALUES ('Test1 Text1 ', 2);";
//          sqlite_cmd.ExecuteNonQuery();
//          sqlite_cmd.CommandText = "INSERT INTO SampleTable(Col1, Col2) VALUES ('Test2 Text2 ', 3);";
//          sqlite_cmd.ExecuteNonQuery();
 
 
//          sqlite_cmd.CommandText = "INSERT INTO SampleTable1(Col1, Col2) VALUES ('Test3 Text3 ', 3);";
//          sqlite_cmd.ExecuteNonQuery();
 
//       }
 
//       static void ReadData(SQLiteConnection conn)
//       {
//          SQLiteDataReader sqlite_datareader;
//          SQLiteCommand sqlite_cmd;
//          sqlite_cmd = conn.CreateCommand();
//          sqlite_cmd.CommandText = "SELECT * FROM SampleTable";
 
//          sqlite_datareader = sqlite_cmd.ExecuteReader();
//          while (sqlite_datareader.Read())
//          {
//             string myreader = sqlite_datareader.GetString(0);
//             Console.WriteLine(myreader);
//          }
//          conn.Close();
//       }
//    }
// }
 