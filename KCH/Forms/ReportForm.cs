using Microsoft.Data.Sqlite;
using KCH.Data;
using System.Drawing.Printing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;

namespace KCH.Forms;

public class ReportForm : Form
{
    private readonly int _invoiceId;
    private float currentInvoiceFontSize = 11f;

    // عناصر عرض معلومات الفاتورة العامة
    private Label lblInvoiceNo = new();
    private Label lblCustomerVal = new();
    private Label lblAddressVal = new();
    private Label lblDateVal = new();
    private Label lblNotesVal = new();

    // عناصر عرض المجاميع والحسابات السفلية التفاعلية
    private Label lblTotalVal = new();
    private Label lblDiscountVal = new();
    private Label lblNetVal = new();
    private Label lblPaidVal = new();
    private Label lblRemainingVal = new();

    private DataGridView dgvItems = new();
    private Button btnPrint = new();
    private Button btnBack = new();
    private Button btnZoomIn = new();
    private Button btnZoomOut = new();
    private Panel pnlInvoice = new();
    private Panel pnlHeader = new();
    private TableLayoutPanel infoTable = new();
    private TableLayoutPanel totalsTable = new();
    private PictureBox pbLogo = new();

    // متغيرات تتبع حالة الصفحات أثناء الطباعة
    private int _currentRowIndex = 0;
    private int _pageNumber = 0;

    private readonly Color ColorPrimaryNavy = Color.FromArgb(24, 43, 73);
    private readonly Color ColorGridBg = Color.FromArgb(240, 242, 245);
    private readonly Color ColorRedAlert = Color.FromArgb(166, 41, 41);
    private readonly Color ColorTextDark = Color.FromArgb(30, 30, 30);

    public ReportForm(int invoiceId)
    {
        _invoiceId = invoiceId;
        InitializeComponent();
        LoadInvoice();
    }

    private Image? GetLogoImage()
    {
        try
        {
            string baseDir = AppDomain.CurrentDomain.BaseDirectory;
            string projectDir = Path.GetFullPath(Path.Combine(baseDir, @"..\..\.."));
            string logoPath = Path.Combine(projectDir, "Resources", "ktc_logo.png");

            if (File.Exists(logoPath)) return Image.FromFile(logoPath);

            string localLogoPath = Path.Combine(baseDir, "Resources", "ktc_logo.png");
            if (File.Exists(localLogoPath)) return Image.FromFile(localLogoPath);
        }
        catch { }
        return null;
    }

