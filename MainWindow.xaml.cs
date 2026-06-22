using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Windows;
using System.Windows.Controls;

namespace YunyunSaveEditor;

public partial class MainWindow : Window
{
    private string _playerDir = "";
    private JsonObject? _root;
    private int _prevTab = 0;
    private bool _langReady;
    private static string LangFile => Path.Combine(AppContext.BaseDirectory, "language.txt");

    private readonly ObservableCollection<ScoreRow> _songs = new();
    private readonly ObservableCollection<TheoryRow> _theories = new();
    private readonly Dictionary<string, TextBox> _paramBoxes = new();
    private readonly Dictionary<string, CheckBox> _flagBoxes = new();

    private static readonly string[] FlagNames =
    {
        "ReadPrologue1","PlayedOP","IsPlayedTutorial","IsPlayedPost","IsNewGame",
        "IsRankingEntry","GoToDiscord","InGamePlayedOffline","IsResetGame",
        "IsBackupData","CheckBackupRecovery"
    };
    private static readonly string[] ParamNames =
    { "DokiDoki","YunYun","Charisma","MaxDokiDoki","MaxYunYun","MaxCharisma" };

    public MainWindow()
    {
        InitializeComponent();
        InitLanguage();
        gridSongs.ItemsSource = _songs;
        gridTheories.ItemsSource = _theories;
        BuildProgresoControls();

        _playerDir = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
            "AppData", "LocalLow", "AllianceArts", "Yunyun_Syndrome", "player");

