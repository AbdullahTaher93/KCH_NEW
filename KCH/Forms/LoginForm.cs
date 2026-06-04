using Microsoft.Data.Sqlite;
using KCH.Data;

namespace KCH.Forms;

public class LoginForm : Form
{
    private TextBox txtUsername = new();
    private TextBox txtPassword = new();
    private Button btnLogin = new();
    private Button btnChangePassword = new();
    private Label lblTitle = new();
    private Label lblSubTitle = new();
    private Panel pnlCard = new(); // حاوية ذكية لتجميع العناصر بمنتصف الشاشة

    public LoginForm()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        // ── 1. إعدادات النافذة الرئيسية (Modern Studio Style) ───────────────────
        this.Text = "تسجيل الدخول - KCH";
        this.Size = new Size(460, 520);
        this.StartPosition = FormStartPosition.CenterScreen;
        this.FormBorderStyle = FormBorderStyle.FixedSingle;
        this.MaximizeBox = false;

        // تفعيل خصائص اليمين إلى اليسار المتناسقة
        this.RightToLeft = RightToLeft.Yes;
        this.RightToLeftLayout = true;

        // خلفية داكنة فاخرة وعميقة
        this.BackColor = Color.FromArgb(20, 24, 30);

        // ── 2. بطاقة الواجهة المركزية (Central Card) ───────────────────────────
        pnlCard = new Panel
        {
            Size = new Size(380, 420),
            Location = new Point(32, 25),
            BackColor = Color.FromArgb(28, 32, 38) // لون أفتح للبطاقة ليعطي عمق بيكسلي
        };
        this.Controls.Add(pnlCard);

        // ── 3. العناوين والنصوص (Header) ───────────────────────────────────────
        lblTitle.Text = "KCH SYSTEM";
        lblTitle.Font = new Font("Segoe UI", 24F, FontStyle.Bold, GraphicsUnit.Point);
        lblTitle.ForeColor = Color.FromArgb(0, 150, 255); // أزرق نيون مشع واحترافي
        lblTitle.Size = new Size(380, 45);
        lblTitle.Location = new Point(0, 35);
        lblTitle.TextAlign = ContentAlignment.MiddleCenter;
        pnlCard.Controls.Add(lblTitle);

        lblSubTitle.Text = "نظام إدارة الفواتير الذكي";
        lblSubTitle.Font = new Font("Segoe UI", 11F, FontStyle.Regular, GraphicsUnit.Point);
        lblSubTitle.ForeColor = Color.FromArgb(140, 150, 165);
        lblSubTitle.Size = new Size(380, 25);
        lblSubTitle.Location = new Point(0, 85);
        lblSubTitle.TextAlign = ContentAlignment.MiddleCenter;
        pnlCard.Controls.Add(lblSubTitle);

        // ── 4. حقول الإدخال المحمية (Input Fields) ─────────────────────────────

        // حقل اسم المستخدم
        txtUsername.Font = new Font("Segoe UI", 12F, FontStyle.Regular, GraphicsUnit.Point);
        txtUsername.Size = new Size(320, 40);
        txtUsername.Location = new Point(30, 150); // إحداثيات ثابتة ومدروسة تمنع الاختفاء والقطع
        txtUsername.BackColor = Color.FromArgb(42, 48, 57);
        txtUsername.ForeColor = Color.White;
        txtUsername.BorderStyle = BorderStyle.FixedSingle;
        txtUsername.PlaceholderText = "👤   اسم المستخدم"; // مسافات إضافية لتنسيق الأيقونة
        pnlCard.Controls.Add(txtUsername);

        // حقل كلمة المرور
        txtPassword.Font = new Font("Segoe UI", 12F, FontStyle.Regular, GraphicsUnit.Point);
        txtPassword.Size = new Size(320, 40);
        txtPassword.Location = new Point(30, 215);
        txtPassword.BackColor = Color.FromArgb(42, 48, 57);
        txtPassword.ForeColor = Color.White;
        txtPassword.BorderStyle = BorderStyle.FixedSingle;
        txtPassword.PasswordChar = '●'; // نقاط تشفير دائرية سميكة
        txtPassword.PlaceholderText = "🔒   كلمة المرور";
        pnlCard.Controls.Add(txtPassword);

        // ── 5. أزرار التحكم الفاخرة (Action Buttons) ───────────────────────────