    private void InitializeComponent()
    {
        this.Text = "معاينة الفاتورة الرسمية والطباعة";
        this.Size = new Size(1150, 980);
        this.StartPosition = FormStartPosition.CenterScreen;
        this.RightToLeft = RightToLeft.Yes;
        this.RightToLeftLayout = true;
        this.BackColor = Color.FromArgb(235, 238, 243);
        this.Font = new Font("Segoe UI", currentInvoiceFontSize);

        // ── 1. شريط الأدوات العلوي (Toolbar) ──────────────────────────
        var toolbar = new TableLayoutPanel
        {
            Dock = DockStyle.Top,
            Height = 65,
            BackColor = Color.FromArgb(20, 32, 50),
            ColumnCount = 3,
            RowCount = 1,
            Padding = new Padding(15, 5, 15, 5)
        };
        toolbar.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 35F));
        toolbar.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 30F));
        toolbar.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 35F));

        var lblTitle = new Label
        {
            Text = "📄 نظام معاينة وطباعة الفواتير الملكية متعددة الصفحات",
            Font = new Font("Segoe UI", 12, FontStyle.Bold),
            ForeColor = Color.White,
            AutoSize = true,
            Anchor = AnchorStyles.Right
        };
        toolbar.Controls.Add(lblTitle, 0, 0);

        var pnlZoomButtons = new FlowLayoutPanel { FlowDirection = FlowDirection.RightToLeft, AutoSize = true, Anchor = AnchorStyles.None, BackColor = Color.Transparent };
        btnZoomIn = new Button { Text = "➕ تكبير العرض", Font = new Font("Segoe UI", 9.5F, FontStyle.Bold), ForeColor = Color.White, BackColor = Color.FromArgb(50, 68, 95), FlatStyle = FlatStyle.Flat, Size = new Size(115, 36), Cursor = Cursors.Hand };
        btnZoomIn.FlatAppearance.BorderSize = 0; btnZoomIn.Click += BtnZoomIn_Click;
        btnZoomOut = new Button { Text = "➖ تصغير العرض", Font = new Font("Segoe UI", 9.5F, FontStyle.Bold), ForeColor = Color.White, BackColor = Color.FromArgb(50, 68, 95), FlatStyle = FlatStyle.Flat, Size = new Size(115, 36), Cursor = Cursors.Hand };
        btnZoomOut.FlatAppearance.BorderSize = 0; btnZoomOut.Click += BtnZoomOut_Click;
        pnlZoomButtons.Controls.Add(btnZoomIn); pnlZoomButtons.Controls.Add(btnZoomOut);
        toolbar.Controls.Add(pnlZoomButtons, 1, 0);

        var pnlActionButtons = new FlowLayoutPanel { FlowDirection = FlowDirection.LeftToRight, AutoSize = true, Anchor = AnchorStyles.Left, BackColor = Color.Transparent };
        btnBack = new Button { Text = "◄ رجوع للرئيسية", Font = new Font("Segoe UI", 9.5F, FontStyle.Bold), ForeColor = Color.White, BackColor = Color.FromArgb(95, 100, 110), FlatStyle = FlatStyle.Flat, Size = new Size(120, 36), Cursor = Cursors.Hand };
        btnBack.FlatAppearance.BorderSize = 0; btnBack.Click += BtnBack_Click;
        btnPrint = new Button { Text = "🖨 طباعة الآن", Font = new Font("Segoe UI", 9.5F, FontStyle.Bold), ForeColor = Color.White, BackColor = Color.FromArgb(24, 144, 255), FlatStyle = FlatStyle.Flat, Size = new Size(120, 36), Cursor = Cursors.Hand };
        btnPrint.FlatAppearance.BorderSize = 0; btnPrint.Click += BtnPrint_Click;
        pnlActionButtons.Controls.Add(btnBack); pnlActionButtons.Controls.Add(btnPrint);
        toolbar.Controls.Add(pnlActionButtons, 2, 0);

        this.Controls.Add(toolbar);

        // ── 2. الحاوية المحيطة بالورقة البيضاء ──────────────────
        var pnlContainer = new Panel { Dock = DockStyle.Fill, Padding = new Padding(30, 20, 30, 20), AutoScroll = true };
        this.Controls.Add(pnlContainer);

        // ── 3. ورقة الفاتورة البيضاء لمشاهدة الصفحة الأولى ───────────────────────
        pnlInvoice = new Panel { BackColor = Color.White, Width = 900, Height = 1250, Anchor = AnchorStyles.Top, Padding = new Padding(45) };
        pnlInvoice.Location = new Point((pnlContainer.Width - pnlInvoice.Width) / 2, 20);
        pnlContainer.Controls.Add(pnlInvoice);

        pnlContainer.SizeChanged += (s, e) => {
            pnlInvoice.Location = new Point(Math.Max(20, (pnlContainer.Width - pnlInvoice.Width) / 2), 20);
        };

        var mainLayout = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 5, BackColor = Color.White };
        mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 120F));
        mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 90F));
        mainLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
        mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 165F));
        mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 40F));
        pnlInvoice.Controls.Add(mainLayout);

        // أ) هيدر الشركة والمؤسسة
        pnlHeader = new Panel { Dock = DockStyle.Fill, BackColor = Color.White };
        pbLogo.Size = new Size(110, 65); pbLogo.Location = new Point(30, 15); pbLogo.SizeMode = PictureBoxSizeMode.Zoom;
        var imgLogo = GetLogoImage();
        if (imgLogo != null) pbLogo.Image = imgLogo;
        pnlHeader.Controls.Add(pbLogo);

        var lblCompany = new Label { Text = "شركة المطابخ العالمية لـ KCH", Font = new Font("Arial", 17, FontStyle.Bold), ForeColor = ColorPrimaryNavy, AutoSize = true, Location = new Point(275, 5) };
        var lblSubTitle = new Label { Text = "لتجهيز المطاعم والفنادق والدوائر الحكومية", Font = new Font("Segoe UI", 9.5F, FontStyle.Bold), ForeColor = ColorTextDark, AutoSize = true, Location = new Point(285, 33) };
        var lblEnTitle = new Label { Text = "AL-MATBAKH AL-ELMEAA CO. For Supplies the Hotel & Rest", Font = new Font("Arial", 9.5F, FontStyle.Bold), ForeColor = ColorPrimaryNavy, AutoSize = true, Location = new Point(230, 56) };

        pnlHeader.Controls.Add(lblCompany); pnlHeader.Controls.Add(lblSubTitle); pnlHeader.Controls.Add(lblEnTitle);
        mainLayout.Controls.Add(pnlHeader, 0, 0);

        // ب) جدول تفاصيل معلومات الفاتورة
        infoTable = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 4, RowCount = 2, Padding = new Padding(0, 5, 0, 5), BackColor = Color.White };
        infoTable.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 15F)); infoTable.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 35F));
        infoTable.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 15F)); infoTable.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 35F));

        lblInvoiceNo = new Label { Font = new Font("Arial", 13, FontStyle.Bold), ForeColor = ColorTextDark, AutoSize = true, Anchor = AnchorStyles.Right };
        lblCustomerVal = new Label { Font = new Font("Segoe UI", 11, FontStyle.Bold), ForeColor = ColorTextDark, AutoSize = true, Anchor = AnchorStyles.Right };
        lblDateVal = new Label { Font = new Font("Arial", 12, FontStyle.Bold), ForeColor = ColorTextDark, AutoSize = true, Anchor = AnchorStyles.Right };
        lblNotesVal = new Label { Font = new Font("Segoe UI", 11, FontStyle.Regular), ForeColor = ColorTextDark, AutoSize = true, Anchor = AnchorStyles.Right };

        infoTable.Controls.Add(MakeBoldLabel("رقم القائمة:"), 0, 0); infoTable.Controls.Add(lblInvoiceNo, 1, 0);
        infoTable.Controls.Add(MakeBoldLabel("التاريخ:"), 2, 0); infoTable.Controls.Add(lblDateVal, 3, 0);
        infoTable.Controls.Add(MakeBoldLabel("اسم الزبون:"), 0, 1); infoTable.Controls.Add(lblCustomerVal, 1, 1);
        infoTable.Controls.Add(MakeBoldLabel("الملاحظات:"), 2, 1); infoTable.Controls.Add(lblNotesVal, 3, 1);
        mainLayout.Controls.Add(infoTable, 0, 1);

        // ج) داتا جريد عرض المواد
        dgvItems = new DataGridView { Dock = DockStyle.Fill, ReadOnly = true, AllowUserToAddRows = false, AllowUserToDeleteRows = false, AllowUserToResizeRows = false, BackgroundColor = Color.White, BorderStyle = BorderStyle.None, GridColor = Color.FromArgb(200, 205, 215), RowHeadersVisible = false, AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill, SelectionMode = DataGridViewSelectionMode.FullRowSelect, RowTemplate = { Height = 35 } };
        dgvItems.ColumnHeadersDefaultCellStyle = new DataGridViewCellStyle { BackColor = ColorPrimaryNavy, ForeColor = Color.White, Font = new Font("Segoe UI", currentInvoiceFontSize + 1f, FontStyle.Bold), Alignment = DataGridViewContentAlignment.MiddleCenter };
        dgvItems.DefaultCellStyle = new DataGridViewCellStyle { Font = new Font("Segoe UI", currentInvoiceFontSize), ForeColor = ColorTextDark, Alignment = DataGridViewContentAlignment.MiddleCenter, SelectionBackColor = Color.FromArgb(220, 230, 245), SelectionForeColor = Color.Black };
        dgvItems.AlternatingRowsDefaultCellStyle = new DataGridViewCellStyle { BackColor = ColorGridBg };
        dgvItems.EnableHeadersVisualStyles = false;

        dgvItems.Columns.Add(new DataGridViewTextBoxColumn { Name = "No", HeaderText = "No.", FillWeight = 10 });
        dgvItems.Columns.Add(new DataGridViewTextBoxColumn { Name = "Name", HeaderText = "أسم المادة / الصنف", FillWeight = 45 });
        dgvItems.Columns.Add(new DataGridViewTextBoxColumn { Name = "UnitPrice", HeaderText = "سعر المفرد", FillWeight = 15 });
        dgvItems.Columns.Add(new DataGridViewTextBoxColumn { Name = "Qty", HeaderText = "العدد", FillWeight = 12 });
        dgvItems.Columns.Add(new DataGridViewTextBoxColumn { Name = "Total", HeaderText = "المبلغ الكلي", FillWeight = 18 });
        mainLayout.Controls.Add(dgvItems, 0, 2);

        // د) تصميم بطاقة حسابات ومجاميع الفاتورة الملمومة
        var pnlTotalsOuter = new Panel { Dock = DockStyle.Fill, BackColor = Color.White };

        // 1. جعلنا الطول يتكيف تلقائياً وضبطنا الطول الافتراضي ليكون مناسباً
        totalsTable = new TableLayoutPanel
        {
            Width = 380,
            Height = 160, // تم تقليل الارتفاع الافتراضي ليتناسب مع 5 أسطر
            Location = new Point(5, 5),
            ColumnCount = 2,
            RowCount = 5,
            Padding = new Padding(1)
        };

        // ضبط نسب الأعمدة كما هي لديك
        totalsTable.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 45F));
        totalsTable.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 55F));

        // 2. إضافة الـ RowStyles لإجبار الجدول على توزيع الأسطر الـ 5 بالتساوي (20% لكل سطر)
        for (int i = 0; i < 5; i++)
        {
            totalsTable.RowStyles.Add(new RowStyle(SizeType.Percent, 20F));
        }

        // إضافة الأسطر
        AddModernSummaryRow(totalsTable, 0, "المجموع الكلي", ref lblTotalVal, ColorTextDark, Color.FromArgb(240, 242, 245));
        AddModernSummaryRow(totalsTable, 1, "خصم الفاتورة", ref lblDiscountVal, Color.FromArgb(166, 41, 41), Color.FromArgb(255, 230, 230));
        AddModernSummaryRow(totalsTable, 2, "المبلغ الصافي $", ref lblNetVal, Color.White, ColorPrimaryNavy);
        AddModernSummaryRow(totalsTable, 3, "المبلغ الواصل", ref lblPaidVal, ColorTextDark, Color.FromArgb(220, 225, 235));
        AddModernSummaryRow(totalsTable, 4, "المتبقي بذمته", ref lblRemainingVal, Color.White, ColorRedAlert);

        pnlTotalsOuter.Controls.Add(totalsTable);
        mainLayout.Controls.Add(pnlTotalsOuter, 0, 3);

        // هـ) فوتر أسفل الورقة
        var pnlFooter = new Panel { Dock = DockStyle.Fill, BackColor = Color.White };
        var pnlFooterLine = new Panel { BackColor = Color.LightGray, Height = 1, Width = 810, Location = Point.Empty };
        var lblContactInfo = new Label { Text = "Email: info@kch-company.com  |  Web: www.kch-system.com", Font = new Font("Consolas", 9, FontStyle.Regular), ForeColor = Color.Gray, AutoSize = true, Location = new Point(0, 12) };
        var lblPrintDate = new Label { Text = "تاريخ الطباعة: " + DateTime.Now.ToString("yyyy-MM-dd h:mm tt"), Font = new Font("Segoe UI", 9, FontStyle.Regular), ForeColor = Color.Gray, AutoSize = true, Location = new Point(550, 12) };
        pnlFooter.Controls.Add(pnlFooterLine); pnlFooter.Controls.Add(lblContactInfo); pnlFooter.Controls.Add(lblPrintDate);
        mainLayout.Controls.Add(pnlFooter, 0, 4);
    }

    private void BtnZoomIn_Click(object? sender, EventArgs e) { if (currentInvoiceFontSize < 18f) { currentInvoiceFontSize += 1f; UpdateInvoiceFonts(); } }
    private void BtnZoomOut_Click(object? sender, EventArgs e) { if (currentInvoiceFontSize > 8f) { currentInvoiceFontSize -= 1f; UpdateInvoiceFonts(); } }

    private void UpdateInvoiceFonts()
    {
        foreach (Control ctrl in infoTable.Controls)
        {
            if (ctrl is Label lbl)
            {
                bool isBold = lbl.Font.Bold;
                lbl.Font = new Font("Segoe UI", currentInvoiceFontSize, isBold ? FontStyle.Bold : FontStyle.Regular);
            }
        }
        dgvItems.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", currentInvoiceFontSize + 1f, FontStyle.Bold);
        dgvItems.DefaultCellStyle.Font = new Font("Segoe UI", currentInvoiceFontSize);
        dgvItems.AlternatingRowsDefaultCellStyle.Font = new Font("Segoe UI", currentInvoiceFontSize);
        dgvItems.RowTemplate.Height = (int)(currentInvoiceFontSize * 2) + 12;
        if (dgvItems.Rows.Count > 0) LoadInvoice();
    }

    private Label MakeBoldLabel(string text) => new Label { Text = text, Font = new Font("Arial", 11, FontStyle.Bold), ForeColor = ColorPrimaryNavy, AutoSize = true, Anchor = AnchorStyles.Right };

    private void AddModernSummaryRow(TableLayoutPanel table, int row, string labelText, ref Label valueLabel, Color textColor, Color blockBgColor)
    {
        var pnlLabel = new Panel { Dock = DockStyle.Fill, BackColor = blockBgColor, Margin = new Padding(1) };
        var lbl = new Label { Text = labelText, Font = new Font("Arial", 9.5F, FontStyle.Bold), ForeColor = textColor, AutoSize = false, Location = new Point(12, 5) };
        pnlLabel.Controls.Add(lbl);

        var pnlValue = new Panel { Dock = DockStyle.Fill, BackColor = blockBgColor, Margin = new Padding(1) };
        valueLabel = new Label { Text = "0", Font = new Font("Consolas", 10.5F, FontStyle.Bold), ForeColor = textColor, AutoSize = false, Location = new Point(15, 4) };
        pnlValue.Controls.Add(valueLabel);

        table.Controls.Add(pnlLabel, 1, row); table.Controls.Add(pnlValue, 0, row);
    }

    private void LoadInvoice()
    {
        try
        {
            using var conn = DatabaseHelper.GetConnection();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT Name_C, Da, Address_C, Discount, Pay, Bro, Final_price, S_P, Nodes FROM Info_Cost WHERE ID_C = @id";
            cmd.Parameters.AddWithValue("@id", _invoiceId);

            using var reader = cmd.ExecuteReader();
            if (!reader.Read()) return;

            lblInvoiceNo.Text = $"{_invoiceId}";
            lblCustomerVal.Text = reader.IsDBNull(0) ? "زبون عام" : reader.GetString(0);
            lblDateVal.Text = reader.IsDBNull(1) ? "" : reader.GetString(1);
            lblNotesVal.Text = reader.IsDBNull(8) ? "لا توجد ملاحظات" : reader.GetString(8);

            double discount = reader.IsDBNull(3) ? 0 : reader.GetDouble(3);
            double paid = reader.IsDBNull(4) ? 0 : reader.GetDouble(4);
            double remaining = reader.IsDBNull(5) ? 0 : reader.GetDouble(5);
            double total = reader.IsDBNull(6) ? 0 : reader.GetDouble(6);
            double net = reader.IsDBNull(7) ? 0 : reader.GetDouble(7);

            lblTotalVal.Text = $"{total:N0}";
            lblDiscountVal.Text = $"{discount:N0}";
            lblNetVal.Text = $"{net:N0}";
            lblPaidVal.Text = $"{paid:N0}";
            lblRemainingVal.Text = $"{remaining:N0}";
            reader.Close();

            using var cmd2 = conn.CreateCommand();
            cmd2.CommandText = "SELECT ID_O, Name_Object, No_Object, Price_Object, Total_price FROM Menu_Cost WHERE ID_C = @id ORDER BY ID_O";
            cmd2.Parameters.AddWithValue("@id", _invoiceId);

            using var r2 = cmd2.ExecuteReader();
            dgvItems.Rows.Clear();
            while (r2.Read())
            {
                dgvItems.Rows.Add(
                    r2.IsDBNull(0) ? "" : r2.GetInt64(0).ToString(),
                    r2.IsDBNull(1) ? "" : r2.GetString(1),
                    r2.IsDBNull(3) ? "0" : r2.GetDouble(3).ToString("N0"),
                    r2.IsDBNull(2) ? "0" : r2.GetDouble(2).ToString("N0"),
                    r2.IsDBNull(4) ? "0" : r2.GetDouble(4).ToString("N0")
                );
            }
        }
        catch (Exception ex) { MessageBox.Show($"خطأ أثناء تعبئة البيانات: {ex.Message}"); }
    }

    // ── دالة الطباعة الفاخرة متعددة الصفحات مع تعديلات التوجيه العربي القياسي ──
    private void BtnPrint_Click(object? sender, EventArgs e)
    {
        var pd = new PrintDocument();
        _currentRowIndex = 0;
        _pageNumber = 0;

        pd.PrintPage += (s, ev) =>
        {
            _pageNumber++;
            Graphics g = ev.Graphics!;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            int printWidth = ev.MarginBounds.Width;
            int marginX = ev.MarginBounds.Left;
            int currentY = ev.MarginBounds.Top;

            Image? logoImg = GetLogoImage();

            // 🌟 رسم العلامة المائية الفاخرة بالخلفية
            if (logoImg != null)
            {
                try
                {
                    float[][] opacityMatrix = {
                        new float[] {1, 0, 0, 0, 0}, new float[] {0, 1, 0, 0, 0}, new float[] {0, 0, 1, 0, 0},
                        new float[] {0, 0, 0, 0.025f, 0}, new float[] {0, 0, 0, 0, 1}
                    };
                    var imageAttributes = new ImageAttributes();
                    imageAttributes.SetColorMatrix(new ColorMatrix(opacityMatrix), ColorMatrixFlag.Default, ColorAdjustType.Bitmap);
                    g.DrawImage(logoImg, new Rectangle(marginX + (printWidth - 320) / 2, currentY + 300, 320, 200), 0, 0, logoImg.Width, logoImg.Height, GraphicsUnit.Pixel, imageAttributes);
                }
                catch { }
            }

            using var navyBrush = new SolidBrush(ColorPrimaryNavy);
            using var darkBrush = new SolidBrush(ColorTextDark);
            using var sideNumbersFont = new Font("Arial", 12f, FontStyle.Regular);

            // 📍 تجهيز كائنات الصيغ النصية لإلزام نظام ويندوز بالاتجاه العربي (Right-To-Left) ومنع تشوه الكلمات
            using var arabicFormat = new StringFormat
            {
                FormatFlags = StringFormatFlags.DirectionRightToLeft,
                Alignment = StringAlignment.Far,
                LineAlignment = StringAlignment.Center
            };

            using var cellCenterFormat = new StringFormat
            {
                FormatFlags = StringFormatFlags.DirectionRightToLeft,
                Alignment = StringAlignment.Center,
                LineAlignment = StringAlignment.Center
            };

            // ── رَسْمُ الهِيدَرِ العُلْوِيِّ ثَابِتًا فِي كُلِّ الصَّفْحَاتِ ──
            if (logoImg != null) g.DrawImage(logoImg, marginX + 15, currentY - 5, 110, 70);

            // 1. رقم القائمة في اليمين
            string invNoStr = $"رقم القائمة: {lblInvoiceNo.Text}";
            g.DrawString(invNoStr, sideNumbersFont, darkBrush, marginX + printWidth - g.MeasureString(invNoStr, sideNumbersFont).Width - 5, currentY + 5);

            // 2. التاريخ أسفل رقم القائمة
            string dateStr = $"التاريخ: {lblDateVal.Text}";
            g.DrawString(dateStr, sideNumbersFont, darkBrush, marginX + printWidth - g.MeasureString(dateStr, sideNumbersFont).Width - 5, currentY + 25);

            // 3. التعديل: رقم الصفحة أسفل التاريخ تماماً في جهة اليمين لمنع التداخل مع الملاحظات
            string pageStr = $"رقم الصفحة: {_pageNumber}";
            g.DrawString(pageStr, sideNumbersFont, Brushes.Gray, marginX + printWidth - g.MeasureString(pageStr, sideNumbersFont).Width - 5, currentY + 45);

            using var titleFont = new Font("Arial", 14f, FontStyle.Bold);
            string textLine1 = "شركة المطابخ العالمية";
            g.DrawString(textLine1, titleFont, navyBrush, marginX + (printWidth - g.MeasureString(textLine1, titleFont).Width) / 2, currentY - 5);
            currentY += 22;
            using var subFont1 = new Font("Arial", 10F, FontStyle.Bold);
            string textLine2 = "لتجهيز المطاعم والفنادق والدوائر الحكومية";
            g.DrawString(textLine2, subFont1, navyBrush, marginX + (printWidth - g.MeasureString(textLine2, subFont1).Width) / 2, currentY);

            currentY += 18;
            using var enFont1 = new Font("Arial", 10.5f, FontStyle.Bold);
            string textLine3 = "AL-MATBAKH AL-ELMEAA CO.";
            g.DrawString(textLine3, enFont1, navyBrush, marginX + (printWidth - g.MeasureString(textLine3, enFont1).Width) / 2, currentY);

            currentY += 20;
            using var mainLinePen = new Pen(ColorPrimaryNavy, 1.5f);
            g.DrawLine(mainLinePen, marginX, currentY, marginX + printWidth, currentY);

            // ── طِبَاعَةُ مَعْلُومَاتِ الزَّبُونِ وَالمُلَاحَظَاتِ (في الصفحة الأولى فقط) ──
            if (_pageNumber == 1)
            {
                currentY += 8;
                int infoBoxHeight = 40;
                using var boxPen = new Pen(Color.FromArgb(160, 165, 175), 1f);
                g.DrawRectangle(boxPen, marginX, currentY, printWidth, infoBoxHeight);

                int colWidth = printWidth / 3;

                // رسم الفواصل العمودية بدقة متساوية
                g.DrawLine(boxPen, marginX + colWidth, currentY, marginX + colWidth, currentY + infoBoxHeight);
                g.DrawLine(boxPen, marginX + (colWidth * 2), currentY, marginX + (colWidth * 2), currentY + infoBoxHeight);

                using var infoBoxFont = new Font("Arial", 10.5f, FontStyle.Bold);

                // القسم الأول (اليمين): جهة الشحن
                Rectangle rect1 = new Rectangle(marginX + (colWidth * 2), currentY, colWidth, infoBoxHeight);
                g.DrawString("جهة الشحن: بغداد / العراق", infoBoxFont, darkBrush, rect1, arabicFormat);

                // القسم الثاني (الوسط): اسم الزبون
                Rectangle rect2 = new Rectangle(marginX + colWidth, currentY, colWidth, infoBoxHeight);
                g.DrawString($"الزبون: {lblCustomerVal.Text}", infoBoxFont, darkBrush, rect2, arabicFormat);

                // القسم الثالث (اليسار): الملاحظات (تأكد أن النص هنا هو الملاحظات فقط)
                Rectangle rect3 = new Rectangle(marginX, currentY, colWidth, infoBoxHeight);
                g.DrawString($"الملاحظات: {lblNotesVal.Text}", infoBoxFont, darkBrush, rect3, arabicFormat);

                currentY += infoBoxHeight + 20;
            }
            else
            {
                currentY += 20;
            }

            // ── هنا نقوم بطباعة رقم الصفحة في مكان مستقل تماماً (مثلاً أسفل أو أعلى الورقة) ──
            using var pageNumFont = new Font("Arial", 9f, FontStyle.Regular);
            // تحديد مستطيل في أقصى يسار الصفحة أعلى أو أسفل الجدول
            Rectangle pageNumRect = new Rectangle(marginX, currentY - 15, 100, 20);
            g.DrawString($"صفحة رقم: {_pageNumber}", pageNumFont, Brushes.Gray, pageNumRect, arabicFormat);

            // ── بِنَاءُ رَأْسِ جَدْوَلِ المَوَادِّ (من اليمين إلى اليسار تماماً) ──
            float printFontSize = currentInvoiceFontSize;
            using var headerFont = new Font("Arial", printFontSize, FontStyle.Bold);
            using var textWhite = new SolidBrush(Color.White);
            using var borderPen = new Pen(Color.FromArgb(180, 185, 195), 1f);

            // عرض الأعمدة بالتسلسل العربي: (No -> 10%, اسم المادة -> 45%, سعر المفرد -> 15%, الكمية -> 12%, المبلغ الكلي -> 18%)
            int[] colWidths = { (int)(printWidth * 0.10), (int)(printWidth * 0.45), (int)(printWidth * 0.15), (int)(printWidth * 0.12), (int)(printWidth * 0.18) };
            int rowHeight = 35;

            g.FillRectangle(new SolidBrush(ColorPrimaryNavy), marginX, currentY, printWidth, rowHeight);

            // التعديل هنا: نبدأ من أقصى اليمين (الهامش + العرض الكلي للطباعة)
            int xOffset = marginX + printWidth;

            for (int i = 0; i < dgvItems.Columns.Count; i++)
            {
                // نطرح عرض العمود الحالي أولاً لنحصل على نقطة البداية (الزاوية اليسرى العليا للمستطيل)
                xOffset -= colWidths[i];

                Rectangle headerRect = new Rectangle(xOffset, currentY, colWidths[i], rowHeight);
                g.DrawRectangle(borderPen, headerRect);

                string hText = dgvItems.Columns[i].HeaderText;
                g.DrawString(hText, headerFont, textWhite, headerRect, cellCenterFormat);
            }
            currentY += rowHeight;

            // ── طِبَاعَةُ سُطُورِ المَوَادِّ (من اليمين إلى اليسار مع الفحص التلقائي للصفحات) ──
            using var cellFont = new Font("Arial", printFontSize - 0.5f);
            using var cellBrush = new SolidBrush(ColorTextDark);
            using var altBgBrush = new SolidBrush(ColorGridBg);

            while (_currentRowIndex < dgvItems.Rows.Count)
            {
                if (currentY + rowHeight > ev.MarginBounds.Bottom - 180)
                {
                    ev.HasMorePages = true;
                    return;
                }

                if (_currentRowIndex % 2 != 0)
                    g.FillRectangle(altBgBrush, marginX, currentY, printWidth, rowHeight);

                // التعديل هنا أيضاً: إعادة تعيين نقطة البدء من أقصى اليمين لكل سطر جديد
                xOffset = marginX + printWidth;
                var row = dgvItems.Rows[_currentRowIndex];

                for (int i = 0; i < dgvItems.Columns.Count; i++)
                {
                    // نطرح العرض ليتجه الرسم نحو اليسار
                    xOffset -= colWidths[i];

                    Rectangle cellRect = new Rectangle(xOffset, currentY, colWidths[i], rowHeight);
                    g.DrawRectangle(borderPen, cellRect);

                    string cellValue = row.Cells[i].Value?.ToString() ?? "";
                    g.DrawString(cellValue, cellFont, cellBrush, cellRect, cellCenterFormat);
                }
                currentY += rowHeight;
                _currentRowIndex++;
            }
            // ── طِبَاعَةُ جَدْوَلِ المَجَامِيعِ الفَاخِرِ فِي الصَّفْحَةِ الأَخِيرَةِ فَقَطْ ──
            currentY += 12;
            int totalBlockWidth = 320;
            int blockX = marginX; // جعل الحسابات بجهة اليمين لجمالية المظهر وثبات التصميم
            int blockHeight = 24;
            using var numberFont = new Font("Consolas", 10.5f, FontStyle.Bold);

            g.FillRectangle(new SolidBrush(Color.FromArgb(240, 242, 245)), blockX, currentY, totalBlockWidth, blockHeight);
            g.DrawString("المجموع الكلي", headerFont, darkBrush, blockX + totalBlockWidth - 100, currentY + 3);
            g.DrawString(lblTotalVal.Text, numberFont, darkBrush, blockX + 15, currentY + 3);

            currentY += blockHeight + 2;
            g.FillRectangle(new SolidBrush(Color.FromArgb(255, 230, 230)), blockX, currentY, totalBlockWidth, blockHeight);
            g.DrawString("خصم الفاتورة", headerFont, new SolidBrush(Color.FromArgb(166, 41, 41)), blockX + totalBlockWidth - 100, currentY + 3);
            g.DrawString(lblDiscountVal.Text, numberFont, new SolidBrush(Color.FromArgb(166, 41, 41)), blockX + 15, currentY + 3);

            currentY += blockHeight + 2;
            g.FillRectangle(new SolidBrush(ColorPrimaryNavy), blockX, currentY, totalBlockWidth, blockHeight);
            g.DrawString("المبلغ الصافي $", headerFont, textWhite, blockX + totalBlockWidth - 100, currentY + 3);
            g.DrawString(lblNetVal.Text, numberFont, textWhite, blockX + 15, currentY + 3);

            currentY += blockHeight + 2;
            g.FillRectangle(new SolidBrush(Color.FromArgb(220, 225, 235)), blockX, currentY, totalBlockWidth, blockHeight);
            g.DrawString("المبلغ الواصل", headerFont, darkBrush, blockX + totalBlockWidth - 100, currentY + 3);
            g.DrawString(lblPaidVal.Text, numberFont, darkBrush, blockX + 15, currentY + 3);

            currentY += blockHeight + 2;
            g.FillRectangle(new SolidBrush(ColorRedAlert), blockX, currentY, totalBlockWidth, blockHeight);
            g.DrawString("المتبقي بذمته", headerFont, textWhite, blockX + totalBlockWidth - 100, currentY + 3);
            g.DrawString(lblRemainingVal.Text, numberFont, textWhite, blockX + 15, currentY + 3);

            // الفوتر والتذييل النهائي
            currentY += 30;
            using var linePen = new Pen(Color.LightGray, 1f);
            g.DrawLine(linePen, marginX, currentY, marginX + printWidth, currentY);
            using var footerFont = new Font("Arial", 9f, FontStyle.Regular);
            g.DrawString("Email: info@kch-company.com  |  Web: www.kch-system.com", footerFont, Brushes.Gray, marginX, currentY + 8);

            ev.HasMorePages = false;
        };

        using var dlg = new PrintDialog { Document = pd };
        if (dlg.ShowDialog() == DialogResult.OK) pd.Print();
    }

    private void BtnBack_Click(object? sender, EventArgs e)
    {
        var searchForm = new SearchForm();
        searchForm.Show();
        this.Hide();
    }

    protected override void OnFormClosed(FormClosedEventArgs e)
    {
        Application.Exit();
        base.OnFormClosed(e);
    }
}