        if (!Directory.Exists(_playerDir))
        {
            MessageBox.Show(string.Format(Loc.T("msg_folder_not_found"), _playerDir),
                Loc.T("app_name"), MessageBoxButton.OK, MessageBoxImage.Information);
        }
        RefreshFileList();
    }

    // ---------- idioma ----------
    private void InitLanguage()
    {
        string lang = "en";
        try { if (File.Exists(LangFile)) lang = File.ReadAllText(LangFile).Trim(); } catch { /* ignore */ }
        if (!Loc.Available.Any(a => a.Code == lang))
        {
            string sys = CultureInfo.CurrentUICulture.TwoLetterISOLanguageName;
            lang = Loc.Available.Any(a => a.Code == sys) ? sys : "en";
        }
        Loc.Instance.Language = lang;

        foreach (var (code, name) in Loc.Available)
            cmbLang.Items.Add(new ComboBoxItem { Tag = code, Content = name });
        foreach (ComboBoxItem it in cmbLang.Items)
            if ((string)it.Tag == lang) { cmbLang.SelectedItem = it; break; }
        _langReady = true;
    }

    private void cmbLang_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (!_langReady) return;
        if (cmbLang.SelectedItem is ComboBoxItem it && it.Tag is string code)
        {
            Loc.Instance.Language = code;
            try { File.WriteAllText(LangFile, code); } catch { /* ignore */ }
            gridSongs?.Items.Refresh();      // refrescar títulos en el nuevo idioma
            gridTheories?.Items.Refresh();
        }
    }

    // ---------- construir controles dinámicos ----------
    private void BuildProgresoControls()
    {
        foreach (var p in ParamNames)
        {
            var sp = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 0, 0, 6) };
            sp.Children.Add(new TextBlock { Text = p + ":", Width = 95, VerticalAlignment = VerticalAlignment.Center });
            var tb = new TextBox { Width = 70, Height = 26 };
            _paramBoxes[p] = tb;
            sp.Children.Add(tb);
            paramGrid.Children.Add(sp);
        }
        foreach (var f in FlagNames)
        {
            var cb = new CheckBox { Content = f, Margin = new Thickness(0, 4, 0, 4) };
            _flagBoxes[f] = cb;
            flagsPanel.Children.Add(cb);
        }
    }

    // ---------- archivos ----------
    private void RefreshFileList()
    {
        cmbFile.Items.Clear();
        if (!Directory.Exists(_playerDir)) return;
        if (File.Exists(Path.Combine(_playerDir, "save_global"))) cmbFile.Items.Add("save_global");
        for (int i = 0; i < 100; i++)
            if (File.Exists(Path.Combine(_playerDir, $"save_slot{i}"))) cmbFile.Items.Add($"save_slot{i}");
        if (cmbFile.Items.Count > 0) cmbFile.SelectedIndex = 0;
    }

    private void Status(string msg, bool ok = true)
    {
        lblStatus.Text = msg;
        lblStatus.Foreground = ok ? System.Windows.Media.Brushes.DarkGreen : System.Windows.Media.Brushes.Firebrick;
    }

    private string? CurrentFilePath()
    {
        if (cmbFile.SelectedItem is not string name) return null;
        return Path.Combine(_playerDir, name);
    }

    private void btnFolder_Click(object sender, RoutedEventArgs e)
    {
        var dlg = new Microsoft.Win32.OpenFolderDialog { Title = "Carpeta 'player' de Yunyun Syndrome" };
        if (Directory.Exists(_playerDir)) dlg.InitialDirectory = _playerDir;
        if (dlg.ShowDialog() == true)
        {
            _playerDir = dlg.FolderName;
            RefreshFileList();
            Status(string.Format(Loc.T("status_folder"), _playerDir));
        }
    }

    private void btnOpen_Click(object sender, RoutedEventArgs e)
    {
        var path = CurrentFilePath();
        if (path == null) { Status(Loc.T("status_no_file"), false); return; }
        if (!File.Exists(path)) { Status(string.Format(Loc.T("status_not_exist"), path), false); return; }
        try
        {
            string json = SaveFile.Decrypt(path);
            _root = JsonNode.Parse(json)!.AsObject();
            PopulateProgreso();
            PopulateSongs();
            PopulateTheories();
            BuildTree();
            _prevTab = tabs.SelectedIndex;
            Status(string.Format(Loc.T("status_loaded"), cmbFile.SelectedItem));
        }
        catch (Exception ex) { Status(string.Format(Loc.T("status_open_error"), ex.Message), false); }
    }

    private void btnSave_Click(object sender, RoutedEventArgs e)
    {
        if (_root == null) { Status(Loc.T("status_open_first"), false); return; }
        var path = CurrentFilePath();
        if (path == null) return;
        CollectTab(_prevTab);
        try
        {
            string json = _root.ToJsonString(new JsonSerializerOptions { WriteIndented = false });
            JsonNode.Parse(json); // validación
            string? bk = Backup(path);
            SaveFile.Encrypt(json, path);
            Status(string.Format(Loc.T("status_saved"), cmbFile.SelectedItem, bk != null ? Path.GetFileName(bk) : "-"));
        }
        catch (Exception ex) { Status(string.Format(Loc.T("status_save_error"), ex.Message), false); }
    }

    private string? Backup(string file)
    {
        if (!File.Exists(file)) return null;
        string dir = Path.Combine(AppContext.BaseDirectory, "backups");
        Directory.CreateDirectory(dir);
        string dst = Path.Combine(dir, Path.GetFileName(file) + "." + DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".bak");
        File.Copy(file, dst, true);
        return dst;
    }

    // ---------- Progreso ----------
    private bool Has(string key) => _root != null && _root.ContainsKey(key);

    private void PopulateProgreso()
    {
        bool on = Has("DenpaPlayPoint");
        txtDenpa.IsEnabled = txtFollower.IsEnabled = btnSongsClear.IsEnabled =
            btnSongsUnlock.IsEnabled = btnTheories.IsEnabled = on;
        foreach (var t in _paramBoxes.Values) t.IsEnabled = on;
        foreach (var c in _flagBoxes.Values) c.IsEnabled = on;
        if (_root == null) return;

        if (Has("DenpaPlayPoint")) txtDenpa.Text = _root["DenpaPlayPoint"]!.ToJsonString();
        if (Has("FollowerCount")) txtFollower.Text = _root["FollowerCount"]!.ToJsonString();

        if (_root["GameParameter"] is JsonObject gp)
            foreach (var p in ParamNames)
                if (gp[p] != null) _paramBoxes[p].Text = gp[p]!.ToJsonString();

        foreach (var f in FlagNames)
            if (_root[f] is JsonNode n && n.GetValueKind() is JsonValueKind.True or JsonValueKind.False)
                _flagBoxes[f].IsChecked = n.GetValue<bool>();

        RefreshDenpaNow();
    }

    private void RefreshDenpaNow()
    {
        if (_root == null || lblDenpaNow == null) return;
        if (!_root.ContainsKey("DenpaPlayPoint")) { lblDenpaNow.Text = ""; return; }
        long total = Denpa.Total(_root);
        int pct = Denpa.LevelForPoints(total);
        lblDenpaNow.Text = string.Format(Loc.T("denpa_now"), pct, total.ToString("N0"));
        if (txtDenpaPct != null) txtDenpaPct.Text = pct.ToString();          // por defecto, el % actual
        if (txtDenpa != null && _root["DenpaPlayPoint"] is JsonNode dp)
            txtDenpa.Text = dp.ToJsonString();                               // y el DenpaPlayPoint actual
    }

    private void CollectProgreso()
    {
        if (_root == null || !Has("DenpaPlayPoint")) return;
        if (long.TryParse(txtDenpa.Text.Trim(), out long d)) _root["DenpaPlayPoint"] = d;
        if (Has("FollowerCount") && long.TryParse(txtFollower.Text.Trim(), out long fc)) _root["FollowerCount"] = fc;

        if (_root["GameParameter"] is JsonObject gp)
            foreach (var p in ParamNames)
                if (gp.ContainsKey(p) && double.TryParse(_paramBoxes[p].Text.Replace(',', '.'),
                        NumberStyles.Any, CultureInfo.InvariantCulture, out double dv))
                    gp[p] = dv;

        foreach (var f in FlagNames)
            if (_root.ContainsKey(f)) _root[f] = _flagBoxes[f].IsChecked == true;
    }

    // ---------- Canciones ----------
    private void PopulateSongs()
    {
        _songs.Clear();
        if (_root?["ScoreRecords"]?["List"] is not JsonArray list) return;
        foreach (var item in list)
        {
            if (item is not JsonObject o) continue;
            _songs.Add(new ScoreRow
            {
                Name = o.GetStr("Name"),
                Level = (int)o.GetLong("Level"),
                Point = o.GetLong("Point"),
                Combo = (int)o.GetLong("Combo"),
                FullCombo = o.GetBool("FullCombo"),
                Rank = (int)o.GetLong("Rank"),
                Rate = o.GetDbl("Rate")
            });
        }
    }

    private void CollectSongs()
    {
        if (_root?["ScoreRecords"] is not JsonObject sr) return;
        var arr = new JsonArray();
        foreach (var s in _songs)
        {
            arr.Add(new JsonObject
            {
                ["Name"] = s.Name,
                ["Level"] = s.Level,
                ["Point"] = s.Point,
                ["Combo"] = s.Combo,
                ["FullCombo"] = s.FullCombo,
                ["Rank"] = s.Rank,
                ["Rate"] = s.Rate
            });
        }
        sr["List"] = arr;
    }

    // ---------- Teorías ----------
    private void PopulateTheories()
    {
        _theories.Clear();
        if (_root?["ConspiracyTheory"]?["List"] is not JsonArray list) return;
        foreach (var item in list)
        {
            if (item is not JsonObject o) continue;
            _theories.Add(new TheoryRow
            {
                No = (int)o.GetLong("No"),
                GetCount = (int)o.GetLong("GetCount"),
                CheckedGetCount = (int)o.GetLong("CheckedGetCount"),
                Seed = o.GetLong("Seed")
            });
        }
    }

    private void CollectTheories()
    {
        if (_root?["ConspiracyTheory"] is not JsonObject ct) return;
        var arr = new JsonArray();
        foreach (var t in _theories)
            arr.Add(new JsonObject
            {
                ["No"] = t.No,
                ["GetCount"] = t.GetCount,
                ["CheckedGetCount"] = t.CheckedGetCount,
                ["Seed"] = t.Seed
            });
        ct["List"] = arr;
    }

    // ---------- pestañas ----------
    private void CollectTab(int idx)
    {
        if (idx == 0) CollectProgreso();
        else if (idx == 1) CollectSongs();
        else if (idx == 2) CollectTheories();
    }
    private void PopulateTab(int idx)
    {
        if (idx == 0) PopulateProgreso();
        else if (idx == 1) PopulateSongs();
        else if (idx == 2) PopulateTheories();
        else if (idx == 3) BuildTree();
    }
    private void tabs_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (e.Source != tabs || _root == null) return;
        CollectTab(_prevTab);
        PopulateTab(tabs.SelectedIndex);
        _prevTab = tabs.SelectedIndex;
    }

    // ---------- acciones rápidas ----------
    private void btnDenpaPct_Click(object sender, RoutedEventArgs e)
    {
        if (_root == null) { Status(Loc.T("status_open_first"), false); return; }
        CollectSongs(); CollectTheories(); // reflejar en _root las ediciones de las tablas
        if (!int.TryParse(txtDenpaPct.Text.Trim(), out int pct)) { Status(Loc.T("status_num_invalid"), false); return; }
        pct = Math.Clamp(pct, 0, 100);
        long pp = Denpa.PlayPointForPercent(_root, pct);
        _root["DenpaPlayPoint"] = pp;
        txtDenpa.Text = pp.ToString();
        RefreshDenpaNow();
        Status(string.Format(Loc.T("denpa_applied"), pp.ToString("N0"), Denpa.CurrentPercent(_root)));
    }

    private void btnSongsClear_Click(object sender, RoutedEventArgs e)
    {
        if (_root == null) return;
        int rank = int.TryParse(txtRank.Text, out int r) ? r : 4;
        foreach (var s in _songs) { s.Point = 1_000_000; s.Combo = 9999; s.FullCombo = true; s.Rank = rank; s.Rate = 1.0; }
        gridSongs.Items.Refresh();
        CollectSongs();
        RefreshDenpaNow();
        Status(string.Format(Loc.T("status_songs_marked"), _songs.Count, rank));
    }

    private void btnSongsUnlock_Click(object sender, RoutedEventArgs e)
    {
        if (_root == null || _root["ScoreRecords"]?["List"] is not JsonArray list) return;
        var arr = new JsonArray();
        foreach (var item in list)
            if (item is JsonObject o) arr.Add(new JsonObject { ["Id"] = o.GetStr("Name") });
        _root["SongUnlockDatas"] = arr;
        Status(string.Format(Loc.T("status_songs_unlocked"), arr.Count));
    }

    private void btnTheories_Click(object sender, RoutedEventArgs e)
    {
        if (_root?["ConspiracyTheory"]?["List"] is not JsonArray list) return;
        foreach (var item in list)
            if (item is JsonObject o)
            {
                long g = Math.Max(1, o.GetLong("GetCount"));
                o["GetCount"] = g; o["CheckedGetCount"] = g;
            }
        PopulateTheories();
        RefreshDenpaNow();
        Status(string.Format(Loc.T("status_theories_marked"), list.Count));
    }

    // ---------- Árbol (Avanzado) ----------
    private void BuildTree()
    {
        tree.Items.Clear();
        if (_root == null) return;
        var root = MakeItem(null, "(save)", _root);
        root.IsExpanded = true;
        tree.Items.Add(root);
    }

    private static string Preview(JsonNode? node)
    {
        if (node is JsonObject o) return $"{{ }} {o.Count} campos";
        if (node is JsonArray a) return $"[ ] {a.Count} elementos";
        if (node == null) return "null";
        return node.GetValueKind() == JsonValueKind.String ? node.GetValue<string>() : node.ToJsonString();
    }

    private TreeViewItem MakeItem(JsonNode? parent, object keyOrIndex, JsonNode? node)
    {
        string keyText = keyOrIndex is int i ? $"[{i}]" : keyOrIndex?.ToString() ?? "";
        var item = new TreeViewItem
        {
            Header = keyOrIndex == null ? Preview(node) : $"{keyText} : {Preview(node)}",
            Tag = new NodeTag { Parent = parent, Key = keyOrIndex, Node = node }
        };
        if (node is JsonObject obj)
            foreach (var kv in obj) item.Items.Add(MakeItem(obj, kv.Key, kv.Value));
        else if (node is JsonArray arr)
            for (int k = 0; k < arr.Count; k++) item.Items.Add(MakeItem(arr, k, arr[k]));
        return item;
    }

    private void RebuildChildren(TreeViewItem item)
    {
        var tag = (NodeTag)item.Tag;
        item.Items.Clear();
        if (tag.Node is JsonObject obj)
            foreach (var kv in obj) item.Items.Add(MakeItem(obj, kv.Key, kv.Value));
        else if (tag.Node is JsonArray arr)
            for (int k = 0; k < arr.Count; k++) item.Items.Add(MakeItem(arr, k, arr[k]));
        string keyText = tag.Key is int i ? $"[{i}]" : tag.Key?.ToString() ?? "";
        item.Header = tag.Key == null ? Preview(tag.Node) : $"{keyText} : {Preview(tag.Node)}";
    }

    private void tree_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
    {
        if (e.NewValue is not TreeViewItem item) return;
        var tag = (NodeTag)item.Tag;
        lblPath.Text = Loc.T("path_prefix") + BuildPath(item);

        bool isLeaf = tag.Node is not (JsonObject or JsonArray);
        cmbType.IsEnabled = txtValue.IsEnabled = btnApply.IsEnabled = isLeaf;
        btnAddEl.IsEnabled = tag.Node is JsonArray;
        btnAddField.IsEnabled = tag.Node is JsonObject;
        btnDelete.IsEnabled = tag.Parent != null;

        if (isLeaf)
        {
            var node = tag.Node;
            if (node == null) { SelectType("Null"); txtValue.Text = ""; }
            else switch (node.GetValueKind())
            {
                case JsonValueKind.True:
                case JsonValueKind.False: SelectType("Booleano"); txtValue.Text = node.GetValue<bool>() ? "true" : "false"; break;
                case JsonValueKind.Number: SelectType("Número"); txtValue.Text = node.ToJsonString(); break;
                default: SelectType("Texto"); txtValue.Text = node.GetValue<string>(); break;
            }
        }
        else txtValue.Text = "";
    }

    private void SelectType(string t)
    {
        foreach (ComboBoxItem it in cmbType.Items)
            if ((string)it.Tag == t) { cmbType.SelectedItem = it; return; }
    }

    private static string BuildPath(TreeViewItem item)
    {
        var parts = new List<string>();
        var cur = item;
        while (cur != null)
        {
            if (cur.Tag is NodeTag t && t.Key != null)
                parts.Insert(0, t.Key is int i ? $"[{i}]" : t.Key.ToString()!);
            cur = ItemsControl.ItemsControlFromItemContainer(cur) as TreeViewItem;
        }
        return parts.Count == 0 ? "(save)" : string.Join(" › ", parts);
    }

    private void btnApply_Click(object sender, RoutedEventArgs e)
    {
        if (tree.SelectedItem is not TreeViewItem item) return;
        var tag = (NodeTag)item.Tag;
        if (tag.Parent == null) return;
        string type = (cmbType.SelectedItem as ComboBoxItem)?.Tag as string ?? "Texto";
        JsonNode? newNode;
        switch (type)
        {
            case "Texto": newNode = JsonValue.Create(txtValue.Text); break;
            case "Booleano": newNode = JsonValue.Create(txtValue.Text.Trim().ToLower() is "true" or "1" or "si" or "sí"); break;
            case "Null": newNode = null; break;
            default:
                string s = txtValue.Text.Trim();
                if (long.TryParse(s, out long l)) newNode = JsonValue.Create(l);
                else if (double.TryParse(s, NumberStyles.Any, CultureInfo.InvariantCulture, out double dv)) newNode = JsonValue.Create(dv);
                else { Status(Loc.T("status_num_invalid"), false); return; }
                break;
        }
        AssignToParent(tag, newNode);
        tag.Node = newNode;
        string keyText = tag.Key is int i ? $"[{i}]" : tag.Key?.ToString() ?? "";
        item.Header = $"{keyText} : {Preview(newNode)}";
        if (tag.Key as string == "DenpaPlayPoint") txtDenpa.Text = newNode?.ToJsonString() ?? "0";
        Status(Loc.T("status_field_updated"));
    }

    private static void AssignToParent(NodeTag tag, JsonNode? value)
    {
        if (tag.Parent is JsonObject po) po[(string)tag.Key!] = value;
        else if (tag.Parent is JsonArray pa) pa[(int)tag.Key!] = value;
    }

    private void btnAddEl_Click(object sender, RoutedEventArgs e)
    {
        if (tree.SelectedItem is not TreeViewItem item) return;
        var tag = (NodeTag)item.Tag;
        if (tag.Node is not JsonArray arr) return;
        JsonNode newEl = arr.Count > 0 && arr[0] != null
            ? JsonNode.Parse(arr[0]!.ToJsonString())!
            : JsonValue.Create("")!;
        arr.Add(newEl);
        RebuildChildren(item);
        item.IsExpanded = true;
        Status(Loc.T("status_el_added"));
    }

    private void btnAddField_Click(object sender, RoutedEventArgs e)
    {
        if (tree.SelectedItem is not TreeViewItem item) return;
        var tag = (NodeTag)item.Tag;
        if (tag.Node is not JsonObject obj) return;
        string? name = Prompt.Show(this, Loc.T("prompt_field_name"));
        if (string.IsNullOrWhiteSpace(name) || obj.ContainsKey(name)) return;
        obj[name] = "";
        RebuildChildren(item);
        item.IsExpanded = true;
        Status(string.Format(Loc.T("status_field_added"), name));
    }

    private void btnDelete_Click(object sender, RoutedEventArgs e)
    {
        if (tree.SelectedItem is not TreeViewItem item) return;
        var tag = (NodeTag)item.Tag;
        if (tag.Parent == null) return;
        if (MessageBox.Show(Loc.T("confirm_delete_text"), Loc.T("confirm_delete_title"),
                MessageBoxButton.YesNo, MessageBoxImage.Warning) != MessageBoxResult.Yes) return;
        if (tag.Parent is JsonObject po) po.Remove((string)tag.Key!);
        else if (tag.Parent is JsonArray pa) pa.RemoveAt((int)tag.Key!);
        if (ItemsControl.ItemsControlFromItemContainer(item) is TreeViewItem parentItem)
            RebuildChildren(parentItem);
        else BuildTree();
        Status(Loc.T("status_deleted"));
    }
}

