using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class EditSettingsWindow : EditorWindow
{
    private SerializedObject serializedObject;
    private SerializedProperty currentProperty;

    Vector2 scrollPos;

    private enum Categories
    {
        GENERATION,
        WALLS,
        CORNERS,
        FLOOR,
        ENEMIES,
        CONTENT,
        OTHER
    }

    private Categories m_categories = Categories.GENERATION;

    public static void OpenWindow()
    {
        EditSettingsWindow window = (EditSettingsWindow)GetWindow(typeof(EditSettingsWindow));
        window.minSize = new Vector2(900, 700);
        window.Show();
    }

    public static void OpenWindow(GenerationSettings genSetts)
    {
        EditSettingsWindow window = (EditSettingsWindow)GetWindow(typeof(EditSettingsWindow));
        window.serializedObject = new SerializedObject(genSetts);
        window.minSize = new Vector2(450, 300);
        window.Show();
    }

    private void DrawProperties(ref SerializedProperty prop, bool drawChildren)
    {
        string lastPropPath = string.Empty;

        foreach (SerializedProperty p in prop)
        {
            bool isShowing = false;

            switch (m_categories)
            {
                case Categories.GENERATION:
                    if (p.name == "minRooms" || p.name == "maxRooms" ||
                        p.name == "amountOfAdjacentRoomsToConnect" ||
                        p.name == "minRoomWidth" || p.name == "maxRoomWidth" ||
                        p.name == "minRoomHeight" || p.name == "maxRoomHeight" ||
                        p.name == "minBossRoomWidth" || p.name == "maxBossRoomWidth" ||
                        p.name == "minBossRoomHeight" || p.name == "maxBossRoomHeight" ||
                        p.name == "areaWidth" || p.name == "areaHeight" ||
                        p.name == "minDistanceBetweenRooms")
                    {
                        isShowing = true;
                    }
                    break;

                case Categories.WALLS:
                    if (p.name == "walls" || p.name == "wallLights" || p.name == "wallLightRate" ||
                        p.name == "wallDecorations" || p.name == "wallDecorationRate")
                        isShowing = true;
                    break;

                case Categories.CORNERS:
                    if (p.name == "outerCorners" || p.name == "innerCorners" ||
                        p.name == "deleteInnerCorners" || p.name == "uCorners" ||
                        p.name == "squareCorners" || p.name == "doors" ||
                        p.name == "doorsDisplacements")
                    {
                        isShowing = true;
                    }
                    break;

                case Categories.FLOOR:
                    if (p.name == "flooring")
                        isShowing = true;
                    break;

                case Categories.ENEMIES:
                    if (p.name == "player" ||
                        p.name == "minEnemiesPerRoom" || p.name == "maxEnemiesPerRoom" ||
                        p.name == "enemies" || p.name == "bosses")
                    {
                        isShowing = true;
                    }
                    break;

                case Categories.CONTENT:
                    if (p.name == "breakables" || p.name == "staticObjects" ||
                        p.name == "wallObjects" || p.name == "wallObjConsequitiveDistance" ||
                        p.name == "wallObjDisplacements" ||
                        p.name == "cornerObjectsRate" || p.name == "destroyablesRate" ||
                        p.name == "middleObjects" || p.name == "middleObjectsRate")
                    {
                        isShowing = true;
                    }
                    break;


                //parte nueva creada por Gabriel para techo y decoraciones
                case Categories.OTHER:
                    if (p.name == "generateCeiling" || p.name == "ceilings" ||
                        p.name == "generateDecorations" || p.name == "decorations" || p.name == "decorationRate" ||
                        p.name == "generateChests" || p.name == "chests" || p.name == "ChestRate" ||
                        p.name == "lodDistance")
                    {
                        isShowing = true;
                    }
                    break;

            }

            if (isShowing)
            {
                if (p.isArray && p.propertyType == SerializedPropertyType.Generic)
                {
                    EditorGUILayout.PropertyField(p, new GUIContent(p.displayName), true);
                }
                else
                {
                    if (p.name != "size" && p.name != "data" && p.name != "gameObject" &&
                        p.name != "name" && p.name != "x" && p.name != "y" && p.name != "z")
                    {
                        if (!string.IsNullOrEmpty(lastPropPath) && p.propertyPath.Contains(lastPropPath))
                            continue;
                        lastPropPath = p.propertyPath;

                        EditorGUILayout.PropertyField(p, true);
                    }
                }
            }
        }
    }

    protected void DrawSidebar()
    {
        EditorGUILayout.BeginVertical("box", GUILayout.MaxWidth(150), GUILayout.ExpandHeight(true));

        if (GUILayout.Button("Generation", GUILayout.Height(90))) m_categories = Categories.GENERATION;
        if (GUILayout.Button("Walls", GUILayout.Height(90))) m_categories = Categories.WALLS;
        if (GUILayout.Button("Corners", GUILayout.Height(90))) m_categories = Categories.CORNERS;
        if (GUILayout.Button("Floor", GUILayout.Height(90))) m_categories = Categories.FLOOR;
        if (GUILayout.Button("Entities", GUILayout.Height(90))) m_categories = Categories.ENEMIES;
        if (GUILayout.Button("Content", GUILayout.Height(90))) m_categories = Categories.CONTENT;
        if (GUILayout.Button("Other", GUILayout.Height(90))) m_categories = Categories.OTHER;

        EditorGUILayout.EndVertical();
    }

    private void OnGUI()
    {
        currentProperty = serializedObject.FindProperty("generation");

        EditorGUILayout.BeginHorizontal();
        DrawSidebar();

        EditorGUILayout.BeginVertical("box", GUILayout.ExpandHeight(true));
        scrollPos = EditorGUILayout.BeginScrollView(scrollPos, GUIStyle.none, GUI.skin.verticalScrollbar);

        DrawProperties(ref currentProperty, true);

        EditorGUILayout.EndScrollView();
        EditorGUILayout.EndVertical();
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginVertical();
        EditorGUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();

        if (GUILayout.Button("Apply Modified Changes", GUILayout.Width(Screen.width / 2), GUILayout.Height(Screen.height / 6)))
        {
            serializedObject.ApplyModifiedProperties();
        }

        if (GUILayout.Button("Generate", GUILayout.Width(Screen.width / 2), GUILayout.Height(Screen.height / 6)))
        {
            if (GameObject.FindObjectOfType<Generator>())
            {
                if (GameObject.FindObjectOfType<Generator>().GenerationSettings)
                {
                    GameObject.FindObjectOfType<Generator>().RunProgram();
                }
                else
                {
                    Debug.LogWarning("Need Generation Settings connected to generator.");
                }
            }
            else
            {
                Debug.LogWarning("Need Object of type Generator in Scene");
            }
        }

        GUILayout.FlexibleSpace();
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.EndVertical();
    }

}
