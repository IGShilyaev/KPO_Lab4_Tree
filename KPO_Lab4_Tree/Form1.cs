using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Configuration;
using System.Collections.Specialized;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace KPO_Lab4_Tree
{
    public partial class MainForm : Form
    {
        private readonly string connectionString = ConfigurationManager.AppSettings.Get("ConnectionString").ToString();

        public MainForm()
        {
            InitializeComponent();
            UpdateTree();
        }

        private void RefreshButton_Click(object sender, EventArgs e)
        {
            UpdateTree();
        }

        private void UpdateTree()
        {
            treeView1.Nodes.Clear();

            using (var cnn = GetOpenedConnection())
            {
                LoadOperatorNodes(cnn);
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

        private void LoadOperatorNodes(SqlConnection connection)
        {
            var cmd = new SqlCommand("Select * From Туроператор", connection);

            using (var reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    TreeNode n = new TreeNode(reader["Название"].ToString(), 0, 0);
                    n.Tag = reader["ID"];
                    treeView1.Nodes.Add(n);
                    LoadToursNodes(int.Parse(reader["ID"].ToString()), n);
                }
            }

        }

        private void LoadToursNodes(int touroperID, TreeNode n)
        {
            var connection = GetOpenedConnection();

            var cmd = new SqlCommand("Select * From Тур Where Туроператор = @operID", connection);
            
            cmd.Parameters.AddWithValue("@operID", touroperID);

            using (var reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    TreeNode node = new TreeNode(reader["Название"].ToString(), 1, 1);
                    node.Tag = reader["ID"];
                    node.ImageIndex = 1;
                    n.Nodes.Add(node);
                    LoadTouristNodes(int.Parse(reader["ID"].ToString()), node);
                }
            }
            connection.Close();
        }

        private void LoadTouristNodes(int touroperID, TreeNode n)
        {
            var connection = GetOpenedConnection();
            var cmd = new SqlCommand(@"Select (Имя + ' ' + Фамилия) As ФИО, ID  From Турист Where ТурID = @operID", connection);
       
            cmd.Parameters.AddWithValue("@operID", touroperID);

            using (var reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    TreeNode node = new TreeNode(reader["ФИО"].ToString(), 2, 2);
                    node.Tag = reader["ID"];
                    node.ImageIndex = 2;
                    n.Nodes.Add(node);
                }
            }
            connection.Close();

        }

        private void удалитьToolStripMenuItem_Click(object sender, EventArgs e)
        {

            if (treeView1.SelectedNode == null) return;
            TreeNode n = treeView1.SelectedNode;

            using (var cnn = GetOpenedConnection())
            {
                switch (n.Level)
                {
                    case 0:
                        {
                            DeleteOperator(n, cnn);
                            treeView1.SelectedNode.Remove();
                            break;
                        }
                    case 1:
                        {
                            DeleteTour(n, cnn);
                            treeView1.SelectedNode.Remove();
                            break;
                        }
                    case 2:
                        {
                            DeleteTourist((int)treeView1.SelectedNode.Tag, cnn);
                            treeView1.SelectedNode.Remove();
                            break;
                        }
                }
            }
        }



        private void DeleteOperator(TreeNode deletingNode, SqlConnection conn)
        {
            foreach(TreeNode n in deletingNode.Nodes)
            {
                using (var subconn = GetOpenedConnection())
                {
                    DeleteTour(n, subconn);
                }
            }

            var cmd = new SqlCommand("delete from Туроператор where ID = @operatorID", conn);
            cmd.Parameters.AddWithValue("@operatorID", (int)deletingNode.Tag);
            cmd.ExecuteNonQuery();


        }

        private void DeleteTour(TreeNode deletingNode, SqlConnection conn)
        {
            foreach(TreeNode n in deletingNode.Nodes) 
            {
                using (var subconn = GetOpenedConnection()) 
                {
                    DeleteTourist((int)n.Tag, subconn);
                }
            }

            var cmd = new SqlCommand("delete from Тур where ID = @tourID", conn);
            cmd.Parameters.AddWithValue("@tourID", (int)deletingNode.Tag);
            cmd.ExecuteNonQuery();
        }


        private void DeleteTourist(int touristID, SqlConnection conn)
        {

            var cmd = new SqlCommand("delete from Турист where ID = @touristID", conn);
            cmd.Parameters.AddWithValue("@touristID", touristID);

            cmd.ExecuteNonQuery();

        }

        private void addParametrs(ref SqlCommand com, DataGridViewCellCollection collection, int flag)
        {
            if (flag < 0)
            {
                int i = 1;
                foreach (DataGridViewCell x in collection)
                {
                    string paramText = '@' + i.ToString();
                    com.Parameters.AddWithValue(paramText, x.Value);
                    i++;
                }
            }
            else
            {
                int i = 1;
                foreach (DataGridViewCell x in collection)
                {
                    string paramText = '@' + i.ToString();
                    if (x.ColumnIndex != 0)
                        com.Parameters.AddWithValue(paramText, x.Value);
                    else com.Parameters.AddWithValue(paramText, flag);
                    i++;
                }
            }
        }

        private void addUpdateParametrs(ref SqlCommand com, DataGridViewCellCollection collection)
        {
            int i = 1;
            foreach (DataGridViewCell x in collection)
            {
                string paramText = '@' + i.ToString();
                com.Parameters.AddWithValue(paramText, x.Value);
                i++;
            }
            string t = '@' + i.ToString();
            com.Parameters.AddWithValue(t, (int)treeView1.SelectedNode.Tag);

        }

        private void добавитьToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string table = "";
            string commandPart = "";
            switch (treeView1.SelectedNode.Level)
            {
                case 0:
                    {
                        table = "Туроператор";
                        commandPart = "insert into Туроператор values (@1, @2, @3)";
                        break;
                    }
                case 1:
                    {
                        table = "Тур";
                        commandPart = "insert into Тур values (@1, @2, @3, @4, @5, @6)";
                        break;
                    }
                case 2:
                    {
                        table = "Турист";
                        commandPart = "insert into Турист values (@1, @2, @3, @4)";
                        break;
                    }
            }
            InsertForm form = new InsertForm(table);
            if(form.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    using (var conn = GetOpenedConnection())
                    {
                        var cmd = new SqlCommand(commandPart, conn);
                        addParametrs(ref cmd, form.dataGridView1.Rows[0].Cells, -1);
                        cmd.ExecuteNonQuery();
                        UpdateTree();
                    }
                }
                catch
                {
                    MessageBox.Show("Данные были введены некорректно", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void редактироватьToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string table = "";
            string commandPart = "";
            switch (treeView1.SelectedNode.Level)
            {
                case 0:
                    {
                        table = "Туроператор";
                        commandPart = "update Туроператор set ID = @1, Название = @2, Телефон = @3 where ID = @4";
                        break;
                    }
                case 1:
                    {
                        table = "Тур";
                        commandPart = "update Тур set Туроператор = @1, ID = @2, Название = @3, Дата_Начала = @4, Дата_Окончания = @5, Стоимость = @6 where ID = @7";
                        break;
                    }
                case 2:
                    {
                        table = "Турист";
                        commandPart = "update Турист set ТурID = @1, ID = @2, Фамилия = @3, Имя = @4 where ID = @5";
                        break;
                    }
            }
            UpdateForm form = new UpdateForm(table, (int) treeView1.SelectedNode.Tag);
            if (form.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    using (var conn = GetOpenedConnection())
                    {
                        var cmd = new SqlCommand(commandPart, conn);
                        addUpdateParametrs(ref cmd, form.dataGridView1.Rows[0].Cells);

                        cmd.ExecuteNonQuery();
                        UpdateTree();
                    }
                }
                catch
                {
                     MessageBox.Show("Данные были введены некорректно", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void добавитьВToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string table = "";
            string commandPart = "";
            int headID = (int)treeView1.SelectedNode.Tag;
            switch (treeView1.SelectedNode.Level + 1)
            {
                case 3:
                    {
                        MessageBox.Show("Невозможно добавить зависимое значение к объекту данного уровня", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                        return;
                    }
                case 1:
                    {
                        table = "Тур";
                        commandPart = "insert into Тур values (@1, @2, @3, @4, @5, @6)";
                        break;
                    }
                case 2:
                    {
                        table = "Турист";
                        commandPart = "insert into Турист values (@1, @2, @3, @4)";
                        break;
                    }
            }
            InsertForm form = new InsertForm(table);
            if (form.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    using (var conn = GetOpenedConnection())
                    {
                        var cmd = new SqlCommand(commandPart, conn);
                        addParametrs(ref cmd, form.dataGridView1.Rows[0].Cells, headID);
                        cmd.ExecuteNonQuery();
                        UpdateTree();
                    }
                }
                catch
                {
                    MessageBox.Show("Данные были введены некорректно", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }
    }
}
