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
    public partial class MainForm : Form
    {
        /* ----------------------variables-----------------------------------------*/
        string constring;// строка подключения
        string sqlProducts = "SELECT * FROM \"Product\""; // строка запроса выбора из таблицы продуктов
        //string sqlTID = "select \"TID\".tid_num, \"TID\".tid_data, \"TID\".id_customer, \"Product\".name_product from \"TID\", \"Product\" where \"TID\".product_id = \"Product\".id_product";
        string sqlTID = "SELECT * FROM \"TID2\"";
        string sqlNormTID = "SELECT * FROM \"NormTID\""; //строка запроса выбора из нормализованной таблицы транзакций
        //string sqlListOfDB = "SELECT datname FROM pg_database"; // выбор списка созданных баз данных

        int columnsCount = 0; // количество столбцов в нормализованной таблице транзакций

        TransactionDB tidDB = new TransactionDB();

        NpgsqlConnection connect;

        public MainForm()
        {
            InitializeComponent();
        }

        /*---------------------methods---------------------------------------------*/

        private void button_Connection_Click(object sender, EventArgs e)
        {
            connect = new NpgsqlConnection(constring);
            try
            {
                connect.Open();

                NpgsqlCommand cmdProducts = new NpgsqlCommand(sqlProducts, connect);
                NpgsqlCommand cmdNormTID = new NpgsqlCommand(sqlNormTID, connect);
                NpgsqlCommand cmdTID = new NpgsqlCommand(sqlTID, connect);

                DataTable dtProducts = new DataTable();
                DataTable dtNormTID = new DataTable();
                DataTable dtTID = new DataTable();

                dtProducts.Load(cmdProducts.ExecuteReader());
                using (var reader = cmdProducts.ExecuteReader())
                {
                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {
                            tidDB.GetListOfProducts(reader);
                        }
                    }
                }


                dtNormTID.Load(cmdNormTID.ExecuteReader());
                columnsCount = dtNormTID.Columns.Count;
                using (var reader = cmdNormTID.ExecuteReader())
                {
                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {
                            tidDB.GetTIDTable(reader, columnsCount);
                        }
                    }
                }

                dtTID.Load(cmdTID.ExecuteReader());
                columnsCount = dtTID.Columns.Count;
                using (var reader = cmdTID.ExecuteReader())
                {
                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {
                            tidDB.GetCIDTable(reader, columnsCount);
                        }
                    }
                }

                connect.Close();

                dataGridView_Products.DataSource = null;
                dataGridView_Products.DataSource = dtProducts;
                dataGridView_Products.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
                dataGridView_Products.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.None;

                dataGridView_NormTID.DataSource = null;
                dataGridView_NormTID.DataSource = dtNormTID;
                dataGridView_NormTID.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
                dataGridView_NormTID.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.None;

                dataGridView_TID.DataSource = null;
                dataGridView_TID.DataSource = dtTID;
                dataGridView_TID.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
                dataGridView_TID.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.None;

                dataGridView_Transactions.DataSource = null;
                dataGridView_Transactions.AutoGenerateColumns = false;
                dataGridView_Transactions.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
                dataGridView_Transactions.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.None;
                dataGridView_Transactions.ColumnCount = 2;
                dataGridView_Transactions.Columns[0].Name = "Номер чека";
                dataGridView_Transactions.Columns[1].Name = "Список покупок";

                int i = 0;
                foreach (KeyValuePair<int, List<string>> KeyValue in tidDB.tidDict)
                {
                    dataGridView_Transactions.Rows.Add(); // добавить строку, чтобы можно было обращаться к уже существующей строке по индексу
                    dataGridView_Transactions.Rows[i].Cells["Номер чека"].Value = KeyValue.Key;
                    foreach (string str in KeyValue.Value)
                    {
                        dataGridView_Transactions.Rows[i].Cells["Список покупок"].Value += str + " ";
                    }
                    i++;
                }

            }
            catch(Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message, "Fail!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                connect.Close();
            }
        }

        private void toolStripButton_Connect_Click(object sender, EventArgs e)
        {
            ConnectForm conForm = new ConnectForm();
            conForm.ShowDialog();
            constring = conForm.getConString();
                 
        }

        private void button_Apriori_Click(object sender, EventArgs e)
        {
            FormApriori aprioriForm = new FormApriori(tidDB);
            aprioriForm.Show();
        }

        private void button_AprioriAll_Click(object sender, EventArgs e)
        {
            FormAprioriAll seqForm = new FormAprioriAll(tidDB);
            seqForm.Show();

        }
    }
    /*-----------------------------CLASS TRANSACTIONDB---------------------------------*/
    public class TransactionDB
    {
        /*-----------------------------variables---------------------------------*/
        private int n_TID; // номер транзакции
        public int transCount = 0; // число транзакций
        public int cidCount = 0; // количество покупателей

        public Dictionary<int, List<string>> tidDict; // словарь транзакций (номер, список продуктов)
        public Dictionary<int, List<int>> cidDict; 
        List<string> itemsInTID; // вспомогательный список продуктов для записи в словарь 
        public List<string> listOfProducts; // список всех продуктов

        /*---------------------methods---------------------------------------------*/
        public TransactionDB()
        {
            tidDict = new Dictionary<int, List<string>>();
            cidDict = new Dictionary<int, List<int>>();
            itemsInTID = new List<string>();
            listOfProducts = new List<string>();
        }

        public TransactionDB(int trans_count)
        {
            transCount = trans_count;
            tidDict = new Dictionary<int, List<string>>();
        }

        public Dictionary<int, List<string>> GetTIDTable(NpgsqlDataReader reader, int _columsCount) // сформировать словарь
        {
            itemsInTID = new List<string>();
            for (int tablename = 1; tablename < _columsCount; tablename++)
            {
                if (reader.GetBoolean(tablename))
                    itemsInTID.Add(reader.GetName(tablename));
            }

            transCount++;

            n_TID = reader.GetInt32(0); tidDict.Add(n_TID, itemsInTID);
            return tidDict;
        }
        public Dictionary<int, List<int>> GetCIDTable(NpgsqlDataReader reader, int _columsCount) // сформировать словарь
        {
            //transCount++;
            int tid_num;
            int cid_num;
            List<int> n_tidList = new List<int>();

            cid_num = reader.GetInt32(0); tid_num = reader.GetInt32(2);
            n_tidList.Add(tid_num);

            if (cidDict.ContainsKey(cid_num))
            {
                List<int> tempValue = cidDict[cid_num];
                tempValue.Add(tid_num);
            }
            else
            {
                cidDict.Add(cid_num, n_tidList);
                cidCount++;
            }
                



            return cidDict;
        }

        public List<string> GetListOfProducts(NpgsqlDataReader reader)
        {
            //listOfProducts.Add(reader.GetString(2));
            listOfProducts.Add(reader.GetString(1));
            listOfProducts = listOfProducts.OrderBy(x => x).ToList();
            return listOfProducts;
        }
    }
}
