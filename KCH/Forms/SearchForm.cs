using Microsoft.Data.Sqlite;
using KCH.Data;

namespace KCH.Forms;

public class SearchForm : Form
{
    private TextBox txtSearch = new();
    private TextBox txtInvoiceNo = new();
    private TextBox txtCustomerName = new();
    private TextBox txtTotal = new();
    private TextBox txtDiscount = new();
    private TextBox txtNetAmount = new();
    private TextBox txtPaid = new();
    private TextBox txtRemaining = new();
    private DateTimePicker dtpDate = new();
    private RadioButton rbByNumber = new();
    private RadioButton rbByName = new();
    private DataGridView dgvItems = new();
    private DataGridView dgvResults = new();
    private Button btnSearch = new();
    private Button btnSave = new();
    private Button btnPrint = new();
    private Button btnBack = new();

    // أزرار تحريك الصفوف المضافة حديثاً للتعديل الذكي
    private Button btnMoveUp = new();
    private Button btnMoveDown = new();

    // قائمة الاقتراحات التلقائية
    private ListBox lstSuggestions = new();
    private System.Windows.Forms.Timer _searchTimer = new();

    private float currentAppFontSize = 11f;

    public SearchForm()
    {
        InitializeComponent();
        dgvItems.ReadOnly = true; // يتم تفعيله عند تحميل فاتورة للتعديل
    }

