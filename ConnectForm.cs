using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Npgsql;

namespace AprioriDiploma
{
    public partial class ConnectForm : Form
    {

        public string server, port, userid, password, database;
        //public bool isConnected = false;
        string firstconnection;
        string constring;
        //string sqlListOfDB = "SELECT datname FROM pg_database"; // выбор списка созданных баз данных
        //List<string> listOfDB;

        NpgsqlConnection connection;

        public ConnectForm()
        {
            InitializeComponent();
            textBox_Server.Text = "localhost";
            textBox_Port.Text = "5432";
            textBox_UserID.Text = "postgres";
            textBox_Password.Text = "1584809";
            textBox_Database.Text = "AprioriTest";
            firstconnection = "Server=" + textBox_Server.Text + "; Port=" + textBox_Port.Text + "; User Id=" + textBox_UserID.Text + "; Password=" + textBox_Password.Text+ ";";
            //using (NpgsqlConnection connect = new NpgsqlConnection(firstconnection))
            //{
            //    NpgsqlCommand cmdListofDB = new NpgsqlCommand(sqlListOfDB, connection);
            //    listOfDB = new List<string>();
            //    connect.Open();
            //    using (var reader = cmdListofDB.ExecuteReader())
            //    {
            //        if (reader.HasRows)
            //        {
            //            while (reader.Read())
            //            {
            //                listOfDB.Add(reader.GetName(0));
            //            }
            //        }
            //    }                
            //}
            //foreach (var el in listOfDB)
            //    comboBoxDB.Items.Add(el);
        }

        private void button_Exit_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        public string getConString()
        {
            string constr = null;

            constr = "Server=" + server + "; Port=" + port + "; User Id=" + userid + "; Password=" + password + "; Database=" + database + ";";

            return constr;

        }

        private void button_Connect_Click(object sender, EventArgs e)
        {
            server = textBox_Server.Text;
            port = textBox_Port.Text;
            userid = textBox_UserID.Text;
            password = textBox_Password.Text;
            database = textBox_Database.Text;

            constring = getConString();

            //constring = "Server=" + server + "; Port=" + port + "; User Id=" + userid + "; Password=" + password + "; Database=" + database+";";
            if (server == "" || port == "" || userid == "" || password == "" || database == "")
            {
                MessageBox.Show("Fill in the fields!");
            }
            else
            {
                try
                {
                    connection = new NpgsqlConnection(constring);
                    connection.Open();
                    MessageBox.Show("Connect successfully!");
                    connection.Close();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error: " + ex.Message, "Fail!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    connection.Close();
                }
            }

            this.Close();

        }
    }
}
