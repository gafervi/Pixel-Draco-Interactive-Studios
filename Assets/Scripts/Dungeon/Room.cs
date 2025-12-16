using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Room : MonoBehaviour
{
    // Vectores de esquinas para verificar conexiones
    private Vector2Int TLCorner;
    private Vector2Int BLCorner;
    private Vector2Int TRCorner;
    private Vector2Int BRCorner;

    private List<Room> connectedRooms = new List<Room>();
    private List<Room> savedConnectedRooms = new List<Room>();
    private List<GameObject> corners = new List<GameObject>();
    private List<GameObject> midPoints = new List<GameObject>();
    private List<GameObject> closestRooms = new List<GameObject>();
    private Dictionary<int, float> connectedRoomsAngles = new Dictionary<int, float>();

    private bool isPlayerInside;
    private int maxAmountOfConnections;

    // --- Other Settings: techos, luces, decoraciones, cofres ---
    private List<GameObject> ceilings = new List<GameObject>();
    private List<GameObject> wallLights = new List<GameObject>();
    private List<GameObject> decorations = new List<GameObject>();
    private List<GameObject> wallDecoration = new List<GameObject>();
    private List<GameObject> chests = new List<GameObject>();

    // --- Propiedades públicas ---
    public int MaxAmountOfConnections => maxAmountOfConnections;
    public List<Room> ConnectedRooms => connectedRooms;
    public List<Room> SavedConnectedRooms { get => savedConnectedRooms; set => savedConnectedRooms = value; }
    public List<GameObject> ClosestRooms => closestRooms;
    public Dictionary<int, float> ConnectedRoomsAngles => connectedRoomsAngles;
    public List<GameObject> MidPoints => midPoints;
    public List<GameObject> Corners { get => corners; set => corners = value; }
    public bool IsPlayerInside { get => isPlayerInside; set => isPlayerInside = value; }

    // --- Propiedades públicas para Other Settings ---
    public List<GameObject> Ceilings => ceilings;
    public List<GameObject> WallLights => wallLights;
    public List<GameObject> Decorations => decorations;
    public List<GameObject> WallDecoration => wallDecoration;
    public List<GameObject> Chests => chests;

    // --- Métodos ---
    public void CalculateConnections()
    {
        int amountRooms = GameObject.FindObjectOfType<Generator>().AmountOfAdjacentRoomsToConnect;
        maxAmountOfConnections = Random.Range(1, amountRooms + 1);
    }

    public void AddConnectedRooms(Room room)
    {
        if (!ConnectedRooms.Contains(room))
        {
            ConnectedRooms.Add(room);

            // Agregar ángulo relativo
            ConnectedRoomsAngles.Add(connectedRooms.Count - 1,
                Vector3.Angle(this.gameObject.transform.position, room.gameObject.transform.position));
        }
    }

    public void AddRoom(GameObject room)
    {
        if (!ClosestRooms.Contains(room))
        {
            ClosestRooms.Add(room);
        }
    }

    public void AddMidPoint()
    {
        if (transform.childCount == 0) return;

        if ((transform.childCount % 2) == 0)
        {
            List<float> childDistances = new List<float>();
            Dictionary<float, int> childAndDistances = new Dictionary<float, int>();

            for (int i = 0; i < transform.childCount; i++)
            {
                float tempDistance = Vector3.Distance(transform.position, transform.GetChild(i).position);

                while (childAndDistances.ContainsKey(tempDistance))
                {
                    tempDistance += Random.Range(-0.0001f, 0.0001f);
                }

                childDistances.Add(tempDistance);
                childAndDistances.Add(tempDistance, i);
            }

            childDistances.Sort();

            for (int i = 0; i < 2; i++)
            {
                int closeChildren = childAndDistances[childDistances[i]];
                midPoints.Add(transform.GetChild(closeChildren).gameObject);
            }
        }
        else
        {
            int childNumber = Mathf.RoundToInt((transform.childCount / 2f));
            midPoints.Add(transform.GetChild(childNumber).gameObject);
        }
    }
}
