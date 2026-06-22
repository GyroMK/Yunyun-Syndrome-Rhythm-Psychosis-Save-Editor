using System;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace YunyunSaveEditor;

/// <summary>
/// Replica el cálculo de denpa del juego.
/// Total = DenpaPlayPoint + (canciones) + (teorías) + (finales).
/// El % mostrado se obtiene mapeando ese total con la tabla DeonpaLevel.
/// </summary>
public static class Denpa
{
    /// <summary>% mostrado en el juego para un total de puntos (réplica de GetDenpaLevel).</summary>
    public static int LevelForPoints(long total)
    {
        long point = total;
        for (int p = 1; p <= 100; p++)
        {
            long band = DenpaData.CumPoints[p] - DenpaData.CumPoints[p - 1];
            if (point >= band) { point -= band; continue; }
            return p - 1;
        }
        return 100;
    }

    /// <summary>Puntos que aportan canciones + teorías + finales (todo menos DenpaPlayPoint).</summary>
    public static long PointsExcludingPlay(JsonObject root)
    {
        long sum = 0;
        if (root?["ScoreRecords"]?["List"] is JsonArray songs)
            foreach (var n in songs) if (n is JsonObject o) sum += SongPoints(o);
        if (root?["ConspiracyTheory"]?["List"] is JsonArray theories)
            foreach (var n in theories) if (n is JsonObject o) sum += TheoryPoints(o);
        if (root?["EndingData"] is JsonArray endings)
            foreach (var n in endings)
                if (n is JsonObject o && GetLong(o, "Status") != 0) sum += DenpaData.EndingPoint;
        return sum;
    }

    public static long Total(JsonObject root) => GetLong(root, "DenpaPlayPoint") + PointsExcludingPlay(root);

    public static int CurrentPercent(JsonObject root) => LevelForPoints(Total(root));

    /// <summary>DenpaPlayPoint necesario para mostrar <paramref name="targetPct"/> (con el resto fijo).</summary>
    public static long PlayPointForPercent(JsonObject root, int targetPct)
    {
        long need = DenpaData.CumPoints[Math.Clamp(targetPct, 0, 100)] - PointsExcludingPlay(root);
        return need < 0 ? 0 : need;
    }

    private static long SongPoints(JsonObject o)
    {
        long pts = 0;
        if (GetLong(o, "Point") > 0) pts += DenpaData.AchFirstClear;
        if (GetBool(o, "FullCombo")) pts += DenpaData.AchFullCombo;
        int rank = (int)GetLong(o, "Rank");      // RecordRank: None=0, S=1, A=2, B=3, C=4, D=5
        if (rank == 1 || rank == 2) pts += DenpaData.AchRankA;
        if (GetDouble(o, "Rate") >= DenpaData.Rate95) pts += DenpaData.AchRate95;
        return pts;
    }

    private static long TheoryPoints(JsonObject o)
    {
        int getCount = (int)GetLong(o, "GetCount");
        if (getCount <= 0) return 0;
        if (!DenpaData.SlotRarity.TryGetValue((int)GetLong(o, "No"), out int rarity)) return 0;
        int level = Math.Clamp(getCount, 1, 3);
        long pts = 0;
        for (int i = 0; i < level; i++)
        {
            int r = i + rarity;
            if (r >= 1 && r <= 5) pts += DenpaData.KaibunshoRank[r];
        }
        return pts;
    }

    private static long GetLong(JsonObject o, string k) =>
        o != null && o[k] is JsonNode n && n.GetValueKind() == JsonValueKind.Number
            && long.TryParse(n.ToJsonString(), out long v) ? v : 0;

    private static double GetDouble(JsonObject o, string k) =>
        o != null && o[k] is JsonNode n && n.GetValueKind() == JsonValueKind.Number
            && double.TryParse(n.ToJsonString(), NumberStyles.Any, CultureInfo.InvariantCulture, out double v) ? v : 0;

    private static bool GetBool(JsonObject o, string k) =>
        o != null && o[k] is JsonNode n && n.GetValueKind() == JsonValueKind.True;
}
