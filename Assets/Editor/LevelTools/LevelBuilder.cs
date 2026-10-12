using System.Collections.Generic;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

// Seviyeleri "yol parçaları" (düz / yay) olarak tarif edip engelleri otomatik dizen ve
// aracın izini süpürerek geçilebilirliği ölçen editör aracı.
// Koordinatlar (x, z) düzleminde, yön (yaw) derece olarak Unity'deki gibidir: ileri = (sin yaw, cos yaw).

public enum LevelPiece { Cone, Barrier, Bollard }

public struct LevelSeg
{
    public float length, radius, turnDeg;

    public static LevelSeg Straight(float length)
    {
        return new LevelSeg { length = length };
    }

    /// <summary>deg &gt; 0 sağa, deg &lt; 0 sola dönüş. radius = arka aks izinin yarıçapı.</summary>
    public static LevelSeg Arc(float radius, float deg)
    {
        return new LevelSeg { radius = radius, turnDeg = deg, length = radius * Mathf.Abs(deg) * Mathf.Deg2Rad };
    }
}

public class LevelExtraLine
{
    public Vector2 a, b;
    public LevelPiece piece;
}

public class LevelSpec
{
    public string name;
    public Vector2 carStart;                 // araç pivotu (x, z)
    public float carYaw;                     // derece
    public List<LevelSeg> segs = new List<LevelSeg>();
    public List<Vector2> widthKeys = new List<Vector2>();   // (s, koridor genişliği)
    public LevelPiece leftPiece = LevelPiece.Cone;
    public LevelPiece rightPiece = LevelPiece.Cone;
    public LevelPiece capPiece = LevelPiece.Barrier;
    public float tail = 4.5f;                // park noktasından sonra koridorun devamı
    public float startCapBack = 3.5f;        // başlangıç kapağının arka aksın ne kadar gerisinde olacağı
    public float starMul3 = 3f, starMul2 = 5f;  // yıldız süreleri = par süre * çarpan
    public List<LevelExtraLine> lines = new List<LevelExtraLine>();
}

public static class LevelBuilder
{
    // Car.prefab'tan ölçülen değerler
    const float RearToOrigin = 1.05f;        // araç pivotu arka aksın 1.05 m önünde
    const float HullCenterY = 0.75f, HullHalfY = 0.55f, HullCenterZ = 0.14f, HullHalfX = 0.97f, HullHalfZ = 2.26f; // fiziksel gövde
    const float TrigCenterZ = 0.10f, TrigHalfX = 0.75f, TrigHalfZ = 2.125f;   // park kontrolünün kullandığı kutu
    const float AreaHalfX = 1.25f, AreaHalfZ = 2.5f, FitTol = 0.1f;           // park alanı (ölçek 0.25 x 0.5)

    public class Sample { public Vector2 p; public float yaw, s, kappa; }
    public class PathData { public List<Sample> pts = new List<Sample>(); public int driveCount; public float driveLen; }
    public struct Item { public LevelPiece piece; public Vector2 pos; public float yawDeg; }

    // ---------------------------------------------------------------- yol
    public static PathData BuildPath(Vector2 rear0, float yawDeg, List<LevelSeg> segs, float tail)
    {
        var path = new PathData();
        Vector2 p = rear0;
        float yaw = yawDeg * Mathf.Deg2Rad;
        float s = 0f;
        path.pts.Add(new Sample { p = p, yaw = yaw, s = 0f, kappa = 0f });
        foreach (var sg in segs)
        {
            float k = Mathf.Approximately(sg.turnDeg, 0f) ? 0f : Mathf.Sign(sg.turnDeg) / sg.radius;
            March(path, sg.length, k, ref p, ref yaw, ref s);
        }
        path.driveCount = path.pts.Count;
        path.driveLen = s;
        March(path, tail, 0f, ref p, ref yaw, ref s);
        return path;
    }

