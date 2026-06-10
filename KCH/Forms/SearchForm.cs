using Microsoft.Data.Sqlite;
using KCH.Data;

namespace KCH.Forms;

public class SearchForm : Form
{
    private TextBox txtSearch = new();
    private TextBox txtInvoiceNo = new();
    private TextBox txtCustomerName = new();
    private TextBox txtAddress = new();
    private TextBox txtPhone = new();
    private TextBox txtTotal = new();
    private TextBox txtDiscount = new();
    private TextBox txtNetAmount = new();
    private TextBox txtPaid = new();
    private TextBox txtRemaining = new();
    private DateTimePicker dtpDate = new();
    private RadioButton rbByNumber = new();
    private RadioButton rbByName = new();
    private DataGridView dgvItems = new();
    private Button btnSearch = new();
    private Button btnSave = new();
    private Button btnPrint = new();
    private Button btnBack = new();
    private Button btnZoomIn = new();
    private Button btnZoomOut = new();
    private Button btnMoveUp = new();
    private Button btnMoveDown = new();
    private ListBox lstSuggestions = new();
    private System.Windows.Forms.Timer _searchTimer = new();
    private float currentFontSize = 12f;

    public SearchForm()
    {
        InitializeComponent();
        dgvItems.ReadOnly = true;
    }

