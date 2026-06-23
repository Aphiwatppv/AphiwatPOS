using System.ComponentModel;
using System.Data;
using System.Drawing.Imaging;
using System.Drawing.Printing;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using ClosedXML.Excel;
using Microsoft.Data.SqlClient;

namespace AphiwatPOS.BulkProductUpdater;

public sealed class BulkProductUpdaterForm : Form
{
    private static readonly Color HeaderStart = Color.FromArgb(5, 37, 34);
    private static readonly Color HeaderEnd = Color.FromArgb(16, 117, 104);
    private static readonly Color PageBackground = Color.FromArgb(241, 245, 249);
    private static readonly Color TextMain = Color.FromArgb(23, 37, 52);
    private static readonly Color TextMuted = Color.FromArgb(71, 85, 105);
    private static readonly Color Accent = Color.FromArgb(13, 116, 101);
    private readonly TextBox _sqlServer = new();
    private readonly TextBox _databaseName = new();
    private readonly CheckBox _trustServerCertificate = new();
    private readonly NumericUpDown _employeeId = new();
    private readonly ComboBox _location = new();
    private readonly Label _connectionLabel = new();
    private readonly TextBox _searchName = new();
    private readonly TextBox _searchCode = new();
    private readonly TextBox _searchBarcode = new();
    private readonly ComboBox _statusFilter = new();
    private readonly CheckBox _unsyncedImagesOnly = new();
    private readonly DataGridView _grid = new();
    private readonly TextBox _status = new();
    private readonly Label _summary = new();
    private readonly OfflineProductStore _offlineStore = new();
    private readonly BindingList<ProductGridRow> _products = [];
    private bool _offlineMode;
    private IReadOnlyList<InventoryLocationRow> _locations = [];
    private IReadOnlyList<LookupRow> _categories = [];
    private IReadOnlyList<LookupRow> _brands = [];
    private IReadOnlyList<LookupRow> _units = [];
    private object? _cellOriginalValue;