    static void March(PathData path, float length, float kappa, ref Vector2 p, ref float yaw, ref float s)
    {
        int n = Mathf.Max(1, Mathf.CeilToInt(length / 0.25f));
        float ds = length / n;
        for (int i = 0; i < n; i++)
        {
            float mid = yaw + kappa * ds * 0.5f;
            float chord = Mathf.Abs(kappa) < 1e-5f ? ds : 2f * Mathf.Sin(kappa * ds * 0.5f) / kappa;
            p += new Vector2(Mathf.Sin(mid), Mathf.Cos(mid)) * chord;
            yaw += kappa * ds;
            s += ds;
            path.pts.Add(new Sample { p = p, yaw = yaw, s = s, kappa = kappa });
        }
    }

    static float WidthAt(List<Vector2> keys, float s)
    {
        if (s <= keys[0].x) return keys[0].y;
        for (int i = 1; i < keys.Count; i++)
            if (s <= keys[i].x)
                return Mathf.Lerp(keys[i - 1].y, keys[i].y, Mathf.InverseLerp(keys[i - 1].x, keys[i].x, s));
        return keys[keys.Count - 1].y;
    }

    static Vector2 RightOf(float yaw) { return new Vector2(Mathf.Cos(yaw), -Mathf.Sin(yaw)); }
    static Vector2 FwdOf(float yaw) { return new Vector2(Mathf.Sin(yaw), Mathf.Cos(yaw)); }

    // ---------------------------------------------------------------- engeller
    static float Spacing(LevelPiece p)
    {
        if (p == LevelPiece.Cone) return 1.5f;
        if (p == LevelPiece.Barrier) return 1.86f;
        return 2.2f;
    }

    static void Wall(PathData path, LevelSpec spec, int side, LevelPiece piece, List<Item> items)
    {
        float spacing = Spacing(piece);
        float acc = spacing;
        Vector2 prev = Vector2.zero;
        bool first = true;
        foreach (var smp in path.pts)
        {
            Vector2 pos = smp.p + RightOf(smp.yaw) * side * WidthAt(spec.widthKeys, smp.s) * 0.5f;
            if (!first) acc += Vector2.Distance(prev, pos);
            first = false;
            prev = pos;
            if (acc >= spacing)
            {
                acc -= spacing;
                items.Add(new Item { piece = piece, pos = pos, yawDeg = smp.yaw * Mathf.Rad2Deg });
            }
        }
    }

    static void Row(Vector2 a, Vector2 b, LevelPiece piece, List<Item> items)
    {
        float len = Vector2.Distance(a, b);
        float sp = Spacing(piece);
        int n = piece == LevelPiece.Barrier ? Mathf.Max(1, Mathf.FloorToInt(len / sp)) : Mathf.Max(1, Mathf.RoundToInt(len / sp));
        Vector2 d = (b - a).normalized;
        float yaw = Mathf.Atan2(d.x, d.y) * Mathf.Rad2Deg;
        for (int i = 0; i <= n; i++)
            items.Add(new Item { piece = piece, pos = Vector2.Lerp(a, b, (float)i / n), yawDeg = yaw });
    }

    static GameObject Load(LevelPiece p)
    {
        string n = p == LevelPiece.Cone ? "TrafficCones" : p == LevelPiece.Barrier ? "RoadBarrier-2" : "RoadBollard-2";
        return AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Obstacles/" + n + ".prefab");
    }

    // ---------------------------------------------------------------- doğrulama
    static bool Hit(Vector2 hullCenter, float yaw, float margin, Transform root)
    {
        var cols = Physics.OverlapBox(new Vector3(hullCenter.x, HullCenterY, hullCenter.y),
            new Vector3(HullHalfX + margin, HullHalfY, HullHalfZ + margin),
            Quaternion.Euler(0f, yaw * Mathf.Rad2Deg, 0f), ~0, QueryTriggerInteraction.Ignore);
        foreach (var c in cols)
            if (c.transform.IsChildOf(root)) return true;
        return false;
    }

