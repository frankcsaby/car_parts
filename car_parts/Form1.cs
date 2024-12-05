using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Data.SqlClient;
using System.Drawing.Text;

namespace car_parts
{
    public partial class main_Form : Form
    {
        public main_Form()
        {
            InitializeComponent();
        }

        public void main_Form_Load(object sender, EventArgs e)
        {
            LoadDataIntoDataGridView();
        }

        public void LoadDataIntoDataGridView()
        {
            string connectionString = "Server=localhost;Database=car_parts;Trusted_Connection=True;";
            string query = "SELECT * FROM car_table"; // Módosítsd a táblanévnek megfelelően

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                try
                {
                    // Kapcsolódás az adatbázishoz
                    connection.Open();

                    // Adat adapter létrehozása
                    SqlDataAdapter dataAdapter = new SqlDataAdapter(query, connection);

                    // Adatok betöltése DataTable-be
                    DataTable dataTable = new DataTable();
                    dataAdapter.Fill(dataTable);

                    // DataGridView feltöltése
                    dataGridView.DataSource = dataTable;
                    dataGridView1.DataSource = dataTable;
                    dataGridView2.DataSource = dataTable;
                    dataGridView3.DataSource = dataTable;
                    dataGridView4.DataSource = dataTable;
                }
                catch (Exception ex)
                {
                    // Hibaüzenet megjelenítése
                    MessageBox.Show($"Nem sikerült betölteni az adatokat: {ex.Message}", "Hiba", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }
        private int selectedRowID = -1; // Az aktuálisan szerkesztett sor ID-ja

        private void dataGridView2_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0) // Ellenőrizzük, hogy nem a fejlécet választották
            {
                // Kiválasztott sor adatai
                DataGridViewRow row = dataGridView1.Rows[e.RowIndex];
                selectedRowID = Convert.ToInt32(row.Cells["ID"].Value); // Feltételezve, hogy az ID oszlop neve "ID"
                priceEditTextBox.Text = row.Cells["Price"].Value.ToString(); // Price oszlop
                quantityEditTextBox.Text = row.Cells["Quantity"].Value.ToString(); // Quantity oszlop
            }
        }