    public BulkProductUpdaterForm()
    {
        Text = "จัดการสินค้าและสต็อกสินค้า";
        MinimumSize = new Size(1360, 780);
        StartPosition = FormStartPosition.CenterScreen;
        Font = new Font("Segoe UI", 10);
        BackColor = PageBackground;

        var root = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 5,
            Padding = new Padding(18),
            BackColor = PageBackground
        };
        root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 98));

        root.Controls.Add(BuildConnectionPanel(), 0, 0);
        root.Controls.Add(BuildSearchPanel(), 0, 1);
        root.Controls.Add(BuildActionPanel(), 0, 2);
        root.Controls.Add(BuildGrid(), 0, 3);
        root.Controls.Add(BuildStatusBox(), 0, 4);
        Controls.Add(root);

        Shown += async (_, _) =>
        {
            if (!PromptForConnection())
            {
                Close();
                return;
            }

            await InitializeAsync();
        };
    }

    private Control BuildConnectionPanel()
    {
        _sqlServer.Text = @".\SQLEXPRESS";
        _databaseName.Text = "AphiwatPOSDB";
        _trustServerCertificate.Text = "Trust certificate";
        _trustServerCertificate.Checked = true;
        _employeeId.Minimum = 0;
        _employeeId.Maximum = 999999;
        _employeeId.Value = 1;
        _location.DropDownStyle = ComboBoxStyle.DropDownList;
        _location.DisplayMember = nameof(InventoryLocationRow.LocationName);
        _location.ValueMember = nameof(InventoryLocationRow.LocationId);
        _location.SelectedIndexChanged += async (_, _) => await LoadProductsAsync();

        var container = new TableLayoutPanel
        {
            Dock = DockStyle.Top,
            Height = 150,
            RowCount = 2,
            ColumnCount = 1,
            BackColor = PageBackground,
            Margin = new Padding(0, 0, 0, 14)
        };
        container.RowStyles.Add(new RowStyle(SizeType.Absolute, 86));
        container.RowStyles.Add(new RowStyle(SizeType.Absolute, 64));

        var banner = new GradientPanel
        {
            Dock = DockStyle.Fill,
            StartColor = HeaderStart,
            EndColor = HeaderEnd,
            Padding = new Padding(22, 16, 22, 12)
        };

        var infoPanel = new Panel { Dock = DockStyle.Fill, BackColor = HeaderStart };
        var title = new Label
        {
            Dock = DockStyle.Top,
            Height = 34,
            Text = "Bulk Product Updater",
            Font = new Font("Segoe UI", 18, FontStyle.Bold),
            ForeColor = Color.White,
            BackColor = HeaderStart
        };
        _connectionLabel.Dock = DockStyle.Top;
        _connectionLabel.Height = 24;
        _connectionLabel.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
        _connectionLabel.ForeColor = Color.FromArgb(205, 242, 235);
        _connectionLabel.BackColor = HeaderStart;
        UpdateConnectionLabel();
        infoPanel.Controls.Add(_connectionLabel);
        infoPanel.Controls.Add(title);

        var database = ThaiButton("Database", 120);
        database.BackColor = Color.FromArgb(18, 135, 119);
        database.ForeColor = Color.White;
        database.Click += async (_, _) =>
        {
            if (PromptForConnection())
            {
                await InitializeAsync();
            }
        };

        var reload = ThaiButton("Reload", 120);
        reload.BackColor = Color.FromArgb(31, 117, 105);
        reload.ForeColor = Color.White;
        reload.Click += async (_, _) => await InitializeAsync();

        var bannerActions = new FlowLayoutPanel
        {
            Dock = DockStyle.Right,
            Width = 260,
            FlowDirection = FlowDirection.RightToLeft,
            WrapContents = false,
            Padding = new Padding(0, 6, 0, 0),
            BackColor = HeaderEnd
        };
        bannerActions.Controls.Add(reload);
        bannerActions.Controls.Add(database);
        banner.Controls.Add(infoPanel);
        banner.Controls.Add(bannerActions);

        var controlBar = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 6,
            Padding = new Padding(18, 12, 18, 10),
            BackColor = Color.White
        };
        controlBar.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 90));
        controlBar.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 90));
        controlBar.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 105));
        controlBar.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 260));
        controlBar.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        controlBar.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 170));

        _employeeId.Dock = DockStyle.Fill;
        _location.Dock = DockStyle.Fill;
        AddLabel(controlBar, "Employee", 0, 0);
        controlBar.Controls.Add(_employeeId, 1, 0);
        AddLabel(controlBar, "Warehouse", 2, 0);
        controlBar.Controls.Add(_location, 3, 0);
        _summary.Dock = DockStyle.Fill;
        _summary.TextAlign = ContentAlignment.MiddleRight;
        _summary.Font = new Font("Segoe UI Semibold", 10, FontStyle.Bold);
        _summary.ForeColor = Accent;
        controlBar.Controls.Add(_summary, 5, 0);

        container.Controls.Add(banner, 0, 0);
        container.Controls.Add(controlBar, 0, 1);
        return container;
    }

    private Control BuildSearchPanel()
    {
        var panel = new TableLayoutPanel
        {
            Dock = DockStyle.Top,
            Height = 94,
            ColumnCount = 10,
            Padding = new Padding(16, 14, 16, 12),
            Margin = new Padding(0, 0, 0, 14),
            BackColor = Color.White
        };
        panel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 128));
        panel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 32));
        panel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 120));
        panel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 24));
        panel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 120));
        panel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 24));
        panel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 104));
        panel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 132));
        panel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 96));
        panel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 118));

        _statusFilter.DropDownStyle = ComboBoxStyle.DropDownList;
        _statusFilter.Items.AddRange(["ทั้งหมด", "เปิดใช้งาน", "ปิดใช้งาน"]);
        _statusFilter.SelectedIndex = 1;
        _unsyncedImagesOnly.Text = "รูปยังไม่ซิงค์";
        _unsyncedImagesOnly.AutoSize = true;
        _unsyncedImagesOnly.Dock = DockStyle.Fill;
        _unsyncedImagesOnly.ForeColor = TextMuted;
        _searchName.Dock = DockStyle.Fill;
        _searchCode.Dock = DockStyle.Fill;
        _searchBarcode.Dock = DockStyle.Fill;
        _statusFilter.Dock = DockStyle.Fill;
        _searchBarcode.KeyDown += async (_, e) =>
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.SuppressKeyPress = true;
                await BarcodeSearchAsync();
            }
        };

        var search = ThaiButton("ค้นหา", 86);
        search.BackColor = Accent;
        search.ForeColor = Color.White;
        search.Click += async (_, _) => await LoadProductsAsync();
        var clear = ThaiButton("ล้างตัวกรอง", 110);
        clear.BackColor = Color.FromArgb(51, 65, 85);
        clear.ForeColor = Color.White;
        clear.Click += async (_, _) =>
        {
            _searchName.Clear();
            _searchCode.Clear();
            _searchBarcode.Clear();
            _statusFilter.SelectedIndex = 1;
            await LoadProductsAsync();
        };

        AddLabel(panel, "ค้นหาจากชื่อสินค้า", 0, 0);
        panel.Controls.Add(_searchName, 1, 0);
        AddLabel(panel, "ค้นหาจากรหัสสินค้า", 2, 0);
        panel.Controls.Add(_searchCode, 3, 0);
        AddLabel(panel, "ค้นหาจากบาร์โค้ด", 4, 0);
        panel.Controls.Add(_searchBarcode, 5, 0);
        AddLabel(panel, "สถานะสินค้า", 6, 0);
        panel.Controls.Add(_statusFilter, 7, 0);
        panel.Controls.Add(_unsyncedImagesOnly, 8, 0);
        panel.Controls.Add(search, 9, 0);
        panel.Controls.Add(clear, 10, 0);
        return panel;
    }

    private Control BuildActionPanel()
    {
        var panel = new FlowLayoutPanel
        {
            Dock = DockStyle.Top,
            AutoSize = true,
            FlowDirection = FlowDirection.LeftToRight,
            Padding = new Padding(0, 0, 0, 14),
            BackColor = PageBackground
        };
        var create = ThaiButton("เพิ่มสินค้าใหม่", 145);
        var print = ThaiButton("พิมพ์บาร์โค้ด", 130);
        var image = ThaiButton("เพิ่มหรือเปลี่ยนรูปสินค้า", 200);
        var syncImage = ThaiButton("ซิงค์รูปภาพขึ้นฐานข้อมูล", 210);
        var export = ThaiButton("ส่งออกไฟล์ Excel สำหรับนับสต็อก", 250);
        var import = ThaiButton("นำเข้า Excel และอัปเดตสต็อก", 230);
        var history = ThaiButton("ประวัติการนำเข้าไฟล์ Excel", 220);
        create.BackColor = Accent;
        create.ForeColor = Color.White;
        foreach (var button in new[] { print, image, syncImage, export, import, history })
        {
            button.BackColor = Color.White;
            button.ForeColor = TextMain;
            button.FlatAppearance.BorderColor = Color.FromArgb(203, 213, 225);
        }

        create.Click += async (_, _) => await CreateProductAsync();
        print.Click += (_, _) => PrintSelectedBarcode();
        image.Click += async (_, _) => await AddOrChangeProductImageAsync();
        syncImage.Click += async (_, _) => await SyncProductImagesAsync();
        export.Click += async (_, _) => await ExportStockCountTemplateAsync();
        import.Click += async (_, _) => await ImportStockCountAsync();
        history.Click += async (_, _) => await ShowImportHistoryAsync();

        panel.Controls.Add(create);
        panel.Controls.Add(print);
        panel.Controls.Add(image);
        panel.Controls.Add(syncImage);
        panel.Controls.Add(export);
        panel.Controls.Add(import);
        panel.Controls.Add(history);
        return panel;
    }

    private Control BuildGrid()
    {
        _grid.Dock = DockStyle.Fill;
        _grid.AutoGenerateColumns = false;
        _grid.AllowUserToAddRows = false;
        _grid.AllowUserToDeleteRows = false;
        _grid.RowHeadersVisible = false;
        _grid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        _grid.MultiSelect = false;
        _grid.BackgroundColor = Color.White;
        _grid.BorderStyle = BorderStyle.None;
        _grid.EnableHeadersVisualStyles = false;
        _grid.GridColor = Color.FromArgb(226, 232, 240);
        _grid.ColumnHeadersHeight = 38;
        _grid.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(15, 23, 42);
        _grid.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
        _grid.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI Semibold", 10, FontStyle.Bold);
        _grid.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
        _grid.DefaultCellStyle.BackColor = Color.White;
        _grid.DefaultCellStyle.ForeColor = TextMain;
        _grid.DefaultCellStyle.SelectionBackColor = Color.FromArgb(209, 250, 229);
        _grid.DefaultCellStyle.SelectionForeColor = Color.FromArgb(15, 23, 42);
        _grid.DefaultCellStyle.Font = new Font("Segoe UI", 10);
        _grid.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(248, 250, 252);
        _grid.RowTemplate.Height = 34;
        _grid.DataSource = _products;
        _grid.CellBeginEdit += (_, e) => _cellOriginalValue = _grid.Rows[e.RowIndex].Cells[e.ColumnIndex].Value;
        _grid.CellValueChanged += async (_, e) => await SaveEditedCellAsync(e.RowIndex, e.ColumnIndex);
        _grid.CellFormatting += (_, e) =>
        {
            if (_grid.Columns[e.ColumnIndex].DataPropertyName == nameof(ProductGridRow.IsActive) && e.Value is bool active && e.CellStyle is not null)
            {
                e.CellStyle.BackColor = active ? Color.FromArgb(220, 252, 231) : Color.FromArgb(229, 231, 235);
            }
        };

        _grid.Columns.Add(new DataGridViewImageColumn
        {
            DataPropertyName = nameof(ProductGridRow.Thumbnail),
            HeaderText = "รูป",
            Width = 64,
            ImageLayout = DataGridViewImageCellLayout.Zoom,
            ReadOnly = true
        });
        _grid.Columns.Add(TextColumn(nameof(ProductGridRow.ProductCode), "รหัสสินค้า", 115, true));
        _grid.Columns.Add(TextColumn(nameof(ProductGridRow.ProductName), "ชื่อสินค้า", 260, false));
        _grid.Columns.Add(TextColumn(nameof(ProductGridRow.Barcode), "บาร์โค้ด", 150, false));
        _grid.Columns.Add(MoneyColumn(nameof(ProductGridRow.CostPrice), "ราคาทุน", 95, false));
        _grid.Columns.Add(MoneyColumn(nameof(ProductGridRow.SellingPrice), "ราคาขายปลีก", 110, false));
        _grid.Columns.Add(MoneyColumn(nameof(ProductGridRow.WholesalePrice), "ราคาขายส่ง", 110, false));
        _grid.Columns.Add(MoneyColumn(nameof(ProductGridRow.CurrentStock), "สต็อกปัจจุบัน", 115, true));
        _grid.Columns.Add(new DataGridViewComboBoxColumn
        {
            DataPropertyName = nameof(ProductGridRow.UnitId),
            HeaderText = "หน่วยสินค้า",
            Width = 130,
            DisplayMember = nameof(LookupRow.Name),
            ValueMember = nameof(LookupRow.Id),
            FlatStyle = FlatStyle.Flat
        });
        _grid.Columns.Add(new DataGridViewCheckBoxColumn { DataPropertyName = nameof(ProductGridRow.IsActive), HeaderText = "สถานะเปิดใช้งาน", Width = 125 });
        _grid.Columns.Add(TextColumn(nameof(ProductGridRow.ImageSyncStatus), "สถานะรูป", 105, true));
        _grid.Columns.Add(TextColumn(nameof(ProductGridRow.LastUpdatedDate), "แก้ไขล่าสุด", 145, true));
        return _grid;
    }

    private Control BuildStatusBox()
    {
        _status.Dock = DockStyle.Fill;
        _status.Multiline = true;
        _status.ReadOnly = true;
        _status.ScrollBars = ScrollBars.Vertical;
        _status.BackColor = Color.FromArgb(25, 28, 34);
        _status.ForeColor = Color.FromArgb(235, 242, 250);
        _status.Font = new Font("Consolas", 10);
        return _status;
    }

    private async Task InitializeAsync()
    {
        Cursor = Cursors.WaitCursor;
        try
        {
            using var repo = CreateRepository();
            await repo.EnsureSupportObjectsAsync();
            _locations = await repo.GetLocationsAsync();
            _categories = await repo.GetCategoriesAsync();
            _brands = await repo.GetBrandsAsync();
            _units = await repo.GetUnitsAsync();
            _location.DataSource = _locations;
            SelectDefaultLocation();
            foreach (var column in _grid.Columns.OfType<DataGridViewComboBoxColumn>())
            {
                column.DataSource = _units.ToArray();
            }
            await LoadProductsCoreAsync(repo);
            _offlineMode = false;
            _offlineStore.Save(new OfflineProductCache(_locations, _categories, _brands, _units, _products.ToArray()));
            Log("เชื่อมต่อฐานข้อมูลสำเร็จ และบันทึกข้อมูลไว้ใช้ offline แล้ว");
            _searchBarcode.Focus();
        }
        catch (Exception ex)
        {
            Log("OFFLINE: " + ex.Message);
            LoadOfflineCache();
            MessageBox.Show(this, "ไม่สามารถเชื่อมต่อฐานข้อมูลได้ ระบบจะเปิดใช้งานแบบ Offline จากข้อมูลที่บันทึกล่าสุด", Text, MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
        finally
        {
            Cursor = Cursors.Default;
        }
    }

    private async Task LoadProductsAsync()
    {
        if (_offlineMode)
        {
            LoadOfflineProducts();
            return;
        }

        await RunAsync(async () =>
        {
            using var repo = CreateRepository();
            await LoadProductsCoreAsync(repo);
        });
    }

    private async Task LoadProductsCoreAsync(ProductStockRepository repo)
    {
        var rows = await repo.GetProductsAsync(SelectedLocationId(), new ProductSearchFilter
        {
            ProductName = _searchName.Text,
            ProductCode = _searchCode.Text,
            Barcode = _searchBarcode.Text,
            IsActive = _statusFilter.SelectedIndex switch { 1 => true, 2 => false, _ => null },
            UnsyncedImagesOnly = _unsyncedImagesOnly.Checked
        });
        _products.Clear();
        foreach (var row in rows) _products.Add(row);
        _summary.Text = $"สินค้า {_products.Count:N0} รายการ";
        _offlineStore.Save(new OfflineProductCache(_locations, _categories, _brands, _units, _products.ToArray()));
        Log($"โหลดสินค้า {_products.Count:N0} รายการ");
    }

    private void LoadOfflineCache()
    {
        var cache = _offlineStore.Load();
        _offlineMode = true;
        _locations = cache.Locations;
        _categories = cache.Categories;
        _brands = cache.Brands;
        _units = cache.Units;
        _location.DataSource = _locations;
        foreach (var column in _grid.Columns.OfType<DataGridViewComboBoxColumn>())
        {
            column.DataSource = _units.ToArray();
        }
        LoadOfflineProducts();
    }

    private void LoadOfflineProducts()
    {
        var cache = _offlineStore.Load();
        var name = _searchName.Text.Trim();
        var code = _searchCode.Text.Trim();
        var barcode = _searchBarcode.Text.Trim();
        var active = _statusFilter.SelectedIndex switch { 1 => true, 2 => false, _ => (bool?)null };
        _products.Clear();
        foreach (var product in cache.Products.Where(p =>
            (string.IsNullOrWhiteSpace(name) || p.ProductName.Contains(name, StringComparison.OrdinalIgnoreCase)) &&
            (string.IsNullOrWhiteSpace(code) || p.ProductCode.Contains(code, StringComparison.OrdinalIgnoreCase)) &&
            (string.IsNullOrWhiteSpace(barcode) || p.Barcode.Contains(barcode, StringComparison.OrdinalIgnoreCase)) &&
            (!active.HasValue || p.IsActive == active.Value) &&
            (!_unsyncedImagesOnly.Checked || p.ImageSyncStatus is "Pending" or "Failed")))
        {
            _products.Add(product);
        }
        _summary.Text = $"Offline mode | สินค้า {_products.Count:N0} รายการ";
        Log($"โหลดข้อมูล offline {_products.Count:N0} รายการ");
    }

    private async Task BarcodeSearchAsync()
    {
        await LoadProductsAsync();
        if (_products.Count > 0) return;

        var barcode = _searchBarcode.Text.Trim();
        if (string.IsNullOrWhiteSpace(barcode)) return;
        var confirm = MessageBox.Show(this, "ไม่พบบาร์โค้ดนี้ในระบบ ต้องการเพิ่มสินค้าใหม่หรือไม่?", Text, MessageBoxButtons.YesNo, MessageBoxIcon.Question);
        if (confirm == DialogResult.Yes) await CreateProductAsync(barcode);
    }

    private async Task SaveEditedCellAsync(int rowIndex, int columnIndex)
    {
        if (rowIndex < 0 || columnIndex < 0) return;
        var column = _grid.Columns[columnIndex];
        var property = column.DataPropertyName;
        if (string.IsNullOrWhiteSpace(property) || property is nameof(ProductGridRow.ProductCode) or nameof(ProductGridRow.CurrentStock) or nameof(ProductGridRow.LastUpdatedDate)) return;
        var row = _grid.Rows[rowIndex].DataBoundItem as ProductGridRow;
        if (row is null) return;

        _grid.Rows[rowIndex].Cells[columnIndex].Style.BackColor = Color.FromArgb(255, 244, 179);
        var validation = ValidateProductRow(row, property);
        if (!string.IsNullOrWhiteSpace(validation))
        {
            MessageBox.Show(this, validation, Text, MessageBoxButtons.OK, MessageBoxIcon.Warning);
            _grid.Rows[rowIndex].Cells[columnIndex].Value = _cellOriginalValue;
            _grid.Rows[rowIndex].Cells[columnIndex].Style.BackColor = Color.White;
            return;
        }

        if ((property is nameof(ProductGridRow.CostPrice) or nameof(ProductGridRow.SellingPrice) or nameof(ProductGridRow.WholesalePrice) or nameof(ProductGridRow.Barcode)) &&
            MessageBox.Show(this, "ยืนยันการแก้ไขข้อมูลสินค้าหรือไม่?", Text, MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
        {
            _grid.Rows[rowIndex].Cells[columnIndex].Value = _cellOriginalValue;
            _grid.Rows[rowIndex].Cells[columnIndex].Style.BackColor = Color.White;
            return;
        }

        if (_offlineMode)
        {
            row.OfflineDirty = true;
            row.LastUpdatedDateValue = DateTime.Now;
            _offlineStore.UpsertProduct(row);
            _grid.Rows[rowIndex].Cells[columnIndex].Style.BackColor = Color.FromArgb(220, 252, 231);
            MessageBox.Show(this, "บันทึกข้อมูลแบบ Offline แล้ว กรุณาซิงค์เมื่อเชื่อมต่อฐานข้อมูลได้", Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        await RunAsync(async () =>
        {
            using var repo = CreateRepository();
            await repo.UpdateProductAsync(row, property, _cellOriginalValue, CurrentEmployeeId());
            var refreshed = await repo.GetProductByIdAsync(row.ProductId, SelectedLocationId());
            if (refreshed is not null)
            {
                _products[rowIndex] = refreshed;
            }
            _grid.Rows[rowIndex].Cells[columnIndex].Style.BackColor = Color.FromArgb(220, 252, 231);
            MessageBox.Show(this, "บันทึกข้อมูลสินค้าเรียบร้อยแล้ว", Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
        });
    }

    private static string? ValidateProductRow(ProductGridRow row, string property)
    {
        if (string.IsNullOrWhiteSpace(row.ProductName)) return "ชื่อสินค้าห้ามว่าง";
        if (row.CostPrice < 0 || property == nameof(ProductGridRow.CostPrice) && row.CostPrice < 0) return "ราคาทุนต้องไม่ติดลบ";
        if (row.SellingPrice < 0) return "ราคาขายต้องไม่ติดลบ";
        if (row.WholesalePrice < 0) return "ราคาขายส่งต้องไม่ติดลบ";
        if (row.SellingPrice < row.CostPrice)
        {
            return MessageBox.Show("ราคาขายต่ำกว่าราคาทุน ต้องการบันทึกต่อหรือไม่?", "จัดการสินค้าและสต็อกสินค้า", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes
                ? null
                : "ยกเลิกการบันทึกเพราะราคาขายต่ำกว่าราคาทุน";
        }
        return null;
    }

    private async Task CreateProductAsync(string? scannedBarcode = null)
    {
        if (_offlineMode)
        {
            MessageBox.Show(this, "โหมด Offline ยังไม่สามารถเพิ่มสินค้าใหม่ได้ กรุณาเชื่อมต่อฐานข้อมูลก่อนเพิ่มสินค้า", Text, MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        await RunAsync(async () =>
        {
            using var repo = CreateRepository();
            if (_categories.Count == 0) _categories = await repo.GetCategoriesAsync();
            if (_brands.Count == 0) _brands = await repo.GetBrandsAsync();
            if (_units.Count == 0) _units = await repo.GetUnitsAsync();
            using var form = new ProductEditorForm(repo, new BarcodeService(repo), _categories, _brands, _units, CurrentEmployeeId(), SelectedLocationId(), scannedBarcode);
            if (form.ShowDialog(this) == DialogResult.OK)
            {
                await LoadProductsCoreAsync(repo);
                MessageBox.Show(this, "บันทึกข้อมูลสินค้าเรียบร้อยแล้ว", Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        });
    }

    private void PrintSelectedBarcode()
    {
        var product = SelectedProduct();
        if (product is null)
        {
            MessageBox.Show(this, "กรุณาเลือกสินค้า", Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }
        if (string.IsNullOrWhiteSpace(product.Barcode))
        {
            MessageBox.Show(this, "สินค้านี้ยังไม่มีบาร์โค้ด", Text, MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        using var dialog = new BarcodePrintForm(product, new BarcodePrintService());
        dialog.ShowDialog(this);
    }

    private async Task AddOrChangeProductImageAsync()
    {
        var product = SelectedProduct();
        if (product is null)
        {
            MessageBox.Show(this, "กรุณาเลือกสินค้า", Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        using var dialog = new OpenFileDialog
        {
            Filter = "Image files (*.jpg;*.jpeg;*.png;*.webp;*.bmp)|*.jpg;*.jpeg;*.png;*.webp;*.bmp",
            Title = "เลือกรูปสินค้า"
        };
        if (dialog.ShowDialog(this) != DialogResult.OK) return;

        await RunAsync(async () =>
        {
            var saved = ProductImageStore.SaveProductImage(dialog.FileName, product.ProductCode);
            if (_offlineMode)
            {
                product.LocalImagePath = saved.LocalPath;
                product.ImageHash = saved.Hash;
                product.ImageSyncStatus = "Pending";
                product.OfflineDirty = true;
                product.LastUpdatedDateValue = DateTime.Now;
                _offlineStore.UpsertProduct(product);
                _grid.Refresh();
                MessageBox.Show(this, "บันทึกรูปสินค้าแบบ Offline แล้ว กรุณาซิงค์เมื่อเชื่อมต่อฐานข้อมูลได้", Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            using var repo = CreateRepository();
            await repo.UpsertProductImageAsync(product.ProductId, saved.LocalPath, saved.Hash, CurrentEmployeeId(), "Pending", null);
            var refreshed = await repo.GetProductByIdAsync(product.ProductId, SelectedLocationId());
            if (refreshed is not null)
            {
                var index = _products.IndexOf(product);
                if (index >= 0) _products[index] = refreshed;
            }
            MessageBox.Show(this, "บันทึกรูปสินค้าไว้ในเครื่องแล้ว กรุณาซิงค์รูปภาพขึ้นฐานข้อมูล", Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
        });
    }

    private async Task SyncProductImagesAsync()
    {
        if (_offlineMode)
        {
            MessageBox.Show(this, "ขณะนี้อยู่ในโหมด Offline กรุณาเชื่อมต่อฐานข้อมูลแล้วกดโหลดข้อมูลใหม่ก่อนซิงค์รูปภาพ", Text, MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        await RunAsync(async () =>
        {
            using var repo = CreateRepository();
            var pending = await repo.GetPendingImagesAsync();
            if (pending.Count == 0)
            {
                MessageBox.Show(this, "ไม่มีรูปสินค้าที่รอซิงค์", Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var success = 0;
            foreach (var image in pending)
            {
                try
                {
                    if (!File.Exists(image.LocalImagePath))
                    {
                        await repo.MarkImageSyncFailedAsync(image.ProductImageSyncId, "ไม่พบไฟล์รูปในเครื่อง");
                        continue;
                    }
                    if (await repo.IsProductImageHashSyncedAsync(image.ProductId, image.ImageHash))
                    {
                        await repo.MarkImageSyncedAsync(image.ProductImageSyncId, CurrentEmployeeId(), image.LocalImagePath);
                        success++;
                        continue;
                    }

                    await repo.SyncProductImageAsync(image, CurrentEmployeeId());
                    success++;
                }
                catch
                {
                    await repo.MarkImageSyncFailedAsync(image.ProductImageSyncId, "ซิงค์รูปภาพไม่สำเร็จ กรุณาลองใหม่อีกครั้ง");
                }
            }

            await LoadProductsAsync();
            MessageBox.Show(this, $"ซิงค์รูปภาพสำเร็จ {success:N0} รายการ", Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
        });
    }

    private async Task ExportStockCountTemplateAsync()
    {
        if (_offlineMode)
        {
            using var offlineDialog = new SaveFileDialog
            {
                Filter = "Excel workbook (*.xlsx)|*.xlsx",
                FileName = $"StockCountTemplate_{DateTime.Now:yyyyMMdd_HHmm}.xlsx"
            };
            if (offlineDialog.ShowDialog(this) != DialogResult.OK) return;
            ExcelStockCountService.ExportTemplate(offlineDialog.FileName, _products.ToArray(), DateTime.Now, CurrentEmployeeId());
            MessageBox.Show(this, "ส่งออกไฟล์ Excel จากข้อมูล Offline เรียบร้อยแล้ว", Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        await RunAsync(async () =>
        {
            using var repo = CreateRepository();
            var products = await repo.GetProductsAsync(SelectedLocationId(), new ProductSearchFilter
            {
                ProductName = _searchName.Text,
                ProductCode = _searchCode.Text,
                Barcode = _searchBarcode.Text,
                IsActive = true
            });

            using var dialog = new SaveFileDialog
            {
                Filter = "Excel workbook (*.xlsx)|*.xlsx",
                FileName = $"StockCountTemplate_{DateTime.Now:yyyyMMdd_HHmm}.xlsx"
            };
            if (dialog.ShowDialog(this) != DialogResult.OK) return;

            ExcelStockCountService.ExportTemplate(dialog.FileName, products, DateTime.Now, CurrentEmployeeId());
            Log($"ส่งออกไฟล์ Excel สำหรับนับสต็อก: {dialog.FileName}");
            MessageBox.Show(this, "ส่งออกไฟล์ Excel เรียบร้อยแล้ว", Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
        });
    }

    private async Task ImportStockCountAsync()
    {
        using var dialog = new OpenFileDialog { Filter = "Excel workbook (*.xlsx)|*.xlsx" };
        if (dialog.ShowDialog(this) != DialogResult.OK) return;

        if (_offlineMode)
        {
            await RunAsync(async () =>
            {
                var preview = await ExcelStockCountService.ImportPreviewFromProductsAsync(dialog.FileName, _products.ToArray());
                using var previewForm = new StockImportPreviewForm(preview);
                if (previewForm.ShowDialog(this) != DialogResult.OK) return;
                foreach (var row in preview.Rows.Where(x => x.Status == ImportRowStatus.Ready && x.NewStock.HasValue && x.Difference != 0))
                {
                    var product = _products.FirstOrDefault(x => x.ProductId == row.ProductId || x.ProductCode.Equals(row.ProductCode, StringComparison.OrdinalIgnoreCase));
                    if (product is null) continue;
                    product.CurrentStock = row.NewStock.GetValueOrDefault();
                    product.OfflineDirty = true;
                    product.LastUpdatedDateValue = DateTime.Now;
                    _offlineStore.UpsertProduct(product);
                }
                LoadOfflineProducts();
                MessageBox.Show(this, "อัปเดตสต็อกลงข้อมูล Offline แล้ว กรุณาซิงค์เมื่อเชื่อมต่อฐานข้อมูลได้", Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
            });
            return;
        }

        await RunAsync(async () =>
        {
            using var repo = CreateRepository();
            var preview = await ExcelStockCountService.ImportPreviewAsync(dialog.FileName, repo, SelectedLocationId());
            using var previewForm = new StockImportPreviewForm(preview);
            if (previewForm.ShowDialog(this) != DialogResult.OK) return;

            await repo.ApplyStockImportAsync(preview, SelectedLocationId(), CurrentEmployeeId(), Path.GetFileName(dialog.FileName));
            MessageBox.Show(this, "นำเข้าข้อมูลสำเร็จ และอัปเดตสต็อกเรียบร้อยแล้ว", Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
            await LoadProductsCoreAsync(repo);
        });
    }

    private async Task ShowImportHistoryAsync()
    {
        await RunAsync(async () =>
        {
            using var repo = CreateRepository();
            using var form = new ImportHistoryForm(await repo.GetImportHistoryAsync());
            form.ShowDialog(this);
        });
    }

    private ProductGridRow? SelectedProduct() => _grid.CurrentRow?.DataBoundItem as ProductGridRow;

    private ProductStockRepository CreateRepository()
    {
        var server = _sqlServer.Text.Trim();
        var database = _databaseName.Text.Trim();
        if (string.IsNullOrWhiteSpace(server)) throw new InvalidOperationException("SQL Server is required.");
        if (string.IsNullOrWhiteSpace(database)) throw new InvalidOperationException("Database is required.");
        var trust = _trustServerCertificate.Checked ? "True" : "False";
        return new ProductStockRepository($"Data Source={server};Initial Catalog={database};Integrated Security=True;Encrypt=True;TrustServerCertificate={trust};Connect Timeout=60");
    }

    private bool PromptForConnection()
    {
        using var form = new BulkProductConnectionForm(_sqlServer.Text, _databaseName.Text, _trustServerCertificate.Checked);
        if (form.ShowDialog(this) != DialogResult.OK)
        {
            return false;
        }

        _sqlServer.Text = form.SqlServer;
        _databaseName.Text = form.DatabaseName;
        _trustServerCertificate.Checked = form.TrustServerCertificate;
        UpdateConnectionLabel();
        return true;
    }

    private void UpdateConnectionLabel()
    {
        _connectionLabel.Text = $"Connected target: {_sqlServer.Text.Trim()} / {_databaseName.Text.Trim()}";
    }

    private int SelectedLocationId() => _location.SelectedItem is InventoryLocationRow row ? row.LocationId : _locations.FirstOrDefault(x => x.IsDefault)?.LocationId ?? _locations.FirstOrDefault()?.LocationId ?? 0;

    private void SelectDefaultLocation()
    {
        var selected = _locations.FirstOrDefault(x => x.IsDefault) ?? _locations.FirstOrDefault();
        if (selected is not null) _location.SelectedValue = selected.LocationId;
    }

    private int CurrentEmployeeId() => Convert.ToInt32(_employeeId.Value);

    private async Task RunAsync(Func<Task> action)
    {
        Cursor = Cursors.WaitCursor;
        try
        {
            await action();
        }
        catch (SqlException ex)
        {
            Log("SQL ERROR: " + ex.Message);
            MessageBox.Show(this, ThaiError(ex), Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        catch (Exception ex)
        {
            Log("ERROR: " + ex.Message);
            MessageBox.Show(this, "เกิดข้อผิดพลาดระหว่างทำงาน กรุณาตรวจสอบข้อมูลและลองใหม่อีกครั้ง", Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        finally
        {
            Cursor = Cursors.Default;
        }
    }

    private static string ThaiError(Exception ex)
    {
        if (ex.Message.Contains("duplicate", StringComparison.OrdinalIgnoreCase) || ex.Message.Contains("บาร์โค้ด")) return "บาร์โค้ดนี้ถูกใช้งานกับสินค้าอื่นแล้ว";
        if (ex.Message.Contains("Selling price cannot be lower", StringComparison.OrdinalIgnoreCase)) return "ราคาขายต่ำกว่าราคาขั้นต่ำที่ระบบกำหนด";
        if (ex.Message.Contains("Insufficient stock", StringComparison.OrdinalIgnoreCase)) return "จำนวนสต็อกไม่เพียงพอ";
        return "เกิดข้อผิดพลาดระหว่างอัปเดตข้อมูล ระบบได้ยกเลิกการเปลี่ยนแปลงแล้ว";
    }

    private void Log(string message) => _status.AppendText($"[{DateTime.Now:HH:mm:ss}] {message}{Environment.NewLine}");

    private static Button ThaiButton(string text, int width) => new()
    {
        Text = text,
        Width = width,
        Height = 40,
        FlatStyle = FlatStyle.Flat,
        BackColor = Color.White,
        Font = new Font("Segoe UI", 10, FontStyle.Bold),
        Margin = new Padding(0, 0, 8, 0),
        Cursor = Cursors.Hand,
        Padding = new Padding(8, 0, 8, 0)
    };

    private static void AddLabel(TableLayoutPanel panel, string text, int column, int row)
    {
        panel.Controls.Add(new Label
        {
            Text = text,
            Dock = DockStyle.Fill,
            TextAlign = ContentAlignment.MiddleLeft,
            Font = new Font("Segoe UI", 10, FontStyle.Bold),
            ForeColor = TextMain,
            Margin = new Padding(0, 4, 8, 4)
        }, column, row);
    }

    private static DataGridViewTextBoxColumn TextColumn(string property, string header, int width, bool readOnly) =>
        new() { DataPropertyName = property, HeaderText = header, Width = width, ReadOnly = readOnly };

    private static DataGridViewTextBoxColumn MoneyColumn(string property, string header, int width, bool readOnly) =>
        new() { DataPropertyName = property, HeaderText = header, Width = width, ReadOnly = readOnly, DefaultCellStyle = new DataGridViewCellStyle { Format = "N2", Alignment = DataGridViewContentAlignment.MiddleRight } };
}

public sealed class ProductStockRepository : IDisposable
{
    private readonly SqlConnection _connection;

    public ProductStockRepository(string connectionString)
    {
        _connection = new SqlConnection(connectionString);
        _connection.Open();
    }

    public void Dispose() => _connection.Dispose();

    public async Task EnsureSupportObjectsAsync()
    {
        foreach (var sql in ProductStockSql.EnsureStatements)
        {
            using var command = new SqlCommand(sql, _connection) { CommandTimeout = 120 };
            await command.ExecuteNonQueryAsync();
        }
    }

    public async Task<IReadOnlyList<ProductGridRow>> GetProductsAsync(int locationId, ProductSearchFilter filter)
    {
        const string sql = """
SELECT TOP (1000) p.ProductId, p.ProductCode, p.SKU, p.Barcode, p.ProductName, p.CategoryId, p.BrandId, p.UnitId, u.UnitName,
       p.CostPrice, p.MinimumCost, p.VatMode, p.VatPercentage, p.MinimumSellingPrice, p.SellingPrice, p.WholesalePrice,
       p.WholesaleMinQty, p.TaxRate, p.DiscountAllowed, p.IsStockTracked, p.MinimumStockLevel,
       ISNULL(s.CurrentStock, p.CurrentStock) CurrentStock, p.ProductImageUrl, p.Description, p.Status, p.IsActive,
       COALESCE(img.LocalImagePath, N'') LocalImagePath, COALESCE(img.ImageHash, N'') ImageHash,
       COALESCE(img.SyncStatus, N'') ImageSyncStatus, img.UploadedDate, img.UploadedByEmployeeId,
       COALESCE(p.UpdatedDate, p.CreatedDate) LastUpdatedDate
FROM dbo.Product p
JOIN dbo.ProductUnit u ON u.UnitId = p.UnitId
LEFT JOIN dbo.InventoryStock s ON s.ProductId = p.ProductId AND s.LocationId = @LocationId
OUTER APPLY (SELECT TOP (1) LocalImagePath, ImageHash, SyncStatus, UploadedDate, UploadedByEmployeeId FROM dbo.ProductImageSync WHERE ProductId = p.ProductId ORDER BY ProductImageSyncId DESC) img
WHERE (@Name IS NULL OR p.ProductName LIKE N'%' + @Name + N'%')
  AND (@Code IS NULL OR p.ProductCode LIKE N'%' + @Code + N'%')
  AND (@Barcode IS NULL OR p.Barcode LIKE N'%' + @Barcode + N'%')
  AND (@IsActive IS NULL OR p.IsActive = @IsActive)
  AND (@UnsyncedImagesOnly = 0 OR COALESCE(img.SyncStatus, N'') IN (N'Pending', N'Failed'))
ORDER BY p.ProductName;
""";
        using var command = new SqlCommand(sql, _connection);
        command.Parameters.Add("@LocationId", SqlDbType.Int).Value = locationId;
        AddNullableString(command, "@Name", filter.ProductName);
        AddNullableString(command, "@Code", filter.ProductCode);
        AddNullableString(command, "@Barcode", filter.Barcode);
        command.Parameters.Add("@IsActive", SqlDbType.Bit).Value = filter.IsActive.HasValue ? filter.IsActive.Value : DBNull.Value;
        command.Parameters.Add("@UnsyncedImagesOnly", SqlDbType.Bit).Value = filter.UnsyncedImagesOnly;
        return await ReadProductsAsync(command);
    }

    public async Task<ProductGridRow?> GetProductByIdAsync(int productId, int locationId)
    {
        var rows = await GetProductsAsync(locationId, new ProductSearchFilter());
        return rows.FirstOrDefault(x => x.ProductId == productId);
    }

    public async Task<IReadOnlyList<InventoryLocationRow>> GetLocationsAsync()
    {
        const string sql = "SELECT LocationId, LocationCode, LocationName, IsDefault, IsActive FROM dbo.InventoryLocation WHERE IsActive=1 ORDER BY IsDefault DESC, LocationName;";
        using var command = new SqlCommand(sql, _connection);
        using var reader = await command.ExecuteReaderAsync();
        var rows = new List<InventoryLocationRow>();
        while (await reader.ReadAsync())
        {
            rows.Add(new InventoryLocationRow(reader.GetInt32("LocationId"), reader.GetString("LocationCode"), reader.GetString("LocationName"), reader.GetBoolean("IsDefault"), reader.GetBoolean("IsActive")));
        }
        return rows;
    }

    public Task<IReadOnlyList<LookupRow>> GetCategoriesAsync() => GetLookupAsync("SELECT CategoryId Id, CategoryName Name FROM dbo.ProductCategory WHERE IsActive=1 ORDER BY CategoryName;");
    public Task<IReadOnlyList<LookupRow>> GetBrandsAsync() => GetLookupAsync("SELECT BrandId Id, BrandName Name FROM dbo.ProductBrand WHERE IsActive=1 ORDER BY BrandName;");
    public Task<IReadOnlyList<LookupRow>> GetUnitsAsync() => GetLookupAsync("SELECT UnitId Id, UnitName Name FROM dbo.ProductUnit WHERE IsActive=1 ORDER BY UnitName;");

    public async Task UpdateProductAsync(ProductGridRow row, string changedField, object? oldValue, int userId)
    {
        if (!string.IsNullOrWhiteSpace(row.Barcode) && await IsBarcodeDuplicateAsync(row.Barcode, row.ProductId))
        {
            throw new InvalidOperationException("บาร์โค้ดนี้ถูกใช้งานกับสินค้าอื่นแล้ว");
        }

        using var transaction = _connection.BeginTransaction();
        try
        {
            using var command = CreateProcedure("dbo.spProductUpdate", transaction);
            AddProductUpdateParameters(command, row, userId);
            await command.ExecuteNonQueryAsync();

            await InsertAuditAsync(row.ProductId, changedField, oldValue?.ToString() ?? string.Empty, CurrentValue(row, changedField), userId, transaction);
            transaction.Commit();
        }
        catch
        {
            transaction.Rollback();
            throw;
        }
    }

    public async Task<int> CreateProductAsync(ProductCreateRequest request, int locationId)
    {
        if (await IsCodeExistsAsync(request.ProductCode, null)) throw new InvalidOperationException("รหัสสินค้าซ้ำ");
        if (!string.IsNullOrWhiteSpace(request.Barcode) && await IsBarcodeDuplicateAsync(request.Barcode, null)) throw new InvalidOperationException("บาร์โค้ดนี้ถูกใช้งานกับสินค้าอื่นแล้ว");

        using var transaction = _connection.BeginTransaction();
        try
        {
            using var command = CreateProcedure("dbo.spProductCreate", transaction);
            AddProductCreateParameters(command, request);
            var productId = Convert.ToInt32(await command.ExecuteScalarAsync());

            if (request.CurrentStock > 0)
            {
                await CreateMovementAsync(productId, locationId, "AdjustmentIn", request.CurrentStock, request.CostPrice, "CreateProduct", null, "CREATE-PRODUCT", "สร้างสินค้าใหม่", "เพิ่มสต็อกเริ่มต้น", request.CreatedByUserId, transaction);
                await SyncProductCurrentStockAsync(productId, request.CreatedByUserId, transaction);
            }
            await InsertAuditAsync(productId, "Create", "", "สร้างสินค้าใหม่", request.CreatedByUserId, transaction);
            transaction.Commit();
            return productId;
        }
        catch
        {
            transaction.Rollback();
            throw;
        }
    }

    public async Task<string> GenerateNextProductCodeAsync()
    {
        for (var i = 1; i <= 99999; i++)
        {
            var code = $"PRO-{i:00}";
            if (!await IsCodeExistsAsync(code, null)) return code;
        }
        throw new InvalidOperationException("ไม่สามารถสร้างรหัสสินค้าใหม่ได้");
    }

    public Task<bool> IsBarcodeDuplicateAsync(string barcode, int? excludedProductId) => ScalarBoolProcedureAsync("dbo.spProductCheckBarcodeExists", ("@Barcode", barcode), ("@ExcludeProductId", excludedProductId));
    public Task<bool> IsCodeExistsAsync(string code, int? excludedProductId) => ScalarBoolProcedureAsync("dbo.spProductCheckCodeExists", ("@ProductCode", code), ("@ExcludeProductId", excludedProductId));

    public async Task<ProductGridRow?> FindByCodeOrBarcodeAsync(string productCode, string barcode, int locationId)
    {
        const string sql = """
SELECT TOP (1) p.ProductId, p.ProductCode, p.SKU, p.Barcode, p.ProductName, p.CategoryId, p.BrandId, p.UnitId, u.UnitName,
       p.CostPrice, p.MinimumCost, p.VatMode, p.VatPercentage, p.MinimumSellingPrice, p.SellingPrice, p.WholesalePrice,
       p.WholesaleMinQty, p.TaxRate, p.DiscountAllowed, p.IsStockTracked, p.MinimumStockLevel,
       ISNULL(s.CurrentStock, p.CurrentStock) CurrentStock, p.ProductImageUrl, p.Description, p.Status, p.IsActive,
       COALESCE(img.LocalImagePath, N'') LocalImagePath, COALESCE(img.ImageHash, N'') ImageHash,
       COALESCE(img.SyncStatus, N'') ImageSyncStatus, img.UploadedDate, img.UploadedByEmployeeId,
       COALESCE(p.UpdatedDate, p.CreatedDate) LastUpdatedDate
FROM dbo.Product p JOIN dbo.ProductUnit u ON u.UnitId=p.UnitId
LEFT JOIN dbo.InventoryStock s ON s.ProductId=p.ProductId AND s.LocationId=@LocationId
OUTER APPLY (SELECT TOP (1) LocalImagePath, ImageHash, SyncStatus, UploadedDate, UploadedByEmployeeId FROM dbo.ProductImageSync WHERE ProductId = p.ProductId ORDER BY ProductImageSyncId DESC) img
WHERE (@ProductCode <> N'' AND p.ProductCode=@ProductCode) OR (@Barcode <> N'' AND p.Barcode=@Barcode)
ORDER BY CASE WHEN p.ProductCode=@ProductCode THEN 0 ELSE 1 END;
""";
        using var command = new SqlCommand(sql, _connection);
        command.Parameters.Add("@LocationId", SqlDbType.Int).Value = locationId;
        command.Parameters.Add("@ProductCode", SqlDbType.NVarChar, 50).Value = productCode;
        command.Parameters.Add("@Barcode", SqlDbType.NVarChar, 100).Value = barcode;
        return (await ReadProductsAsync(command)).FirstOrDefault();
    }

    public async Task ApplyStockImportAsync(StockImportPreview preview, int locationId, int userId, string filename)
    {
        var ready = preview.Rows.Where(x => x.Status == ImportRowStatus.Ready && x.NewStock.HasValue && x.Difference != 0).ToArray();
        using var transaction = _connection.BeginTransaction();
        try
        {
            var fileHash = Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(Encoding.UTF8.GetBytes(filename + "|" + string.Join("|", ready.Select(x => $"{x.ProductCode}:{x.NewStock}")))));
            if (await ImportHashExistsAsync(fileHash, transaction)) throw new InvalidOperationException("ไฟล์นี้เคยถูกนำเข้าแล้ว");

            var batchId = await CreateImportBatchAsync(filename, fileHash, userId, preview.TotalRows, ready.Length, preview.SkippedRows, preview.ErrorRows, transaction);
            foreach (var row in ready)
            {
                var movementType = row.Difference > 0 ? "AdjustmentIn" : "AdjustmentOut";
                var movementId = await CreateMovementAsync(row.ProductId, locationId, movementType, Math.Abs(row.Difference), row.CostPrice, "StockCountImport", batchId, $"SCI-{batchId}", "ปรับสต็อกจากการนำเข้า Excel สำหรับตรวจนับสต็อก", $"Old={row.CurrentStock:N4}; New={row.NewStock:N4}; File={filename}", userId, transaction);
                await InsertImportItemAsync(batchId, row, movementId, transaction);
                await SyncProductCurrentStockAsync(row.ProductId, userId, transaction);
            }
            await CompleteImportBatchAsync(batchId, "Completed", transaction);
            transaction.Commit();
        }
        catch
        {
            transaction.Rollback();
            throw;
        }
    }

    public async Task<IReadOnlyList<ImportHistoryRow>> GetImportHistoryAsync()
    {
        const string sql = "SELECT TOP (100) ImportBatchId, Filename, ImportedByEmployeeId, ImportedDate, TotalRows, UpdatedRows, SkippedRows, ErrorRows, Status, Remark FROM dbo.ProductStockImportBatch ORDER BY ImportedDate DESC;";
        using var command = new SqlCommand(sql, _connection);
        using var reader = await command.ExecuteReaderAsync();
        var rows = new List<ImportHistoryRow>();
        while (await reader.ReadAsync())
        {
            rows.Add(new ImportHistoryRow(reader.GetInt64("ImportBatchId"), reader.GetString("Filename"), reader.GetInt32("ImportedByEmployeeId"), reader.GetDateTime("ImportedDate"), reader.GetInt32("TotalRows"), reader.GetInt32("UpdatedRows"), reader.GetInt32("SkippedRows"), reader.GetInt32("ErrorRows"), reader.GetString("Status"), reader.GetString("Remark")));
        }
        return rows;
    }

    public async Task UpsertProductImageAsync(int productId, string localPath, string imageHash, int userId, string syncStatus, SqlTransaction? transaction)
    {
        const string sql = """
INSERT dbo.ProductImageSync(ProductId, LocalImagePath, ImageHash, SyncStatus, CreatedByEmployeeId, LastError)
VALUES(@ProductId, @LocalImagePath, @ImageHash, @SyncStatus, NULLIF(@UserId,0), N'');
""";
        using var command = transaction is null ? new SqlCommand(sql, _connection) : new SqlCommand(sql, _connection, transaction);
        command.Parameters.Add("@ProductId", SqlDbType.Int).Value = productId;
        command.Parameters.Add("@LocalImagePath", SqlDbType.NVarChar, 500).Value = localPath;
        command.Parameters.Add("@ImageHash", SqlDbType.NVarChar, 128).Value = imageHash;
        command.Parameters.Add("@SyncStatus", SqlDbType.NVarChar, 30).Value = syncStatus;
        command.Parameters.Add("@UserId", SqlDbType.Int).Value = userId;
        await command.ExecuteNonQueryAsync();
    }

    public async Task<IReadOnlyList<ProductImageSyncRow>> GetPendingImagesAsync()
    {
        const string sql = """
SELECT ProductImageSyncId, ProductId, LocalImagePath, ImageHash, SyncStatus
FROM dbo.ProductImageSync
WHERE SyncStatus IN (N'Pending', N'Failed')
ORDER BY ProductImageSyncId;
""";
        using var command = new SqlCommand(sql, _connection);
        using var reader = await command.ExecuteReaderAsync();
        var rows = new List<ProductImageSyncRow>();
        while (await reader.ReadAsync())
        {
            rows.Add(new ProductImageSyncRow(reader.GetInt64("ProductImageSyncId"), reader.GetInt32("ProductId"), reader.GetString("LocalImagePath"), reader.GetString("ImageHash"), reader.GetString("SyncStatus")));
        }
        return rows;
    }

    public async Task<bool> IsProductImageHashSyncedAsync(int productId, string imageHash)
    {
        const string sql = "SELECT CAST(CASE WHEN EXISTS(SELECT 1 FROM dbo.ProductImageSync WHERE ProductId=@ProductId AND ImageHash=@ImageHash AND SyncStatus=N'Synced') THEN 1 ELSE 0 END AS BIT);";
        using var command = new SqlCommand(sql, _connection);
        command.Parameters.Add("@ProductId", SqlDbType.Int).Value = productId;
        command.Parameters.Add("@ImageHash", SqlDbType.NVarChar, 128).Value = imageHash;
        return Convert.ToBoolean(await command.ExecuteScalarAsync());
    }

    public async Task SyncProductImageAsync(ProductImageSyncRow image, int userId)
    {
        using var transaction = _connection.BeginTransaction();
        try
        {
            using var command = CreateProcedure("dbo.spProductUpdateImage", transaction);
            command.Parameters.Add("@ProductId", SqlDbType.Int).Value = image.ProductId;
            command.Parameters.Add("@ProductImageUrl", SqlDbType.NVarChar, 500).Value = image.LocalImagePath;
            command.Parameters.Add("@UpdatedByUserId", SqlDbType.Int).Value = userId;
            await command.ExecuteNonQueryAsync();
            await MarkImageSyncedAsync(image.ProductImageSyncId, userId, image.LocalImagePath, transaction);
            transaction.Commit();
        }
        catch
        {
            transaction.Rollback();
            throw;
        }
    }

    public Task MarkImageSyncedAsync(long syncId, int userId, string uploadedPath) => MarkImageSyncedAsync(syncId, userId, uploadedPath, null);

    private async Task MarkImageSyncedAsync(long syncId, int userId, string uploadedPath, SqlTransaction? transaction)
    {
        const string sql = """
UPDATE dbo.ProductImageSync
SET SyncStatus=N'Synced', UploadedPath=@UploadedPath, UploadedDate=SYSUTCDATETIME(), UploadedByEmployeeId=NULLIF(@UserId,0), LastError=N''
WHERE ProductImageSyncId=@Id;
""";
        using var command = transaction is null ? new SqlCommand(sql, _connection) : new SqlCommand(sql, _connection, transaction);
        command.Parameters.Add("@Id", SqlDbType.BigInt).Value = syncId;
        command.Parameters.Add("@UploadedPath", SqlDbType.NVarChar, 500).Value = uploadedPath;
        command.Parameters.Add("@UserId", SqlDbType.Int).Value = userId;
        await command.ExecuteNonQueryAsync();
    }

    public async Task MarkImageSyncFailedAsync(long syncId, string message)
    {
        const string sql = "UPDATE dbo.ProductImageSync SET SyncStatus=N'Failed', LastError=@Message WHERE ProductImageSyncId=@Id;";
        using var command = new SqlCommand(sql, _connection);
        command.Parameters.Add("@Id", SqlDbType.BigInt).Value = syncId;
        command.Parameters.Add("@Message", SqlDbType.NVarChar, 500).Value = message;
        await command.ExecuteNonQueryAsync();
    }

    private async Task<IReadOnlyList<ProductGridRow>> ReadProductsAsync(SqlCommand command)
    {
        using var reader = await command.ExecuteReaderAsync();
        var rows = new List<ProductGridRow>();
        while (await reader.ReadAsync())
        {
            rows.Add(new ProductGridRow
            {
                ProductId = reader.GetInt32("ProductId"),
                ProductCode = reader.GetString("ProductCode"),
                SKU = reader.GetString("SKU"),
                Barcode = reader.IsDBNull("Barcode") ? string.Empty : reader.GetString("Barcode"),
                ProductName = reader.GetString("ProductName"),
                CategoryId = reader.GetInt32("CategoryId"),
                BrandId = reader.IsDBNull("BrandId") ? null : reader.GetInt32("BrandId"),
                UnitId = reader.GetInt32("UnitId"),
                UnitName = reader.GetString("UnitName"),
                CostPrice = reader.GetDecimal("CostPrice"),
                MinimumCost = reader.GetDecimal("MinimumCost"),
                VatMode = reader.GetString("VatMode"),
                VatPercentage = reader.GetDecimal("VatPercentage"),
                MinimumSellingPrice = reader.GetDecimal("MinimumSellingPrice"),
                SellingPrice = reader.GetDecimal("SellingPrice"),
                WholesalePrice = reader.GetDecimal("WholesalePrice"),
                WholesaleMinQty = reader.GetDecimal("WholesaleMinQty"),
                TaxRate = reader.GetDecimal("TaxRate"),
                DiscountAllowed = reader.GetBoolean("DiscountAllowed"),
                IsStockTracked = reader.GetBoolean("IsStockTracked"),
                MinimumStockLevel = reader.GetDecimal("MinimumStockLevel"),
                CurrentStock = reader.GetDecimal("CurrentStock"),
                ProductImageUrl = reader.GetString("ProductImageUrl"),
                LocalImagePath = reader.GetString("LocalImagePath"),
                ImageHash = reader.GetString("ImageHash"),
                ImageSyncStatus = reader.GetString("ImageSyncStatus"),
                UploadedDate = reader.IsDBNull("UploadedDate") ? null : reader.GetDateTime("UploadedDate"),
                UploadedByEmployeeId = reader.IsDBNull("UploadedByEmployeeId") ? null : reader.GetInt32("UploadedByEmployeeId"),
                Description = reader.GetString("Description"),
                Status = reader.GetString("Status"),
                IsActive = reader.GetBoolean("IsActive"),
                LastUpdatedDateValue = reader.GetDateTime("LastUpdatedDate")
            });
        }
        return rows;
    }

    private async Task<IReadOnlyList<LookupRow>> GetLookupAsync(string sql)
    {
        using var command = new SqlCommand(sql, _connection);
        using var reader = await command.ExecuteReaderAsync();
        var rows = new List<LookupRow>();
        while (await reader.ReadAsync()) rows.Add(new LookupRow(reader.GetInt32("Id"), reader.GetString("Name")));
        return rows;
    }

    private async Task<bool> ScalarBoolProcedureAsync(string procedure, params (string Name, object? Value)[] parameters)
    {
        using var command = new SqlCommand(procedure, _connection) { CommandType = CommandType.StoredProcedure };
        foreach (var p in parameters) command.Parameters.AddWithValue(p.Name, p.Value ?? DBNull.Value);
        return Convert.ToBoolean(await command.ExecuteScalarAsync());
    }

    private async Task<long> CreateMovementAsync(int productId, int locationId, string movementType, decimal quantity, decimal unitCost, string referenceType, long? referenceId, string referenceNo, string reason, string remarks, int userId, SqlTransaction transaction)
    {
        using var command = CreateProcedure("dbo.spInventoryMovementCreate", transaction);
        command.Parameters.Add("@ProductId", SqlDbType.Int).Value = productId;
        command.Parameters.Add("@LocationId", SqlDbType.Int).Value = locationId;
        command.Parameters.Add("@MovementType", SqlDbType.NVarChar, 30).Value = movementType;
        AddDecimal(command, "@Quantity", quantity);
        AddDecimal(command, "@UnitCost", unitCost);
        command.Parameters.Add("@ReferenceType", SqlDbType.NVarChar, 50).Value = referenceType;
        command.Parameters.Add("@ReferenceId", SqlDbType.BigInt).Value = referenceId.HasValue ? referenceId.Value : DBNull.Value;
        command.Parameters.Add("@ReferenceNo", SqlDbType.NVarChar, 100).Value = referenceNo;
        command.Parameters.Add("@Reason", SqlDbType.NVarChar, 500).Value = reason;
        command.Parameters.Add("@Remarks", SqlDbType.NVarChar, 1000).Value = remarks;
        command.Parameters.Add("@AllowNegativeStock", SqlDbType.Bit).Value = false;
        command.Parameters.Add("@CreatedByUserId", SqlDbType.Int).Value = userId;
        return Convert.ToInt64(await command.ExecuteScalarAsync());
    }

    private async Task SyncProductCurrentStockAsync(int productId, int userId, SqlTransaction transaction)
    {
        const string sql = """
UPDATE p
SET CurrentStock = ISNULL(s.TotalStock, 0), UpdatedByUserId = NULLIF(@UserId,0), UpdatedDate = SYSUTCDATETIME()
FROM dbo.Product p
OUTER APPLY (SELECT SUM(CurrentStock) TotalStock FROM dbo.InventoryStock WHERE ProductId=p.ProductId) s
WHERE p.ProductId=@ProductId;
""";
        using var command = new SqlCommand(sql, _connection, transaction);
        command.Parameters.Add("@ProductId", SqlDbType.Int).Value = productId;
        command.Parameters.Add("@UserId", SqlDbType.Int).Value = userId;
        await command.ExecuteNonQueryAsync();
    }

    private async Task InsertAuditAsync(int productId, string fieldName, string oldValue, string newValue, int userId, SqlTransaction transaction)
    {
        const string sql = "INSERT dbo.ProductBulkUpdateAudit(ProductId, FieldName, OldValue, NewValue, UpdatedByEmployeeId) VALUES(@ProductId,@FieldName,@OldValue,@NewValue,NULLIF(@UserId,0));";
        using var command = new SqlCommand(sql, _connection, transaction);
        command.Parameters.Add("@ProductId", SqlDbType.Int).Value = productId;
        command.Parameters.Add("@FieldName", SqlDbType.NVarChar, 100).Value = fieldName;
        command.Parameters.Add("@OldValue", SqlDbType.NVarChar, -1).Value = oldValue;
        command.Parameters.Add("@NewValue", SqlDbType.NVarChar, -1).Value = newValue;
        command.Parameters.Add("@UserId", SqlDbType.Int).Value = userId;
        await command.ExecuteNonQueryAsync();
    }

    private async Task<bool> ImportHashExistsAsync(string fileHash, SqlTransaction transaction)
    {
        using var command = new SqlCommand("SELECT CAST(CASE WHEN EXISTS(SELECT 1 FROM dbo.ProductStockImportBatch WHERE FileHash=@FileHash AND Status=N'Completed') THEN 1 ELSE 0 END AS BIT);", _connection, transaction);
        command.Parameters.Add("@FileHash", SqlDbType.NVarChar, 128).Value = fileHash;
        return Convert.ToBoolean(await command.ExecuteScalarAsync());
    }

    private async Task<long> CreateImportBatchAsync(string filename, string fileHash, int userId, int total, int updated, int skipped, int errors, SqlTransaction transaction)
    {
        const string sql = "INSERT dbo.ProductStockImportBatch(Filename,FileHash,ImportedByEmployeeId,TotalRows,UpdatedRows,SkippedRows,ErrorRows,Status,Remark) OUTPUT INSERTED.ImportBatchId VALUES(@Filename,@FileHash,@UserId,@Total,@Updated,@Skipped,@Errors,N'Processing',N'Excel Stock Count Import');";
        using var command = new SqlCommand(sql, _connection, transaction);
        command.Parameters.Add("@Filename", SqlDbType.NVarChar, 260).Value = filename;
        command.Parameters.Add("@FileHash", SqlDbType.NVarChar, 128).Value = fileHash;
        command.Parameters.Add("@UserId", SqlDbType.Int).Value = userId;
        command.Parameters.Add("@Total", SqlDbType.Int).Value = total;
        command.Parameters.Add("@Updated", SqlDbType.Int).Value = updated;
        command.Parameters.Add("@Skipped", SqlDbType.Int).Value = skipped;
        command.Parameters.Add("@Errors", SqlDbType.Int).Value = errors;
        return Convert.ToInt64(await command.ExecuteScalarAsync());
    }

    private async Task CompleteImportBatchAsync(long batchId, string status, SqlTransaction transaction)
    {
        using var command = new SqlCommand("UPDATE dbo.ProductStockImportBatch SET Status=@Status WHERE ImportBatchId=@BatchId;", _connection, transaction);
        command.Parameters.Add("@Status", SqlDbType.NVarChar, 30).Value = status;
        command.Parameters.Add("@BatchId", SqlDbType.BigInt).Value = batchId;
        await command.ExecuteNonQueryAsync();
    }

    private async Task InsertImportItemAsync(long batchId, StockImportPreviewRow row, long movementId, SqlTransaction transaction)
    {
        const string sql = "INSERT dbo.ProductStockImportItem(ImportBatchId,ProductId,ProductCode,Barcode,OldStock,NewStock,Variance,InventoryMovementId,Status,Message) VALUES(@BatchId,@ProductId,@ProductCode,@Barcode,@Old,@New,@Variance,@MovementId,N'Updated',@Message);";
        using var command = new SqlCommand(sql, _connection, transaction);
        command.Parameters.Add("@BatchId", SqlDbType.BigInt).Value = batchId;
        command.Parameters.Add("@ProductId", SqlDbType.Int).Value = row.ProductId;
        command.Parameters.Add("@ProductCode", SqlDbType.NVarChar, 50).Value = row.ProductCode;
        command.Parameters.Add("@Barcode", SqlDbType.NVarChar, 100).Value = row.Barcode;
        AddDecimal(command, "@Old", row.CurrentStock);
        AddDecimal(command, "@New", row.NewStock ?? 0);
        AddDecimal(command, "@Variance", row.Difference);
        command.Parameters.Add("@MovementId", SqlDbType.BigInt).Value = movementId;
        command.Parameters.Add("@Message", SqlDbType.NVarChar, 500).Value = row.Message;
        await command.ExecuteNonQueryAsync();
    }

    private SqlCommand CreateProcedure(string name, SqlTransaction transaction) => new(name, _connection, transaction) { CommandType = CommandType.StoredProcedure, CommandTimeout = 120 };

    private static void AddProductUpdateParameters(SqlCommand command, ProductGridRow row, int userId)
    {
        command.Parameters.Add("@ProductId", SqlDbType.Int).Value = row.ProductId;
        AddCommonProductParameters(command, row.ProductCode, row.SKU, row.Barcode, row.ProductName, row.CategoryId, row.BrandId, row.UnitId, row.CostPrice, row.MinimumCost, row.VatMode, row.VatPercentage, row.SellingPrice, row.WholesalePrice, row.WholesaleMinQty, row.TaxRate, row.DiscountAllowed, row.IsStockTracked, row.MinimumStockLevel, row.CurrentStock, row.ProductImageUrl, row.Description, row.IsActive ? "Active" : "Inactive");
        command.Parameters.Add("@IsActive", SqlDbType.Bit).Value = row.IsActive;
        command.Parameters.Add("@UpdatedByUserId", SqlDbType.Int).Value = userId;
    }

    private static void AddProductCreateParameters(SqlCommand command, ProductCreateRequest row)
    {
        AddCommonProductParameters(command, row.ProductCode, row.SKU, row.Barcode, row.ProductName, row.CategoryId, row.BrandId, row.UnitId, row.CostPrice, row.CostPrice, "VatExcluded", 0, row.SellingPrice, row.WholesalePrice, row.WholesaleMinQty, 0, true, true, 0, 0, "", row.Description, row.IsActive ? "Active" : "Inactive");
        command.Parameters.Add("@CreatedByUserId", SqlDbType.Int).Value = row.CreatedByUserId;
    }

    private static void AddCommonProductParameters(SqlCommand command, string productCode, string sku, string barcode, string productName, int categoryId, int? brandId, int unitId, decimal costPrice, decimal minimumCost, string vatMode, decimal vatPercentage, decimal sellingPrice, decimal wholesalePrice, decimal wholesaleMinQty, decimal taxRate, bool discountAllowed, bool isStockTracked, decimal minimumStockLevel, decimal currentStock, string productImageUrl, string description, string status)
    {
        command.Parameters.Add("@ProductCode", SqlDbType.NVarChar, 50).Value = productCode;
        command.Parameters.Add("@SKU", SqlDbType.NVarChar, 100).Value = sku;
        command.Parameters.Add("@Barcode", SqlDbType.NVarChar, 100).Value = string.IsNullOrWhiteSpace(barcode) ? DBNull.Value : barcode.Trim();
        command.Parameters.Add("@ProductName", SqlDbType.NVarChar, 200).Value = productName.Trim();
        command.Parameters.Add("@CategoryId", SqlDbType.Int).Value = categoryId;
        command.Parameters.Add("@BrandId", SqlDbType.Int).Value = brandId.HasValue ? brandId.Value : DBNull.Value;
        command.Parameters.Add("@UnitId", SqlDbType.Int).Value = unitId;
        AddDecimal(command, "@CostPrice", costPrice);
        AddDecimal(command, "@MinimumCost", minimumCost);
        command.Parameters.Add("@VatMode", SqlDbType.NVarChar, 20).Value = vatMode;
        AddDecimal(command, "@VatPercentage", vatPercentage);
        AddDecimal(command, "@VatAmount", 0);
        AddDecimal(command, "@MinimumSellingPrice", minimumCost);
        AddDecimal(command, "@SellingPrice", sellingPrice);
        AddDecimal(command, "@WholesalePrice", wholesalePrice);
        AddDecimal(command, "@WholesaleMinQty", wholesaleMinQty);
        AddDecimal(command, "@TaxRate", taxRate);
        command.Parameters.Add("@DiscountAllowed", SqlDbType.Bit).Value = discountAllowed;
        command.Parameters.Add("@IsStockTracked", SqlDbType.Bit).Value = isStockTracked;
        AddDecimal(command, "@MinimumStockLevel", minimumStockLevel);
        AddDecimal(command, "@CurrentStock", currentStock);
        command.Parameters.Add("@ProductImageUrl", SqlDbType.NVarChar, 500).Value = productImageUrl;
        command.Parameters.Add("@Description", SqlDbType.NVarChar, 1000).Value = description;
        command.Parameters.Add("@Status", SqlDbType.NVarChar, 30).Value = status;
    }

    private static string CurrentValue(ProductGridRow row, string property) => property switch
    {
        nameof(ProductGridRow.ProductName) => row.ProductName,
        nameof(ProductGridRow.Barcode) => row.Barcode,
        nameof(ProductGridRow.CostPrice) => row.CostPrice.ToString("N4"),
        nameof(ProductGridRow.SellingPrice) => row.SellingPrice.ToString("N4"),
        nameof(ProductGridRow.WholesalePrice) => row.WholesalePrice.ToString("N4"),
        nameof(ProductGridRow.UnitId) => row.UnitId.ToString(),
        nameof(ProductGridRow.IsActive) => row.IsActive.ToString(),
        _ => ""
    };

    private static void AddNullableString(SqlCommand command, string name, string? value) => command.Parameters.Add(name, SqlDbType.NVarChar, 200).Value = string.IsNullOrWhiteSpace(value) ? DBNull.Value : value.Trim();

    private static void AddDecimal(SqlCommand command, string name, decimal value)
    {
        var parameter = command.Parameters.Add(name, SqlDbType.Decimal);
        parameter.Precision = 18;
        parameter.Scale = 4;
        parameter.Value = value;
    }
}

public sealed class OfflineProductStore
{
    private readonly string _path = Path.Combine(AppContext.BaseDirectory, "OfflineData", "product-cache.json");
    private static readonly JsonSerializerOptions Options = new() { WriteIndented = true };

    public OfflineProductCache Load()
    {
        try
        {
            if (!File.Exists(_path)) return OfflineProductCache.Empty;
            return JsonSerializer.Deserialize<OfflineProductCache>(File.ReadAllText(_path), Options) ?? OfflineProductCache.Empty;
        }
        catch
        {
            return OfflineProductCache.Empty;
        }
    }

    public void Save(OfflineProductCache cache)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(_path)!);
        File.WriteAllText(_path, JsonSerializer.Serialize(cache, Options), Encoding.UTF8);
    }

    public void UpsertProduct(ProductGridRow product)
    {
        var cache = Load();
        var products = cache.Products.ToList();
        var index = products.FindIndex(x => x.ProductId == product.ProductId || x.ProductCode.Equals(product.ProductCode, StringComparison.OrdinalIgnoreCase));
        if (index >= 0) products[index] = product;
        else products.Add(product);
        Save(cache with { Products = products });
    }
}

public sealed record OfflineProductCache(
    IReadOnlyList<InventoryLocationRow> Locations,
    IReadOnlyList<LookupRow> Categories,
    IReadOnlyList<LookupRow> Brands,
    IReadOnlyList<LookupRow> Units,
    IReadOnlyList<ProductGridRow> Products)
{
    public static OfflineProductCache Empty { get; } = new([], [], [], [], []);
}

public sealed class BarcodeService(ProductStockRepository repository)
{
    public async Task<string> GenerateUniqueBarcodeAsync(CancellationToken cancellationToken = default)
    {
        for (var attempt = 0; attempt < 1000; attempt++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var seed = DateTime.UtcNow.ToString("yyMMddHHmmss") + Random.Shared.Next(0, 9);
            var twelve = ("20" + seed)[..12];
            var barcode = twelve + CalculateEan13CheckDigit(twelve);
            if (!await repository.IsBarcodeDuplicateAsync(barcode, null)) return barcode;
        }
        throw new InvalidOperationException("ไม่สามารถสร้างบาร์โค้ดใหม่ได้");
    }

    public bool IsValidBarcode(string barcode) => barcode.All(char.IsDigit) && barcode.Length == 13 && barcode[^1] == CalculateEan13CheckDigit(barcode[..12]);
    public Task<bool> IsBarcodeDuplicateAsync(string barcode, int? excludedProductId = null, CancellationToken cancellationToken = default) => repository.IsBarcodeDuplicateAsync(barcode, excludedProductId);

    private static char CalculateEan13CheckDigit(string firstTwelveDigits)
    {
        var sum = 0;
        for (var i = 0; i < 12; i++) sum += (firstTwelveDigits[i] - '0') * (i % 2 == 0 ? 1 : 3);
        return (char)('0' + ((10 - (sum % 10)) % 10));
    }
}

public static class ProductImageStore
{
    private const int MaxPixels = 900;

    public static ProductImageSaveResult SaveProductImage(string sourcePath, string productCode)
    {
        var safeCode = string.Concat(productCode.Select(ch => char.IsLetterOrDigit(ch) || ch == '-' || ch == '_' ? ch : '_'));
        var dir = Path.Combine(AppContext.BaseDirectory, "ProductImages", safeCode);
        Directory.CreateDirectory(dir);
        var fileName = $"{DateTime.Now:yyyyMMddHHmmssfff}.jpg";
        var target = Path.Combine(dir, fileName);

        using var source = Image.FromFile(sourcePath);
        var scale = Math.Min(1m, (decimal)MaxPixels / Math.Max(source.Width, source.Height));
        var width = Math.Max(1, (int)Math.Round(source.Width * scale));
        var height = Math.Max(1, (int)Math.Round(source.Height * scale));
        using var bitmap = new Bitmap(width, height);
        using (var graphics = Graphics.FromImage(bitmap))
        {
            graphics.Clear(Color.White);
            graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
            graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
            graphics.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.HighQuality;
            graphics.DrawImage(source, 0, 0, width, height);
        }

        var encoder = ImageCodecInfo.GetImageEncoders().First(codec => codec.MimeType == "image/jpeg");
        using var parameters = new EncoderParameters(1);
        parameters.Param[0] = new EncoderParameter(System.Drawing.Imaging.Encoder.Quality, 82L);
        bitmap.Save(target, encoder, parameters);
        var hash = Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(target)));
        return new ProductImageSaveResult(target, hash);
    }

    public static Image? LoadThumbnail(string path)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(path) || !File.Exists(path)) return null;
            using var source = Image.FromFile(path);
            var thumb = new Bitmap(56, 56);
            using var graphics = Graphics.FromImage(thumb);
            graphics.Clear(Color.White);
            graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
            var scale = Math.Min(56m / source.Width, 56m / source.Height);
            var width = (int)(source.Width * scale);
            var height = (int)(source.Height * scale);
            var x = (56 - width) / 2;
            var y = (56 - height) / 2;
            graphics.DrawImage(source, x, y, width, height);
            return thumb;
        }
        catch
        {
            return null;
        }
    }
}

public sealed class BarcodePrintService
{
    public void PrintBarcode(ProductBarcodePrintModel model, bool preview)
    {
        using var document = new PrintDocument();
        document.PrinterSettings.PrinterName = model.PrinterName;
        document.PrintPage += (_, e) =>
        {
            if (e.Graphics is not null) DrawLabel(e.Graphics, model);
        };
        if (preview)
        {
            using var dialog = new PrintPreviewDialog { Document = document, Width = 900, Height = 650 };
            dialog.ShowDialog();
        }
        else
        {
            for (var i = 0; i < model.Quantity; i++) document.Print();
        }
    }

    private static void DrawLabel(Graphics g, ProductBarcodePrintModel model)
    {
        g.Clear(Color.White);
        using var title = new Font("Segoe UI", 10, FontStyle.Bold);
        using var normal = new Font("Segoe UI", 9);
        var y = 10;
        if (!string.IsNullOrWhiteSpace(model.StoreName))
        {
            g.DrawString(model.StoreName, normal, Brushes.Black, 12, y);
            y += 18;
        }
        g.DrawString(model.ProductName, title, Brushes.Black, 12, y);
        y += 24;
        DrawCode128(g, model.Barcode, 12, y, 260, 70);
        y += 74;
        g.DrawString(model.Barcode, normal, Brushes.Black, 72, y);
        y += 18;
        g.DrawString($"ราคา {model.SellingPrice:N2} บาท", title, Brushes.Black, 12, y);
    }

    private static void DrawCode128(Graphics g, string text, int x, int y, int width, int height)
    {
        var bits = Code128.Encode(text);
        var module = Math.Max(1, width / bits.Length);
        var drawX = x;
        foreach (var bit in bits)
        {
            if (bit == '1') g.FillRectangle(Brushes.Black, drawX, y, module, height);
            drawX += module;
        }
    }
}

public static class ExcelStockCountService
{
    private static readonly string[] Headers = ["รหัสสินค้า", "ชื่อสินค้า", "บาร์โค้ด", "ราคาทุน", "ราคาขายปลีก", "ราคาขายส่ง", "สต็อกปัจจุบัน", "สต็อกใหม่", "หมายเหตุ"];

    public static void ExportTemplate(string path, IReadOnlyList<ProductGridRow> products, DateTime exportDate, int employeeId)
    {
        using var workbook = new XLWorkbook();
        var sheet = workbook.Worksheets.Add("ตรวจนับสต็อก");
        sheet.Cell(1, 1).Value = $"ไฟล์ตรวจนับสต็อก ส่งออกเมื่อ {exportDate:yyyy-MM-dd HH:mm} โดยพนักงาน #{employeeId}";
        sheet.Range(1, 1, 1, Headers.Length).Merge().Style.Font.Bold = true;
        for (var i = 0; i < Headers.Length; i++)
        {
            sheet.Cell(3, i + 1).Value = Headers[i];
            sheet.Cell(3, i + 1).Style.Fill.BackgroundColor = XLColor.FromArgb(31, 78, 121);
            sheet.Cell(3, i + 1).Style.Font.FontColor = XLColor.White;
            sheet.Cell(3, i + 1).Style.Font.Bold = true;
        }

        for (var i = 0; i < products.Count; i++)
        {
            var p = products[i];
            var row = i + 4;
            sheet.Cell(row, 1).Value = p.ProductCode;
            sheet.Cell(row, 2).Value = p.ProductName;
            sheet.Cell(row, 3).SetValue(p.Barcode);
            sheet.Cell(row, 3).Style.NumberFormat.Format = "@";
            sheet.Cell(row, 4).Value = p.CostPrice;
            sheet.Cell(row, 5).Value = p.SellingPrice;
            sheet.Cell(row, 6).Value = p.WholesalePrice;
            sheet.Cell(row, 7).Value = p.CurrentStock;
            sheet.Cell(row, 8).Value = string.Empty;
            sheet.Cell(row, 9).Value = string.Empty;
        }

        sheet.Cell(products.Count + 6, 1).Value = "หมายเหตุ: กรุณากรอกจำนวนที่ตรวจนับจริงในคอลัมน์ สต็อกใหม่ แล้วนำไฟล์กลับมา Import";
        sheet.Range(3, 1, Math.Max(4, products.Count + 3), Headers.Length).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
        sheet.Range(3, 1, Math.Max(4, products.Count + 3), Headers.Length).Style.Border.InsideBorder = XLBorderStyleValues.Thin;
        sheet.Columns().AdjustToContents();
        sheet.Column(8).Width = 14;
        sheet.SheetView.FreezeRows(3);
        workbook.SaveAs(path);
    }

    public static async Task<StockImportPreview> ImportPreviewAsync(string path, ProductStockRepository repository, int locationId)
    {
        using var workbook = new XLWorkbook(path);
        var sheet = workbook.Worksheets.FirstOrDefault(x => x.Name == "ตรวจนับสต็อก") ?? workbook.Worksheets.First();
        var headerRow = 3;
        var map = new Dictionary<string, int>();
        for (var c = 1; c <= sheet.LastColumnUsed().ColumnNumber(); c++) map[sheet.Cell(headerRow, c).GetString().Trim()] = c;
        if (!map.ContainsKey("รหัสสินค้า") || !map.ContainsKey("บาร์โค้ด") || !map.ContainsKey("สต็อกใหม่"))
        {
            throw new InvalidOperationException("ไฟล์ Excel ไม่ตรงกับรูปแบบที่ระบบกำหนด กรุณาใช้ไฟล์ Template ที่ส่งออกจากระบบ");
        }

        var preview = new StockImportPreview(Path.GetFileName(path));
        for (var r = headerRow + 1; r <= sheet.LastRowUsed().RowNumber(); r++)
        {
            var productCode = sheet.Cell(r, map["รหัสสินค้า"]).GetString().Trim();
            var barcode = sheet.Cell(r, map["บาร์โค้ด"]).GetString().Trim();
            var newStockText = sheet.Cell(r, map["สต็อกใหม่"]).GetString().Trim();
            if (string.IsNullOrWhiteSpace(productCode) && string.IsNullOrWhiteSpace(barcode)) continue;
            if (string.IsNullOrWhiteSpace(newStockText))
            {
                preview.Rows.Add(StockImportPreviewRow.Skipped(productCode, barcode, "ข้ามรายการ: ไม่ได้กรอกสต็อกใหม่"));
                continue;
            }
            if (!decimal.TryParse(newStockText, out var newStock) || newStock < 0)
            {
                preview.Rows.Add(StockImportPreviewRow.Error(productCode, barcode, "จำนวนสต็อกใหม่ต้องเป็นตัวเลขและต้องไม่ต่ำกว่า 0"));
                continue;
            }
            var product = await repository.FindByCodeOrBarcodeAsync(productCode, barcode, locationId);
            if (product is null)
            {
                preview.Rows.Add(StockImportPreviewRow.Error(productCode, barcode, "ไม่พบสินค้าที่ตรงกับรหัสสินค้าหรือบาร์โค้ด"));
                continue;
            }
            var diff = newStock - product.CurrentStock;
            if (diff == 0)
            {
                preview.Rows.Add(StockImportPreviewRow.Skipped(product.ProductCode, product.Barcode, "ข้ามรายการ: สต็อกไม่เปลี่ยนแปลง", product));
                continue;
            }
            preview.Rows.Add(StockImportPreviewRow.Ready(product, newStock));
        }
        return preview;
    }

    public static Task<StockImportPreview> ImportPreviewFromProductsAsync(string path, IReadOnlyList<ProductGridRow> products)
    {
        using var workbook = new XLWorkbook(path);
        var sheet = workbook.Worksheets.FirstOrDefault(x => x.Name == "ตรวจนับสต็อก") ?? workbook.Worksheets.First();
        var headerRow = 3;
        var map = new Dictionary<string, int>();
        for (var c = 1; c <= sheet.LastColumnUsed().ColumnNumber(); c++) map[sheet.Cell(headerRow, c).GetString().Trim()] = c;
        if (!map.ContainsKey("รหัสสินค้า") || !map.ContainsKey("บาร์โค้ด") || !map.ContainsKey("สต็อกใหม่"))
        {
            throw new InvalidOperationException("ไฟล์ Excel ไม่ตรงกับรูปแบบที่ระบบกำหนด กรุณาใช้ไฟล์ Template ที่ส่งออกจากระบบ");
        }

        var preview = new StockImportPreview(Path.GetFileName(path));
        for (var r = headerRow + 1; r <= sheet.LastRowUsed().RowNumber(); r++)
        {
            var productCode = sheet.Cell(r, map["รหัสสินค้า"]).GetString().Trim();
            var barcode = sheet.Cell(r, map["บาร์โค้ด"]).GetString().Trim();
            var newStockText = sheet.Cell(r, map["สต็อกใหม่"]).GetString().Trim();
            if (string.IsNullOrWhiteSpace(productCode) && string.IsNullOrWhiteSpace(barcode)) continue;
            if (string.IsNullOrWhiteSpace(newStockText))
            {
                preview.Rows.Add(StockImportPreviewRow.Skipped(productCode, barcode, "ข้ามรายการ: ไม่ได้กรอกสต็อกใหม่"));
                continue;
            }
            if (!decimal.TryParse(newStockText, out var newStock) || newStock < 0)
            {
                preview.Rows.Add(StockImportPreviewRow.Error(productCode, barcode, "จำนวนสต็อกใหม่ต้องเป็นตัวเลขและต้องไม่ต่ำกว่า 0"));
                continue;
            }
            var product = products.FirstOrDefault(p =>
                (!string.IsNullOrWhiteSpace(productCode) && p.ProductCode.Equals(productCode, StringComparison.OrdinalIgnoreCase)) ||
                (!string.IsNullOrWhiteSpace(barcode) && p.Barcode.Equals(barcode, StringComparison.OrdinalIgnoreCase)));
            if (product is null)
            {
                preview.Rows.Add(StockImportPreviewRow.Error(productCode, barcode, "ไม่พบสินค้าที่ตรงกับรหัสสินค้าหรือบาร์โค้ด"));
                continue;
            }
            var diff = newStock - product.CurrentStock;
            if (diff == 0)
            {
                preview.Rows.Add(StockImportPreviewRow.Skipped(product.ProductCode, product.Barcode, "ข้ามรายการ: สต็อกไม่เปลี่ยนแปลง", product));
                continue;
            }
            preview.Rows.Add(StockImportPreviewRow.Ready(product, newStock));
        }
        return Task.FromResult(preview);
    }
}

public sealed class ProductEditorForm : Form
{
    private readonly ProductStockRepository _repo;
    private readonly BarcodeService _barcodeService;
    private readonly int _employeeId;
    private readonly int _locationId;
    private readonly TextBox _code = new();
    private readonly TextBox _name = new();
    private readonly ComboBox _category = new();
    private readonly ComboBox _brand = new();
    private readonly ComboBox _unit = new();
    private readonly TextBox _barcode = new();
    private readonly NumericUpDown _cost = MoneyInput();
    private readonly NumericUpDown _selling = MoneyInput();
    private readonly NumericUpDown _wholesale = MoneyInput();
    private readonly NumericUpDown _stock = MoneyInput();
    private readonly CheckBox _active = new() { Text = "เปิดใช้งาน", Checked = true };
    private readonly TextBox _description = new();

    public ProductEditorForm(ProductStockRepository repo, BarcodeService barcodeService, IReadOnlyList<LookupRow> categories, IReadOnlyList<LookupRow> brands, IReadOnlyList<LookupRow> units, int employeeId, int locationId, string? scannedBarcode)
    {
        _repo = repo;
        _barcodeService = barcodeService;
        _employeeId = employeeId;
        _locationId = locationId;
        Text = "เพิ่มสินค้าใหม่";
        Width = 720;
        Height = 560;
        StartPosition = FormStartPosition.CenterParent;
        _category.DataSource = categories.ToArray();
        _brand.DataSource = new[] { new LookupRow(0, "-") }.Concat(brands).ToArray();
        _unit.DataSource = units.ToArray();
        foreach (var combo in new[] { _category, _brand, _unit })
        {
            combo.DisplayMember = nameof(LookupRow.Name);
            combo.ValueMember = nameof(LookupRow.Id);
            combo.DropDownStyle = ComboBoxStyle.DropDownList;
        }
        _barcode.Text = scannedBarcode ?? "";
        Build();
        Shown += async (_, _) =>
        {
            _code.Text = await _repo.GenerateNextProductCodeAsync();
            _name.Focus();
        };
    }

    private void Build()
    {
        var root = new TableLayoutPanel { Dock = DockStyle.Fill, Padding = new Padding(18), ColumnCount = 2, RowCount = 12 };
        root.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 170));
        root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        AddRow(root, 0, "รหัสสินค้า", _code);
        _code.ReadOnly = true;
        AddRow(root, 1, "ชื่อสินค้า", _name);
        AddRow(root, 2, "หมวดหมู่สินค้า", _category);
        AddRow(root, 3, "แบรนด์สินค้า", _brand);
        AddRow(root, 4, "หน่วยสินค้า", _unit);
        var barcodePanel = new FlowLayoutPanel { Dock = DockStyle.Fill };
        _barcode.Width = 260;
        var generate = new Button { Text = "สร้างบาร์โค้ดอัตโนมัติ", Width = 180, Height = 32 };
        generate.Click += async (_, _) => _barcode.Text = await _barcodeService.GenerateUniqueBarcodeAsync();
        barcodePanel.Controls.Add(_barcode);
        barcodePanel.Controls.Add(generate);
        AddRow(root, 5, "บาร์โค้ด", barcodePanel);
        AddRow(root, 6, "ราคาทุน", _cost);
        AddRow(root, 7, "ราคาขายปลีก", _selling);
        AddRow(root, 8, "ราคาขายส่ง", _wholesale);
        AddRow(root, 9, "จำนวนสต็อกเริ่มต้น", _stock);
        AddRow(root, 10, "สถานะเปิดใช้งาน", _active);
        _description.Multiline = true;
        AddRow(root, 11, "หมายเหตุ", _description);
        var buttons = new FlowLayoutPanel { Dock = DockStyle.Bottom, FlowDirection = FlowDirection.RightToLeft, Height = 52 };
        var save = new Button { Text = "บันทึก", Width = 110, Height = 36 };
        var cancel = new Button { Text = "ยกเลิก", Width = 110, Height = 36 };
        save.Click += async (_, _) => await SaveAsync();
        cancel.Click += (_, _) => DialogResult = DialogResult.Cancel;
        buttons.Controls.Add(save);
        buttons.Controls.Add(cancel);
        Controls.Add(root);
        Controls.Add(buttons);
    }

    private async Task SaveAsync()
    {
        if (string.IsNullOrWhiteSpace(_name.Text)) { MessageBox.Show(this, "ชื่อสินค้าห้ามว่าง"); return; }
        if (!string.IsNullOrWhiteSpace(_barcode.Text) && await _barcodeService.IsBarcodeDuplicateAsync(_barcode.Text)) { MessageBox.Show(this, "บาร์โค้ดนี้ถูกใช้งานกับสินค้าอื่นแล้ว"); return; }
        if (_selling.Value < _cost.Value && MessageBox.Show(this, "ราคาขายต่ำกว่าราคาทุน ต้องการบันทึกต่อหรือไม่?", Text, MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes) return;
        await _repo.CreateProductAsync(new ProductCreateRequest
        {
            ProductCode = _code.Text,
            SKU = _code.Text,
            ProductName = _name.Text,
            CategoryId = Convert.ToInt32(_category.SelectedValue),
            BrandId = Convert.ToInt32(_brand.SelectedValue) == 0 ? null : Convert.ToInt32(_brand.SelectedValue),
            UnitId = Convert.ToInt32(_unit.SelectedValue),
            Barcode = _barcode.Text,
            CostPrice = _cost.Value,
            SellingPrice = _selling.Value,
            WholesalePrice = _wholesale.Value,
            WholesaleMinQty = 1,
            CurrentStock = _stock.Value,
            IsActive = _active.Checked,
            Description = _description.Text,
            CreatedByUserId = _employeeId
        }, _locationId);
        DialogResult = DialogResult.OK;
    }

    private static void AddRow(TableLayoutPanel root, int row, string label, Control control)
    {
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, row == 11 ? 82 : 38));
        root.Controls.Add(new Label { Text = label, Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft, Font = new Font("Segoe UI", 10, FontStyle.Bold) }, 0, row);
        control.Dock = DockStyle.Fill;
        root.Controls.Add(control, 1, row);
    }

    private static NumericUpDown MoneyInput() => new() { DecimalPlaces = 2, Maximum = 99999999, Minimum = 0, ThousandsSeparator = true };
}

public sealed class BarcodePrintForm : Form
{
    private readonly ProductGridRow _product;
    private readonly BarcodePrintService _service;
    private readonly NumericUpDown _qty = new() { Minimum = 1, Maximum = 999, Value = 1 };
    private readonly ComboBox _printer = new();
    private readonly ComboBox _size = new();
    private readonly TextBox _store = new() { Text = "AphiwatPOS" };

    public BarcodePrintForm(ProductGridRow product, BarcodePrintService service)
    {
        _product = product;
        _service = service;
        Text = "พิมพ์บาร์โค้ด";
        Width = 540;
        Height = 330;
        StartPosition = FormStartPosition.CenterParent;
        foreach (string printer in PrinterSettings.InstalledPrinters) _printer.Items.Add(printer);
        if (_printer.Items.Count > 0) _printer.SelectedIndex = 0;
        _size.Items.AddRange(["50x30 mm", "60x40 mm"]);
        _size.SelectedIndex = 0;
        Build();
    }

    private void Build()
    {
        var root = new TableLayoutPanel { Dock = DockStyle.Fill, Padding = new Padding(18), ColumnCount = 2 };
        root.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 160));
        root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        Add(root, 0, "สินค้า", new Label { Text = _product.ProductName });
        Add(root, 1, "บาร์โค้ด", new Label { Text = _product.Barcode });
        Add(root, 2, "ราคาขาย", new Label { Text = _product.SellingPrice.ToString("N2") });
        Add(root, 3, "จำนวนฉลาก", _qty);
        Add(root, 4, "เลือกเครื่องพิมพ์", _printer);
        Add(root, 5, "ขนาดฉลาก", _size);
        Add(root, 6, "ชื่อร้าน", _store);
        var buttons = new FlowLayoutPanel { Dock = DockStyle.Bottom, FlowDirection = FlowDirection.RightToLeft, Height = 52 };
        var print = new Button { Text = "พิมพ์", Width = 100, Height = 36 };
        var preview = new Button { Text = "ตัวอย่างก่อนพิมพ์", Width = 140, Height = 36 };
        var cancel = new Button { Text = "ยกเลิก", Width = 100, Height = 36 };
        print.Click += (_, _) => _service.PrintBarcode(Model(), false);
        preview.Click += (_, _) => _service.PrintBarcode(Model(), true);
        cancel.Click += (_, _) => DialogResult = DialogResult.Cancel;
        buttons.Controls.Add(print);
        buttons.Controls.Add(preview);
        buttons.Controls.Add(cancel);
        Controls.Add(root);
        Controls.Add(buttons);
    }

    private ProductBarcodePrintModel Model() => new(_product.ProductName, _product.Barcode, _product.SellingPrice, Convert.ToInt32(_qty.Value), _printer.Text, _size.Text, _store.Text);
    private static void Add(TableLayoutPanel root, int row, string label, Control control)
    {
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 34));
        root.Controls.Add(new Label { Text = label, Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft, Font = new Font("Segoe UI", 10, FontStyle.Bold) }, 0, row);
        control.Dock = DockStyle.Fill;
        root.Controls.Add(control, 1, row);
    }
}

public sealed class StockImportPreviewForm : Form
{
    public StockImportPreviewForm(StockImportPreview preview)
    {
        Text = "ตรวจสอบรายการนำเข้า Excel";
        Width = 1100;
        Height = 680;
        StartPosition = FormStartPosition.CenterParent;
        var root = new TableLayoutPanel { Dock = DockStyle.Fill, RowCount = 3, Padding = new Padding(16) };
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 62));
        root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 52));
        root.Controls.Add(new Label { Text = preview.SummaryText, Dock = DockStyle.Fill, Font = new Font("Segoe UI", 11, FontStyle.Bold) }, 0, 0);
        var grid = new DataGridView { Dock = DockStyle.Fill, AutoGenerateColumns = true, DataSource = preview.Rows, ReadOnly = true, AllowUserToAddRows = false };
        root.Controls.Add(grid, 0, 1);
        var buttons = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.RightToLeft };
        var confirm = new Button { Text = "ยืนยันการอัปเดตสต็อก", Width = 190, Height = 36 };
        var cancel = new Button { Text = "ยกเลิก", Width = 100, Height = 36 };
        confirm.Click += (_, _) => DialogResult = DialogResult.OK;
        cancel.Click += (_, _) => DialogResult = DialogResult.Cancel;
        buttons.Controls.Add(confirm);
        buttons.Controls.Add(cancel);
        root.Controls.Add(buttons, 0, 2);
        Controls.Add(root);
    }
}

public sealed class ImportHistoryForm : Form
{
    public ImportHistoryForm(IReadOnlyList<ImportHistoryRow> rows)
    {
        Text = "ประวัติการนำเข้าไฟล์ Excel";
        Width = 1000;
        Height = 560;
        StartPosition = FormStartPosition.CenterParent;
        Controls.Add(new DataGridView { Dock = DockStyle.Fill, AutoGenerateColumns = true, DataSource = rows, ReadOnly = true, AllowUserToAddRows = false });
    }
}

public static class ProductStockSql
{
    public static IReadOnlyList<string> EnsureStatements { get; } =
    [
        """
IF OBJECT_ID(N'dbo.ProductBulkUpdateAudit', N'U') IS NULL
CREATE TABLE dbo.ProductBulkUpdateAudit
(
    ProductAuditId BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT PK_ProductBulkUpdateAudit PRIMARY KEY,
    ProductId INT NOT NULL,
    FieldName NVARCHAR(100) NOT NULL,
    OldValue NVARCHAR(MAX) NOT NULL,
    NewValue NVARCHAR(MAX) NOT NULL,
    UpdatedByEmployeeId INT NULL,
    UpdatedDate DATETIME2(0) NOT NULL CONSTRAINT DF_ProductBulkUpdateAudit_UpdatedDate DEFAULT SYSUTCDATETIME()
);
""",
        """
IF OBJECT_ID(N'dbo.ProductStockImportBatch', N'U') IS NULL
CREATE TABLE dbo.ProductStockImportBatch
(
    ImportBatchId BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT PK_ProductStockImportBatch PRIMARY KEY,
    Filename NVARCHAR(260) NOT NULL,
    FileHash NVARCHAR(128) NOT NULL,
    ImportedByEmployeeId INT NOT NULL,
    ImportedDate DATETIME2(0) NOT NULL CONSTRAINT DF_ProductStockImportBatch_ImportedDate DEFAULT SYSUTCDATETIME(),
    TotalRows INT NOT NULL,
    UpdatedRows INT NOT NULL,
    SkippedRows INT NOT NULL,
    ErrorRows INT NOT NULL,
    Status NVARCHAR(30) NOT NULL,
    Remark NVARCHAR(500) NOT NULL CONSTRAINT DF_ProductStockImportBatch_Remark DEFAULT N''
);
""",
        """
IF OBJECT_ID(N'dbo.ProductStockImportItem', N'U') IS NULL
CREATE TABLE dbo.ProductStockImportItem
(
    ImportItemId BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT PK_ProductStockImportItem PRIMARY KEY,
    ImportBatchId BIGINT NOT NULL,
    ProductId INT NOT NULL,
    ProductCode NVARCHAR(50) NOT NULL,
    Barcode NVARCHAR(100) NOT NULL,
    OldStock DECIMAL(18,4) NOT NULL,
    NewStock DECIMAL(18,4) NOT NULL,
    Variance DECIMAL(18,4) NOT NULL,
    InventoryMovementId BIGINT NULL,
    Status NVARCHAR(30) NOT NULL,
    Message NVARCHAR(500) NOT NULL CONSTRAINT DF_ProductStockImportItem_Message DEFAULT N'',
    CONSTRAINT FK_ProductStockImportItem_Batch FOREIGN KEY (ImportBatchId) REFERENCES dbo.ProductStockImportBatch(ImportBatchId)
);
""",
        """
IF OBJECT_ID(N'dbo.ProductImageSync', N'U') IS NULL
CREATE TABLE dbo.ProductImageSync
(
    ProductImageSyncId BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT PK_ProductImageSync PRIMARY KEY,
    ProductId INT NOT NULL,
    LocalImagePath NVARCHAR(500) NOT NULL,
    ImageHash NVARCHAR(128) NOT NULL,
    SyncStatus NVARCHAR(30) NOT NULL CONSTRAINT DF_ProductImageSync_SyncStatus DEFAULT N'Pending',
    UploadedPath NVARCHAR(500) NOT NULL CONSTRAINT DF_ProductImageSync_UploadedPath DEFAULT N'',
    UploadedDate DATETIME2(0) NULL,
    UploadedByEmployeeId INT NULL,
    CreatedByEmployeeId INT NULL,
    CreatedDate DATETIME2(0) NOT NULL CONSTRAINT DF_ProductImageSync_CreatedDate DEFAULT SYSUTCDATETIME(),
    LastError NVARCHAR(500) NOT NULL CONSTRAINT DF_ProductImageSync_LastError DEFAULT N''
);
""",
        "IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name=N'IX_Product_Barcode_NotNull' AND object_id=OBJECT_ID(N'dbo.Product')) CREATE UNIQUE INDEX IX_Product_Barcode_NotNull ON dbo.Product(Barcode) WHERE Barcode IS NOT NULL;",
        "IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name=N'IX_ProductStockImportBatch_FileHash' AND object_id=OBJECT_ID(N'dbo.ProductStockImportBatch')) CREATE UNIQUE INDEX IX_ProductStockImportBatch_FileHash ON dbo.ProductStockImportBatch(FileHash);",
        "IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name=N'IX_ProductStockImportItem_Batch' AND object_id=OBJECT_ID(N'dbo.ProductStockImportItem')) CREATE INDEX IX_ProductStockImportItem_Batch ON dbo.ProductStockImportItem(ImportBatchId);",
        "IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name=N'IX_ProductImageSync_Pending' AND object_id=OBJECT_ID(N'dbo.ProductImageSync')) CREATE INDEX IX_ProductImageSync_Pending ON dbo.ProductImageSync(SyncStatus, ProductId);",
        "IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name=N'IX_ProductImageSync_ProductHashSynced' AND object_id=OBJECT_ID(N'dbo.ProductImageSync')) CREATE INDEX IX_ProductImageSync_ProductHashSynced ON dbo.ProductImageSync(ProductId, ImageHash, SyncStatus);"
    ];
}

public sealed class ProductGridRow
{
    public int ProductId { get; set; }
    public string ProductCode { get; set; } = "";
    public string SKU { get; set; } = "";
    public string Barcode { get; set; } = "";
    public string ProductName { get; set; } = "";
    public int CategoryId { get; set; }
    public int? BrandId { get; set; }
    public int UnitId { get; set; }
    public string UnitName { get; set; } = "";
    public decimal CostPrice { get; set; }
    public decimal MinimumCost { get; set; }
    public string VatMode { get; set; } = "VatExcluded";
    public decimal VatPercentage { get; set; }
    public decimal MinimumSellingPrice { get; set; }
    public decimal SellingPrice { get; set; }
    public decimal WholesalePrice { get; set; }
    public decimal WholesaleMinQty { get; set; } = 1;
    public decimal TaxRate { get; set; }
    public bool DiscountAllowed { get; set; } = true;
    public bool IsStockTracked { get; set; } = true;
    public decimal MinimumStockLevel { get; set; }
    public decimal CurrentStock { get; set; }
    public string ProductImageUrl { get; set; } = "";
    public string LocalImagePath { get; set; } = "";
    public string ImageHash { get; set; } = "";
    public string ImageSyncStatus { get; set; } = "";
    public DateTime? UploadedDate { get; set; }
    public int? UploadedByEmployeeId { get; set; }
    public bool OfflineDirty { get; set; }
    public string Description { get; set; } = "";
    public string Status { get; set; } = "Active";
    public bool IsActive { get; set; }
    public DateTime LastUpdatedDateValue { get; set; }
    public string LastUpdatedDate => LastUpdatedDateValue.ToString("yyyy-MM-dd HH:mm");
    [JsonIgnore]
    public Image? Thumbnail => ProductImageStore.LoadThumbnail(LocalImagePath);
}

public sealed record ProductSearchFilter(string? ProductName = null, string? ProductCode = null, string? Barcode = null, bool? IsActive = true, bool UnsyncedImagesOnly = false);
public sealed record InventoryLocationRow(int LocationId, string LocationCode, string LocationName, bool IsDefault, bool IsActive);
public sealed record LookupRow(int Id, string Name);
public sealed record ProductBarcodePrintModel(string ProductName, string Barcode, decimal SellingPrice, int Quantity, string PrinterName, string LabelSize, string StoreName);
public sealed record ProductImageSaveResult(string LocalPath, string Hash);
public sealed record ProductImageSyncRow(long ProductImageSyncId, int ProductId, string LocalImagePath, string ImageHash, string SyncStatus);
public sealed record ProductCreateRequest
{
    public string ProductCode { get; init; } = "";
    public string SKU { get; init; } = "";
    public string Barcode { get; init; } = "";
    public string ProductName { get; init; } = "";
    public int CategoryId { get; init; }
    public int? BrandId { get; init; }
    public int UnitId { get; init; }
    public decimal CostPrice { get; init; }
    public decimal SellingPrice { get; init; }
    public decimal WholesalePrice { get; init; }
    public decimal WholesaleMinQty { get; init; } = 1;
    public decimal CurrentStock { get; init; }
    public bool IsActive { get; init; }
    public string Description { get; init; } = "";
    public int CreatedByUserId { get; init; }
}

public sealed class StockImportPreview(string filename)
{
    public string Filename { get; } = filename;
    public BindingList<StockImportPreviewRow> Rows { get; } = [];
    public int TotalRows => Rows.Count;
    public int ReadyRows => Rows.Count(x => x.Status == ImportRowStatus.Ready);
    public int SkippedRows => Rows.Count(x => x.Status == ImportRowStatus.Skipped);
    public int ErrorRows => Rows.Count(x => x.Status == ImportRowStatus.Error);
    public decimal IncreaseQty => Rows.Where(x => x.Status == ImportRowStatus.Ready && x.Difference > 0).Sum(x => x.Difference);
    public decimal DecreaseQty => Math.Abs(Rows.Where(x => x.Status == ImportRowStatus.Ready && x.Difference < 0).Sum(x => x.Difference));
    public string SummaryText => $"จำนวนรายการทั้งหมด {TotalRows:N0} | พร้อมอัปเดต {ReadyRows:N0} | ข้าม {SkippedRows:N0} | ข้อผิดพลาด {ErrorRows:N0} | สต็อกเพิ่มขึ้น {IncreaseQty:N2} | สต็อกลดลง {DecreaseQty:N2}";
}

public sealed class StockImportPreviewRow
{
    public int ProductId { get; init; }
    public string ProductCode { get; init; } = "";
    public string ProductName { get; init; } = "";
    public string Barcode { get; init; } = "";
    public decimal CostPrice { get; init; }
    public decimal CurrentStock { get; init; }
    public decimal? NewStock { get; init; }
    public decimal Difference => NewStock.GetValueOrDefault() - CurrentStock;
    public ImportRowStatus Status { get; init; }
    public string Message { get; init; } = "";
    public string สถานะ => Status switch { ImportRowStatus.Ready => "พร้อมอัปเดต", ImportRowStatus.Skipped => "ข้าม", ImportRowStatus.Error => "ผิดพลาด", _ => "เตือน" };

    public static StockImportPreviewRow Ready(ProductGridRow product, decimal newStock) => new() { ProductId = product.ProductId, ProductCode = product.ProductCode, ProductName = product.ProductName, Barcode = product.Barcode, CostPrice = product.CostPrice, CurrentStock = product.CurrentStock, NewStock = newStock, Status = ImportRowStatus.Ready, Message = "พร้อมอัปเดต" };
    public static StockImportPreviewRow Error(string code, string barcode, string message) => new() { ProductCode = code, Barcode = barcode, Status = ImportRowStatus.Error, Message = message };
    public static StockImportPreviewRow Skipped(string code, string barcode, string message, ProductGridRow? product = null) => new() { ProductId = product?.ProductId ?? 0, ProductCode = product?.ProductCode ?? code, ProductName = product?.ProductName ?? "", Barcode = product?.Barcode ?? barcode, CurrentStock = product?.CurrentStock ?? 0, NewStock = product?.CurrentStock, Status = ImportRowStatus.Skipped, Message = message };
}

public enum ImportRowStatus { Ready, Skipped, Warning, Error }
public sealed record ImportHistoryRow(long ImportBatchId, string Filename, int ImportedByEmployeeId, DateTime ImportedDate, int TotalRows, int UpdatedRows, int SkippedRows, int ErrorRows, string Status, string Remark);

internal static class Code128
{
    private static readonly string[] Patterns =
    [
        "11011001100","11001101100","11001100110","10010011000","10010001100","10001001100","10011001000","10011000100","10001100100","11001001000",
        "11001000100","11000100100","10110011100","10011011100","10011001110","10111001100","10011101100","10011100110","11001110010","11001011100",
        "11001001110","11011100100","11001110100","11101101110","11101001100","11100101100","11100100110","11101100100","11100110100","11100110010",
        "11011011000","11011000110","11000110110","10100011000","10001011000","10001000110","10110001000","10001101000","10001100010","11010001000",
        "11000101000","11000100010","10110111000","10110001110","10001101110","10111011000","10111000110","10001110110","11101110110","11010001110",
        "11000101110","11011101000","11011100010","11011101110","11101011000","11101000110","11100010110","11101101000","11101100010","11100011010",
        "11101111010","11001000010","11110001010","10100110000","10100001100","10010110000","10010000110","10000101100","10000100110","10110010000",
        "10110000100","10011010000","10011000010","10000110100","10000110010","11000010010","11001010000","11110111010","11000010100","10001111010",
        "10100111100","10010111100","10010011110","10111100100","10011110100","10011110010","11110100100","11110010100","11110010010","11011011110",
        "11011110110","11110110110","10101111000","10100011110","10001011110","10111101000","10111100010","11110101000","11110100010","10111011110",
        "10111101110","11101011110","11110101110","11010000100","11010010000","11010011100","1100011101011"
    ];

    public static string Encode(string value)
    {
        var codes = new List<int> { 104 };
        codes.AddRange(value.Select(c => c is >= ' ' and <= '~' ? c - 32 : 0));
        var checksum = 104;
        for (var i = 1; i < codes.Count; i++) checksum += codes[i] * i;
        codes.Add(checksum % 103);
        codes.Add(106);
        return string.Concat(codes.Select(c => Patterns[c]));
    }
}