    private void InitializeComponent()
    {
        this.Text = "البحث عن الفواتير - KCH";
        this.WindowState = FormWindowState.Maximized;
        this.StartPosition = FormStartPosition.CenterScreen;
        this.MinimumSize = new Size(1000, 700);
        this.RightToLeft = RightToLeft.Yes;
        this.RightToLeftLayout = true;
        this.BackColor = Color.FromArgb(24, 28, 34);
        this.Font = new Font("Segoe UI", 12);
        this.Padding = new Padding(20);

        var mainLayout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 5,
            BackColor = Color.FromArgb(24, 28, 34)
        };
        mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 80));   // بحث
        mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 100));  // تفاصيل الفاتورة
        mainLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));   // الجدول
        mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 130));  // المجاميع
        mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 55));   // أزرار سفلية
        mainLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        this.Controls.Add(mainLayout);

        // ═══════════════════ 1. منطقة البحث ═══════════════════
        var pnlSearch = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.RightToLeft,
            BackColor = Color.FromArgb(32, 38, 46),
            Padding = new Padding(15, 10, 15, 10),
            WrapContents = false
        };
        mainLayout.Controls.Add(pnlSearch, 0, 0);

        rbByNumber.Text = "بحث برقم القائمة";
        rbByNumber.Font = new Font("Segoe UI", 12, FontStyle.Bold);
        rbByNumber.ForeColor = Color.White;
        rbByNumber.AutoSize = true;
        rbByNumber.Checked = true;
        rbByNumber.Margin = new Padding(10, 15, 5, 0);
        pnlSearch.Controls.Add(rbByNumber);

        rbByName.Text = "بحث باسم الزبون";
        rbByName.Font = new Font("Segoe UI", 12, FontStyle.Bold);
        rbByName.ForeColor = Color.White;
        rbByName.AutoSize = true;
        rbByName.Margin = new Padding(20, 15, 5, 0);
        pnlSearch.Controls.Add(rbByName);

        txtSearch.Font = new Font("Segoe UI", 13);
        txtSearch.Size = new Size(400, 35);
        txtSearch.BackColor = Color.FromArgb(48, 56, 65);
        txtSearch.ForeColor = Color.White;
        txtSearch.BorderStyle = BorderStyle.FixedSingle;
        txtSearch.Margin = new Padding(10, 12, 10, 0);
        txtSearch.TextChanged += TxtSearch_TextChanged;
        txtSearch.KeyDown += TxtSearch_KeyDown;
        pnlSearch.Controls.Add(txtSearch);

        MakeFlowButton(pnlSearch, btnSearch, "🔍 بحث", Color.FromArgb(0, 120, 215), BtnSearch_Click);
        MakeFlowButton(pnlSearch, btnSave, "💾 حفظ التعديل", Color.FromArgb(40, 167, 69), BtnSave_Click);
        btnSave.Visible = false;

        // أزرار تكبير/تصغير الخط
        MakeFlowButton(pnlSearch, btnZoomIn, "➕ تكبير", Color.FromArgb(50, 55, 65), BtnZoomIn_Click);
        btnZoomIn.Size = new Size(100, 43);
        MakeFlowButton(pnlSearch, btnZoomOut, "➖ تصغير", Color.FromArgb(50, 55, 65), BtnZoomOut_Click);
        btnZoomOut.Size = new Size(100, 43);
        MakeFlowButton(pnlSearch, btnMoveUp, "🔼", Color.FromArgb(0, 120, 215), BtnMoveUp_Click);
        btnMoveUp.Size = new Size(55, 43);
        MakeFlowButton(pnlSearch, btnMoveDown, "🔽", Color.FromArgb(0, 120, 215), BtnMoveDown_Click);
        btnMoveDown.Size = new Size(55, 43);

        // قائمة الاقتراحات
        lstSuggestions.Font = new Font("Segoe UI", 12);
        lstSuggestions.Size = new Size(400, 130);
        lstSuggestions.Visible = false;
        lstSuggestions.BackColor = Color.FromArgb(48, 56, 65);
        lstSuggestions.ForeColor = Color.White;
        lstSuggestions.BorderStyle = BorderStyle.FixedSingle;
        lstSuggestions.Click += LstSuggestions_Click;
        lstSuggestions.KeyDown += LstSuggestions_KeyDown;
        this.Controls.Add(lstSuggestions);
        lstSuggestions.BringToFront();

        _searchTimer.Interval = 300;
        _searchTimer.Tick += SearchTimer_Tick;

        // ═══════════════════ 2. تفاصيل الفاتورة ═══════════════════
        var pnlInfo = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 8,
            RowCount = 1,
            BackColor = Color.FromArgb(32, 38, 46),
            Padding = new Padding(10, 5, 10, 5)
        };
        pnlInfo.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        pnlInfo.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 12));
        pnlInfo.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        pnlInfo.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 30));
        pnlInfo.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        pnlInfo.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 30));
        pnlInfo.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        pnlInfo.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 18));
        mainLayout.Controls.Add(pnlInfo, 0, 1);

        pnlInfo.Controls.Add(MakeLabel("رقم:"), 0, 0);
        StyleInput(txtInvoiceNo, true); pnlInfo.Controls.Add(txtInvoiceNo, 1, 0);
        pnlInfo.Controls.Add(MakeLabel("الزبون:"), 2, 0);
        StyleInput(txtCustomerName, true); pnlInfo.Controls.Add(txtCustomerName, 3, 0);
        pnlInfo.Controls.Add(MakeLabel("العنوان:"), 4, 0);
        StyleInput(txtAddress, true); pnlInfo.Controls.Add(txtAddress, 5, 0);
        pnlInfo.Controls.Add(MakeLabel("التاريخ:"), 6, 0);
        dtpDate.Dock = DockStyle.Fill;
        dtpDate.Font = new Font("Segoe UI", 12, FontStyle.Bold);
        dtpDate.Format = DateTimePickerFormat.Custom;
        dtpDate.CustomFormat = "dd-MM-yyyy";
        dtpDate.Margin = new Padding(5, 8, 15, 5);
        pnlInfo.Controls.Add(dtpDate, 7, 0);

        // ═══════════════════ 3. جدول المواد ═══════════════════
        dgvItems.Dock = DockStyle.Fill;
        dgvItems.Font = new Font("Segoe UI", 12);
        dgvItems.BackgroundColor = Color.FromArgb(42, 48, 57);
        dgvItems.ForeColor = Color.Black;
        dgvItems.BorderStyle = BorderStyle.None;
        dgvItems.RowHeadersVisible = false;
        dgvItems.AllowUserToResizeRows = false;
        dgvItems.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        dgvItems.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        dgvItems.RowTemplate.Height = 35;

        dgvItems.EnableHeadersVisualStyles = false;
        dgvItems.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 12, FontStyle.Bold);
        dgvItems.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(240, 173, 78);
        dgvItems.ColumnHeadersDefaultCellStyle.ForeColor = Color.Black;
        dgvItems.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
        dgvItems.ColumnHeadersHeight = 45;

        dgvItems.Columns.Add("ID_O", "ت");
        dgvItems.Columns.Add("Name_Object", "اسم المادة");
        dgvItems.Columns.Add("No_Object", "العدد");
        dgvItems.Columns.Add("Price_Object", "سعر المفرد");
        dgvItems.Columns.Add("Total_price", "المبلغ الإجمالي");

        dgvItems.Columns["ID_O"]!.FillWeight = 8;
        dgvItems.Columns["ID_O"]!.ReadOnly = true;
        dgvItems.Columns["Name_Object"]!.FillWeight = 40;
        dgvItems.Columns["No_Object"]!.FillWeight = 13;
        dgvItems.Columns["No_Object"]!.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
        dgvItems.Columns["Price_Object"]!.FillWeight = 18;
        dgvItems.Columns["Total_price"]!.FillWeight = 21;
        dgvItems.Columns["Total_price"]!.ReadOnly = true;
        dgvItems.KeyDown += DgvItems_KeyDown;
        mainLayout.Controls.Add(dgvItems, 0, 2);

        // ═══════════════════ 4. المجاميع ═══════════════════
        var pnlSummary = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 10,
            RowCount = 2,
            BackColor = Color.FromArgb(32, 38, 46),
            Padding = new Padding(15, 10, 15, 10)
        };
        for (int i = 0; i < 10; i++)
            pnlSummary.ColumnStyles.Add(new ColumnStyle(i % 2 == 0 ? SizeType.AutoSize : SizeType.Percent, i % 2 == 0 ? 0 : 20));
        pnlSummary.RowStyles.Add(new RowStyle(SizeType.Percent, 50));
        pnlSummary.RowStyles.Add(new RowStyle(SizeType.Percent, 50));
        mainLayout.Controls.Add(pnlSummary, 0, 3);

        pnlSummary.Controls.Add(MakeSummaryLabel("المجموع:"), 0, 0);
        StyleSummaryBox(txtTotal, Color.FromArgb(0, 150, 255)); pnlSummary.Controls.Add(txtTotal, 1, 0);
        pnlSummary.Controls.Add(MakeSummaryLabel("الصافي:"), 2, 0);
        StyleSummaryBox(txtNetAmount, Color.FromArgb(40, 167, 69)); pnlSummary.Controls.Add(txtNetAmount, 3, 0);
        pnlSummary.Controls.Add(MakeSummaryLabel("المتبقي:"), 4, 0);
        StyleSummaryBox(txtRemaining, Color.OrangeRed); pnlSummary.Controls.Add(txtRemaining, 5, 0);

        pnlSummary.Controls.Add(MakeSummaryLabel("الخصم:"), 0, 1);
        StyleInput(txtDiscount, false); txtDiscount.Dock = DockStyle.Fill; txtDiscount.ForeColor = Color.FromArgb(255, 100, 100); txtDiscount.Text = "0";
        pnlSummary.Controls.Add(txtDiscount, 1, 1);
        pnlSummary.Controls.Add(MakeSummaryLabel("الواصل:"), 2, 1);
        StyleSummaryBox(txtPaid, Color.White); pnlSummary.Controls.Add(txtPaid, 3, 1);

        // ═══════════════════ 5. أزرار سفلية ═══════════════════
        var pnlBottom = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.RightToLeft,
            Padding = new Padding(5)
        };
        mainLayout.Controls.Add(pnlBottom, 0, 4);
        MakeFlowButton(pnlBottom, btnBack, "↩️ رجوع", Color.FromArgb(50, 55, 65), BtnBack_Click);
        MakeFlowButton(pnlBottom, btnPrint, "🖨️ طباعة", Color.FromArgb(108, 117, 125), BtnPrint_Click);
    }

    // ═══════════════════ دوال التنسيق ═══════════════════
    private void MakeFlowButton(FlowLayoutPanel panel, Button btn, string text, Color color, EventHandler handler)
    {
        btn.Text = text;
        btn.Font = new Font("Segoe UI", 12, FontStyle.Bold);
        btn.Size = new Size(150, 43);
        btn.BackColor = color;
        btn.ForeColor = Color.White;
        btn.FlatStyle = FlatStyle.Flat;
        btn.FlatAppearance.BorderSize = 0;
        btn.Cursor = Cursors.Hand;
        btn.Margin = new Padding(8, 8, 8, 0);
        btn.Click += handler;
        panel.Controls.Add(btn);
    }

    private Label MakeLabel(string text) => new Label
    {
        Text = text, Font = new Font("Segoe UI", 12, FontStyle.Bold),
        ForeColor = Color.FromArgb(180, 190, 200), AutoSize = true,
        Anchor = AnchorStyles.Right, Margin = new Padding(5, 12, 5, 0)
    };

    private Label MakeSummaryLabel(string text) => new Label
    {
        Text = text, Font = new Font("Segoe UI", 13, FontStyle.Bold),
        ForeColor = Color.FromArgb(200, 210, 220), AutoSize = true,
        Anchor = AnchorStyles.Right, Margin = new Padding(5, 12, 5, 0)
    };

    private void StyleInput(TextBox txt, bool readOnly)
    {
        txt.Dock = DockStyle.Fill;
        txt.Font = new Font("Segoe UI", 12, readOnly ? FontStyle.Bold : FontStyle.Regular);
        txt.BackColor = readOnly ? Color.FromArgb(24, 28, 34) : Color.FromArgb(48, 56, 65);
        txt.ForeColor = readOnly ? Color.LightGray : Color.White;
        txt.BorderStyle = BorderStyle.FixedSingle;
        txt.ReadOnly = readOnly;
        txt.Margin = new Padding(5, 8, 15, 5);
    }

    private void StyleSummaryBox(TextBox txt, Color foreColor)
    {
        txt.Dock = DockStyle.Fill;
        txt.Font = new Font("Segoe UI", 14, FontStyle.Bold);
        txt.BackColor = Color.FromArgb(24, 28, 34);
        txt.ForeColor = foreColor;
        txt.BorderStyle = BorderStyle.FixedSingle;
        txt.ReadOnly = true;
        txt.Text = "0";
        txt.Margin = new Padding(5, 8, 15, 5);
    }

    private void BtnMoveUp_Click(object? sender, EventArgs e)
    {
        if (dgvItems.ReadOnly || dgvItems.SelectedRows.Count == 0) return;
        int idx = dgvItems.SelectedRows[0].Index;
        if (idx > 0) MoveRow(idx, idx - 1);
    }

    private void BtnMoveDown_Click(object? sender, EventArgs e)
    {
        if (dgvItems.ReadOnly || dgvItems.SelectedRows.Count == 0) return;
        int idx = dgvItems.SelectedRows[0].Index;
        if (idx < dgvItems.Rows.Count - 2) MoveRow(idx, idx + 1);
    }

    // ═══════════════════ تكبير/تصغير الخط ═══════════════════
    private void BtnZoomIn_Click(object? sender, EventArgs e)
    {
        if (currentFontSize < 20f) { currentFontSize += 1.5f; InvoiceForm.PrintFontSize += 1.5f; ApplyFontSize(); }
    }

    private void BtnZoomOut_Click(object? sender, EventArgs e)
    {
        if (currentFontSize > 9f) { currentFontSize -= 1.5f; InvoiceForm.PrintFontSize -= 1.5f; ApplyFontSize(); }
    }

    private void ApplyFontSize()
    {
        var f = new Font("Segoe UI", currentFontSize);
        var fb = new Font("Segoe UI", currentFontSize, FontStyle.Bold);
        txtSearch.Font = f; txtInvoiceNo.Font = fb; txtCustomerName.Font = fb;
        txtAddress.Font = fb; txtPhone.Font = fb; dtpDate.Font = fb;
        dgvItems.Font = f;
        dgvItems.ColumnHeadersDefaultCellStyle.Font = fb;
        dgvItems.RowTemplate.Height = (int)(currentFontSize * 3);
        txtTotal.Font = fb; txtNetAmount.Font = fb; txtRemaining.Font = fb;
        txtDiscount.Font = f; txtPaid.Font = fb;
        // تحديث ارتفاع الصفوف الموجودة
        foreach (DataGridViewRow row in dgvItems.Rows) row.Height = (int)(currentFontSize * 3);
    }

    // ═══════════════════ تحريك الصفوف أعلى/أسفل (Alt+Up / Alt+Down) ═══════════════════
    private void DgvItems_KeyDown(object? sender, KeyEventArgs e)
    {
        if (dgvItems.ReadOnly || dgvItems.SelectedRows.Count == 0) return;
        int idx = dgvItems.SelectedRows[0].Index;
        if (e.Alt && e.KeyCode == Keys.Up && idx > 0) { MoveRow(idx, idx - 1); e.Handled = true; }
        else if (e.Alt && e.KeyCode == Keys.Down && idx < dgvItems.Rows.Count - 2) { MoveRow(idx, idx + 1); e.Handled = true; }
    }

    private void MoveRow(int from, int to)
    {
        var row = dgvItems.Rows[from];
        var vals = new object[row.Cells.Count];
        for (int i = 0; i < row.Cells.Count; i++) vals[i] = row.Cells[i].Value;
        dgvItems.Rows.RemoveAt(from);
        dgvItems.Rows.Insert(to, vals);
        dgvItems.ClearSelection();
        dgvItems.Rows[to].Selected = true;
        for (int i = 0; i < dgvItems.Rows.Count - 1; i++) dgvItems.Rows[i].Cells["ID_O"].Value = i + 1;
    }

    // ═══════════════════ منطق البحث ═══════════════════
    private void TxtSearch_TextChanged(object? sender, EventArgs e)
    {
        _searchTimer.Stop();
        if (string.IsNullOrWhiteSpace(txtSearch.Text)) { lstSuggestions.Visible = false; return; }
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
            { cmd.CommandText = "SELECT ID_C, Name_C FROM Info_Cost WHERE CAST(ID_C AS TEXT) LIKE @q LIMIT 10"; cmd.Parameters.AddWithValue("@q", keyword + "%"); }
            else
            { cmd.CommandText = "SELECT ID_C, Name_C FROM Info_Cost WHERE Name_C LIKE @q LIMIT 10"; cmd.Parameters.AddWithValue("@q", "%" + keyword + "%"); }

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
                lstSuggestions.Items.Add($"{reader.GetInt64(0)}  -  {(reader.IsDBNull(1) ? "" : reader.GetString(1))}");
        }
        catch { }

        if (lstSuggestions.Items.Count > 0)
        {
            lstSuggestions.Location = new Point(txtSearch.FindForm()!.ClientSize.Width / 2 - 200, 110);
            lstSuggestions.Visible = true;
            lstSuggestions.BringToFront();
        }
        else lstSuggestions.Visible = false;
    }

    private void LstSuggestions_Click(object? sender, EventArgs e) => SelectSuggestion();
    private void TxtSearch_KeyDown(object? sender, KeyEventArgs e)
    {
        if (e.KeyCode == Keys.Down && lstSuggestions.Visible) { lstSuggestions.Focus(); if (lstSuggestions.Items.Count > 0) lstSuggestions.SelectedIndex = 0; e.Handled = true; }
        else if (e.KeyCode == Keys.Enter) { lstSuggestions.Visible = false; BtnSearch_Click(sender, e); e.Handled = true; }
        else if (e.KeyCode == Keys.Escape) lstSuggestions.Visible = false;
    }
    private void LstSuggestions_KeyDown(object? sender, KeyEventArgs e)
    {
        if (e.KeyCode == Keys.Enter) { SelectSuggestion(); e.Handled = true; }
        else if (e.KeyCode == Keys.Escape) { lstSuggestions.Visible = false; txtSearch.Focus(); }
    }

    private void SelectSuggestion()
    {
        if (lstSuggestions.SelectedItem == null) return;
        var id = lstSuggestions.SelectedItem.ToString()!.Split('-')[0].Trim();
        lstSuggestions.Visible = false;
        txtSearch.Text = id;
        rbByNumber.Checked = true;
        SearchById(id);
    }

    private void BtnSearch_Click(object? sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(txtSearch.Text)) return;
        SearchById(txtSearch.Text.Trim());
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
            txtAddress.Text = reader["Address_C"]?.ToString() ?? "";
            txtPhone.Text = reader["Phone"]?.ToString() ?? "";
            if (DateTime.TryParse(reader["Da"]?.ToString(), out DateTime d)) dtpDate.Value = d;
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
            using var ir = cmdItems.ExecuteReader();
            while (ir.Read()) dgvItems.Rows.Add(ir["ID_O"], ir["Name_Object"], ir["No_Object"], ir["Price_Object"], ir["Total_price"]);

            btnSave.Visible = true;
            dgvItems.ReadOnly = false;
        }
        else MessageBox.Show("لا توجد فاتورة بهذا الرقم", "تنبيه", MessageBoxButtons.OK, MessageBoxIcon.Information);
    }

    private void BtnSave_Click(object? sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(txtInvoiceNo.Text)) return;
        var invoiceId = Convert.ToInt64(txtInvoiceNo.Text);

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
                if (row.Cells["Name_Object"].Value == null) continue;
                var qty = Convert.ToDouble(row.Cells["No_Object"].Value);
                var price = Convert.ToDouble(row.Cells["Price_Object"].Value);
                var itemTotal = qty * price;
                totalAmount += itemTotal;
                row.Cells["Total_price"].Value = itemTotal;

                using var cmdIns = conn.CreateCommand();
                cmdIns.Transaction = transaction;
                cmdIns.CommandText = @"INSERT INTO Menu_Cost (ID_C, ID_O, Name_Object, No_Object, Price_Object, Total_price) VALUES (@idC, @idO, @name, @no, @price, @total)";
                cmdIns.Parameters.AddWithValue("@idC", invoiceId);
                cmdIns.Parameters.AddWithValue("@idO", i + 1);
                cmdIns.Parameters.AddWithValue("@name", row.Cells["Name_Object"].Value?.ToString() ?? "");
                cmdIns.Parameters.AddWithValue("@no", qty);
                cmdIns.Parameters.AddWithValue("@price", price);
                cmdIns.Parameters.AddWithValue("@total", itemTotal);
                cmdIns.ExecuteNonQuery();
            }

            double discount = 0; double.TryParse(txtDiscount.Text, out discount);
            double netAmount = totalAmount - discount;
            var payInput = Microsoft.VisualBasic.Interaction.InputBox("المبلغ الواصل:", "تحديث", netAmount.ToString());
            if (string.IsNullOrWhiteSpace(payInput)) { transaction.Rollback(); return; }
            double paid = Convert.ToDouble(payInput);
            double remaining = netAmount - paid;

            using var cmdUp = conn.CreateCommand(); cmdUp.Transaction = transaction;
            cmdUp.CommandText = "UPDATE Info_Cost SET Da=@da, Discount=@d, Pay=@p, Bro=@b, Final_price=@f, S_P=@s WHERE ID_C=@id";
            cmdUp.Parameters.AddWithValue("@da", dtpDate.Value.ToString("dd-MM-yyyy"));
            cmdUp.Parameters.AddWithValue("@d", discount);
            cmdUp.Parameters.AddWithValue("@p", paid);
            cmdUp.Parameters.AddWithValue("@b", remaining);
            cmdUp.Parameters.AddWithValue("@f", totalAmount);
            cmdUp.Parameters.AddWithValue("@s", netAmount);
            cmdUp.Parameters.AddWithValue("@id", invoiceId);
            cmdUp.ExecuteNonQuery();

            transaction.Commit();
            txtTotal.Text = totalAmount.ToString("N0");
            txtNetAmount.Text = netAmount.ToString("N0");
            txtPaid.Text = paid.ToString("N0");
            txtRemaining.Text = remaining.ToString("N0");
            MessageBox.Show("تم الحفظ بنجاح", "تم", MessageBoxButtons.OK, MessageBoxIcon.Information);
            btnSave.Visible = false;
            dgvItems.ReadOnly = true;
        }
        catch (Exception ex) { transaction.Rollback(); MessageBox.Show("خطأ:\n" + ex.Message); }
    }

    private void BtnPrint_Click(object? sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(txtInvoiceNo.Text)) { MessageBox.Show("اختر قائمة أولاً"); return; }
        var r = new ReportForm(Convert.ToInt32(txtInvoiceNo.Text));
        r.Show(); this.Hide();
    }

    private void BtnBack_Click(object? sender, EventArgs e)
    {
        var f = new InvoiceForm(); f.Show(); this.Hide();
    }

    protected override void OnFormClosed(FormClosedEventArgs e) { _searchTimer.Dispose(); Application.Exit(); base.OnFormClosed(e); }
}