        // زر تسجيل الدخول الرئيسي
        btnLogin.Text = "🔑   تسجيل الدخول";
        btnLogin.Font = new Font("Segoe UI", 12F, FontStyle.Bold, GraphicsUnit.Point);
        btnLogin.Size = new Size(320, 46);
        btnLogin.Location = new Point(30, 295);
        btnLogin.BackColor = Color.FromArgb(0, 120, 215);
        btnLogin.ForeColor = Color.White;
        btnLogin.FlatStyle = FlatStyle.Flat;
        btnLogin.Cursor = Cursors.Hand;
        btnLogin.FlatAppearance.BorderSize = 0; // إلغاء الحواف التقليدية المزعجة للويندوز
        btnLogin.Click += BtnLogin_Click;
        pnlCard.Controls.Add(btnLogin);

        // زر تغيير كلمة المرور الرابط
        btnChangePassword.Text = "⚙️   تغيير بيانات الدخول؟";
        btnChangePassword.Font = new Font("Segoe UI", 10F, FontStyle.Regular, GraphicsUnit.Point);
        btnChangePassword.Size = new Size(320, 35);
        btnChangePassword.Location = new Point(30, 355);
        btnChangePassword.BackColor = Color.Transparent;
        btnChangePassword.ForeColor = Color.FromArgb(150, 160, 175);
        btnChangePassword.FlatStyle = FlatStyle.Flat;
        btnChangePassword.Cursor = Cursors.Hand;
        btnChangePassword.FlatAppearance.BorderSize = 0;
        btnChangePassword.FlatAppearance.MouseOverBackColor = Color.FromArgb(42, 48, 57); // تلميح عند مرور الماوس
        btnChangePassword.Click += BtnChangePassword_Click;
        pnlCard.Controls.Add(btnChangePassword);

        this.AcceptButton = btnLogin;
    }

    // ── المنطق البرمجي (دون أي تعديل على قاعدة بياناتك ليعمل فوراً) ───
    private void BtnLogin_Click(object? sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(txtUsername.Text) || string.IsNullOrWhiteSpace(txtPassword.Text))
        {
            MessageBox.Show("الرجاء إدخال اسم المستخدم وكلمة المرور", "تنبيه", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        using var conn = DatabaseHelper.GetConnection();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT COUNT(*) FROM Login WHERE Username = @user AND Password = @pass";
        cmd.Parameters.AddWithValue("@user", txtUsername.Text);
        cmd.Parameters.AddWithValue("@pass", txtPassword.Text);

        var count = Convert.ToInt64(cmd.ExecuteScalar());
        if (count > 0)
        {
            var invoiceForm = new InvoiceForm();
            invoiceForm.Show();
            this.Hide();
        }
        else
        {
            MessageBox.Show("اسم المستخدم أو كلمة المرور غير صحيحة", "خطأ", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void BtnChangePassword_Click(object? sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(txtUsername.Text) || string.IsNullOrWhiteSpace(txtPassword.Text))
        {
            MessageBox.Show("الرجاء إدخال بيانات الدخول الحالية أولاً", "تنبيه", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        using var conn = DatabaseHelper.GetConnection();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT COUNT(*) FROM Login WHERE Username = @user AND Password = @pass";
        cmd.Parameters.AddWithValue("@user", txtUsername.Text);
        cmd.Parameters.AddWithValue("@pass", txtPassword.Text);

        var count = Convert.ToInt64(cmd.ExecuteScalar());
        if (count == 0)
        {
            MessageBox.Show("بيانات الدخول الحالية غير صحيحة", "خطأ", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }

        var newUser = Microsoft.VisualBasic.Interaction.InputBox("الرجاء إدخال اسم المستخدم الجديد:", "تغيير البيانات");
        if (string.IsNullOrWhiteSpace(newUser)) { MessageBox.Show("اسم المستخدم لا يمكن أن يكون فارغاً"); return; }

        var newPass = Microsoft.VisualBasic.Interaction.InputBox("الرجاء إدخال كلمة المرور الجديدة:", "تغيير البيانات");
        if (string.IsNullOrWhiteSpace(newPass)) { MessageBox.Show("كلمة المرور لا يمكن أن تكون فارغة"); return; }

        using var updateCmd = conn.CreateCommand();
        updateCmd.CommandText = "UPDATE Login SET Username = @newUser, Password = @newPass WHERE Username = @oldUser";
        updateCmd.Parameters.AddWithValue("@newUser", newUser);
        updateCmd.Parameters.AddWithValue("@newPass", newPass);
        updateCmd.Parameters.AddWithValue("@oldUser", txtUsername.Text);
        updateCmd.ExecuteNonQuery();

        MessageBox.Show("تم تعديل البيانات بنجاح", "نجاح", MessageBoxButtons.OK, MessageBoxIcon.Information);
        txtUsername.Clear();
        txtPassword.Clear();
    }

    protected override void OnFormClosed(FormClosedEventArgs e)
    {
        Application.Exit();
        base.OnFormClosed(e);
    }
}