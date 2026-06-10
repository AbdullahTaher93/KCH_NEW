using Microsoft.Data.Sqlite;
using KCH.Data;

namespace KCH.Forms;

public class InvoiceForm : Form
{
    private TextBox txtInvoiceNo = new();
    private TextBox txtCustomerName = new();
    private TextBox txtAddress = new();
    private TextBox txtPhone = new();
    private TextBox txtNotes = new();
    private TextBox txtDiscount = new();
    private TextBox txtTotal = new();
    private TextBox txtNetAmount = new();
    private TextBox txtPaid = new();
    private TextBox txtRemaining = new();
    private DateTimePicker dtpDate = new();
    private DataGridView dgvItems = new();

    private Button btnNew = new();
    private Button btnSave = new();
    private Button btnPrint = new();
    private Button btnSearch = new();
    private Button btnDelete = new();
    private Button btnLogout = new();

    public static float PrintFontSize = 12f;
    private float currentFontSize = 12f;
    private Button btnZoomIn = new();
    private Button btnZoomOut = new();
    private Button btnMoveUp = new();
    private Button btnMoveDown = new();

    public InvoiceForm()
    {
        InitializeComponent();
        SetReadOnly(true);
    }

    private void InitializeComponent()
    {
        this.Text = "نظام إدارة الفواتير - KCH";
        this.WindowState = FormWindowState.Maximized;
        this.StartPosition = FormStartPosition.CenterScreen;
        this.MinimumSize = new Size(1000, 700);
        this.RightToLeft = RightToLeft.Yes;
        this.RightToLeftLayout = true;
        this.BackColor = Color.FromArgb(24, 28, 34);
        this.Font = new Font("Segoe UI", 12);
        this.Padding = new Padding(20);

        // ── التخطيط الرئيسي: 4 صفوف (أزرار | بيانات | جدول | مجاميع) ──
        var mainLayout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 4,
            BackColor = Color.FromArgb(24, 28, 34)
        };
        mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 70));   // أزرار
        mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 180));  // بيانات الفاتورة
        mainLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));   // الجدول (يأخذ باقي المساحة)
        mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 130));  // المجاميع
        mainLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        this.Controls.Add(mainLayout);

        // ═══════════════════ 1. شريط الأزرار ═══════════════════
        var pnlButtons = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.RightToLeft,
            BackColor = Color.FromArgb(32, 38, 46),
            Padding = new Padding(10, 12, 10, 10),
            WrapContents = false
        };
        mainLayout.Controls.Add(pnlButtons, 0, 0);

        AddButton(pnlButtons, btnNew, "📄 قائمة جديدة", Color.FromArgb(0, 120, 215), BtnNew_Click);
        AddButton(pnlButtons, btnSave, "💾 حفظ", Color.FromArgb(40, 167, 69), BtnSave_Click);
        AddButton(pnlButtons, btnPrint, "🖨️ طباعة", Color.FromArgb(108, 117, 125), BtnPrint_Click);
        AddButton(pnlButtons, btnSearch, "🔍 بحث", Color.FromArgb(240, 173, 78), BtnSearch_Click);
        btnSearch.ForeColor = Color.Black;
        AddButton(pnlButtons, btnDelete, "🗑️ حذف قوائم", Color.FromArgb(220, 53, 69), BtnDelete_Click);
        AddButton(pnlButtons, btnLogout, "خروج", Color.FromArgb(150, 40, 50), BtnLogout_Click);

        // أزرار تكبير/تصغير الخط
        AddButton(pnlButtons, btnZoomIn, "➕ تكبير", Color.FromArgb(50, 55, 65), BtnZoomIn_Click);
        AddButton(pnlButtons, btnZoomOut, "➖ تصغير", Color.FromArgb(50, 55, 65), BtnZoomOut_Click);
        AddButton(pnlButtons, btnMoveUp, "🔼", Color.FromArgb(0, 100, 180), BtnMoveUp_Click);
        btnMoveUp.Size = new Size(55, 45);
        AddButton(pnlButtons, btnMoveDown, "🔽", Color.FromArgb(0, 100, 180), BtnMoveDown_Click);
        btnMoveDown.Size = new Size(55, 45);

        // ═══════════════════ 2. بيانات الفاتورة ═══════════════════
        var pnlHeader = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 6,
            RowCount = 3,
            BackColor = Color.FromArgb(32, 38, 46),
            Padding = new Padding(15, 10, 15, 10)
        };
        // 6 أعمدة: label | input | label | input | label | input
        pnlHeader.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        pnlHeader.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 20));
        pnlHeader.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        pnlHeader.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 40));
        pnlHeader.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        pnlHeader.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 40));
        pnlHeader.RowStyles.Add(new RowStyle(SizeType.Percent, 33));
        pnlHeader.RowStyles.Add(new RowStyle(SizeType.Percent, 33));
        pnlHeader.RowStyles.Add(new RowStyle(SizeType.Percent, 34));
        mainLayout.Controls.Add(pnlHeader, 0, 1);

        // الصف 1: رقم القائمة | اسم الزبون | التاريخ
        pnlHeader.Controls.Add(MakeLabel("رقم القائمة:"), 0, 0);
        StyleInput(txtInvoiceNo, true);
        pnlHeader.Controls.Add(txtInvoiceNo, 1, 0);

        pnlHeader.Controls.Add(MakeLabel("اسم الزبون:"), 2, 0);
        StyleInput(txtCustomerName, false);
        pnlHeader.Controls.Add(txtCustomerName, 3, 0);

        pnlHeader.Controls.Add(MakeLabel("التاريخ:"), 4, 0);
        dtpDate.Dock = DockStyle.Fill;
        dtpDate.Font = new Font("Segoe UI", 12, FontStyle.Bold);
        dtpDate.Format = DateTimePickerFormat.Custom;
        dtpDate.CustomFormat = "dd-MM-yyyy";
        pnlHeader.Controls.Add(dtpDate, 5, 0);

        // الصف 2: العنوان | الهاتف
        pnlHeader.Controls.Add(MakeLabel("العنوان:"), 0, 1);
        StyleInput(txtAddress, false);
        pnlHeader.Controls.Add(txtAddress, 1, 1);
        pnlHeader.SetColumnSpan(txtAddress, 3);

        pnlHeader.Controls.Add(MakeLabel("الهاتف:"), 4, 1);
        StyleInput(txtPhone, false);
        pnlHeader.Controls.Add(txtPhone, 5, 1);

        // الصف 3: ملاحظات
        pnlHeader.Controls.Add(MakeLabel("ملاحظات:"), 0, 2);
        StyleInput(txtNotes, false);
        txtNotes.Multiline = true;
        pnlHeader.Controls.Add(txtNotes, 1, 2);
        pnlHeader.SetColumnSpan(txtNotes, 5);

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
        dgvItems.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(0, 120, 215);
        dgvItems.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
        dgvItems.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
        dgvItems.ColumnHeadersHeight = 45;

        dgvItems.Columns.Add("No", "ت");
        dgvItems.Columns.Add("Name_Object", "اسم المادة");
        dgvItems.Columns.Add("No_Object", "العدد");
        dgvItems.Columns.Add("Price_Object", "سعر المفرد");
        dgvItems.Columns.Add("Total_price", "المبلغ الإجمالي");

        dgvItems.Columns["No"]!.FillWeight = 8;
        dgvItems.Columns["No"]!.ReadOnly = true;
        dgvItems.Columns["No"]!.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
        dgvItems.Columns["Name_Object"]!.FillWeight = 40;
        dgvItems.Columns["No_Object"]!.FillWeight = 13;
        dgvItems.Columns["No_Object"]!.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
        dgvItems.Columns["Price_Object"]!.FillWeight = 18;
        dgvItems.Columns["Total_price"]!.FillWeight = 21;
        dgvItems.Columns["Total_price"]!.ReadOnly = true;

        mainLayout.Controls.Add(dgvItems, 0, 2);

        // أزرار تحريك الصفوف داخل الجدول (أعلى/أسفل)
        dgvItems.KeyDown += DgvItems_KeyDown;

        // ═══════════════════ 4. المجاميع السفلية ═══════════════════
        var pnlSummary = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 10,
            RowCount = 2,
            BackColor = Color.FromArgb(32, 38, 46),
            Padding = new Padding(15, 10, 15, 10)
        };
        // 10 أعمدة: label+input × 5
        for (int i = 0; i < 10; i++)
            pnlSummary.ColumnStyles.Add(new ColumnStyle(i % 2 == 0 ? SizeType.AutoSize : SizeType.Percent, i % 2 == 0 ? 0 : 20));
        pnlSummary.RowStyles.Add(new RowStyle(SizeType.Percent, 50));
        pnlSummary.RowStyles.Add(new RowStyle(SizeType.Percent, 50));
        mainLayout.Controls.Add(pnlSummary, 0, 3);

        // الصف 1: المجموع | الصافي | المتبقي
        pnlSummary.Controls.Add(MakeSummaryLabel("المجموع الكلي:"), 0, 0);
        StyleSummaryBox(txtTotal, Color.FromArgb(0, 150, 255));
        pnlSummary.Controls.Add(txtTotal, 1, 0);

        pnlSummary.Controls.Add(MakeSummaryLabel("المبلغ الصافي:"), 2, 0);
        StyleSummaryBox(txtNetAmount, Color.FromArgb(40, 167, 69));
        pnlSummary.Controls.Add(txtNetAmount, 3, 0);

        pnlSummary.Controls.Add(MakeSummaryLabel("المتبقي بذمته:"), 4, 0);
        StyleSummaryBox(txtRemaining, Color.OrangeRed);
        pnlSummary.Controls.Add(txtRemaining, 5, 0);

        // الصف 2: الخصم | الواصل
        pnlSummary.Controls.Add(MakeSummaryLabel("الخصم:"), 0, 1);
        StyleInput(txtDiscount, false);
        txtDiscount.Dock = DockStyle.Fill;
        txtDiscount.ForeColor = Color.FromArgb(255, 100, 100);
        txtDiscount.Text = "0";
        pnlSummary.Controls.Add(txtDiscount, 1, 1);

        pnlSummary.Controls.Add(MakeSummaryLabel("المبلغ الواصل:"), 2, 1);
        StyleSummaryBox(txtPaid, Color.White);
        pnlSummary.Controls.Add(txtPaid, 3, 1);
    }

    // ═══════════════════ الدوال المساعدة للتنسيق ═══════════════════
    private void AddButton(FlowLayoutPanel panel, Button btn, string text, Color color, EventHandler handler)
    {
        btn.Text = text;
        btn.Font = new Font("Segoe UI", 12, FontStyle.Bold);
        btn.Size = new Size(155, 45);
        btn.BackColor = color;
        btn.ForeColor = Color.White;
        btn.FlatStyle = FlatStyle.Flat;
        btn.FlatAppearance.BorderSize = 0;
        btn.Cursor = Cursors.Hand;
        btn.Margin = new Padding(5);
        btn.Click += handler;
        panel.Controls.Add(btn);
    }

    private Label MakeLabel(string text)
    {
        return new Label
        {
            Text = text,
            Font = new Font("Segoe UI", 12, FontStyle.Bold),
            ForeColor = Color.FromArgb(180, 190, 200),
            AutoSize = true,
            Anchor = AnchorStyles.Right,
            Margin = new Padding(5, 12, 5, 0)
        };
    }

    private Label MakeSummaryLabel(string text)
    {
        return new Label
        {
            Text = text,
            Font = new Font("Segoe UI", 13, FontStyle.Bold),
            ForeColor = Color.FromArgb(200, 210, 220),
            AutoSize = true,
            Anchor = AnchorStyles.Right,
            Margin = new Padding(5, 12, 5, 0)
        };
    }

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
        if (currentFontSize < 20f) { currentFontSize += 1.5f; PrintFontSize += 1.5f; ApplyFontSize(); }
    }

    private void BtnZoomOut_Click(object? sender, EventArgs e)
    {
        if (currentFontSize > 9f) { currentFontSize -= 1.5f; PrintFontSize -= 1.5f; ApplyFontSize(); }
    }

    private void ApplyFontSize()
    {
        var f = new Font("Segoe UI", currentFontSize);
        var fb = new Font("Segoe UI", currentFontSize, FontStyle.Bold);
        txtCustomerName.Font = f; txtAddress.Font = f; txtPhone.Font = f; txtNotes.Font = f;
        dtpDate.Font = fb; txtInvoiceNo.Font = fb;
        dgvItems.Font = f;
        dgvItems.ColumnHeadersDefaultCellStyle.Font = fb;
        dgvItems.RowTemplate.Height = (int)(currentFontSize * 3);
        txtTotal.Font = fb; txtNetAmount.Font = fb; txtRemaining.Font = fb;
        txtDiscount.Font = f; txtPaid.Font = fb;
        // تحديث ارتفاع الصفوف الموجودة
        foreach (DataGridViewRow row in dgvItems.Rows) row.Height = (int)(currentFontSize * 3);
    }

    // ═══════════════════ تحريك الصفوف أعلى/أسفل ═══════════════════
    private void DgvItems_KeyDown(object? sender, KeyEventArgs e)
    {
        if (dgvItems.ReadOnly || dgvItems.SelectedRows.Count == 0) return;
        int idx = dgvItems.SelectedRows[0].Index;

        if (e.Alt && e.KeyCode == Keys.Up && idx > 0)
        { MoveRow(idx, idx - 1); e.Handled = true; }
        else if (e.Alt && e.KeyCode == Keys.Down && idx < dgvItems.Rows.Count - 2)
        { MoveRow(idx, idx + 1); e.Handled = true; }
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
        // إعادة ترقيم
        for (int i = 0; i < dgvItems.Rows.Count - 1; i++) dgvItems.Rows[i].Cells["No"].Value = i + 1;
    }

    // ═══════════════════ منطق العمليات ═══════════════════
    private void SetReadOnly(bool readOnly)
    {
        txtCustomerName.ReadOnly = readOnly;
        txtAddress.ReadOnly = readOnly;
        txtPhone.ReadOnly = readOnly;
        txtNotes.ReadOnly = readOnly;
        txtDiscount.ReadOnly = readOnly;
        dgvItems.ReadOnly = readOnly;
        btnSave.Enabled = !readOnly;
        var editColor = Color.FromArgb(48, 56, 65);
        var lockColor = Color.FromArgb(24, 28, 34);
        txtCustomerName.BackColor = readOnly ? lockColor : editColor;
        txtAddress.BackColor = readOnly ? lockColor : editColor;
        txtPhone.BackColor = readOnly ? lockColor : editColor;
        txtNotes.BackColor = readOnly ? lockColor : editColor;
        txtDiscount.BackColor = readOnly ? lockColor : editColor;
    }

    private void ClearAll()
    {
        dgvItems.Rows.Clear();
        txtInvoiceNo.Clear();
        txtCustomerName.Clear();
        txtAddress.Clear();
        txtPhone.Clear();
        txtNotes.Clear();
        txtDiscount.Text = "0";
        txtTotal.Text = "0";
        txtNetAmount.Text = "0";
        txtPaid.Text = "0";
        txtRemaining.Text = "0";
        SetReadOnly(true);
        btnNew.Text = "📄 قائمة جديدة";
    }

    private void BtnNew_Click(object? sender, EventArgs e)
    {
        if (btnNew.Text.Contains("تراجع")) { ClearAll(); }
        else { ClearAll(); SetReadOnly(false); btnNew.Text = "↩️ تراجع"; }
    }

    private void BtnSave_Click(object? sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(txtCustomerName.Text))
        { MessageBox.Show("الرجاء إدخال اسم الزبون", "تنبيه", MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }
        if (dgvItems.Rows.Count <= 1)
        { MessageBox.Show("الرجاء إدخال مادة واحدة على الأقل", "تنبيه", MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }

        try
        {
            using var conn = DatabaseHelper.GetConnection();
            using var transaction = conn.BeginTransaction();

            using var cmdInsert = conn.CreateCommand();
            cmdInsert.Transaction = transaction;
            cmdInsert.CommandText = @"INSERT INTO Info_Cost (Name_C, Da, Address_C, Phone, Discount, Pay, Bro, Final_price, S_P, Nodes)
                                     VALUES (@name, @da, @address, @phone, 0, 0, 0, 0, 0, @nodes) RETURNING ID_C";
            cmdInsert.Parameters.AddWithValue("@name", txtCustomerName.Text);
            cmdInsert.Parameters.AddWithValue("@da", dtpDate.Text);
            cmdInsert.Parameters.AddWithValue("@address", txtAddress.Text);
            cmdInsert.Parameters.AddWithValue("@phone", txtPhone.Text);
            cmdInsert.Parameters.AddWithValue("@nodes", txtNotes.Text);

            long invoiceId;
            using (var rdr = cmdInsert.ExecuteReader())
            {
                if (!rdr.Read()) throw new Exception("فشل الحصول على رقم الفاتورة");
                invoiceId = rdr.GetInt64(0);
            }
            txtInvoiceNo.Text = invoiceId.ToString();

            double totalAmount = 0;
            for (int i = 0; i < dgvItems.Rows.Count - 1; i++)
            {
                var row = dgvItems.Rows[i];
                var nameVal = row.Cells["Name_Object"].Value;
                var qtyVal = row.Cells["No_Object"].Value;
                var priceVal = row.Cells["Price_Object"].Value;

                if (nameVal == null || qtyVal == null || priceVal == null) continue;
                if (!double.TryParse(qtyVal.ToString(), out double qty)) continue;
                if (!double.TryParse(priceVal.ToString(), out double price)) continue;

                var itemTotal = qty * price;
                totalAmount += itemTotal;
                row.Cells["No"].Value = i + 1;
                row.Cells["Total_price"].Value = itemTotal;

                using var cmdItem = conn.CreateCommand();
                cmdItem.Transaction = transaction;
                cmdItem.CommandText = @"INSERT INTO Menu_Cost (ID_C, ID_O, Name_Object, No_Object, Price_Object, Total_price)
                                       VALUES (@idC, @idO, @name, @no, @price, @total)";
                cmdItem.Parameters.AddWithValue("@idC", invoiceId);
                cmdItem.Parameters.AddWithValue("@idO", i + 1);
                cmdItem.Parameters.AddWithValue("@name", nameVal.ToString()!);
                cmdItem.Parameters.AddWithValue("@no", qty);
                cmdItem.Parameters.AddWithValue("@price", price);
                cmdItem.Parameters.AddWithValue("@total", itemTotal);
                cmdItem.ExecuteNonQuery();
            }

            double discount = 0;
            double.TryParse(txtDiscount.Text, out discount);
            double netAmount = totalAmount - discount;

            var payInput = Microsoft.VisualBasic.Interaction.InputBox("الرجاء إدخال المبلغ الواصل:", "إدخال المبلغ", netAmount.ToString());
            if (string.IsNullOrWhiteSpace(payInput)) { transaction.Rollback(); return; }

            double paid = Convert.ToDouble(payInput);
            double remaining = netAmount - paid;

            using var cmdUpdate = conn.CreateCommand();
            cmdUpdate.Transaction = transaction;
            cmdUpdate.CommandText = @"UPDATE Info_Cost SET Discount=@disc, Pay=@pay, Bro=@bro, Final_price=@final, S_P=@sp WHERE ID_C=@id";
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
            MessageBox.Show("تم حفظ البيانات بنجاح", "نجاح", MessageBoxButtons.OK, MessageBoxIcon.Information);
            SetReadOnly(true);
            btnNew.Text = "📄 قائمة جديدة";
        }
        catch (Exception ex)
        { MessageBox.Show("خطأ:\n" + ex.Message, "خطأ", MessageBoxButtons.OK, MessageBoxIcon.Error); }
    }

    private void BtnPrint_Click(object? sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(txtInvoiceNo.Text))
        { MessageBox.Show("يرجى حفظ أو اختيار قائمة أولاً", "تنبيه"); return; }
        var reportForm = new ReportForm(Convert.ToInt32(txtInvoiceNo.Text));
        reportForm.Show();
        this.Hide();
    }

    private void BtnSearch_Click(object? sender, EventArgs e)
    {
        var searchForm = new SearchForm();
        searchForm.Show();
        this.Hide();
    }

    private void BtnDelete_Click(object? sender, EventArgs e)
    {
        var input = Microsoft.VisualBasic.Interaction.InputBox("عدد القوائم المراد حذفها:", "حذف", "10");
        if (string.IsNullOrWhiteSpace(input)) return;
        if (!int.TryParse(input, out int count) || count <= 0) return;

        if (MessageBox.Show($"حذف {count} قوائم من الأقدم؟", "تأكيد", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes) return;

        using var conn = DatabaseHelper.GetConnection();
        using var transaction = conn.BeginTransaction();
        using var cmdSel = conn.CreateCommand();
        cmdSel.Transaction = transaction;
        cmdSel.CommandText = "SELECT ID_C FROM Info_Cost ORDER BY ID_C ASC LIMIT @c";
        cmdSel.Parameters.AddWithValue("@c", count);
        var ids = new List<long>();
        using (var r = cmdSel.ExecuteReader()) { while (r.Read()) ids.Add(r.GetInt64(0)); }

        foreach (var id in ids)
        {
            using var c1 = conn.CreateCommand(); c1.Transaction = transaction;
            c1.CommandText = "DELETE FROM Menu_Cost WHERE ID_C=@id"; c1.Parameters.AddWithValue("@id", id); c1.ExecuteNonQuery();
            using var c2 = conn.CreateCommand(); c2.Transaction = transaction;
            c2.CommandText = "DELETE FROM Info_Cost WHERE ID_C=@id"; c2.Parameters.AddWithValue("@id", id); c2.ExecuteNonQuery();
        }
        transaction.Commit();
        MessageBox.Show($"تم حذف {ids.Count} قوائم", "تم", MessageBoxButtons.OK, MessageBoxIcon.Information);
    }

    private void BtnLogout_Click(object? sender, EventArgs e)
    {
        var loginForm = new LoginForm();
        loginForm.Show();
        this.Hide();
    }

    protected override void OnFormClosed(FormClosedEventArgs e) { Application.Exit(); base.OnFormClosed(e); }
}