    static string Sweep(PathData path, Transform root)
    {
        float[] margins = { 0f, 0.25f, 0.5f, 0.75f, 1.0f, 1.5f };
        float worst = 99f, worstS = 0f;
        for (int i = 0; i < path.driveCount; i++)
        {
            var sp = path.pts[i];
            Vector2 c = sp.p + FwdOf(sp.yaw) * (RearToOrigin + HullCenterZ);
            float ok = -0.25f;
            foreach (var m in margins)
            {
                if (Hit(c, sp.yaw, m, root)) break;
                ok = m;
            }
            if (ok < worst) { worst = ok; worstS = sp.s; }
        }
        if (worst < 0f) return "ÇARPIŞMA VAR (s=" + worstS.ToString("F1") + " m civarı)";
        return "en dar yerde gövde payı >= " + worst.ToString("F2") + " m (s=" + worstS.ToString("F1") + " m)";
    }

    static bool Fits(Vector2 center, float yaw, Vector2 areaCenter, float areaYaw, out float slack)
    {
        Vector2 f = FwdOf(yaw), r = RightOf(yaw);
        Vector2 aF = FwdOf(areaYaw), aR = RightOf(areaYaw);
        slack = float.MaxValue;
        for (int sx = -1; sx <= 1; sx += 2)
            for (int sz = -1; sz <= 1; sz += 2)
            {
                Vector2 corner = center + r * (sx * TrigHalfX) + f * (sz * TrigHalfZ);
                Vector2 d = corner - areaCenter;
                slack = Mathf.Min(slack, AreaHalfX + FitTol - Mathf.Abs(Vector2.Dot(d, aR)), AreaHalfZ + FitTol - Mathf.Abs(Vector2.Dot(d, aF)));
            }
        return slack >= 0f && Vector2.Dot(aF, f) >= 0f;
    }

    static string ParkWindow(Vector2 areaCenter, float areaYaw)
    {
        float s;
        FwdOf(areaYaw);
        Vector2 aR = RightOf(areaYaw), aF = FwdOf(areaYaw);
        float lat = 0f, lon = 0f, ang = 0f;
        for (float d = 0f; d < 2f; d += 0.01f)
        {
            if (!Fits(areaCenter + aR * d, areaYaw, areaCenter, areaYaw, out s) || !Fits(areaCenter - aR * d, areaYaw, areaCenter, areaYaw, out s)) break;
            lat = d;
        }
        for (float d = 0f; d < 2f; d += 0.01f)
        {
            if (!Fits(areaCenter + aF * d, areaYaw, areaCenter, areaYaw, out s) || !Fits(areaCenter - aF * d, areaYaw, areaCenter, areaYaw, out s)) break;
            lon = d;
        }
        for (float d = 0f; d < 60f; d += 0.25f)
        {
            if (!Fits(areaCenter, areaYaw + d * Mathf.Deg2Rad, areaCenter, areaYaw, out s) || !Fits(areaCenter, areaYaw - d * Mathf.Deg2Rad, areaCenter, areaYaw, out s)) break;
            ang = d;
        }
        return "park toleransı: yanal +-" + lat.ToString("F2") + " m, boyuna +-" + lon.ToString("F2") + " m, açı +-" + ang.ToString("F0") + " derece";
    }

    public static float ParSeconds(PathData path)
    {
        const float vMax = 6f, aLat = 1.8f, approachDist = 8f, approachV = 2.5f;
        float t = 1.5f + 2f;   // kalkış + 2 sn park bekleme
        for (int i = 1; i < path.driveCount; i++)
        {
            var b = path.pts[i];
            float ds = b.s - path.pts[i - 1].s;
            float v = vMax;
            if (Mathf.Abs(b.kappa) > 1e-4f) v = Mathf.Min(v, Mathf.Sqrt(aLat / Mathf.Abs(b.kappa)));
            if (path.driveLen - b.s < approachDist) v = Mathf.Min(v, approachV);
            t += ds / v;
        }
        return t;
    }

