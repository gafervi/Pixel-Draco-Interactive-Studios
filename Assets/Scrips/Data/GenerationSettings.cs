using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Generation Settings")]
public class GenerationSettings : ScriptableObject
{
    [SerializeField] public Data generation;
}

[Serializable]
public class Data
{
    [SerializeField] public int minRooms;
    [SerializeField] public int maxRooms;

    [SerializeField] public int amountOfAdjacentRoomsToConnect;

    [SerializeField] public int minRoomWidth;
    [SerializeField] public int maxRoomWidth;

    [SerializeField] public int minRoomHeight;
    [SerializeField] public int maxRoomHeight;

    [SerializeField] public int minBossRoomWidth;
    [SerializeField] public int maxBossRoomWidth;

    [SerializeField] public int minBossRoomHeight;
    [SerializeField] public int maxBossRoomHeight;

    [SerializeField] public int areaWidth;
    [SerializeField] public int areaHeight;

    [SerializeField] public int minDistanceBetweenRooms;

    // Walls & Corners
    [SerializeField] public List<ObjectList> walls = new List<ObjectList>();
    [SerializeField] public List<ObjectList> wallLights = new List<ObjectList>();
    [SerializeField] public List<ObjectList> wallDecorations = new List<ObjectList>();
    [Range(0, 100)]
    [SerializeField] public int wallLightRate;
    [Range(0, 100)]
    [SerializeField] public int wallDecorationRate;
    [SerializeField] public List<ObjectList> outerCorners = new List<ObjectList>();
    [SerializeField] public List<ObjectList> innerCorners = new List<ObjectList>();
    [SerializeField] public bool deleteInnerCorners;
    [SerializeField] public List<ObjectList> uCorners = new List<ObjectList>();
    [SerializeField] public List<ObjectList> squareCorners = new List<ObjectList>();
    [SerializeField] public List<ObjectList> doors = new List<ObjectList>();
    [SerializeField] public List<Vector3> doorsDisplacements;

    // Floor
    [SerializeField] public List<ObjectList> flooring = new List<ObjectList>();

    // Player & Enemies
    [SerializeField] public GameObject player;
    [SerializeField] public int minEnemiesPerRoom;
    [SerializeField] public int maxEnemiesPerRoom;
    [SerializeField] public List<ObjectList> enemies = new List<ObjectList>();
    [SerializeField] public List<ObjectList> bosses = new List<ObjectList>();

    // Objects
    [SerializeField] public List<ObjectList> breakables = new List<ObjectList>();
    [SerializeField] public List<ObjectList> staticObjects = new List<ObjectList>();
    [SerializeField] public List<ObjectList> wallObjects = new List<ObjectList>();
    [SerializeField] public int wallObjConsequitiveDistance;
    [SerializeField] public List<Vector3> wallObjDisplacements;
    [Range(0, 100)]
    [SerializeField] public int cornerObjectsRate;
    [Range(0, 100)]
    [SerializeField] public int destroyablesRate;
    [SerializeField] public List<ObjectList> middleObjects = new List<ObjectList>();
    [Range(0, 100)]
    [SerializeField] public int middleObjectsRate;

    // Other Settings: Ceilings, Decorations, Chests
    [Header("Other Settings")]
    [SerializeField] public bool generateCeiling;
    [SerializeField] public List<ObjectList> ceilings = new List<ObjectList>();

    [SerializeField] public bool generateDecorations;
    [SerializeField] public List<ObjectList> decorations = new List<ObjectList>();
    [Range(0, 100)]
    [SerializeField] public int decorationRate;

    [SerializeField] public bool generateChests;
    [SerializeField] public List<ObjectList> chests = new List<ObjectList>();
    [Range(0, 100)]
    [SerializeField] public int ChestRate;

    // LOD Settings
    [Header("LOD Settings")]
    [Range(0.1f, 50f)]
    [SerializeField] public float lodDistance = 30f; // Distancia a la que se ocultan los objetos
}

[System.Serializable]
public class ObjectList
{
    public GameObject gameObject;
    public string name;
}
