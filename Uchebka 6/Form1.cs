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

namespace Uchebka_6
{
	public partial class Form1 : Form
	{
		public string conntection = "host = localhost; username = postgres; password = 1; database = data_base_for_uchebka6";
		private DataSet ds = new DataSet();
		
			
		
		public Form1()
		{
			InitializeComponent();
			using (var connect = new NpgsqlConnection(conntection))
			{
				connect.Open();
				var command = new NpgsqlCommand("select *from people inner join lib_books lb on people.lib_id = lb.id_book; ", connect);
				var adapter = new NpgsqlDataAdapter(command);
				var table = new DataTable();

				adapter.Fill(ds, "Adv_table");
				dataGridView1.DataSource = ds;
				dataGridView1.DataMember = "Adv_table";
				dataGridView1.Columns[0].Visible = false;
				dataGridView1.Columns[4].Visible = false; // hide columns
				dataGridView1.Columns[5].Visible = false;


				connect.Close();
			}
			foreach(DataGridViewRow row in dataGridView1.Rows)
			{
				
				List<object> values = new List<object>();
				foreach(DataGridViewCell cell in row.Cells)
				{
					values.Add(cell.Value);
				}

				row.Tag = values;
				if (row.IsNewRow)
				{
					row.Tag = null;
				 }
			}

		}

		private void Form1_Load(object sender, EventArgs e)
		{

		}

		private void button_delete_Click(object sender, EventArgs e)
		{
			
			using (var connect = new NpgsqlConnection(conntection))
			{		
				connect.Open();
				var commandDel = new NpgsqlCommand("delete from people where id =@id;", connect);
				var adapter = new NpgsqlDataAdapter(commandDel);
				commandDel.Parameters.AddWithValue("@id", int.Parse(dataGridView1.SelectedRows[0].Cells["id"].Value.ToString()));
																	// возьмем строку, которуб выделили и удалим
				commandDel.ExecuteNonQuery();
				dataGridView1.Rows.Remove(dataGridView1.SelectedRows[0]);
				connect.Close();
			}

		}

		private void dataGridView1_RowValidating(object sender, DataGridViewCellCancelEventArgs e)
		{
			var row = dataGridView1.Rows[e.RowIndex];
			if (dataGridView1.IsCurrentRowDirty) // если датагрибвью еще не зафиксированна(не сохранена) 
			{
				if (!e.Cancel) // событие не отмененно 
				{
					using (var connect = new NpgsqlConnection(conntection))
					{
						connect.Open();
						var command = new NpgsqlCommand() { Connection = connect};
						var command1 = new NpgsqlCommand() { Connection = connect };
						command.Parameters.AddWithValue("@name", row.Cells["name"].Value);
						command.Parameters.AddWithValue("@surname", row.Cells["surname"].Value);
						command.Parameters.AddWithValue("@age", row.Cells["age"].Value);
						command1.Parameters.AddWithValue("@name1", row.Cells["name1"].Value);
						command1.Parameters.AddWithValue("@num_pages", row.Cells["num_pages"].Value);
						command1.Parameters.AddWithValue("@book_was_set", row.Cells["book_was_set"].Value);
						if (row.Tag == null)
						{
							command1.CommandText = @"INSERT into lib_books(name,num_pages, book_was_set) values (@name1, @num_pages, @book_was_set ) returning id_book;";
							try
							{
								int id = (int)command1.ExecuteScalar();//
								command.Parameters.AddWithValue("@lib_id", id);
							}
							catch (Exception E)
							{
								Console.WriteLine(E.Message);
							}


							command.CommandText = @"INSERT into people(name,surname,age,lib_id) values (@name, @surname, @age, @lib_id);";


							command.ExecuteNonQuery();
							command1.ExecuteNonQuery();

						}
						else if (row.Tag != null)
						{
							command.Parameters.AddWithValue("@id", row.Cells["id"].Value);
							command1.Parameters.AddWithValue("@id_book", row.Cells["id_book"].Value);
							command.CommandText = @"UPDATE people set name = @name, surname= @surname, age = @age where id =@id";
							command1.CommandText = @"UPDATE lib_books set name = @name1, num_pages = @num_pages, book_was_set = @book_was_set where id_book =@id_book";

							command.ExecuteNonQuery();
							command1.ExecuteNonQuery();
						}
					
					}
				}
			}
			foreach (DataGridViewRow row_ in dataGridView1.Rows)
			{

				List<object> values = new List<object>();
				foreach (DataGridViewCell cell in row_.Cells)
				{
					values.Add(cell.Value);
				}

				row_.Tag = values;
				if (row_.IsNewRow)
				{
					row_.Tag = null;
				}
			}

		}
	
	}
}