    // ---------------------------------------------------------------- ana giriş
    public static string Build(int level, LevelSpec spec, bool save)
    {
        var scene = EditorSceneManager.OpenScene("Assets/Scenes/Levels/" + level + ".unity", OpenSceneMode.Single);
        var rootGo = GameObject.Find("Obstacles");
        var root = rootGo.transform;
        for (int i = root.childCount - 1; i >= 0; i--) Object.DestroyImmediate(root.GetChild(i).gameObject);

        // araç
        var car = GameObject.Find("Car");
        float yawRad = spec.carYaw * Mathf.Deg2Rad;
        Vector2 fwd = FwdOf(yawRad);
        car.transform.SetPositionAndRotation(new Vector3(spec.carStart.x, car.transform.position.y, spec.carStart.y), Quaternion.Euler(0f, spec.carYaw, 0f));
        Vector2 rear0 = spec.carStart - fwd * RearToOrigin;

        // yol + engeller
        var path = BuildPath(rear0, spec.carYaw, spec.segs, spec.tail);
        var items = new List<Item>();
        Wall(path, spec, -1, spec.leftPiece, items);
        Wall(path, spec, +1, spec.rightPiece, items);
        {
            Vector2 rv = RightOf(yawRad);
            float w = WidthAt(spec.widthKeys, 0f);
            Vector2 b = rear0 - fwd * spec.startCapBack;
            Row(b - rv * w * 0.5f, b + rv * w * 0.5f, spec.capPiece, items);
        }
        {
            var e = path.pts[path.pts.Count - 1];
            Vector2 rv = RightOf(e.yaw);
            float w = WidthAt(spec.widthKeys, e.s);
            Row(e.p - rv * w * 0.5f, e.p + rv * w * 0.5f, spec.capPiece, items);
        }
        foreach (var l in spec.lines) Row(l.a, l.b, l.piece, items);

        int idx = 0;
        foreach (var it in items)
        {
            var go = (GameObject)PrefabUtility.InstantiatePrefab(Load(it.piece), root);
            float rotY = it.piece == LevelPiece.Barrier ? it.yawDeg - 90f : it.yawDeg;
            go.transform.SetPositionAndRotation(new Vector3(it.pos.x, 0f, it.pos.y), Quaternion.Euler(0f, rotY, 0f));
            go.name = it.piece + "_" + (idx++);
        }

        // park alanı: aracın son pozunda, kutusu alanın tam ortasına gelecek şekilde
        var pa = GameObject.Find("ParkingArea");
        var last = path.pts[path.driveCount - 1];
        Vector2 areaCenter = last.p + FwdOf(last.yaw) * (RearToOrigin + TrigCenterZ);
        float areaYaw = last.yaw;
        pa.transform.SetPositionAndRotation(new Vector3(areaCenter.x, pa.transform.position.y, areaCenter.y), Quaternion.Euler(0f, areaYaw * Mathf.Rad2Deg, 0f));

        // yıldız süreleri
        float par = ParSeconds(path);
        int t3 = Mathf.Max(5, Mathf.RoundToInt(par * spec.starMul3 / 5f) * 5);
        int t2 = Mathf.Max(t3 + 5, Mathf.RoundToInt(par * spec.starMul2 / 5f) * 5);
        var sc = GameObject.Find("Managers").GetComponent<StarCounter>();
        sc._threeStarTime = t3;
        sc._twoStarTime = t2;
        EditorUtility.SetDirty(sc);

        // kamera, araca göre ilk konuma hizalanır (başlangıçtaki kayma olmasın)
        var cam = Camera.main;
        var camPos = car.transform.Find("CameraPositions");
        if (cam != null && camPos != null && camPos.childCount > 0)
            cam.transform.SetPositionAndRotation(camPos.GetChild(0).position, camPos.GetChild(0).rotation);

        Physics.SyncTransforms();

        // doğrulama raporu
        int cones = 0, barriers = 0, bollards = 0;
        foreach (var it in items)
        {
            if (it.piece == LevelPiece.Cone) cones++;
            else if (it.piece == LevelPiece.Barrier) barriers++;
            else bollards++;
        }
        float slack;
        bool fits = Fits(areaCenter, areaYaw, areaCenter, areaYaw, out slack);
        var sb = new StringBuilder();
        sb.AppendLine("Seviye " + level + " (" + spec.name + ")");
        sb.AppendLine("  yol uzunluğu: " + path.driveLen.ToString("F1") + " m, par süre: " + par.ToString("F1") + " sn -> 3 yıldız <= " + t3 + " sn, 2 yıldız <= " + t2 + " sn");
        sb.AppendLine("  engel: " + cones + " koni, " + barriers + " bariyer, " + bollards + " bolard");
        sb.AppendLine("  süpürme: " + Sweep(path, root));
        sb.AppendLine("  son poz park alanına sığıyor mu: " + fits + " (pay " + slack.ToString("F2") + " m)");
        sb.AppendLine("  " + ParkWindow(areaCenter, areaYaw));

        EditorSceneManager.MarkSceneDirty(scene);
        if (save) EditorSceneManager.SaveScene(scene);
        return sb.ToString();
    }
}

