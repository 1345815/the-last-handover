using System.IO;
using LastHandover;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class PrototypeSceneBuilder
{
    private const string ScenePath = "Assets/Scenes/Prototype.unity";

    [MenuItem("Last Handover/Build Prototype Scene")]
    public static void Build()
    {
        Directory.CreateDirectory("Assets/Scenes");
        Directory.CreateDirectory("Assets/Materials");
        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        var dark = Material("DarkMetal", new Color(0.055f, 0.06f, 0.055f));
        var road = Material("Road", new Color(0.035f, 0.04f, 0.045f));
        var line = Material("RoadLine", new Color(0.55f, 0.48f, 0.25f));
        var lamp = Material("Lamp", new Color(0.65f, 0.52f, 0.28f));

        var systems = new GameObject("Systems");
        var shift = systems.AddComponent<ShiftController>();
        var looper = systems.AddComponent<RoadLooper>();
        var reality = systems.AddComponent<RealityController>();
        var hud = systems.AddComponent<PrototypeHUD>();
        var firstStop = systems.AddComponent<FirstStopEvent>();
        var repeatedStop = systems.AddComponent<RepeatedStopEvent>();
        var mirrorEvent = systems.AddComponent<MirrorDriverEvent>();

        var cameraObject = new GameObject("Driver Camera");
        cameraObject.transform.position = new Vector3(0f, 2.15f, -3.5f);
        var camera = cameraObject.AddComponent<Camera>();
        camera.fieldOfView = 68f;
        camera.nearClipPlane = 0.05f;
        camera.farClipPlane = 95f;
        camera.clearFlags = CameraClearFlags.SolidColor;
        cameraObject.AddComponent<AudioListener>();
        cameraObject.AddComponent<DriverCamera>();

        var cabin = new GameObject("Old Bus Cabin");
        Cube("Dashboard", cabin.transform, new Vector3(0f, 1.2f, -1.25f), new Vector3(4.2f, 0.55f, 1.15f), dark);
        Cube("LeftFrame", cabin.transform, new Vector3(-2.1f, 2.25f, -0.25f), new Vector3(0.18f, 2.8f, 0.25f), dark);
        Cube("RightFrame", cabin.transform, new Vector3(2.1f, 2.25f, -0.25f), new Vector3(0.18f, 2.8f, 0.25f), dark);
        Cube("Roof", cabin.transform, new Vector3(0f, 3.65f, -0.25f), new Vector3(4.35f, 0.22f, 0.35f), dark);
        Cube("GearLever", cabin.transform, new Vector3(1.25f, 1.55f, -1.05f), new Vector3(0.13f, 0.75f, 0.13f), dark, new Vector3(18f, 0f, 0f));
        var wheel = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        wheel.name = "SteeringWheel";
        wheel.transform.SetParent(cabin.transform);
        wheel.transform.position = new Vector3(-0.75f, 1.65f, -1.15f);
        wheel.transform.localScale = new Vector3(0.52f, 0.08f, 0.52f);
        wheel.transform.rotation = Quaternion.Euler(72f, 0f, 0f);
        wheel.GetComponent<Renderer>().sharedMaterial = dark;

        var cabinLightObject = new GameObject("Cabin Light");
        cabinLightObject.transform.SetParent(cabin.transform);
        cabinLightObject.transform.position = new Vector3(0f, 3.25f, -1.5f);
        var cabinLight = cabinLightObject.AddComponent<Light>();
        cabinLight.type = LightType.Point;
        cabinLight.range = 8f;
        cabinLight.intensity = 2.2f;

        const float segmentLength = 40f;
        var segments = new Transform[4];
        for (int i = 0; i < segments.Length; i++)
        {
            var segment = new GameObject($"Road Segment {i + 1}");
            segment.transform.position = new Vector3(0f, 0f, i * segmentLength);
            Cube("Asphalt", segment.transform, new Vector3(0f, -0.15f, 0f), new Vector3(8f, 0.25f, segmentLength), road);
            Cube("CentreLine", segment.transform, new Vector3(0f, 0.005f, 0f), new Vector3(0.12f, 0.015f, segmentLength - 2f), line);
            for (int z = -15; z <= 15; z += 10)
            {
                StreetLamp(segment.transform, new Vector3(-5f, 0f, z), dark, lamp);
                StreetLamp(segment.transform, new Vector3(5f, 0f, z + 5f), dark, lamp);
            }
            segments[i] = segment.transform;
        }

        looper.Configure(shift, segments, segmentLength);
        reality.Configure(shift, camera, cabinLight);

        var stop = new GameObject("First Stop Event");
        stop.transform.position = new Vector3(5.4f, 0f, 62f);
        Cube("ShelterBack", stop.transform, new Vector3(0f, 1.6f, 0f), new Vector3(2.8f, 3.2f, 0.15f), dark);
        Cube("ShelterRoof", stop.transform, new Vector3(0f, 3.25f, -0.45f), new Vector3(3.2f, 0.18f, 1.2f), dark);
        Cube("StopSign", stop.transform, new Vector3(-2f, 1.8f, 0f), new Vector3(0.18f, 3.6f, 0.18f), dark);
        var anomaly = Cube("Fourth Passenger", stop.transform, new Vector3(0f, 1.1f, -0.7f), new Vector3(0.55f, 2.2f, 0.42f), dark);
        firstStop.Configure(shift, stop.transform, anomaly);

        var repeatedSigns = new GameObject("Repeated Stop Signs");
        repeatedSigns.transform.position = new Vector3(-4.8f, 0f, 72f);
        for (int i = 0; i < 3; i++)
        {
            var sign = new GameObject($"Repeated Sign {i + 1}");
            sign.transform.SetParent(repeatedSigns.transform);
            sign.transform.localPosition = new Vector3(0f, 0f, i * 9f);
            Cube("Post", sign.transform, new Vector3(0f, 1.7f, 0f), new Vector3(0.16f, 3.4f, 0.16f), dark);
            Cube("Board", sign.transform, new Vector3(0f, 3.1f, 0f), new Vector3(1.8f, 0.75f, 0.16f), lamp);
        }
        repeatedStop.Configure(shift, firstStop, repeatedSigns.transform);

        var mirror = Cube("Rear-view Mirror", cabin.transform, new Vector3(0f, 3.05f, -0.72f), new Vector3(1.65f, 0.48f, 0.12f), dark);
        var mirrorDriver = Cube("Mirror Driver", mirror.transform, new Vector3(0f, 0f, -0.18f), new Vector3(0.22f, 0.65f, 0.08f), lamp);
        mirrorEvent.Configure(shift, repeatedStop, mirrorDriver);
        hud.Configure(shift, firstStop, repeatedStop, mirrorEvent);

        RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
        RenderSettings.ambientLight = new Color(0.02f, 0.025f, 0.03f);
        EditorSceneManager.SaveScene(scene, ScenePath);
        EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(ScenePath, true) };
        AssetDatabase.SaveAssets();
        Debug.Log($"Prototype scene created at {ScenePath}");
    }

    public static void BuildFromCommandLine()
    {
        Build();
        EditorApplication.Exit(0);
    }

    private static Material Material(string name, Color color)
    {
        string path = $"Assets/Materials/{name}.mat";
        var material = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (material == null)
        {
            material = new Material(Shader.Find("Standard")) { name = name, color = color };
            material.SetFloat("_Glossiness", 0.08f);
            AssetDatabase.CreateAsset(material, path);
        }
        return material;
    }

    private static GameObject Cube(string name, Transform parent, Vector3 position, Vector3 scale, Material material, Vector3 rotation = default)
    {
        var item = GameObject.CreatePrimitive(PrimitiveType.Cube);
        item.name = name;
        item.transform.SetParent(parent);
        item.transform.localPosition = position;
        item.transform.localScale = scale;
        item.transform.localRotation = Quaternion.Euler(rotation);
        item.GetComponent<Renderer>().sharedMaterial = material;
        Object.DestroyImmediate(item.GetComponent<Collider>());
        return item;
    }

    private static void StreetLamp(Transform parent, Vector3 position, Material metal, Material glow)
    {
        var root = new GameObject("Passing Street Lamp");
        root.transform.SetParent(parent);
        root.transform.localPosition = position;
        Cube("Pole", root.transform, new Vector3(0f, 2.5f, 0f), new Vector3(0.1f, 5f, 0.1f), metal);
        Cube("Light", root.transform, new Vector3(0f, 5f, 0f), new Vector3(0.6f, 0.18f, 0.35f), glow);
    }
}

