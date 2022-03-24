namespace Lab2
{
    /// <summary>
    /// We want to give tables as parameters in the code
    /// </summary>
    using System.Configuration;
    using System.Data;
    using System.Data.SqlClient;

    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {

            String con = ConfigurationManager.AppSettings["Connection"];
            MessageBox.Show(con);
            // crapa la chestia asta poate ii trbe eghirimele sau cv de genu gotta work on that
            using(SqlConnection connection =
                new SqlConnection(con)){

                try
                {
                    connection.Open();
                    MessageBox.Show(ConfigurationManager.AppSettings["ParentTable"]);
                    String PKQuery = "SELECT " +
                     "KU.table_name as TABLENAME" +
    ",column_name as PRIMARYKEYCOLUMN" +
    "FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS AS TC" +
"INNER JOIN INFORMATION_SCHEMA.KEY_COLUMN_USAGE AS KU" +
    "ON TC.CONSTRAINT_TYPE = 'PRIMARY KEY'" +
    "AND TC.CONSTRAINT_NAME = KU.CONSTRAINT_NAME" +
    "AND KU.table_name = '"+ ConfigurationManager.AppSettings["ParentTable"]+"'";
                    SqlCommand cmd = new SqlCommand(PKQuery);
                    SqlDataReader reader = cmd.ExecuteReader();

                    String ParentPK = (string)reader.GetValue("PRIMARYKEYCOLUMN");
                    MessageBox.Show(ParentPK);
                    connection.Close();
                }
                catch
                {
                    MessageBox.Show("Ooops crapa!");
                }
            }
        }
    }
}