public static class LevelDefs
{
    public static LevelSpec Get(int level)
    {
        if (level == 1) return L1();
        if (level == 2) return L2();
        return L3();
    }

    static LevelSpec Base(string name)
    {
        var s = new LevelSpec { name = name, carStart = Vector2.zero, carYaw = 0f };
        return s;
    }

    // Seviye 1: gaz, direksiyon, fren. Hafif bir S virajı ve sonda yumuşak bir huni.
    public static LevelSpec L1()
    {
        var s = Base("Ilk Surus");
        s.segs.Add(LevelSeg.Straight(8f));
        s.segs.Add(LevelSeg.Arc(18f, 25f));
        s.segs.Add(LevelSeg.Arc(18f, -25f));
        s.segs.Add(LevelSeg.Straight(12f));
        s.widthKeys.AddRange(new[] { new Vector2(0f, 7f), new Vector2(24f, 7f), new Vector2(30f, 4.6f), new Vector2(100f, 4.6f) });
        s.starMul3 = 3f; s.starMul2 = 5f;
        return s;
    }

    // Seviye 2: iki 90 derece köşe (sağ, sonra sol), ardından park.
    public static LevelSpec L2()
    {
        var s = Base("Iki Kose");
        s.segs.Add(LevelSeg.Straight(8f));
        s.segs.Add(LevelSeg.Arc(7f, 90f));
        s.segs.Add(LevelSeg.Straight(9f));
        s.segs.Add(LevelSeg.Arc(7f, -90f));
        s.segs.Add(LevelSeg.Straight(11f));
        s.widthKeys.AddRange(new[] { new Vector2(0f, 6.5f), new Vector2(39f, 6.5f), new Vector2(44f, 4.4f), new Vector2(100f, 4.4f) });
        s.starMul3 = 2.8f; s.starMul2 = 4.5f;
        return s;
    }

    // Seviye 3: dar kapı, 180 derece U dönüşü ve dar park. Duvarlar ilk kez solid bariyer.
    public static LevelSpec L3()
    {
        var s = Base("U Donusu");
        s.segs.Add(LevelSeg.Straight(12f));
        s.segs.Add(LevelSeg.Arc(6.5f, -180f));
        s.segs.Add(LevelSeg.Straight(15f));
        s.leftPiece = LevelPiece.Barrier;
        s.rightPiece = LevelPiece.Barrier;
        s.widthKeys.AddRange(new[] {
            new Vector2(0f, 6.2f), new Vector2(3f, 6.2f), new Vector2(5f, 3.6f), new Vector2(8f, 3.6f), new Vector2(10f, 6.2f),
            new Vector2(38f, 6.2f), new Vector2(42f, 3.6f), new Vector2(100f, 3.6f) });
        s.starMul3 = 2.6f; s.starMul2 = 4.2f;
        return s;
    }
}

public static class LevelMenu
{
    [MenuItem("Tools/Parking Mastery/Seviye 1-3'u Yeniden Uret")]
    static void RebuildFirstThree()
    {
        for (int i = 1; i <= 3; i++) Debug.Log(LevelBuilder.Build(i, LevelDefs.Get(i), true));
    }
}
