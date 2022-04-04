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



        public List<TextBox> textBoxes1 = new List<TextBox>();

        public void PlaceTextBoxes(List<TextBox> textBoxes)
        {
            
            String columnName;
            for (int index = 0; index < textBoxes.Count; index++)
            {

                columnName = dataGridView2.Columns[index].HeaderText;
                Label newLabel = new Label();
                newLabel.Text = columnName;
                newLabel.Location = new Point(10, 300 + index * 30);
                newLabel.AutoSize = true;
                newLabel.Font = new Font("Arial", 10, FontStyle.Bold);
                this.Controls.Add(newLabel);
                textBoxes[index].Location = new Point(200, 300 + index * 30);
                this.Controls.Add(textBoxes[index]);
            }
           
        }
      

        
        
        private void button1_Click(object sender, EventArgs e)
        {
            
            String con = ConfigurationManager.AppSettings["Connection"].ToString();
            //String con = "Data Source = DESKTOP-PBGUL9N\\MY_SQL_SERVICE;Initial Catalog=Library;Integrated Security=true";

            // crapa la chestia asta poate ii trbe eghirimele sau cv de genu gotta work on that
            using (SqlConnection connection =
                new SqlConnection(con)) {

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
                    " AND KU.table_name = '" + ConfigurationManager.AppSettings["ParentTable"] + "'";
                    SqlCommand cmd = new SqlCommand(PKQuery, connection);
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
                    " AND KU.table_name = '" + ConfigurationManager.AppSettings["ChildTable"] + "'";

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
                    cmd.Parameters.AddWithValue("@ParentTableName", ConfigurationManager.AppSettings["ParentTable"]);
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


                    String FK = " EXEC sp_fkeys @pktable_name = @ParentTableName" +
                        ",@fktable_name = @ChildTableName";
                    SqlCommand command = new SqlCommand(FK, connection);
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
                    String PopulateTable2 = "Select * From " + ConfigurationManager.AppSettings["ChildTable"] +
                        " Where " + foreignKey + " = " + CellValue.ToString();

                    //we get the values from the child table

                    SqlDataAdapter adapter = new SqlDataAdapter(PopulateTable2, con);
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


                    String columnName = "";
                    // we need to make the textboxes and Labels for the child table in order to make the CRUD operations


                    connection.Close();
                }
                catch (SqlException exception)
                {
                    MessageBox.Show(exception.StackTrace);
                }
            }
        }

        private void dataGridView2_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            String con = ConfigurationManager.AppSettings["Connection"];
            using (SqlConnection connection =
                new SqlConnection(con))
            {
                try
                {
                    connection.Open();
                    if (this.textBoxes1.Any())
                    {
                        for (int index = 0; index < textBoxes1.Count; index++)
                      
                        {
                            this.textBoxes1[index].Text = dataGridView2.Rows[e.RowIndex].Cells[index].Value.ToString();
                        }

                        this.PlaceTextBoxes(this.textBoxes1);
                    }

                    else
                    {
                        for (int index = 0; index < dataGridView2.Columns.Count; index++)
                        {
                            TextBox textBox = new TextBox();
                            textBox.Text = dataGridView2.Rows[e.RowIndex].Cells[index].Value.ToString();
                            textBox.Tag = dataGridView2.Columns[index].HeaderText;
                            this.textBoxes1.Add(textBox);
                        }
                        this.PlaceTextBoxes(this.textBoxes1);

                    }
                    connection.Close();

                }
                catch
                {

                }
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {


            String con = ConfigurationManager.AppSettings["connection"];
            using(SqlConnection connection = 
                new SqlConnection(con))
            {
                try
                {
                    connection.Open();

                    String insertQuery = "Inser into @Table Values (";
                    for (int index = 0; index < dataGridView2.ColumnCount; index++)
                    {
                        insertQuery += "@" + dataGridView2.Columns[index].HeaderText;
                        if (index != dataGridView2.ColumnCount - 1)
                        {
                            insertQuery += ",";
                        }
                    }
                    insertQuery += ")";
                    MessageBox.Show(insertQuery);
                    SqlCommand command = new SqlCommand(insertQuery, connection);
                    command.Parameters.AddWithValue("@Table", ConfigurationManager.AppSettings["ChildTable"]);
                    for (int index = 0; index < dataGridView2.ColumnCount; index++)
                    {

                        //no more selected shit
                        
                        command.Parameters.AddWithValue("@" + dataGridView2.Columns[index].HeaderText, this.textBoxes1[index].Text);
                    }
                    SqlDataReader reader = command.ExecuteReader();
                    reader.Close();
                    //we need to autoRefresh the Tables
                    
                    connection.Close();
                }
                catch
                {
                    MessageBox.Show("Ooops crapa!");
                }
            }
            
        }

        private void button3_Click(object sender, EventArgs e)
        {
            String con = ConfigurationManager.AppSettings["connection"];
            using(SqlConnection connection =
                new SqlConnection(con))
            {
                try
                {
                    connection.Open();

                    // get Primary Key From parent Table
                    String PKQuery = "SELECT " +
                    " KU.table_name as TABLENAME " +
                   " ,column_name as PRIMARYKEYCOLUMN " +
                   " FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS AS TC " +
                   " INNER JOIN INFORMATION_SCHEMA.KEY_COLUMN_USAGE AS KU " +
                   " ON TC.CONSTRAINT_TYPE = 'PRIMARY KEY' " +
                   " AND TC.CONSTRAINT_NAME = KU.CONSTRAINT_NAME " +
                   " AND KU.table_name = ' @ParentTable '";

                    SqlCommand command = new SqlCommand(PKQuery,connection);

                    command.Parameters.AddWithValue("@ParentTable", ConfigurationManager.AppSettings["ParentTable"]);
                    SqlDataReader reader = command.ExecuteReader();
                    String ParentPk = "";
                    while (reader.Read())
                    {
                        ParentPk = reader.GetString(1);
                    }
                    reader.Close();

                    //Get Foreign Key From Child Table
                    String ForeignKeyQuery = "EXEC sp_fkeys @pktable_name = @ParentTable" +
                        ",@fktable_name = @ChildTableName";
                    SqlCommand command2 = new SqlCommand(ForeignKeyQuery, connection);
                    command2.Parameters.AddWithValue("@ParentTable", ConfigurationManager.AppSettings["ParentTable"]);
                    command2.Parameters.AddWithValue("@ChildTableName", ConfigurationManager.AppSettings["ChildTable"]);
                    reader = command2.ExecuteReader();
                    String foreignKey = "";
                    while (reader.Read())
                    {
                        foreignKey = reader.GetString(7);
                    }
                    reader.Close();

                    //Start The Update Query
                    //String UpdateQuery = "Update @Table Set ";
                    String UpdateQuery = "Update" + ConfigurationManager.AppSettings["ChildTable"] + "Set";

                    // parametrization is backwards we want to have @COlumnName = textbox[index].Text fuck me sideways
                    for (int index = 0; index < dataGridView2.ColumnCount; index++)
                    {
                        
                        UpdateQuery += dataGridView2.Columns[index].HeaderText + " = @" + dataGridView2.Columns[index].HeaderText;
                        if (index != dataGridView2.ColumnCount - 1)
                        {
                            UpdateQuery += ",";
                        }
                    }
                    // we change each column
                    

                    String ForeignKeyValue="";
                    //get the value that we want to change
                    for(int index = 0; index < this.textBoxes1.Count; index++)
                    {
                        
                        if(textBoxes1[index].Tag.ToString() == foreignKey)
                        {
                            ForeignKeyValue = textBoxes1[index].Text;
                            break;
                        }
                    }
                    //UpdateQuery += " Where @ForeignKey = @GivenKey";
                    UpdateQuery += " Where @ForeignKey = " + ForeignKeyValue;
                    SqlCommand cmd = new SqlCommand(UpdateQuery, connection);
                  //  cmd.Parameters.AddWithValue("@Table", ConfigurationManager.AppSettings["ChildTable"]);
                    cmd.Parameters.AddWithValue("@ForeignKey", foreignKey);
                    //cmd.Parameters.AddWithValue("@GivenKey", ForeignKeyValue); // this should be the value of the key that we are updating
                    MessageBox.Show(ForeignKeyValue);

                    //all from below should be values from the text boxes
                    for (int index = 0; index < dataGridView2.ColumnCount; index++)
                    {   
                        cmd.Parameters.AddWithValue("@" + dataGridView2.Columns[index].HeaderText, this.textBoxes1[index].Text);
                    }
                    reader = cmd.ExecuteReader();
                    reader.Close();
                    MessageBox.Show("Updated Succesfully!");
                    connection.Close();
                }
                catch
                {
                    MessageBox.Show("Ooops Crapa!");
                }
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            String con = ConfigurationManager.AppSettings["connection"];
            using(SqlConnection connection = 
                new SqlConnection(con))
            {
                try
                {
                    connection.Open();
                    String PrimaryKeyChildQuery = "SELECT " +
                        " KU.table_name as TABLENAME " +
                        " ,column_name as PRIMARYKEYCOLUMN " +
                        " FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS AS TC " +
                        " INNER JOIN INFORMATION_SCHEMA.KEY_COLUMN_USAGE AS KU " +
                        " ON TC.CONSTRAINT_TYPE = 'PRIMARY KEY' " +
                        " AND TC.CONSTRAINT_NAME = KU.CONSTRAINT_NAME " +
                        " AND KU.table_name = ' @ChildTable '";
                    String PKChild = "";
                    SqlCommand command = new SqlCommand(PrimaryKeyChildQuery,connection);
                    command.Parameters.AddWithValue("@ChildTable", ConfigurationManager.AppSettings["ChildTable"]);
                    SqlDataReader reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        PKChild = reader.GetString(1);
                    }
                    reader.Close();


                    String ForeignKeyQuery = "EXEC sp_fkeys @pktable_name = @ParentTable" +
                        ",@fktable_name = @ChildTableName";
                    SqlCommand command2 = new SqlCommand(ForeignKeyQuery, connection);
                    reader = command2.ExecuteReader();
                    String foreignKey = "";
                    while (reader.Read())
                    {
                        foreignKey = reader.GetString(7);
                    }

                    String DeleteQuery = "Delete from @Table where @PrimaryKey = @GivenKey";
                    SqlCommand cmd = new SqlCommand(DeleteQuery, connection);
                    cmd.Parameters.AddWithValue("@Table", ConfigurationManager.AppSettings["ChildTable"]);
                    cmd.Parameters.AddWithValue("@PrimaryKey", PKChild);
                    //SelectedRows[0].Cells[foreignkey]
                    cmd.Parameters.AddWithValue("@GivenKey", dataGridView2.SelectedRows[0].Cells[foreignKey].Value.ToString());

                    reader = cmd.ExecuteReader();
                    reader.Close();
                    connection.Close();
                }
                catch
                {
                    
                }
            }
        }
    }
}