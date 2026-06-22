using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Data;
using System.Windows.Markup;

namespace YunyunSaveEditor;

/// <summary>
/// Localización en tiempo de ejecución. Las cadenas se traducen mediante un
/// indexador (this[key]); al cambiar de idioma se notifica "Item[]" y todos los
/// bindings de la interfaz se refrescan en vivo.
///
/// Para añadir un idioma nuevo: añade su código a <see cref="Available"/> y un
/// diccionario en <see cref="Strings"/> con las mismas claves.
/// </summary>
public sealed class Loc : INotifyPropertyChanged
{
    public static Loc Instance { get; } = new();

    public event PropertyChangedEventHandler? PropertyChanged;

    /// <summary>Idiomas disponibles (código ISO, nombre mostrado).</summary>
    public static readonly (string Code, string Name)[] Available =
    {
        ("es", "Español"),
        ("en", "English"),
        ("ja", "日本語"),
    };

    private string _lang = "es";
    public string Language
    {
        get => _lang;
        set
        {
            if (value == _lang || !Strings.ContainsKey(value)) return;
            _lang = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Item[]"));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Language)));
        }
    }

    public string this[string key]
    {
        get
        {
            if (Strings.TryGetValue(_lang, out var d) && d.TryGetValue(key, out var s)) return s;
            if (Strings["en"].TryGetValue(key, out var en)) return en;
            return key;
        }
    }

    /// <summary>Atajo para el code-behind: Loc.T("clave").</summary>
    public static string T(string key) => Instance[key];

