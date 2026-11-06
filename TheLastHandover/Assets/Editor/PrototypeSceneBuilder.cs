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

        var dark = TexMat("DarkMetal", new Color(0.055f, 0.06f, 0.055f), "cabin_metal", 1f, 1f);
        var road = TexMat("Road", new Color(0.05f, 0.055f, 0.06f), "asphalt", 2f, 10f);
        var line = Material("RoadLine", new Color(0.55f, 0.48f, 0.25f));
        var lamp = Material("Lamp", new Color(0.65f, 0.52f, 0.28f));
        var concrete = TexMat("Concrete", new Color(0.5f, 0.49f, 0.47f), "concrete", 2f, 1f);
        var rubber = TexMat("Rubber", new Color(0.07f, 0.07f, 0.075f), "rubber", 1f, 1f);
        var signFace = TexMat("SignFace", new Color(0.62f, 0.64f, 0.62f), "sign_face", 1f, 1f);
        var lampGlow = EmissiveTexMat("LampGlow", Color.white, "lampglow", 1f, 1f, new Color(1f, 0.85f, 0.55f) * 0.9f);
        var billboardMat = EmissiveTexMat("Billboard", Color.white, "billboard", 1f, 1f, new Color(0.85f, 0.8f, 0.7f) * 0.55f);
        var raincoat = EmissiveTexMat("FigureSkin", new Color(0.72f, 0.68f, 0.55f), "raincoat", 1f, 1f, new Color(0.85f, 0.8f, 0.62f) * 0.35f);
        var building = TexMat("Building", new Color(0.35f, 0.35f, 0.36f), "building", 2f, 1f);
        var rail = TexMat("Rail", new Color(0.3f, 0.31f, 0.32f), "cabin_metal", 4f, 0.3f);

        var systems = new GameObject("Systems");
        var shift = systems.AddComponent<ShiftController>();
        var looper = systems.AddComponent<RoadLooper>();
        var reality = systems.AddComponent<RealityController>();
        var hud = systems.AddComponent<PrototypeHUD>();
        var firstStop = systems.AddComponent<FirstStopEvent>();
        var repeatedStop = systems.AddComponent<RepeatedStopEvent>();
        var mirrorEvent = systems.AddComponent<MirrorDriverEvent>();
        var finalEvent = systems.AddComponent<FinalShiftEvent>();

        var cameraObject = new GameObject("Driver Camera");
        cameraObject.transform.position = new Vector3(0f, 2.15f, -3.5f);
        var camera = cameraObject.AddComponent<Camera>();
        camera.fieldOfView = 68f;
        camera.nearClipPlane = 0.05f;
        camera.farClipPlane = 95f;
        camera.clearFlags = CameraClearFlags.SolidColor;
        cameraObject.AddComponent<AudioListener>();
        var driverCamera = cameraObject.AddComponent<DriverCamera>();
        driverCamera.Configure(shift);
        var vhs = cameraObject.AddComponent<VhsEffect>();
        vhs.Configure(AssetDatabase.LoadAssetAtPath<Shader>("Assets/Shaders/Vhs.shader"));

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
        for (int s = 0; s < 3; s++)
        {
            float a = s * 120f * Mathf.Deg2Rad;
            Cube($"Wheel Spoke {s + 1}", cabin.transform,
                new Vector3(-0.75f + Mathf.Cos(a) * 0.17f, 1.65f + Mathf.Sin(a) * 0.07f, -1.15f + Mathf.Sin(a) * 0.055f),
                new Vector3(0.34f, 0.045f, 0.03f), rubber, new Vector3(72f, 0f, s * 120f));
        }
        var knob = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        knob.name = "Gear Knob";
        knob.transform.SetParent(cabin.transform);
        knob.transform.localPosition = new Vector3(1.25f, 1.96f, -1.02f);
        knob.transform.localScale = new Vector3(0.15f, 0.15f, 0.15f);
        knob.GetComponent<Renderer>().sharedMaterial = rubber;
        Object.DestroyImmediate(knob.GetComponent<Collider>());
        Cube("Handrail Left", cabin.transform, new Vector3(-1.45f, 3.4f, -0.6f), new Vector3(0.035f, 0.035f, 2.4f), rubber);
        Cube("Handrail Right", cabin.transform, new Vector3(1.45f, 3.4f, -0.6f), new Vector3(0.035f, 0.035f, 2.4f), rubber);

        var glass = TransparentMaterial("WindshieldGlass", new Color(0.75f, 0.82f, 0.85f, 0.07f));
        Cube("Windshield", cabin.transform, new Vector3(0f, 2.6f, -0.5f), new Vector3(4.1f, 1.7f, 0.03f), glass);
        var wiperLeft = new GameObject("Wiper Left Pivot");
        wiperLeft.transform.SetParent(cabin.transform);
        wiperLeft.transform.localPosition = new Vector3(-0.9f, 1.8f, -0.44f);
        Cube("Wiper Left Arm", wiperLeft.transform, new Vector3(0f, 0.42f, 0.02f), new Vector3(0.05f, 0.84f, 0.03f), dark);
        var wiperRight = new GameObject("Wiper Right Pivot");
        wiperRight.transform.SetParent(cabin.transform);
        wiperRight.transform.localPosition = new Vector3(0.9f, 1.8f, -0.44f);
        Cube("Wiper Right Arm", wiperRight.transform, new Vector3(0f, 0.42f, 0.02f), new Vector3(0.05f, 0.84f, 0.03f), dark);
        var wiperController = new GameObject("Wipers").AddComponent<WiperController>();
        wiperController.Configure(wiperLeft.transform, wiperRight.transform);

        var cabinLightObject = new GameObject("Cabin Light");
        cabinLightObject.transform.SetParent(cabin.transform);
        cabinLightObject.transform.position = new Vector3(0f, 3.25f, -1.5f);
        var cabinLight = cabinLightObject.AddComponent<Light>();
        cabinLight.type = LightType.Point;
        cabinLight.range = 8f;
        cabinLight.intensity = 2.2f;

        var gaugeLight = EmissiveMaterial("GaugePanel", new Color(0.1f, 0.14f, 0.12f), new Color(0.25f, 0.75f, 0.45f) * 0.55f);
        Cube("InstrumentPanel", cabin.transform, new Vector3(-0.75f, 1.52f, -0.92f), new Vector3(1.1f, 0.2f, 0.04f), gaugeLight, new Vector3(-18f, 0f, 0f));
        var dashGlow = new GameObject("Dashboard Glow");
        dashGlow.transform.SetParent(cabin.transform);
        dashGlow.transform.localPosition = new Vector3(-0.75f, 1.55f, -0.8f);
        var dashLight = dashGlow.AddComponent<Light>();
        dashLight.type = LightType.Point;
        dashLight.range = 2.5f;
        dashLight.intensity = 0.9f;
        dashLight.color = new Color(0.35f, 0.75f, 0.5f);

        foreach (float sideX in new[] { -1.35f, 1.35f })
        {
            var head = new GameObject($"Headlight {(sideX < 0 ? "Left" : "Right")}");
            head.transform.SetParent(cabin.transform);
            head.transform.localPosition = new Vector3(sideX, 1.0f, 0.6f);
            head.transform.localRotation = Quaternion.Euler(10f, 0f, 0f);
            var hl = head.AddComponent<Light>();
            hl.type = LightType.Spot;
            hl.range = 55f;
            hl.spotAngle = 64f;
            hl.intensity = 3.4f;
            hl.color = new Color(1f, 0.94f, 0.82f);
        }

        var moon = new GameObject("Moonlight");
        moon.transform.rotation = Quaternion.Euler(35f, 215f, 0f);
        var moonLight = moon.AddComponent<Light>();
        moonLight.type = LightType.Directional;
        moonLight.intensity = 0.14f;
        moonLight.color = new Color(0.55f, 0.65f, 0.85f);

        const float segmentLength = 40f;
        var segments = new Transform[4];
        var buildingHeights = new[] { 9f, 15f, 11f };
        for (int i = 0; i < segments.Length; i++)
        {
            var segment = new GameObject($"Road Segment {i + 1}");
            segment.transform.position = new Vector3(0f, 0f, i * segmentLength);
            Cube("Asphalt", segment.transform, new Vector3(0f, -0.15f, 0f), new Vector3(8f, 0.25f, segmentLength), road);
            Cube("CentreLine", segment.transform, new Vector3(0f, 0.005f, 0f), new Vector3(0.12f, 0.015f, segmentLength - 2f), line);
            Cube("GuardRail Left", segment.transform, new Vector3(-4.7f, 0.55f, 0f), new Vector3(0.08f, 0.45f, segmentLength), rail);
            Cube("GuardRail Right", segment.transform, new Vector3(4.7f, 0.55f, 0f), new Vector3(0.08f, 0.45f, segmentLength), rail);
            for (int z = -15; z <= 15; z += 10)
            {
                StreetLamp(segment.transform, new Vector3(-5f, 0f, z), dark, lampGlow);
                StreetLamp(segment.transform, new Vector3(5f, 0f, z + 5f), dark, lampGlow);
            }
            for (int b = 0; b < buildingHeights.Length; b++)
            {
                float height = buildingHeights[(b + i) % buildingHeights.Length];
                Cube($"Building Block {b + 1} Left", segment.transform,
                    new Vector3(-(13.5f + b * 4f), height * 0.5f, -13f + b * 14f),
                    new Vector3(6.5f + b, height, 7.5f), building);
                Cube($"Building Block {b + 1} Right", segment.transform,
                    new Vector3(13.5f + b * 4f, height * 0.55f + 1f, -6f + b * 14f),
                    new Vector3(7.5f + b, height + 2f, 7f), building);
            }
            segments[i] = segment.transform;
        }

        looper.Configure(shift, segments, segmentLength);
        reality.Configure(shift, camera, cabinLight);

        var stop = new GameObject("First Stop Event");
        stop.transform.position = new Vector3(5.4f, 0f, 62f);
        Cube("ShelterBack", stop.transform, new Vector3(0f, 1.6f, 0f), new Vector3(2.8f, 3.2f, 0.15f), concrete);
        Cube("ShelterRoof", stop.transform, new Vector3(0f, 3.25f, -0.45f), new Vector3(3.2f, 0.18f, 1.2f), concrete);
        Cube("StopSign", stop.transform, new Vector3(-2f, 1.8f, 0f), new Vector3(0.18f, 3.6f, 0.18f), dark);
        Cube("StopSignBoard", stop.transform, new Vector3(-2f, 3.1f, 0f), new Vector3(1.1f, 0.9f, 0.1f), signFace);
        Cube("Billboard Box", stop.transform, new Vector3(1.3f, 2.4f, 0.6f), new Vector3(2.2f, 1.3f, 0.15f), billboardMat);
        var anomaly = Cube("Fourth Passenger", stop.transform, new Vector3(0f, 1.1f, -0.7f), new Vector3(0.55f, 2.2f, 0.42f), raincoat);
        firstStop.Configure(shift, stop.transform, anomaly);

        var repeatedSigns = new GameObject("Repeated Stop Signs");
        repeatedSigns.transform.position = new Vector3(-4.8f, 0f, 72f);
        for (int i = 0; i < 3; i++)
        {
            var sign = new GameObject($"Repeated Sign {i + 1}");
            sign.transform.SetParent(repeatedSigns.transform);
            sign.transform.localPosition = new Vector3(0f, 0f, i * 9f);
            Cube("Post", sign.transform, new Vector3(0f, 1.7f, 0f), new Vector3(0.16f, 3.4f, 0.16f), dark);
            Cube("Board", sign.transform, new Vector3(0f, 3.1f, 0f), new Vector3(1.8f, 0.75f, 0.16f), signFace);
        }
        repeatedStop.Configure(shift, firstStop, repeatedSigns.transform);

        var mirror = Cube("Rear-view Mirror", cabin.transform, new Vector3(0f, 3.05f, -0.72f), new Vector3(1.65f, 0.48f, 0.12f), dark);
        var mirrorDriver = Cube("Mirror Driver", mirror.transform, new Vector3(0f, 0f, -0.18f), new Vector3(0.22f, 0.65f, 0.08f), lamp);
        mirrorEvent.Configure(shift, repeatedStop, mirrorDriver);
        var fifthMark = Cube("Hidden Fifth Gear Mark", cabin.transform, new Vector3(1.55f, 1.52f, -1.28f), new Vector3(0.12f, 0.42f, 0.025f), lamp, new Vector3(0f, 0f, -20f));
        finalEvent.Configure(shift, mirrorEvent, fifthMark);
        var roadFigure = new GameObject("Road Figure");
        roadFigure.transform.position = new Vector3(0f, 0f, 40f);
        Cube("Figure Body", roadFigure.transform, new Vector3(0f, 1.1f, 0f), new Vector3(0.55f, 2.2f, 0.32f), raincoat);
        Cube("Figure Head", roadFigure.transform, new Vector3(0f, 2.4f, 0f), new Vector3(0.28f, 0.32f, 0.26f), raincoat);
        var crossingEvent = new GameObject("Crossing Figure Event").AddComponent<CrossingFigureEvent>();
        crossingEvent.Configure(shift, repeatedStop, roadFigure.transform);
        hud.Configure(shift, firstStop, repeatedStop, mirrorEvent, finalEvent, crossingEvent);
        var endingLoop = systems.AddComponent<EndingLoop>();
        endingLoop.Configure(finalEvent);
        var audioDirector = systems.AddComponent<AudioDirector>();
        audioDirector.Configure(shift, firstStop, repeatedStop, mirrorEvent, finalEvent,
            Clip("engine_loop"), Clip("radio_static"), Clip("gear_clunk"),
            Clip("door_hiss"), Clip("broadcast_chime"), Clip("ending_surge"),
            Clip("rain_loop"), crossingEvent);

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

    private static Material TexMat(string name, Color color, string texture, float tx, float ty)
    {
        var material = Material(name, color);
        var tex = AssetDatabase.LoadAssetAtPath<Texture2D>($"Assets/Textures/{texture}.png");
        if (tex != null)
        {
            material.mainTexture = tex;
            material.mainTextureScale = new Vector2(tx, ty);
        }
        return material;
    }

    private static Material EmissiveTexMat(string name, Color color, string texture, float tx, float ty, Color emission)
    {
        var material = TexMat(name, color, texture, tx, ty);
        material.EnableKeyword("_EMISSION");
        material.SetColor("_EmissionColor", emission);
        return material;
    }

    private static Material EmissiveMaterial(string name, Color color, Color emission)
    {
        var material = Material(name, color);
        material.EnableKeyword("_EMISSION");
        material.SetColor("_EmissionColor", emission);
        return material;
    }

    private static Material TransparentMaterial(string name, Color color)
    {
        var material = Material(name, color);
        material.SetFloat("_Mode", 3f);
        material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        material.SetInt("_ZWrite", 0);
        material.DisableKeyword("_ALPHATEST_ON");
        material.EnableKeyword("_ALPHABLEND_ON");
        material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
        material.renderQueue = 3000;
        return material;
    }

    private static AudioClip Clip(string name)
    {
        return AssetDatabase.LoadAssetAtPath<AudioClip>($"Assets/Audio/{name}.wav");
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