public class NodeTag
{
    public JsonNode? Parent;
    public object? Key;     // string (objeto) o int (índice de array); null en la raíz
    public JsonNode? Node;
}

public class TheoryRow
{
    public int No { get; set; }
    public string Title => TheoryNames.Get(No);
    public int GetCount { get; set; }
    public int CheckedGetCount { get; set; }
    public long Seed { get; set; }
}

public class ScoreRow
{
    public string Name { get; set; } = "";
    public string Title => SongNames.Get(Name);
    public int Level { get; set; }
    public long Point { get; set; }
    public int Combo { get; set; }
    public bool FullCombo { get; set; }
    public int Rank { get; set; }
    public double Rate { get; set; }
}

internal static class JsonExt
{
    public static string GetStr(this JsonObject o, string k) =>
        o[k] is JsonNode n && n.GetValueKind() == JsonValueKind.String ? n.GetValue<string>() : (o[k]?.ToString() ?? "");
    public static long GetLong(this JsonObject o, string k) =>
        o[k] is JsonNode n && n.GetValueKind() == JsonValueKind.Number && long.TryParse(n.ToJsonString(), out long v) ? v : 0;
    public static double GetDbl(this JsonObject o, string k) =>
        o[k] is JsonNode n && n.GetValueKind() == JsonValueKind.Number &&
        double.TryParse(n.ToJsonString(), NumberStyles.Any, CultureInfo.InvariantCulture, out double v) ? v : 0;
    public static bool GetBool(this JsonObject o, string k) =>
        o[k] is JsonNode n && n.GetValueKind() is JsonValueKind.True or JsonValueKind.False && n.GetValue<bool>();
}
