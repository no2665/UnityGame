using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityStandardAssets.CrossPlatformInput;

public class PositionTurrets : MonoBehaviour
{

    public GameObject lineRendererPrefab;
    public GameObject player;
    public float maxDistance = 15;
    public float minDistance = 1;

    private MovePotato potatoController;
    private TurretManager turretManager;
    private Vector3 mousePosition = Vector3.zero;
    private Vector3 mouseDownPosition = Vector3.zero;
    private bool halfClick = false;
    private bool mouseClick = false;
    private int floorMask;
    private readonly float camRayLength = 100f;
    private LineRenderer[] lineRenderers;

    private bool validPosition = false;

    private void Awake()
    {
        floorMask = LayerMask.GetMask("Floor");
        lineRenderers = new LineRenderer[4];
        for (int i = 0; i < 4; i++)
        {
            GameObject lineGameObject = (GameObject)Instantiate(lineRendererPrefab, gameObject.transform);
            LineRenderer lineRenderer = lineGameObject.GetComponent<LineRenderer>();
            lineRenderer.positionCount = 2;
            lineRenderer.enabled = false;
            lineRenderers[i] = lineRenderer;
        }
        turretManager = GetComponent<TurretManager>();
    }

    // Use this for initialization
    void Start()
    {
        potatoController = player.GetComponent<MovePotato>();
    }

    // Update is called once per frame
    void Update()
    {
        // Get mouse input, and determine if the mouse has been clicked
        mousePosition = CrossPlatformInputManager.mousePosition;
        bool mouseUp = CrossPlatformInputManager.GetButtonUp("Fire1");
        bool mouseDown = CrossPlatformInputManager.GetButtonDown("Fire1");
        if ( ! halfClick )
        {
            if ( mouseDown )
            {
                mouseDownPosition = new Vector3(mousePosition.x, mousePosition.y, mousePosition.z);
                halfClick = true;
            }
        } else if (mouseUp)
        {
            // Make sure the down and up where in the same position
            // May need to make this less sensitive
            if ( mousePosition == mouseDownPosition )
            {
                mouseClick = true;
            }
            halfClick = false;
        }

    }

    void FixedUpdate()
    {
        Vector3 mouseLocation = Vector3.zero;

        // Can't place turrets when we're moving
        if (potatoController.IsRolling())
        {
            DisableLineRenderers();
        }
        else
        {
            // Use a raycast to get the point on the floor that our mouse is pointing at
            Ray camRay = Camera.main.ScreenPointToRay(mousePosition);
            RaycastHit floorHit;
            // Use an if here, because it's possible the raycast won't hit the floor
            if (Physics.Raycast(camRay, out floorHit, camRayLength, floorMask))
            {
                mouseLocation = floorHit.point;
                // First check if the point is close to the player
                Vector3 playerPos = player.transform.position;
                if ( player.transform.up.y < 0 )
                {
                    playerPos.y -= 2.7f;
                }
                float distance = Vector3.Distance(floorHit.point, playerPos);
                if (distance > maxDistance || distance < minDistance)
                {
                    DisableLineRenderers();
                }
                else
                {
                    // Now find the four corners of the cell in which turrets can be placed
                    float smallUp = 0.1f;

                    float upX = Mathf.Ceil(floorHit.point.x);
                    float downX = Mathf.Floor(floorHit.point.x);
                    float rightZ = Mathf.Ceil(floorHit.point.z);
                    float leftZ = Mathf.Floor(floorHit.point.z);

                    float[][] corners = { new float[] { upX, leftZ }, new float[] { upX, rightZ }, new float[] { downX, rightZ }, new float[] { downX, leftZ } };

                    // Cast rays at each corner to get the heights of each corner
                    RaycastHit[] hits = new RaycastHit[4];

                    for (int i = 0; i < 4; i++)
                    {
                        Ray corner = new Ray(new Vector3(corners[i][0], 10, corners[i][1]), Vector3.down);
                        RaycastHit cornerHit;
                        // If the ray doesn't hit the floor, or the corner is not flat with the mouse point
                        if (!Physics.Raycast(corner, out cornerHit, camRayLength, floorMask) || Mathf.Abs(cornerHit.point.y - floorHit.point.y) > 0.05f)
                        {
                            DisableLineRenderers();
                            return;
                        }
                        hits[i] = cornerHit;
                    }

                    // Reconfigure the line renderers with the new corners
                    for (int i = 0; i < 4; i++)
                    {
                        lineRenderers[i].SetPosition(0, new Vector3(corners[i][0], hits[i].point.y + smallUp, corners[i][1]));
                        lineRenderers[i].SetPosition(1, new Vector3(corners[(i + 1) % 4][0], hits[(i + 1) % 4].point.y + smallUp, corners[(i + 1) % 4][1]));
                    }

                    EnableLineRenderers();

                }
            }
            else
            {
                DisableLineRenderers();
            }
        }

        // Click at a valid position to interact with that cell
        if ( mouseClick )
        {
            mouseClick = false;
            if ( validPosition )
            {
                turretManager.HandleClick(Mathf.Floor(mouseLocation.x) + 0.5f, mouseLocation.y, Mathf.Floor(mouseLocation.z) + 0.5f);
            }
        }
    }

    private void DisableLineRenderers()
    {
        for (int i = 0; i < 4; i++)
        {
            lineRenderers[i].enabled = false;
        }
        validPosition = false;
    }

    private void EnableLineRenderers()
    {
        for (int i = 0; i < 4; i++)
        {
            lineRenderers[i].enabled = true;
        }
        validPosition = true;
    }
}
