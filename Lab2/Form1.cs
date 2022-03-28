namespace Lab2
{
    /// <summary>
    /// We want to give tables as parameters in the code
    /// </summary>
    using System.Configuration;
    using System.Data;
    using System.Data.SqlClient;
    using System;
    using System.Data.OleDb;
    using System.Globalization;
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {

            String con = ConfigurationManager.AppSettings["Connection"].ToString(); 
            //String con = "Data Source = DESKTOP-PBGUL9N\\MY_SQL_SERVICE;Initial Catalog=Library;Integrated Security=true";
           
            // crapa la chestia asta poate ii trbe eghirimele sau cv de genu gotta work on that
            using(SqlConnection connection =
                new SqlConnection(con)){

                try
                {
                    connection.Open();
                    String PKQuery = "SELECT " +
                     " KU.table_name as TABLENAME " +
                    " ,column_name as PRIMARYKEYCOLUMN " +
                    " FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS AS TC " +
                    " INNER JOIN INFORMATION_SCHEMA.KEY_COLUMN_USAGE AS KU " +
                    " ON TC.CONSTRAINT_TYPE = 'PRIMARY KEY' " +
                    " AND TC.CONSTRAINT_NAME = KU.CONSTRAINT_NAME " +
                    " AND KU.table_name = '"+ ConfigurationManager.AppSettings["ParentTable"]+"'";
                    SqlCommand cmd = new SqlCommand(PKQuery,connection);
                    SqlDataReader reader = cmd.ExecuteReader();

                    while (reader.Read())
                    {
                        String ParentPK = reader.GetString(1);
                    }
                    //we Got the primary key from the parent 
                    PKQuery = "SELECT " +
                     " KU.table_name as TABLENAME " +
                    " ,column_name as PRIMARYKEYCOLUMN " +
                    " FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS AS TC " +
                    " INNER JOIN INFORMATION_SCHEMA.KEY_COLUMN_USAGE AS KU " +
                    " ON TC.CONSTRAINT_TYPE = 'PRIMARY KEY' " +
                    " AND TC.CONSTRAINT_NAME = KU.CONSTRAINT_NAME " +
                    " AND KU.table_name = '" + ConfigurationManager.AppSettings["ChildTable"]+ "'";

                    SqlCommand command = new SqlCommand(PKQuery);
                    //reader = cmd.ExecuteReader();
                    //while (reader.Read())
                   // {
                       // String PKChild = reader.GetString(1);
                   // }

                    String getAll = "Select * From " + ConfigurationManager.AppSettings["ParentTable"];
                    SqlDataAdapter adapter = new SqlDataAdapter(getAll, con);
                    SqlCommandBuilder commandBuilder = new SqlCommandBuilder(adapter);
                    DataTable table = new DataTable
                    {
                        Locale = CultureInfo.InvariantCulture
                    };

                    adapter.Fill(table);
                    BindingSource binding = new BindingSource();
                    binding.DataSource = table;
                    dataGridView1.AutoResizeColumns(DataGridViewAutoSizeColumnsMode.AllCells);
                    dataGridView1.DataSource = binding;
                    connection.Close();
                }
                catch
                {
                    MessageBox.Show("Ooops crapa!");
                }
            }
        }

        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            String con = ConfigurationManager.AppSettings["Connection"];

            using (SqlConnection connection = 
                new SqlConnection(con))
            {
                try
                {
                    connection.Open();
                    String PKQuery = "SELECT " +
                    " KU.table_name as TABLENAME " +
                   " ,column_name as PRIMARYKEYCOLUMN " +
                   " FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS AS TC " +
                   " INNER JOIN INFORMATION_SCHEMA.KEY_COLUMN_USAGE AS KU " +
                   " ON TC.CONSTRAINT_TYPE = 'PRIMARY KEY' " +
                   " AND TC.CONSTRAINT_NAME = KU.CONSTRAINT_NAME " +
                   " AND KU.table_name = @ParentTableName";
                    SqlCommand cmd = new SqlCommand(PKQuery, connection);
                    cmd.Parameters.AddWithValue("@ParentTableName",ConfigurationManager.AppSettings["ParentTable"]);
                    SqlDataReader reader = cmd.ExecuteReader();
                    String ParentPK = "";
                    while (reader.Read())
                    {
                         ParentPK = reader.GetString(1);
                    }


                    // we got the primary key from the parent Table 


                    reader.Close();
                    int selectedIndex = dataGridView1.SelectedCells[0].RowIndex;
                    DataGridViewRow selectedRow = dataGridView1.Rows[selectedIndex];
                    var CellValue = selectedRow.Cells[ParentPK].Value;


                    // got the value of the primary key that we pressed on


                    String FK = " EXEC sp_fkeys @pktable_name = @ParentTableName"+
                        ",@fktable_name = @ChildTableName";
                    SqlCommand command = new SqlCommand(FK,connection);
                    command.Parameters.AddWithValue("@ParentTableName", ConfigurationManager.AppSettings["ParentTable"]);
                    command.Parameters.AddWithValue("@ChildTableName", ConfigurationManager.AppSettings["ChildTable"]);
                    SqlDataReader FKreader = command.ExecuteReader();
                    String foreignKey = "";
                    while (FKreader.Read())
                    {
                        foreignKey = FKreader.GetString(7);
                    }


                    //we got the name of the foreign key 

                    FKreader.Close();
                    String PopulateTable2 = "Select * From "+ ConfigurationManager.AppSettings["ChildTable"]+
                        " Where "+ foreignKey+" = "+CellValue.ToString();

                    //we get the values from the child table

                    SqlDataAdapter adapter = new SqlDataAdapter(PopulateTable2,con);
                    SqlCommandBuilder builder = new SqlCommandBuilder(adapter);
                    DataTable table = new DataTable
                    {
                        Locale = CultureInfo.InvariantCulture
                    };
                    adapter.Fill(table);
                    BindingSource binding = new BindingSource();
                    binding.DataSource = table;
                    dataGridView2.AutoResizeColumns(DataGridViewAutoSizeColumnsMode.AllCells);
                    dataGridView2.DataSource = binding;

                    // and we populate the second GridView
                    connection.Close();
                }
                catch(SqlException exception)
                {
                    MessageBox.Show(exception.StackTrace);
                }
            }
        }
    }
}