    private void InitializeComponent()
    {
        // ── 1. إعدادات النافذة الرئيسية ──────────────────────
        this.Text = "البحث عن الفواتير والتعديل - KCH";
        this.Size = new Size(1100, 750);
        this.StartPosition = FormStartPosition.CenterScreen;
        this.FormBorderStyle = FormBorderStyle.FixedSingle;
        this.MaximizeBox = false;
        this.RightToLeft = RightToLeft.Yes;
        this.RightToLeftLayout = true;
        this.BackColor = Color.FromArgb(24, 28, 34);
        this.Font = new Font("Segoe UI", currentAppFontSize);

        // ── 2. منطقة خيارات البحث (Top Search Card) ──────────────────────
        var pnlSearchCard = new Panel
        {
            Size = new Size(1060, 110),
            Location = new Point(15, 15),
            BackColor = Color.FromArgb(32, 38, 46)
        };
        this.Controls.Add(pnlSearchCard);

        rbByNumber.Text = "🔍  بحث برقم القائمة";
        rbByNumber.Font = new Font("Segoe UI Semibold", 11, FontStyle.Bold);
        rbByNumber.ForeColor = Color.White;
        rbByNumber.Location = new Point(870, 20);
        rbByNumber.AutoSize = true;
        rbByNumber.Checked = true;
        rbByNumber.BackColor = Color.Transparent;
        pnlSearchCard.Controls.Add(rbByNumber);

        rbByName.Text = "👤  بحث باسم الزبون";
        rbByName.Font = new Font("Segoe UI Semibold", 11, FontStyle.Bold);
        rbByName.ForeColor = Color.White;
        rbByName.Location = new Point(870, 60);
        rbByName.AutoSize = true;
        rbByName.BackColor = Color.Transparent;
        pnlSearchCard.Controls.Add(rbByName);

        StyleTextBox(txtSearch, new Size(350, 32), new Point(490, 38), false);
        txtSearch.TextChanged += TxtSearch_TextChanged;
        txtSearch.KeyDown += TxtSearch_KeyDown;
        pnlSearchCard.Controls.Add(txtSearch);

        StyleButton(btnSearch, "🔍 بحث", Color.FromArgb(0, 120, 215), new Point(345, 35));
        btnSearch.Size = new Size(130, 40);
        btnSearch.Click += BtnSearch_Click;
        pnlSearchCard.Controls.Add(btnSearch);

        StyleButton(btnSave, "💾 حفظ التعديل", Color.FromArgb(40, 167, 69), new Point(205, 35));
        btnSave.Size = new Size(130, 40);
        btnSave.Visible = false;
        btnSave.Click += BtnSave_Click;
        pnlSearchCard.Controls.Add(btnSave);

        // جدول نتائج البحث عن طريق الاسم
        dgvResults.Location = new Point(20, 15);
        dgvResults.Size = new Size(170, 80);
        dgvResults.BackgroundColor = Color.FromArgb(48, 56, 65);
        dgvResults.ForeColor = Color.Black;
        dgvResults.RightToLeft = RightToLeft.Yes;
        dgvResults.ReadOnly = true;
        dgvResults.RowHeadersVisible = false;
        dgvResults.AllowUserToAddRows = false;
        dgvResults.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        dgvResults.CellClick += dgvResults_CellClick;
        dgvResults.Visible = false;
        pnlSearchCard.Controls.Add(dgvResults);

        // ── قائمة الاقتراحات التلقائية الداكنة ──
        lstSuggestions.Font = new Font("Segoe UI", 11);
        lstSuggestions.Location = new Point(505, 91);
        lstSuggestions.Size = new Size(350, 120);
        lstSuggestions.Visible = false;
        lstSuggestions.BorderStyle = BorderStyle.FixedSingle;
        lstSuggestions.BackColor = Color.FromArgb(48, 56, 65);
        lstSuggestions.ForeColor = Color.White;
        lstSuggestions.Click += LstSuggestions_Click;
        lstSuggestions.KeyDown += LstSuggestions_KeyDown;
        this.Controls.Add(lstSuggestions);

        _searchTimer.Interval = 300;
        _searchTimer.Tick += SearchTimer_Tick;

        // ── 3. بطاقة عرض تفاصيل الفاتورة المحملة ────────────────
        var pnlHeaderCard = new Panel
        {
            Size = new Size(1060, 85),
            Location = new Point(15, 140),
            BackColor = Color.FromArgb(32, 38, 46)
        };
        this.Controls.Add(pnlHeaderCard);

        pnlHeaderCard.Controls.Add(CreateLabel("رقم القائمة:", new Point(950, 28)));
        StyleTextBox(txtInvoiceNo, new Size(110, 30), new Point(830, 24), true);
        pnlHeaderCard.Controls.Add(txtInvoiceNo);

        pnlHeaderCard.Controls.Add(CreateLabel("اسم الزبون:", new Point(710, 28)));
        StyleTextBox(txtCustomerName, new Size(320, 30), new Point(380, 24), true);
        pnlHeaderCard.Controls.Add(txtCustomerName);

        pnlHeaderCard.Controls.Add(CreateLabel("تاريخ الشراء:", new Point(240, 28)));
        dtpDate.Font = new Font("Segoe UI", currentAppFontSize, FontStyle.Bold);
        dtpDate.Size = new Size(150, 30);
        dtpDate.Location = new Point(80, 24);
        dtpDate.Format = DateTimePickerFormat.Custom;
        dtpDate.CustomFormat = "dd-MM-yyyy";
        pnlHeaderCard.Controls.Add(dtpDate);

        // ── 4. جدول عرض المواد والأزرار الجانبية للتحريك ──────────────────────
        dgvItems.Location = new Point(75, 245);
        dgvItems.Size = new Size(1000, 260);
        dgvItems.BackgroundColor = Color.FromArgb(42, 48, 57);
        dgvItems.ForeColor = Color.Black;
        dgvItems.BorderStyle = BorderStyle.None;
        dgvItems.AllowUserToResizeRows = false;
        dgvItems.RowHeadersVisible = false;
        dgvItems.SelectionMode = DataGridViewSelectionMode.FullRowSelect;

        dgvItems.EnableHeadersVisualStyles = false;
        dgvItems.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI Semibold", currentAppFontSize, FontStyle.Bold);
        dgvItems.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(240, 173, 78);
        dgvItems.ColumnHeadersDefaultCellStyle.ForeColor = Color.Black;
        dgvItems.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
        dgvItems.ColumnHeadersHeight = 38;

        dgvItems.Columns.Add("ID_O", "ت");
        dgvItems.Columns.Add("Name_Object", "📦  اسم المادة / الصنف");
        dgvItems.Columns.Add("No_Object", "🔢  العدد (الكمية)");
        dgvItems.Columns.Add("Price_Object", "💰  سعر المفرد");
        dgvItems.Columns.Add("Total_price", "💵  المبلغ الإجمالي");

        dgvItems.Columns["ID_O"]!.Width = 60;
        dgvItems.Columns["ID_O"]!.ReadOnly = true;
        dgvItems.Columns["ID_O"]!.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
        dgvItems.Columns["Name_Object"]!.Width = 410;
        dgvItems.Columns["No_Object"]!.Width = 120;
        dgvItems.Columns["No_Object"]!.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
        dgvItems.Columns["Price_Object"]!.Width = 170;
        dgvItems.Columns["Total_price"]!.Width = 240;
        dgvItems.Columns["Total_price"]!.ReadOnly = true;
        this.Controls.Add(dgvItems);

        // زر التحريك للأعلى 🔼
        btnMoveUp.Text = "🔼";
        btnMoveUp.Font = new Font("Segoe UI", 14F, FontStyle.Bold);
        btnMoveUp.Size = new Size(50, 50);
        btnMoveUp.Location = new Point(15, 320);
        btnMoveUp.BackColor = Color.FromArgb(240, 173, 78);
        btnMoveUp.ForeColor = Color.Black;
        btnMoveUp.FlatStyle = FlatStyle.Flat;
        btnMoveUp.FlatAppearance.BorderSize = 0;
        btnMoveUp.Cursor = Cursors.Hand;
        btnMoveUp.Click += BtnMoveUp_Click;
        this.Controls.Add(btnMoveUp);

        // زر التحريك للأسفل 🔽
        btnMoveDown.Text = "🔽";
        btnMoveDown.Font = new Font("Segoe UI", 14F, FontStyle.Bold);
        btnMoveDown.Size = new Size(50, 50);
        btnMoveDown.Location = new Point(15, 380);
        btnMoveDown.BackColor = Color.FromArgb(240, 173, 78);
        btnMoveDown.ForeColor = Color.Black;
        btnMoveDown.FlatStyle = FlatStyle.Flat;
        btnMoveDown.FlatAppearance.BorderSize = 0;
        btnMoveDown.Cursor = Cursors.Hand;
        btnMoveDown.Click += BtnMoveDown_Click;
        this.Controls.Add(btnMoveDown);

        // ── 5. الحسابات والمجاميع السفلية ───────────────
        var pnlSummaryCard = new Panel
        {
            Size = new Size(1060, 120),
            Location = new Point(15, 520),
            BackColor = Color.FromArgb(32, 38, 46)
        };
        this.Controls.Add(pnlSummaryCard);

        pnlSummaryCard.Controls.Add(CreateLabel("المجموع الكلي:", new Point(940, 25)));
        StyleTextBox(txtTotal, new Size(160, 30), new Point(765, 21), true);
        txtTotal.ForeColor = Color.FromArgb(0, 150, 255);
        pnlSummaryCard.Controls.Add(txtTotal);

        pnlSummaryCard.Controls.Add(CreateLabel("خصم الفاتورة:", new Point(940, 70)));
        StyleTextBox(txtDiscount, new Size(160, 30), new Point(765, 66), false);
        txtDiscount.ForeColor = Color.FromArgb(255, 100, 100);
        pnlSummaryCard.Controls.Add(txtDiscount);

        pnlSummaryCard.Controls.Add(CreateLabel("المبلغ الصافي:", new Point(590, 25)));
        StyleTextBox(txtNetAmount, new Size(160, 30), new Point(415, 21), true);
        txtNetAmount.ForeColor = Color.FromArgb(40, 167, 69);
        pnlSummaryCard.Controls.Add(txtNetAmount);

        pnlSummaryCard.Controls.Add(CreateLabel("المبلغ الواصل:", new Point(590, 70)));
        StyleTextBox(txtPaid, new Size(160, 30), new Point(415, 66), true);
        txtPaid.ForeColor = Color.White;
        pnlSummaryCard.Controls.Add(txtPaid);

        pnlSummaryCard.Controls.Add(CreateLabel("المتبقي بذمته:", new Point(230, 48)));
        StyleTextBox(txtRemaining, new Size(170, 32), new Point(40, 44), true);
        txtRemaining.Font = new Font("Segoe UI", currentAppFontSize + 1f, FontStyle.Bold);
        txtRemaining.ForeColor = Color.OrangeRed;
        pnlSummaryCard.Controls.Add(txtRemaining);

        // ── 6. أزرار التحكم السفلية ─────────────────────────
        StyleButton(btnPrint, "🖨️ طباعة الفاتورة", Color.FromArgb(108, 117, 125), new Point(165, 660));
        btnPrint.Size = new Size(140, 40);
        btnPrint.Click += BtnPrint_Click;
        this.Controls.Add(btnPrint);

        StyleButton(btnBack, "↩️ رجوع للرئيسية", Color.FromArgb(50, 55, 65), new Point(15, 660));
        btnBack.Size = new Size(140, 40);
        btnBack.Click += BtnBack_Click;
        this.Controls.Add(btnBack);

        lstSuggestions.BringToFront();
    }