        private void saveButton_Click_1(object sender, EventArgs e)
        {
            if (selectedRowID == -1)
            {
                MessageBox.Show("Válasszon ki egy sort a szerkesztéshez!", "Figyelmeztetés", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string connectionString = "Server=localhost;Database=car_parts;Trusted_Connection=True;";
            string updateQuery = "UPDATE car_table SET Price = @Price, Quantity = @Quantity WHERE ID = @ID";

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                try
                {
                    connection.Open();

                    // Frissítési parancs
                    using (SqlCommand command = new SqlCommand(updateQuery, connection))
                    {
                        command.Parameters.AddWithValue("@Price", Convert.ToDecimal(priceEditTextBox.Text));
                        command.Parameters.AddWithValue("@Quantity", Convert.ToInt32(quantityEditTextBox.Text));
                        command.Parameters.AddWithValue("@ID", selectedRowID);

                        int rowsAffected = command.ExecuteNonQuery();

                        if (rowsAffected > 0)
                        {
                            MessageBox.Show("Adatok sikeresen frissítve!", "Siker", MessageBoxButtons.OK, MessageBoxIcon.Information);

                            // DataGridView frissítése
                            LoadDataIntoDataGridView();
                        }
                        else
                        {
                            MessageBox.Show("Nem sikerült az adatok frissítése!", "Hiba", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Hiba történt a frissítés során: {ex.Message}", "Hiba", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void saveAddBtn_Click(object sender, EventArgs e)
        {
            string connectionString = "Server=localhost;Database=car_parts;Trusted_Connection=True;";

            // SQL command beszuro
            string insertQuery = "INSERT INTO car_table (ID, Name, Type, Price, Quantity, Manufacturer, Compatibility) " +
                                 "VALUES (@ID, @Name, @Type, @Price, @Quantity, @Manufacturer, @Compatibility);";


            // Ellenőrizzük, hogy minden TextBox ki van-e töltve
            if (string.IsNullOrWhiteSpace(nameAddTextBox.Text) ||
                string.IsNullOrWhiteSpace(typeAddTextBox.Text) ||
                string.IsNullOrWhiteSpace(priceAddTextBox.Text) ||
                string.IsNullOrWhiteSpace(quantityAddTextBox.Text) ||
                string.IsNullOrWhiteSpace(manufacturerAddTextBox.Text) ||
                string.IsNullOrWhiteSpace(compatibilityAddTextBox.Text))
            {
                MessageBox.Show("Minden mezőt ki kell tölteni!", "Hiba", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Kapcsolat létrehozása
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                try
                {
                    connection.Open();

                    // SQL parancs inicializálása
                    using (SqlCommand command = new SqlCommand(insertQuery, connection))
                    {
                        // Paraméterek hozzáadása
                        command.Parameters.AddWithValue("@Name", nameAddTextBox.Text);
                        command.Parameters.AddWithValue("@Type", typeAddTextBox.Text);
                        command.Parameters.AddWithValue("@Price", Convert.ToDecimal(priceAddTextBox.Text));
                        command.Parameters.AddWithValue("@Quantity", Convert.ToInt32(quantityAddTextBox.Text));
                        command.Parameters.AddWithValue("@Manufacturer", manufacturerAddTextBox.Text);
                        command.Parameters.AddWithValue("@Compatibility", compatibilityAddTextBox.Text);

                        // Az új ID lekérése
                        int newId = GetNextID(connection);

                        if (newId > 0)
                        {
                            MessageBox.Show($"Új termék sikeresen hozzáadva! Az új ID: {newId}", "Siker", MessageBoxButtons.OK, MessageBoxIcon.Information);

                            // DataGridView frissítése
                            LoadDataIntoDataGridView();

                            // TextBox-ok kiürítése
                            ClearTextBoxes();
                        }
                        else
                        {
                            MessageBox.Show("Nem sikerült hozzáadni a terméket.", "Hiba", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }

                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Hiba történt a mentés során: {ex.Message}", "Hiba", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }

            }
        }

        private int GetNextID(SqlConnection connection)
        {
            string query = "SELECT ISNULL(MAX(ID), 0) + 1 FROM car_table;";
            using (SqlCommand command = new SqlCommand(query, connection))
            {
                return Convert.ToInt32(command.ExecuteScalar());
            }
        }

        // TextBox-ok kiürítése
        private void ClearTextBoxes()
        {
            nameAddTextBox.Text = "";
            typeAddTextBox.Text = "";
            priceAddTextBox.Text = "";
            quantityAddTextBox.Text = "";
            manufacturerAddTextBox.Text = "";
            compatibilityAddTextBox.Text = "";
        }

        private void deleteButton_Click(object sender, EventArgs e)
        {
            if (selectedRowID == -1)
            {
                MessageBox.Show("Válasszon ki egy sort a törléshez!", "Figyelmeztetés", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Figyelmeztetés a törlés előtt
            DialogResult dialogResult = MessageBox.Show("Biztosan törölni szeretné a kiválasztott sort?", "Megerősítés", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
            if (dialogResult != DialogResult.Yes)
            {
                return;
            }

            string connectionString = "Server=localhost;Database=car_parts;Trusted_Connection=True;";
            string deleteQuery = "DELETE FROM car_table WHERE ID = @ID";

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                try
                {
                    connection.Open();

                    // Törlési parancs végrehajtása
                    using (SqlCommand command = new SqlCommand(deleteQuery, connection))
                    {
                        command.Parameters.AddWithValue("@ID", selectedRowID);

                        int rowsAffected = command.ExecuteNonQuery();

                        if (rowsAffected > 0)
                        {
                            MessageBox.Show("A kiválasztott sor sikeresen törölve lett!", "Siker", MessageBoxButtons.OK, MessageBoxIcon.Information);

                            // DataGridView frissítése
                            LoadDataIntoDataGridView();

                            // Reset selectedRowID
                            selectedRowID = -1;
                        }
                        else
                        {
                            MessageBox.Show("Nem sikerült törölni a sort. Ellenőrizze, hogy létezik-e még!", "Hiba", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Hiba történt a törlés során: {ex.Message}", "Hiba", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void dataGridView4_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0) // Ellenőrizzük, hogy nem a fejlécet választották
            {
                DataGridViewRow row = dataGridView2.Rows[e.RowIndex];
                selectedRowID = Convert.ToInt32(row.Cells["ID"].Value); // Feltételezve, hogy az ID oszlop neve "ID"
            }
        }

        private void searchSerachButton_Click(object sender, EventArgs e)
        {
            string connectionString = "Server=localhost;Database=car_parts;Trusted_Connection=True;";
            string baseQuery = "SELECT * FROM car_table WHERE 1=1"; // Alap lekérdezés, amelyhez feltételeket adunk hozzá
            List<SqlParameter> parameters = new List<SqlParameter>(); // Paraméterek tárolása

            // Feltételek hozzáadása, ha a TextBox nem üres
            if (!string.IsNullOrWhiteSpace(priceSearchTextBox.Text))
            {
                baseQuery += " AND Price = @Price";
                parameters.Add(new SqlParameter("@Price", Convert.ToDecimal(priceSearchTextBox.Text)));
            }

            if (!string.IsNullOrWhiteSpace(manufacturerSearchTextBox.Text))
            {
                baseQuery += " AND Manufacturer LIKE @Manufacturer";
                parameters.Add(new SqlParameter("@Manufacturer", "%" + manufacturerSearchTextBox.Text + "%")); // Részleges keresés
            }

            if (!string.IsNullOrWhiteSpace(compatibilitySearchTextBox.Text))
            {
                baseQuery += " AND Compatibility LIKE @Compatibility";
                parameters.Add(new SqlParameter("@Compatibility", "%" + compatibilitySearchTextBox.Text + "%")); // Részleges keresés
            }

            if (!string.IsNullOrWhiteSpace(typeSearchTextBox.Text))
            {
                baseQuery += " AND Type LIKE @Type";
                parameters.Add(new SqlParameter("@Type", "%" + typeSearchTextBox.Text + "%")); // Részleges keresés
            }

            // Kapcsolódás az adatbázishoz és a keresés végrehajtása
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                try
                {
                    connection.Open();

                    using (SqlCommand command = new SqlCommand(baseQuery, connection))
                    {
                        // Paraméterek hozzáadása a lekérdezéshez
                        command.Parameters.AddRange(parameters.ToArray());

                        SqlDataAdapter dataAdapter = new SqlDataAdapter(command);
                        DataTable dataTable = new DataTable();
                        dataAdapter.Fill(dataTable);

                        // Eredmények megjelenítése a DataGridView-ben
                        dataGridView.DataSource = dataTable;
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Hiba történt a keresés során: {ex.Message}", "Hiba", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void filterButton_Click(object sender, EventArgs e)
        {
            string connectionString = "Server=localhost;Database=car_parts;Trusted_Connection=True;";
            string baseQuery = "SELECT * FROM car_table WHERE 1=1"; // Alap SQL lekérdezés
            List<SqlParameter> parameters = new List<SqlParameter>(); // SQL paraméterek gyűjtése

            try
            {
                // Ha a Price mező ki van töltve, pontos értékre szűr
                if (!string.IsNullOrWhiteSpace(priceFilterTextBox.Text))
                {
                    if (decimal.TryParse(priceFilterTextBox.Text, out decimal price))
                    {
                        baseQuery += " AND Price = @Price";
                        parameters.Add(new SqlParameter("@Price", price));
                    }
                    else
                    {
                        MessageBox.Show("A Price mező csak számot tartalmazhat!", "Hiba", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }
                }

                // Ha a Type mező ki van töltve, részleges egyezést használ
                if (!string.IsNullOrWhiteSpace(typeFilterTextBox.Text))
                {
                    baseQuery += " AND Type LIKE @Type";
                    parameters.Add(new SqlParameter("@Type", "%" + typeFilterTextBox.Text + "%"));
                }

                // Ha a Manufacturer mező ki van töltve, részleges egyezést használ
                if (!string.IsNullOrWhiteSpace(manufacturerFilterTextBox.Text))
                {
                    baseQuery += " AND Manufacturer LIKE @Manufacturer";
                    parameters.Add(new SqlParameter("@Manufacturer", "%" + manufacturerFilterTextBox.Text + "%"));
                }

                // Kapcsolódás az adatbázishoz és adatlekérés
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    using (SqlCommand command = new SqlCommand(baseQuery, connection))
                    {
                        // SQL paraméterek hozzáadása
                        command.Parameters.AddRange(parameters.ToArray());

                        SqlDataAdapter dataAdapter = new SqlDataAdapter(command);
                        DataTable dataTable = new DataTable();
                        dataAdapter.Fill(dataTable);

                        // Eredmény megjelenítése a DataGridView-ben
                        dataGridView1.DataSource = dataTable;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Hiba történt a filterezés során: {ex.Message}", "Hiba", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}