    private static readonly Dictionary<string, Dictionary<string, string>> Strings = new()
    {
        ["es"] = new()
        {
            ["window_title"] = "Yunyun Syndrome · Save Editor",
            ["header_subtitle"] = "Edita tu progreso, canciones y desbloqueos. Se crea una copia de seguridad automática al guardar.",
            ["lbl_lang"] = "Idioma:",
            ["lbl_file"] = "Archivo:",
            ["btn_open"] = "Abrir / Recargar",
            ["btn_save"] = "💾  Guardar en el juego",
            ["btn_folder"] = "Cambiar carpeta…",
            ["status_ready"] = "Listo. Elige un archivo y pulsa «Abrir / Recargar».",
            ["tab_progress"] = "Progreso",
            ["card_denpa_title"] = "Porcentaje de Denpa (la barra del menú)",
            ["card_denpa_desc"] = "El % sale del total: canciones + teorías + finales + DenpaPlayPoint. Escribe el % que quieres y pulsa «Aplicar %»; el editor calcula los puntos por ti.",
            ["denpa_now"] = "Denpa actual: {0}%  ({1} pts en total)",
            ["lbl_denpa_pct"] = "Poner denpa al %:",
            ["btn_denpa_pct"] = "Aplicar %",
            ["denpa_applied"] = "DenpaPlayPoint = {0}  →  denpa ≈ {1}%. Pulsa Guardar.",
            ["col_title"] = "Título",
            ["tab_theories"] = "Teorías",
            ["col_no"] = "Nº",
            ["col_theory"] = "Teoría",
            ["col_getcount"] = "Conseguida (0-3)",
            ["col_checked"] = "Vista",
            ["theories_hint"] = "«Conseguida» 1-3 marca la teoría (sube el % de denpa). Los cambios se aplican al pulsar «Guardar en el juego».",
            ["card_quick_title"] = "Acciones rápidas",
            ["btn_songs_clear"] = "Completar todas las canciones",
            ["btn_songs_unlock"] = "Desbloquear todas las canciones",
            ["btn_theories"] = "Conseguir todas las teorías",
            ["lbl_rank"] = "Rank al completar:",
            ["card_params_title"] = "Parámetros",
            ["card_flags_title"] = "Banderas principales",
            ["lbl_follower"] = "FollowerCount:",
            ["tab_songs"] = "Canciones",
            ["songs_hint"] = "Edita directamente en la tabla (doble clic en una celda). Los cambios se aplican al pulsar «Guardar en el juego».",
            ["col_song"] = "Canción",
            ["col_level"] = "Nivel",
            ["col_points"] = "Puntos",
            ["col_combo"] = "Combo",
            ["col_fullcombo"] = "FullCombo",
            ["col_rank"] = "Rank",
            ["col_rate"] = "Rate",
            ["tab_advanced"] = "Avanzado (todo)",
            ["adv_select"] = "Selecciona un campo del árbol.",
            ["lbl_type"] = "Tipo:",
            ["type_text"] = "Texto",
            ["type_number"] = "Número",
            ["type_bool"] = "Booleano",
            ["type_null"] = "Null",
            ["lbl_value"] = "Valor:",
            ["btn_apply"] = "Aplicar cambio",
            ["btn_add_el"] = "+ Elemento",
            ["btn_add_field"] = "+ Campo",
            ["btn_delete"] = "Eliminar",
            ["btn_ok"] = "Aceptar",
            ["btn_cancel"] = "Cancelar",
            ["app_name"] = "Yunyun Save Editor",
            ["msg_folder_not_found"] = "No encuentro la carpeta de guardados automáticamente:\n{0}\n\nPulsa «Cambiar carpeta…» y elige la carpeta 'player'.",
            ["status_folder"] = "Carpeta: {0}",
            ["status_no_file"] = "No hay archivo seleccionado.",
            ["status_not_exist"] = "No existe: {0}",
            ["status_loaded"] = "Cargado «{0}». Edita en las pestañas y pulsa Guardar.",
            ["status_open_error"] = "Error al abrir: {0}",
            ["status_open_first"] = "Primero abre un archivo.",
            ["status_saved"] = "✔ Guardado en el juego: «{0}». Copia: {1}",
            ["status_save_error"] = "Error al guardar: {0}",
            ["status_field_updated"] = "Campo actualizado. Pulsa Guardar para aplicarlo al juego.",
            ["status_num_invalid"] = "Número no válido (usa punto decimal).",
            ["status_songs_marked"] = "Marcadas {0} canciones como completadas (Rank {1}). Pulsa Guardar.",
            ["status_songs_unlocked"] = "Desbloqueadas {0} canciones. Pulsa Guardar.",
            ["status_theories_marked"] = "Marcadas {0} teorías. Pulsa Guardar.",
            ["status_el_added"] = "Elemento añadido. Edítalo y pulsa Guardar.",
            ["status_field_added"] = "Campo «{0}» añadido (vacío). Selecciónalo para darle valor.",
            ["status_deleted"] = "Eliminado. Pulsa Guardar para aplicarlo.",
            ["confirm_delete_text"] = "¿Eliminar este elemento/campo?",
            ["confirm_delete_title"] = "Confirmar",
            ["prompt_field_name"] = "Nombre del nuevo campo:",
            ["path_prefix"] = "Ruta: ",
        },
        ["en"] = new()
        {
            ["window_title"] = "Yunyun Syndrome · Save Editor",
            ["header_subtitle"] = "Edit your progress, songs and unlocks. A backup is created automatically when you save.",
            ["lbl_lang"] = "Language:",
            ["lbl_file"] = "File:",
            ["btn_open"] = "Open / Reload",
            ["btn_save"] = "💾  Save to game",
            ["btn_folder"] = "Change folder…",
            ["status_ready"] = "Ready. Choose a file and click “Open / Reload”.",
            ["tab_progress"] = "Progress",
            ["card_denpa_title"] = "Denpa percentage (the menu bar)",
            ["card_denpa_desc"] = "The % comes from the total: songs + theories + endings + DenpaPlayPoint. Type the % you want and click “Apply %”; the editor computes the points for you.",
            ["denpa_now"] = "Current denpa: {0}%  ({1} pts total)",
            ["lbl_denpa_pct"] = "Set denpa to %:",
            ["btn_denpa_pct"] = "Apply %",
            ["denpa_applied"] = "DenpaPlayPoint = {0}  →  denpa ≈ {1}%. Click Save.",
            ["col_title"] = "Title",
            ["tab_theories"] = "Theories",
            ["col_no"] = "No.",
            ["col_theory"] = "Theory",
            ["col_getcount"] = "Obtained (0-3)",
            ["col_checked"] = "Seen",
            ["theories_hint"] = "“Obtained” 1-3 marks the theory (raises denpa %). Changes apply when you click “Save to game”.",
            ["card_quick_title"] = "Quick actions",
            ["btn_songs_clear"] = "Complete all songs",
            ["btn_songs_unlock"] = "Unlock all songs",
            ["btn_theories"] = "Get all theories",
            ["lbl_rank"] = "Rank when completing:",
            ["card_params_title"] = "Parameters",
            ["card_flags_title"] = "Main flags",
            ["lbl_follower"] = "FollowerCount:",
            ["tab_songs"] = "Songs",
            ["songs_hint"] = "Edit directly in the table (double-click a cell). Changes apply when you click “Save to game”.",
            ["col_song"] = "Song",
            ["col_level"] = "Level",
            ["col_points"] = "Points",
            ["col_combo"] = "Combo",
            ["col_fullcombo"] = "FullCombo",
            ["col_rank"] = "Rank",
            ["col_rate"] = "Rate",
            ["tab_advanced"] = "Advanced (everything)",
            ["adv_select"] = "Select a field in the tree.",
            ["lbl_type"] = "Type:",
            ["type_text"] = "Text",
            ["type_number"] = "Number",
            ["type_bool"] = "Boolean",
            ["type_null"] = "Null",
            ["lbl_value"] = "Value:",
            ["btn_apply"] = "Apply change",
            ["btn_add_el"] = "+ Element",
            ["btn_add_field"] = "+ Field",
            ["btn_delete"] = "Delete",
            ["btn_ok"] = "OK",
            ["btn_cancel"] = "Cancel",
            ["app_name"] = "Yunyun Save Editor",
            ["msg_folder_not_found"] = "Could not find the save folder automatically:\n{0}\n\nClick “Change folder…” and select the 'player' folder.",
            ["status_folder"] = "Folder: {0}",
            ["status_no_file"] = "No file selected.",
            ["status_not_exist"] = "Does not exist: {0}",
            ["status_loaded"] = "Loaded “{0}”. Edit in the tabs and click Save.",
            ["status_open_error"] = "Error opening: {0}",
            ["status_open_first"] = "Open a file first.",
            ["status_saved"] = "✔ Saved to game: “{0}”. Backup: {1}",
            ["status_save_error"] = "Error saving: {0}",
            ["status_field_updated"] = "Field updated. Click Save to apply it to the game.",
            ["status_num_invalid"] = "Invalid number (use a decimal point).",
            ["status_songs_marked"] = "Marked {0} songs as completed (Rank {1}). Click Save.",
            ["status_songs_unlocked"] = "Unlocked {0} songs. Click Save.",
            ["status_theories_marked"] = "Marked {0} theories. Click Save.",
            ["status_el_added"] = "Element added. Edit it and click Save.",
            ["status_field_added"] = "Field “{0}” added (empty). Select it to set a value.",
            ["status_deleted"] = "Deleted. Click Save to apply.",
            ["confirm_delete_text"] = "Delete this element/field?",
            ["confirm_delete_title"] = "Confirm",
            ["prompt_field_name"] = "Name of the new field:",
            ["path_prefix"] = "Path: ",
        },
        ["ja"] = new()
        {
            ["window_title"] = "Yunyun Syndrome · セーブエディター",
            ["header_subtitle"] = "進行度・楽曲・アンロックを編集できます。保存時に自動でバックアップを作成します。",
            ["lbl_lang"] = "言語:",
            ["lbl_file"] = "ファイル:",
            ["btn_open"] = "開く / 再読み込み",
            ["btn_save"] = "💾  ゲームに保存",
            ["btn_folder"] = "フォルダーを変更…",
            ["status_ready"] = "準備完了。ファイルを選んで「開く / 再読み込み」を押してください。",
            ["tab_progress"] = "進行度",
            ["card_denpa_title"] = "デンパ％（メニューのバー）",
            ["card_denpa_desc"] = "％は合計（楽曲＋考察＋エンディング＋DenpaPlayPoint）から算出されます。目標の％を入力して「％を適用」を押すと、必要な値を自動計算します。",
            ["denpa_now"] = "現在のデンパ: {0}%（合計 {1} pts）",
            ["lbl_denpa_pct"] = "デンパを％に設定:",
            ["btn_denpa_pct"] = "％を適用",
            ["denpa_applied"] = "DenpaPlayPoint = {0}  →  デンパ ≈ {1}%。保存を押してください。",
            ["col_title"] = "タイトル",
            ["tab_theories"] = "考察",
            ["col_no"] = "番号",
            ["col_theory"] = "考察",
            ["col_getcount"] = "入手 (0-3)",
            ["col_checked"] = "確認済み",
            ["theories_hint"] = "「入手」1-3で考察を獲得済みにします（デンパ％が上がります）。「ゲームに保存」で反映されます。",
            ["card_quick_title"] = "クイック操作",
            ["btn_songs_clear"] = "全楽曲をクリアにする",
            ["btn_songs_unlock"] = "全楽曲をアンロック",
            ["btn_theories"] = "全ての考察を入手",
            ["lbl_rank"] = "クリア時のランク:",
            ["card_params_title"] = "パラメータ",
            ["card_flags_title"] = "主要フラグ",
            ["lbl_follower"] = "FollowerCount:",
            ["tab_songs"] = "楽曲",
            ["songs_hint"] = "表で直接編集できます（セルをダブルクリック）。「ゲームに保存」を押すと反映されます。",
            ["col_song"] = "楽曲",
            ["col_level"] = "レベル",
            ["col_points"] = "スコア",
            ["col_combo"] = "コンボ",
            ["col_fullcombo"] = "フルコンボ",
            ["col_rank"] = "ランク",
            ["col_rate"] = "レート",
            ["tab_advanced"] = "詳細（すべて）",
            ["adv_select"] = "ツリーから項目を選択してください。",
            ["lbl_type"] = "型:",
            ["type_text"] = "テキスト",
            ["type_number"] = "数値",
            ["type_bool"] = "真偽値",
            ["type_null"] = "Null",
            ["lbl_value"] = "値:",
            ["btn_apply"] = "変更を適用",
            ["btn_add_el"] = "+ 要素",
            ["btn_add_field"] = "+ フィールド",
            ["btn_delete"] = "削除",
            ["btn_ok"] = "OK",
            ["btn_cancel"] = "キャンセル",
            ["app_name"] = "Yunyun Save Editor",
            ["msg_folder_not_found"] = "保存フォルダーを自動で見つけられませんでした:\n{0}\n\n「フォルダーを変更…」を押して 'player' フォルダーを選んでください。",
            ["status_folder"] = "フォルダー: {0}",
            ["status_no_file"] = "ファイルが選択されていません。",
            ["status_not_exist"] = "存在しません: {0}",
            ["status_loaded"] = "「{0}」を読み込みました。タブで編集して保存を押してください。",
            ["status_open_error"] = "オープン失敗: {0}",
            ["status_open_first"] = "先にファイルを開いてください。",
            ["status_saved"] = "✔ ゲームに保存しました: 「{0}」。バックアップ: {1}",
            ["status_save_error"] = "保存失敗: {0}",
            ["status_field_updated"] = "項目を更新しました。保存を押すとゲームに反映されます。",
            ["status_num_invalid"] = "数値が無効です（小数点はピリオド）。",
            ["status_songs_marked"] = "{0} 曲をクリア済みにしました（ランク {1}）。保存を押してください。",
            ["status_songs_unlocked"] = "{0} 曲をアンロックしました。保存を押してください。",
            ["status_theories_marked"] = "{0} 件の考察を入手にしました。保存を押してください。",
            ["status_el_added"] = "要素を追加しました。編集して保存してください。",
            ["status_field_added"] = "フィールド「{0}」を追加しました（空）。選択して値を設定してください。",
            ["status_deleted"] = "削除しました。保存を押すと反映されます。",
            ["confirm_delete_text"] = "この要素／フィールドを削除しますか？",
            ["confirm_delete_title"] = "確認",
            ["prompt_field_name"] = "新しいフィールドの名前:",
            ["path_prefix"] = "パス: ",
        },
    };
}

/// <summary>
/// Extensión de marcado para XAML: <c>Text="{loc:Tr clave}"</c>.
/// Crea un binding al indexador de <see cref="Loc.Instance"/> que se actualiza
/// automáticamente al cambiar de idioma.
/// </summary>
public sealed class TrExtension : MarkupExtension
{
    public string Key { get; set; } = "";

    public TrExtension() { }
    public TrExtension(string key) { Key = key; }

    public override object ProvideValue(IServiceProvider serviceProvider)
    {
        var binding = new Binding($"[{Key}]")
        {
            Source = Loc.Instance,
            Mode = BindingMode.OneWay,
        };
        return binding.ProvideValue(serviceProvider);
    }
}