    private void BtnMoveUp_Click(object? sender, EventArgs e)
    {
        if (dgvItems.SelectedRows.Count > 0 && !dgvItems.ReadOnly)
        {
            int currentIndex = dgvItems.SelectedRows[0].Index;
            if (currentIndex > 0 && currentIndex < dgvItems.Rows.Count - 1)
            {
                MoveRow(currentIndex, currentIndex - 1);
            }
        }
    }

    private void BtnMoveDown_Click(object? sender, EventArgs e)
    {
        if (dgvItems.SelectedRows.Count > 0 && !dgvItems.ReadOnly)
        {
            int currentIndex = dgvItems.SelectedRows[0].Index;
            if (currentIndex >= 0 && currentIndex < dgvItems.Rows.Count - 2)
            {
                MoveRow(currentIndex, currentIndex + 1);
            }
        }
    }

    private void MoveRow(int sourceIndex, int destIndex)
    {
        DataGridViewRow currentRow = dgvItems.Rows[sourceIndex];
        object[] cellValues = new object[currentRow.Cells.Count];
        for (int i = 0; i < currentRow.Cells.Count; i++)
        {
            cellValues[i] = currentRow.Cells[i].Value;
        }

        dgvItems.Rows.RemoveAt(sourceIndex);
        dgvItems.Rows.Insert(destIndex, cellValues);

        dgvItems.ClearSelection();
        dgvItems.Rows[destIndex].Selected = true;

        RecalculateRowNumbers();
    }

