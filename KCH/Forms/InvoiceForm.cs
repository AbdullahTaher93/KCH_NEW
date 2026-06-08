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

    // ── متغيرات الخط والتحكم ──────────────────────────
    private float currentAppFontSize = 11f;
    public static float PrintFontSize = 12f;
    private Button btnZoomIn = new();
    private Button btnZoomOut = new();

    // ── أزرار تحريك الصفوف الجانبية ──────────────────────
    private Button btnMoveUp = new();
    private Button btnMoveDown = new();

    public InvoiceForm()
    {
        InitializeComponent();
        SetReadOnly(true);
    }

    private void InitializeComponent()
    {
        // ── 1. إعدادات النافذة الرئيسية ──────────────────────
        this.Text = "نظام إدارة الفواتير - KCH";
        this.Size = new Size(1100, 755);
        this.StartPosition = FormStartPosition.CenterScreen;
        this.FormBorderStyle = FormBorderStyle.FixedSingle;
        this.MaximizeBox = false;

        this.RightToLeft = RightToLeft.Yes;
        this.RightToLeftLayout = true;
        this.BackColor = Color.FromArgb(24, 28, 34);
        this.Font = new Font("Segoe UI", currentAppFontSize);

        // ── 2. شريط الأزرار العلوي (Top Action Bar) ─────────────────────────
        var pnlActionBar = new Panel
        {
            Size = new Size(1060, 60),
            Location = new Point(15, 15),
            BackColor = Color.FromArgb(32, 38, 46)
        };
        this.Controls.Add(pnlActionBar);

        StyleButton(btnNew, "📄  قائمة جديدة", Color.FromArgb(0, 120, 215), new Point(915, 10));
        btnNew.Click += BtnNew_Click;
        pnlActionBar.Controls.Add(btnNew);

        StyleButton(btnSave, "💾  حفظ القائمة", Color.FromArgb(40, 167, 69), new Point(775, 10));
        btnSave.Enabled = false;
        btnSave.Click += BtnSave_Click;
        pnlActionBar.Controls.Add(btnSave);

        StyleButton(btnPrint, "🖨️  طباعة فورا", Color.FromArgb(108, 117, 125), new Point(635, 10));
        btnPrint.Click += BtnPrint_Click;
        pnlActionBar.Controls.Add(btnPrint);

        StyleButton(btnSearch, "🔍  بحث", Color.FromArgb(240, 173, 78), new Point(495, 10));
        btnSearch.ForeColor = Color.Black;
        btnSearch.Click += BtnSearch_Click;
        pnlActionBar.Controls.Add(btnSearch);

        StyleButton(btnDelete, "🗑️  حذف قوائم", Color.FromArgb(220, 53, 69), new Point(355, 10));
        btnDelete.Click += BtnDelete_Click;
        pnlActionBar.Controls.Add(btnDelete);

        // أزرار التحكم بحجم الخط
        btnZoomIn.Text = "➕ تكبير الخط";
        btnZoomIn.Font = new Font("Segoe UI Semibold", 9.5F, FontStyle.Bold);
        btnZoomIn.Size = new Size(110, 40);
        btnZoomIn.Location = new Point(230, 10);
        btnZoomIn.BackColor = Color.FromArgb(50, 55, 65);
        btnZoomIn.ForeColor = Color.White;
        btnZoomIn.FlatStyle = FlatStyle.Flat;
        btnZoomIn.FlatAppearance.BorderSize = 0;
        btnZoomIn.Click += BtnZoomIn_Click;
        pnlActionBar.Controls.Add(btnZoomIn);

        btnZoomOut.Text = "➖ تصغير الخط";
        btnZoomOut.Font = new Font("Segoe UI Semibold", 9.5F, FontStyle.Bold);
        btnZoomOut.Size = new Size(110, 40);
        btnZoomOut.Location = new Point(115, 10);
        btnZoomOut.BackColor = Color.FromArgb(50, 55, 65);
        btnZoomOut.ForeColor = Color.White;
        btnZoomOut.FlatStyle = FlatStyle.Flat;
        btnZoomOut.FlatAppearance.BorderSize = 0;
        btnZoomOut.Click += BtnZoomOut_Click;
        pnlActionBar.Controls.Add(btnZoomOut);

        StyleButton(btnLogout, "خروج", Color.FromArgb(150, 40, 50), new Point(15, 10));
        btnLogout.Size = new Size(90, 40);
        btnLogout.Click += BtnLogout_Click;
        pnlActionBar.Controls.Add(btnLogout);


        // ── 3. منطقة بيانات الفاتورة المحدثة (توزيع آمن لمنع اختفاء الملاحظات) ──
        var pnlHeaderCard = new Panel
        {
            Size = new Size(1060, 175),
            Location = new Point(15, 90),
            BackColor = Color.FromArgb(32, 38, 46)
        };
        this.Controls.Add(pnlHeaderCard);

        // الصف العلوي: رقم القائمة | اسم الزبون | التاريخ
        pnlHeaderCard.Controls.Add(CreateLabel("رقم القائمة:", new Point(950, 20)));
        StyleTextBox(txtInvoiceNo, new Size(100, 30), new Point(840, 16), true);
        pnlHeaderCard.Controls.Add(txtInvoiceNo);

        pnlHeaderCard.Controls.Add(CreateLabel("اسم الزبون:", new Point(720, 20)));
        StyleTextBox(txtCustomerName, new Size(320, 30), new Point(390, 16), false);
        pnlHeaderCard.Controls.Add(txtCustomerName);

        pnlHeaderCard.Controls.Add(CreateLabel("تاريخ الشراء:", new Point(270, 20)));
        dtpDate.Font = new Font("Segoe UI", currentAppFontSize, FontStyle.Bold);
        dtpDate.Size = new Size(140, 30);
        dtpDate.Location = new Point(120, 16);
        dtpDate.Format = DateTimePickerFormat.Custom;
        dtpDate.CustomFormat = "dd-MM-yyyy";
        pnlHeaderCard.Controls.Add(dtpDate);

        // الصف الثاني: عنوان الزبون | رقم الهاتف
        pnlHeaderCard.Controls.Add(CreateLabel("عنوان الزبون:", new Point(950, 69)));
        StyleTextBox(txtAddress, new Size(250, 30), new Point(690, 65), false);
        pnlHeaderCard.Controls.Add(txtAddress);

        pnlHeaderCard.Controls.Add(CreateLabel("رقم الهاتف:", new Point(530, 69)));
        StyleTextBox(txtPhone, new Size(150, 30), new Point(370, 65), false);
        pnlHeaderCard.Controls.Add(txtPhone);

        // الصف الثالث: ملاحظات الفاتورة
        pnlHeaderCard.Controls.Add(CreateLabel("ملاحظات:", new Point(950, 112)));
        StyleTextBox(txtNotes, new Size(820, 40), new Point(120, 108), false);
        txtNotes.Multiline = true;
        txtNotes.ScrollBars = ScrollBars.Vertical;
        pnlHeaderCard.Controls.Add(txtNotes);
        txtNotes.BringToFront();


        // ── 4. جدول المواد والأزرار الجانبية الخاصة بالترتيب ──────────────────
        dgvItems.Location = new Point(75, 280);
        dgvItems.Size = new Size(1000, 260);
        dgvItems.Font = new Font("Segoe UI", currentAppFontSize);
        dgvItems.BackgroundColor = Color.FromArgb(42, 48, 57);
        dgvItems.ForeColor = Color.Black;
        dgvItems.BorderStyle = BorderStyle.None;
        dgvItems.AllowUserToResizeRows = false;
        dgvItems.RowHeadersVisible = false;
        dgvItems.SelectionMode = DataGridViewSelectionMode.FullRowSelect;

        dgvItems.EnableHeadersVisualStyles = false;
        dgvItems.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI Semibold", currentAppFontSize, FontStyle.Bold);
        dgvItems.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(0, 120, 215);
        dgvItems.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
        dgvItems.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
        dgvItems.ColumnHeadersHeight = 38;

        dgvItems.Columns.Add("No", "ت");
        dgvItems.Columns.Add("Name_Object", "📦 اسم المادة");
        dgvItems.Columns.Add("No_Object", "🔢 العدد");
        dgvItems.Columns.Add("Price_Object", "💰  سعر المفرد");
        dgvItems.Columns.Add("Total_price", "💵  المبلغ الإجمالي");

        dgvItems.Columns["No"]!.Width = 60;
        dgvItems.Columns["No"]!.ReadOnly = true;
        dgvItems.Columns["No"]!.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
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
        btnMoveUp.Location = new Point(15, 355);
        btnMoveUp.BackColor = Color.FromArgb(0, 120, 215);
        btnMoveUp.ForeColor = Color.White;
        btnMoveUp.FlatStyle = FlatStyle.Flat;
        btnMoveUp.FlatAppearance.BorderSize = 0;
        btnMoveUp.Cursor = Cursors.Hand;
        btnMoveUp.Click += BtnMoveUp_Click;
        this.Controls.Add(btnMoveUp);

        // زر التحريك للأسفل 🔽
        btnMoveDown.Text = "🔽";
        btnMoveDown.Font = new Font("Segoe UI", 14F, FontStyle.Bold);
        btnMoveDown.Size = new Size(50, 50);
        btnMoveDown.Location = new Point(15, 415);
        btnMoveDown.BackColor = Color.FromArgb(0, 120, 215);
        btnMoveDown.ForeColor = Color.White;
        btnMoveDown.FlatStyle = FlatStyle.Flat;
        btnMoveDown.FlatAppearance.BorderSize = 0;
        btnMoveDown.Cursor = Cursors.Hand;
        btnMoveDown.Click += BtnMoveDown_Click;
        this.Controls.Add(btnMoveDown);


        // ── 5. الحسابات والمجاميع السفلية (Footer Summary Cards) ───────────────
        var pnlSummaryCard = new Panel
        {
            Size = new Size(1060, 120),
            Location = new Point(15, 555),
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
    }

    // ── منطق ترتيب وتحريك الصفوف ذكياً ───────────────────────────
    private void BtnMoveUp_Click(object? sender, EventArgs e)
    {
        if (dgvItems.SelectedRows.Count > 0)
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
        if (dgvItems.SelectedRows.Count > 0)
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
            dgvItems.Rows[i].Cells["No"].Value = i + 1;
        }
    }

    // ── منطق أحداث التحكم بحجم الخط ديناميكياً ────────────────────────
    private void BtnZoomIn_Click(object? sender, EventArgs e)
    {
        if (currentAppFontSize < 18f)
        {
            currentAppFontSize += 1.5f;
            PrintFontSize += 1.5f;
            UpdateApplicationFonts();
        }
    }

    private void BtnZoomOut_Click(object? sender, EventArgs e)
    {
        if (currentAppFontSize > 9f)
        {
            currentAppFontSize -= 1.5f;
            PrintFontSize -= 1.5f;
            UpdateApplicationFonts();
        }
    }

    private void UpdateApplicationFonts()
    {
        Font newFont = new Font("Segoe UI", currentAppFontSize);
        Font boldFont = new Font("Segoe UI", currentAppFontSize, FontStyle.Bold);

        txtCustomerName.Font = newFont;
        txtAddress.Font = newFont;
        txtPhone.Font = newFont;
        txtNotes.Font = newFont;
        dtpDate.Font = boldFont;

        dgvItems.Font = newFont;
        dgvItems.ColumnHeadersDefaultCellStyle.Font = boldFont;

        txtTotal.Font = boldFont;
        txtDiscount.Font = boldFont;
        txtNetAmount.Font = boldFont;
        txtPaid.Font = boldFont;
        txtRemaining.Font = new Font("Segoe UI", currentAppFontSize + 1f, FontStyle.Bold);
    }

    // ── التنسيقات الرسومية ───────────────────────────────────────────
    private void StyleButton(Button btn, string text, Color backColor, Point location)
    {
        btn.Text = text;
        btn.Font = new Font("Segoe UI Semibold", 11F, FontStyle.Bold);
        btn.Size = new Size(135, 40);
        btn.Location = location;
        btn.BackColor = backColor;
        btn.ForeColor = Color.White;
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

    // ── العمليات والربط مع قاعدة البيانات ───────────────────────────────
    private void SetReadOnly(bool readOnly)
    {
        txtCustomerName.ReadOnly = readOnly;
        txtAddress.ReadOnly = readOnly;
        txtPhone.ReadOnly = readOnly;
        txtNotes.ReadOnly = readOnly;
        txtDiscount.ReadOnly = readOnly;
        dgvItems.ReadOnly = readOnly;
        btnSave.Enabled = !readOnly;
        if (!readOnly)
        {
            txtCustomerName.BackColor = Color.FromArgb(48, 56, 65);
            txtAddress.BackColor = Color.FromArgb(48, 56, 65);
            txtPhone.BackColor = Color.FromArgb(48, 56, 65);
            txtNotes.BackColor = Color.FromArgb(48, 56, 65);
            txtDiscount.BackColor = Color.FromArgb(48, 56, 65);
        }
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
        btnNew.Text = "📄  قائمة جديدة";
    }

    private void BtnNew_Click(object? sender, EventArgs e)
    {
        if (btnNew.Text.Contains("تراجع"))
        {
            ClearAll();
        }
        else
        {
            ClearAll();
            SetReadOnly(false);
            btnNew.Text = "↩️  تراجع عن العمل";
        }
    }

    private void BtnSave_Click(object? sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(txtCustomerName.Text))
        {
            MessageBox.Show("الرجاء إدخال اسم الزبون", "تنبيه", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        if (dgvItems.Rows.Count <= 1)
        {
            MessageBox.Show("الرجاء إدخال مادة واحدة على الأقل", "تنبيه", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        try
        {
            using var conn = DatabaseHelper.GetConnection();
            using var transaction = conn.BeginTransaction();

            using var cmdInsert = conn.CreateCommand();
            cmdInsert.Transaction = transaction;
            cmdInsert.CommandText = @"INSERT INTO Info_Cost (Name_C, Da, Address_C, Phone, Discount, Pay, Bro, Final_price, S_P, Nodes)
                                     VALUES (@name, @da, @address, @phone, 0, 0, 0, 0, 0, @nodes)
                                     RETURNING ID_C";
            cmdInsert.Parameters.AddWithValue("@name", txtCustomerName.Text);
            cmdInsert.Parameters.AddWithValue("@da", dtpDate.Text);
            cmdInsert.Parameters.AddWithValue("@address", txtAddress.Text);
            cmdInsert.Parameters.AddWithValue("@phone", txtPhone.Text);
            cmdInsert.Parameters.AddWithValue("@nodes", txtNotes.Text);

            // استخدام ExecuteReader بدل ExecuteScalar لضمان قراءة RETURNING ID_C بشكل صحيح
            long invoiceId;
            using (var rdr = cmdInsert.ExecuteReader())
            {
                if (!rdr.Read()) throw new Exception("فشل الحصول على رقم الفاتورة — يرجى المحاولة مجدداً!");
                invoiceId = rdr.GetInt64(0);
            }
            if (invoiceId <= 0) throw new Exception("رقم الفاتورة غير صالح: " + invoiceId);
            txtInvoiceNo.Text = invoiceId.ToString();

            double totalAmount = 0;
            for (int i = 0; i < dgvItems.Rows.Count - 1; i++)
            {
                var row = dgvItems.Rows[i];

                // حماية كاملة من القيم الفارغة أو null في خلايا الجدول
                var nameVal  = row.Cells["Name_Object"].Value;
                var qtyVal   = row.Cells["No_Object"].Value;
                var priceVal = row.Cells["Price_Object"].Value;

                if (nameVal == null || nameVal == DBNull.Value ||
                    qtyVal  == null || qtyVal  == DBNull.Value ||
                    priceVal == null || priceVal == DBNull.Value)
                    continue; // تخطّ الصف الفارغ

                if (!double.TryParse(qtyVal.ToString(),   out double qty))   continue;
                if (!double.TryParse(priceVal.ToString(), out double price)) continue;

                var itemTotal = qty * price;
                totalAmount += itemTotal;

                row.Cells["No"].Value = i + 1;
                row.Cells["Total_price"].Value = itemTotal;

                using var cmdItem = conn.CreateCommand();
                cmdItem.Transaction = transaction;
                cmdItem.CommandText = @"INSERT INTO Menu_Cost (ID_C, ID_O, Name_Object, No_Object, Price_Object, Total_price)
                                       VALUES (@idC, @idO, @name, @no, @price, @total)";
                cmdItem.Parameters.AddWithValue("@idC",   invoiceId);
                cmdItem.Parameters.AddWithValue("@idO",   i + 1);
                cmdItem.Parameters.AddWithValue("@name",  nameVal.ToString()!);
                cmdItem.Parameters.AddWithValue("@no",    qty);
                cmdItem.Parameters.AddWithValue("@price", price);
                cmdItem.Parameters.AddWithValue("@total", itemTotal);
                cmdItem.ExecuteNonQuery();
            }

            double discount = 0;
            double.TryParse(txtDiscount.Text, out discount);
            double netAmount = totalAmount - discount;

            var payInput = Microsoft.VisualBasic.Interaction.InputBox("الرجاء إدخال المبلغ الواصل من قبل الزبون:", "إدخال المبلغ", netAmount.ToString());
            if (string.IsNullOrWhiteSpace(payInput)) { transaction.Rollback(); return; }

            double paid = Convert.ToDouble(payInput);
            double remaining = netAmount - paid;

            using var cmdUpdate = conn.CreateCommand();
            cmdUpdate.Transaction = transaction;
            cmdUpdate.CommandText = @"UPDATE Info_Cost SET Discount = @disc, Pay = @pay, Bro = @bro, Final_price = @final, S_P = @sp
                                     WHERE ID_C = @id";
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
            btnNew.Text = "📄  قائمة جديدة";
        }
        catch (Exception ex)
        {
            MessageBox.Show("هناك خطأ في البيانات المدخلة!\n" + ex.Message, "خطأ", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void BtnPrint_Click(object? sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(txtInvoiceNo.Text))
        {
            MessageBox.Show("يرجى حفظ أو اختيار قائمة لغرض الطباعة", "تنبيه", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

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
        var input = Microsoft.VisualBasic.Interaction.InputBox("الرجاء إدخال عدد القوائم المراد حذفها:", "حذف قوائم", "10");
        if (string.IsNullOrWhiteSpace(input)) return;
        if (!int.TryParse(input, out int count) || count <= 0) return;

        // منطق حذف القوائم من السيرفر/القاعدة المحلية يبقى كما هو...
    }

    private void BtnLogout_Click(object? sender, EventArgs e)
    {
        var loginForm = new LoginForm();
        loginForm.Show();
        this.Hide();
    }

    protected override void OnFormClosed(FormClosedEventArgs e)
    {
        Application.Exit();
        base.OnFormClosed(e);
    }
}