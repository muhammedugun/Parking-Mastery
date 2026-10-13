using System.Collections.Generic;
using System.IO;
using ParkingMastery.Car;
using ParkingMastery.Core;
using ParkingMastery.Gameplay;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ParkingMastery.EditorTools
{
    /// <summary>
    /// Mevcut (eski) bir level sahnesinden LevelData üretir (B6). Sahne adı sayı olmalı ("1", "15"...).
    /// Okunanlar: LevelScoring eşikleri, Car konumu/yönü ve maxObstacleHits, ParkingZone (ParkingArea nesnesi) konumu/boyutu,
    /// 'Obstacles' altındaki PREFAB bağlantılı engeller. Prefab bağlantısı olmayan engeller atlanır ve raporlanır.
    /// Çıktı: Assets/_Project/Data/Levels/Level_XX.asset (varsa üzerine yazar).
    /// </summary>
    public static class LevelDataExtractor
    {
        private const string OutputFolder = "Assets/_Project/Data/Levels";
        private static readonly HashSet<string> StandardRoots = new HashSet<string>
        {
            "Audio", "Canvas_InLevel", "Car", "Directional Light", "EventSystem", "Ground", "Main Camera", "Managers", "Obstacles", "ParkingArea"
        };

        [MenuItem("Parking-Mastery/Levels/Extract LevelData From Open Scene")]
        private static void ExtractActive()
        {
            var data = Extract(SceneManager.GetActiveScene(), out var report);
            Debug.Log(report);
            if (data != null) Selection.activeObject = data;
        }

        public static LevelData Extract(Scene scene, out string report)
        {
            var sb = new System.Text.StringBuilder();
            if (!int.TryParse(scene.name, out int id))
            {
                report = "[LevelDataExtractor] Sahne adı sayı değil: " + scene.name;
                return null;
            }

            LevelScoring stars = null; CarControl car = null; ParkingZone area = null; Transform obstaclesRoot = null;
            foreach (var root in scene.GetRootGameObjects())
            {
                if (stars == null) stars = root.GetComponentInChildren<LevelScoring>(true);
                if (car == null) car = root.GetComponentInChildren<CarControl>(true);
                if (area == null) area = root.GetComponentInChildren<ParkingZone>(true);
                if (obstaclesRoot == null && root.name == "Obstacles") obstaclesRoot = root.transform;
            }
            if (car == null || area == null || stars == null)
            {
                report = "[LevelDataExtractor] Eksik bileşen (Car:" + (car != null) + " ParkingZone:" + (area != null) + " LevelScoring:" + (stars != null) + ") — sahne " + scene.name;
                return null;
            }

            Directory.CreateDirectory(OutputFolder);
            string path = OutputFolder + "/Level_" + id.ToString("00") + ".asset";
            var data = AssetDatabase.LoadAssetAtPath<LevelData>(path);
            if (data == null)
            {
                data = ScriptableObject.CreateInstance<LevelData>();
                AssetDatabase.CreateAsset(data, path);
            }

            var so = new SerializedObject(data);
            so.FindProperty("levelId").intValue = id;
            so.FindProperty("theme").enumValueIndex = Mathf.Clamp((id - 1) / 10, 0, 4);
            so.FindProperty("threeStarTime").intValue = stars.ThreeStarTime;
            so.FindProperty("twoStarTime").intValue = stars.TwoStarTime;
            so.FindProperty("targetTimeSeconds").intValue = stars.ThreeStarTime;

            var carSo = new SerializedObject(car);
            var hits = carSo.FindProperty("maxObstacleHits");
            so.FindProperty("maxObstacleHits").intValue = hits != null ? hits.intValue : 3;

            so.FindProperty("carSpawn.position").vector3Value = car.transform.position;
            so.FindProperty("carSpawn.yaw").floatValue = car.transform.eulerAngles.y;

            var box = area.GetComponent<BoxCollider>();
            Vector3 size = box != null ? Vector3.Scale(box.size, area.transform.lossyScale) : area.transform.lossyScale;
            so.FindProperty("parkingTarget.position").vector3Value = area.transform.position;
            so.FindProperty("parkingTarget.yaw").floatValue = area.transform.eulerAngles.y;
            so.FindProperty("parkingTarget.size").vector3Value = size;
            var tol = new SerializedObject(area).FindProperty("_fitTolerance");
            so.FindProperty("parkingTarget.fitTolerance").floatValue = tol != null ? tol.floatValue : 0.1f;

            // Engeller: yalnız prefab bağlantılı olanlar
            var placements = new List<(GameObject prefab, Transform t)>();
            int unlinked = 0, total = 0;
            if (obstaclesRoot != null)
            {
                foreach (Transform child in obstaclesRoot)
                {
                    total++;
                    var prefab = PrefabUtility.GetCorrespondingObjectFromSource(child.gameObject);
                    if (prefab != null) placements.Add((prefab, child)); else unlinked++;
                }
            }
            // Obstacles dışında kök seviyede duran prefab engeller (ör. sahne 8, 15, 21)
            foreach (var rootGo in scene.GetRootGameObjects())
            {
                if (StandardRoots.Contains(rootGo.name)) continue;
                total++;
                var prefab = PrefabUtility.GetCorrespondingObjectFromSource(rootGo);
                if (prefab != null) placements.Add((prefab, rootGo.transform)); else unlinked++;
            }
            var list = so.FindProperty("obstacles");
            list.arraySize = placements.Count;
            for (int i = 0; i < placements.Count; i++)
            {
                var el = list.GetArrayElementAtIndex(i);
                el.FindPropertyRelative("prefab").objectReferenceValue = placements[i].prefab;
                el.FindPropertyRelative("position").vector3Value = placements[i].t.position;
                el.FindPropertyRelative("eulerAngles").vector3Value = placements[i].t.eulerAngles;
                el.FindPropertyRelative("scale").vector3Value = placements[i].t.localScale;
            }

            so.ApplyModifiedProperties();
            EditorUtility.SetDirty(data);
            AssetDatabase.SaveAssets();

            sb.AppendLine("[LevelDataExtractor] " + path + " üretildi: 3★<=" + stars.ThreeStarTime + "sn, 2★<=" + stars.TwoStarTime + "sn, hak=" + so.FindProperty("maxObstacleHits").intValue + ", park alanı=" + size);
            sb.AppendLine("  Engeller: toplam=" + total + ", prefab bağlantılı aktarılan=" + placements.Count + ", bağlantısız atlanan=" + unlinked);
            report = sb.ToString();
            return data;
        }
    }
}
