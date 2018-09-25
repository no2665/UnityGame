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

    }

    // Update is called once per frame
    void Update()
    {
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
            if ( mousePosition == mouseDownPosition )
            {
                mouseClick = true;
            }
            halfClick = false;
        }

    }

    void FixedUpdate()
    {
        Ray camRay = Camera.main.ScreenPointToRay(mousePosition);
        RaycastHit floorHit;
        if (Physics.Raycast(camRay, out floorHit, camRayLength, floorMask))
        {
            Vector3 playerPos = player.transform.position;
            float distance = Vector3.Distance(floorHit.point, playerPos);
            if (distance > maxDistance || distance < minDistance)
            {
                DisableLineRenderers();
                return;
            }
            float smallUp = 0.1f;

            float upX = Mathf.Ceil(floorHit.point.x);
            float downX = Mathf.Floor(floorHit.point.x);
            float rightZ = Mathf.Ceil(floorHit.point.z);
            float leftZ = Mathf.Floor(floorHit.point.z);

            float[][] corners = { new float[] { upX, leftZ }, new float[] { upX, rightZ }, new float[] { downX, rightZ }, new float[] { downX, leftZ } };
            RaycastHit[] hits = new RaycastHit[4];

            for (int i = 0; i < 4; i++)
            {
                Ray corner = new Ray(new Vector3(corners[i][0], 10, corners[i][1]), Vector3.down);
                RaycastHit cornerHit;
                if (!Physics.Raycast(corner, out cornerHit, camRayLength, floorMask) || Mathf.Abs(cornerHit.point.y - floorHit.point.y) > 0.05f)
                {
                    DisableLineRenderers();
                    return;
                }
                hits[i] = cornerHit;
            }

            for (int i = 0; i < 4; i++)
            {
                lineRenderers[i].SetPosition(0, new Vector3(corners[i][0], hits[i].point.y + smallUp, corners[i][1]));
                lineRenderers[i].SetPosition(1, new Vector3(corners[(i + 1) % 4][0], hits[(i + 1) % 4].point.y + smallUp, corners[(i + 1) % 4][1]));
            }

            EnableLineRenderers();
        }
        else
        {
            DisableLineRenderers();
        }

        if (validPosition && mouseClick)
        {
            mouseClick = false;
            turretManager.HandleClick(Mathf.Floor(floorHit.point.x) + 0.5f, floorHit.point.y, Mathf.Floor(floorHit.point.z) + 0.5f);
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
