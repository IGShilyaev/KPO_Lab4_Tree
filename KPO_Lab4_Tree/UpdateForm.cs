using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace KPO_Lab4_Tree
{
    public partial class UpdateForm : Form
    {
        private readonly string connectionString = ConfigurationManager.AppSettings.Get("ConnectionString").ToString();
        int id = -1;
        string ent = "";

        public UpdateForm()
        {
            InitializeComponent();
        }

        public UpdateForm(string table, int ind)
        {
            id = ind;
            ent = table;
            InitializeComponent();
        }

        private void UpdateForm_Load(object sender, EventArgs e)
        {
            LoadData();
        }

        private void LoadData()
        {
            using (var con = GetOpenedConnection())
            {
                try
                {
                    SqlCommand cmd = new SqlCommand($"select * from {ent} where ID = @id", con);
                    cmd.Parameters.AddWithValue("@id", id);
                    

                    using (var reader = cmd.ExecuteReader())
                    {
                        reader.Read();
                        for (int i = 0; i < reader.FieldCount; i++)
                        {
                            dataGridView1.Columns.Add(reader.GetName(i), reader.GetName(i));
                        }
                        dataGridView1.Rows.Add();
                        for (int i = 0; i < reader.FieldCount; i++)
                        {
                            dataGridView1.Rows[0].Cells[i].Value = reader.GetValue(i);
                        }
                    }
                    
                }
                catch
                {
                    MessageBox.Show("Не удалось выполнить чтение данных", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    Close();
                }
            }
        }

        private SqlConnection GetOpenedConnection()
        {
            var cnn = new SqlConnection();
            cnn.ConnectionString = connectionString;
            try
            {
                cnn.Open();
            }
            catch
            {
                MessageBox.Show("Не получилось установить соединение", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return null;
            }
            return cnn;
        }

    }
}