    private void RecalculateRowNumbers()
    {
        for (int i = 0; i < dgvItems.Rows.Count - 1; i++)
        {
            dgvItems.Rows[i].Cells["ID_O"].Value = i + 1;
        }
    }

    private void TxtSearch_TextChanged(object? sender, EventArgs e)
    {
        _searchTimer.Stop();
        if (string.IsNullOrWhiteSpace(txtSearch.Text))
        {
            lstSuggestions.Visible = false;
            return;
        }
        _searchTimer.Start();
    }

    private void SearchTimer_Tick(object? sender, EventArgs e)
    {
        _searchTimer.Stop();
        LoadSuggestions(txtSearch.Text.Trim());
    }

    private void LoadSuggestions(string keyword)
    {
        lstSuggestions.Items.Clear();
        try
        {
            using var conn = DatabaseHelper.GetConnection();
            using var cmd = conn.CreateCommand();

            if (rbByNumber.Checked)
            {
                cmd.CommandText = "SELECT ID_C, Name_C FROM Info_Cost WHERE CAST(ID_C AS TEXT) LIKE @q LIMIT 10";
                cmd.Parameters.AddWithValue("@q", keyword + "%");
            }
            else
            {
                cmd.CommandText = "SELECT ID_C, Name_C FROM Info_Cost WHERE Name_C LIKE @q LIMIT 10";
                cmd.Parameters.AddWithValue("@q", "%" + keyword + "%");
            }

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                string id = reader.GetInt64(0).ToString();
                string name = reader.IsDBNull(1) ? "" : reader.GetString(1);
                lstSuggestions.Items.Add(new SuggestionItem(id, name));
            }
        }
        catch { }

        if (lstSuggestions.Items.Count > 0)
        {
            lstSuggestions.Height = Math.Min(lstSuggestions.Items.Count * 22 + 8, 160);
            lstSuggestions.Visible = true;
            lstSuggestions.BringToFront();
        }
        else
        {
            lstSuggestions.Visible = false;
        }
    }

    private void LstSuggestions_Click(object? sender, EventArgs e)
    {
        SelectSuggestion();
    }

    private void TxtSearch_KeyDown(object? sender, KeyEventArgs e)
    {
        if (!lstSuggestions.Visible) return;
        if (e.KeyCode == Keys.Down)
        {
            lstSuggestions.Focus();
            if (lstSuggestions.Items.Count > 0)
                lstSuggestions.SelectedIndex = 0;
            e.Handled = true;
        }
        else if (e.KeyCode == Keys.Enter)
        {
            lstSuggestions.Visible = false;
            BtnSearch_Click(sender, e);
            e.Handled = true;
        }
        else if (e.KeyCode == Keys.Escape)
        {
            lstSuggestions.Visible = false;
        }
    }

    private void LstSuggestions_KeyDown(object? sender, KeyEventArgs e)
    {
        if (e.KeyCode == Keys.Enter || e.KeyCode == Keys.Return)
        {
            SelectSuggestion();
            e.Handled = true;
        }
        else if (e.KeyCode == Keys.Escape)
        {
            lstSuggestions.Visible = false;
            txtSearch.Focus();
        }
    }

    private void SelectSuggestion()
    {
        if (lstSuggestions.SelectedItem is SuggestionItem item)
        {
            lstSuggestions.Visible = false;
            txtSearch.Text = item.Id;
            rbByNumber.Checked = true;
            SearchById(item.Id);
        }
    }

    private void dgvResults_CellClick(object? sender, DataGridViewCellEventArgs e)
    {
        if (e.RowIndex < 0) return;
        var id = dgvResults.Rows[e.RowIndex].Cells[0].Value?.ToString();
        if (id != null)
        {
            txtSearch.Text = id;
            rbByNumber.Checked = true;
            SearchById(id);
        }
    }

    private void BtnSearch_Click(object? sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(txtSearch.Text)) return;

        if (rbByNumber.Checked)
        {
            SearchById(txtSearch.Text.Trim());
        }
        else
        {
            try
            {
                using var conn = DatabaseHelper.GetConnection();
                using var cmd = conn.CreateCommand();
                cmd.CommandText = "SELECT ID_C AS [رقم الفاتورة], Name_C AS [اسم الزبون] FROM Info_Cost WHERE Name_C LIKE @name";
                cmd.Parameters.AddWithValue("@name", "%" + txtSearch.Text.Trim() + "%");

                var dt = new System.Data.DataTable();
                using var reader = cmd.ExecuteReader();
                dt.Load(reader);
                dgvResults.DataSource = dt;
                dgvResults.Visible = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show("خطأ أثناء البحث بالاسم: " + ex.Message);
            }
        }
    }

    private void SearchById(string id)
    {
        using var conn = DatabaseHelper.GetConnection();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT * FROM Info_Cost WHERE ID_C = @id";
        cmd.Parameters.AddWithValue("@id", id);

        using var reader = cmd.ExecuteReader();
        if (reader.Read())
        {
            txtInvoiceNo.Text = reader["ID_C"]?.ToString();
            txtCustomerName.Text = reader["Name_C"]?.ToString();
            dtpDate.Text = reader["Da"]?.ToString();
            txtTotal.Text = reader["Final_price"]?.ToString();
            txtDiscount.Text = reader["Discount"]?.ToString();
            txtNetAmount.Text = reader["S_P"]?.ToString();
            txtPaid.Text = reader["Pay"]?.ToString();
            txtRemaining.Text = reader["Bro"]?.ToString();
            reader.Close();

            dgvItems.Rows.Clear();
            using var cmdItems = conn.CreateCommand();
            cmdItems.CommandText = "SELECT ID_O, Name_Object, No_Object, Price_Object, Total_price FROM Menu_Cost WHERE ID_C = @id ORDER BY ID_O";
            cmdItems.Parameters.AddWithValue("@id", id);

            using var itemReader = cmdItems.ExecuteReader();
            while (itemReader.Read())
            {
                dgvItems.Rows.Add(
                    itemReader["ID_O"],
                    itemReader["Name_Object"],
                    itemReader["No_Object"],
                    itemReader["Price_Object"],
                    itemReader["Total_price"]
                );
            }

            btnSave.Visible = true;
            dgvItems.ReadOnly = false;
            dgvResults.Visible = false;
        }
        else
        {
            MessageBox.Show("لا توجد فاتورة مسجلة بهذا الرقم", "تنبيه", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }

    private void BtnSave_Click(object? sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(txtInvoiceNo.Text)) return;

        var invoiceId = Convert.ToInt64(txtInvoiceNo.Text);

        for (int i = 0; i < dgvItems.Rows.Count - 1; i++)
        {
            var row = dgvItems.Rows[i];
            if (row.Cells["Name_Object"].Value == null || row.Cells["No_Object"].Value == null || row.Cells["Price_Object"].Value == null)
            {
                MessageBox.Show("تنبيه: هناك نقص في بيانات الجدول المحرك!", "خطأ", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
        }

        using var conn = DatabaseHelper.GetConnection();
        using var transaction = conn.BeginTransaction();

        try
        {
            using var cmdDel = conn.CreateCommand();
            cmdDel.Transaction = transaction;
            cmdDel.CommandText = "DELETE FROM Menu_Cost WHERE ID_C = @id";
            cmdDel.Parameters.AddWithValue("@id", invoiceId);
            cmdDel.ExecuteNonQuery();

            double totalAmount = 0;
            for (int i = 0; i < dgvItems.Rows.Count - 1; i++)
            {
                var row = dgvItems.Rows[i];
                var qty = Convert.ToDouble(row.Cells["No_Object"].Value);
                var price = Convert.ToDouble(row.Cells["Price_Object"].Value);
                var itemTotal = qty * price;
                totalAmount += itemTotal;
                row.Cells["Total_price"].Value = itemTotal;

                using var cmdIns = conn.CreateCommand();
                cmdIns.Transaction = transaction;
                cmdIns.CommandText = @"INSERT INTO Menu_Cost (ID_C, ID_O, Name_Object, No_Object, Price_Object, Total_price)
                                      VALUES (@idC, @idO, @name, @no, @price, @total)";
                cmdIns.Parameters.AddWithValue("@idC", invoiceId);
                cmdIns.Parameters.AddWithValue("@idO", i + 1);
                cmdIns.Parameters.AddWithValue("@name", row.Cells["Name_Object"].Value?.ToString() ?? "");
                cmdIns.Parameters.AddWithValue("@no", qty);
                cmdIns.Parameters.AddWithValue("@price", price);
                cmdIns.Parameters.AddWithValue("@total", itemTotal);
                cmdIns.ExecuteNonQuery();
            }

            double discount = 0;
            double.TryParse(txtDiscount.Text, out discount);
            double netAmount = totalAmount - discount;

            var payInput = Microsoft.VisualBasic.Interaction.InputBox("الرجاء تحديث المبلغ الواصل من الزبون بعد التعديل:", "تحديث المدفوعات", netAmount.ToString());
            if (string.IsNullOrWhiteSpace(payInput)) { transaction.Rollback(); return; }

            double paid = Convert.ToDouble(payInput);
            double remaining = netAmount - paid;

            using var cmdUpdate = conn.CreateCommand();
            cmdUpdate.Transaction = transaction;
            cmdUpdate.CommandText = @"UPDATE Info_Cost SET Da = @da, Discount = @disc, Pay = @pay, Bro = @bro, Final_price = @final, S_P = @sp
                                     WHERE ID_C = @id";
            cmdUpdate.Parameters.AddWithValue("@da", dtpDate.Text);
            cmdUpdate.Parameters.AddWithValue("@disc", discount);
            cmdUpdate.Parameters.AddWithValue("@pay", paid);
            cmdUpdate.Parameters.AddWithValue("@bro", remaining);
            cmdUpdate.Parameters.AddWithValue("@final", totalAmount);
            cmdUpdate.Parameters.AddWithValue("@sp", netAmount);
            cmdUpdate.Parameters.AddWithValue("@id", invoiceId);
            cmdUpdate.ExecuteNonQuery();

            transaction.Commit();

            txtTotal.Text = totalAmount.ToString("N0");
            txtNetAmount.Text = netAmount.ToString("N0");
            txtPaid.Text = paid.ToString("N0");
            txtRemaining.Text = remaining.ToString("N0");

            MessageBox.Show("تم حفظ وإعادة هيكلة الفاتورة بنجاح التام", "تم بنجاح", MessageBoxButtons.OK, MessageBoxIcon.Information);
            btnSave.Visible = false;
            dgvItems.ReadOnly = true;
        }
        catch (Exception ex)
        {
            transaction.Rollback();
            MessageBox.Show("فشلت عملية الحفظ بسبب: \n" + ex.Message, "خطأ غير متوقع", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void BtnPrint_Click(object? sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(txtInvoiceNo.Text))
        {
            MessageBox.Show("يرجى اختيار قائمة لغرض الطباعة", "تنبيه", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        if (btnSave.Visible)
        {
            if (MessageBox.Show("سيتم طباعة البيانات دون حفظ التعديلات، للاستمرار اضغط نعم", "تنبيه",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
            {
                MessageBox.Show("يرجى حفظ البيانات أولاً", "تنبيه");
                return;
            }
        }

        var reportForm = new ReportForm(Convert.ToInt32(txtInvoiceNo.Text));
        reportForm.Show();
        this.Hide();
    }

    private void BtnBack_Click(object? sender, EventArgs e)
    {
        var invoiceForm = new InvoiceForm();
        invoiceForm.Show();
        this.Hide();
    }

    private void StyleButton(Button btn, string text, Color backColor, Point location)
    {
        btn.Text = text;
        btn.Font = new Font("Segoe UI Semibold", 11F, FontStyle.Bold);
        btn.Size = new Size(135, 40);
        btn.Location = location;
        btn.BackColor = backColor;
        btn.ForeColor = backColor == Color.FromArgb(240, 173, 78) ? Color.Black : Color.White;
        btn.FlatStyle = FlatStyle.Flat;
        btn.Cursor = Cursors.Hand;
        btn.FlatAppearance.BorderSize = 0;
        btn.TextAlign = ContentAlignment.MiddleCenter;
    }

    private void StyleTextBox(TextBox txt, Size size, Point location, bool isReadOnly)
    {
        txt.Font = new Font("Segoe UI", currentAppFontSize, isReadOnly ? FontStyle.Bold : FontStyle.Regular);
        txt.Size = size;
        txt.Location = location;
        txt.BackColor = isReadOnly ? Color.FromArgb(24, 28, 34) : Color.FromArgb(48, 56, 65);
        txt.ForeColor = isReadOnly ? Color.LightGray : Color.White;
        txt.BorderStyle = BorderStyle.FixedSingle;
        txt.ReadOnly = isReadOnly;
    }

    private Label CreateLabel(string text, Point location)
    {
        return new Label
        {
            Text = text,
            Font = new Font("Segoe UI Semibold", 11F, FontStyle.Bold),
            ForeColor = Color.FromArgb(180, 190, 200),
            AutoSize = true,
            Location = location,
            BackColor = Color.Transparent
        };
    }

    protected override void OnFormClosed(FormClosedEventArgs e)
    {
        _searchTimer.Dispose();
        Application.Exit();
        base.OnFormClosed(e);
    }
}

// ── كلاس المساعد لعناصر الاقتراحات (Suggestion Items) ──
internal class SuggestionItem
{
    public string Id { get; }
    public string Name { get; }
    public SuggestionItem(string id, string name) { Id = id; Name = name; }
    public override string ToString() => $"  {Id}  -  {Name}";
}