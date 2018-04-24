using UnityEngine;
using System.Collections;

public class Scare07Controller : MonoBehaviour {

    public bool playScare = true;
    public float waitTime = 12.0f;

    [SerializeField] private GameObject ghostShadowOnly;
    private Renderer ghostRenderer;
    private Transform ghostTransform;
    private Camera mainCamera;
    private GameObject player;

    private float minGhostSpawnDistance = 3.0f;
    private float maxGhostSpawnDistance = 10.0f;
    private float startTime;

    // relative height of ghost transform from mainCAmera.transform
    private float relHeight = -0.5f; 

    private Ray ray;
    private RaycastHit hit;

	// Use this for initialization
	void Start ()
    {
        ghostRenderer = ghostShadowOnly.GetComponent<Renderer>();
        ghostTransform = ghostShadowOnly.GetComponent<Transform>();
        mainCamera = Camera.main;
        player = GameObject.FindGameObjectWithTag("Player");

        startTime = Time.time;
	}
	
	// Update is called once per frame
	void Update ()
    {
        if (!playScare)
            return;

        float duration = Time.time - startTime;
        if (duration > waitTime)
        {
            StartCoroutine("PlayScare");
        }
	}

    IEnumerator PlayScare()
    {
        ray = mainCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        if (Physics.Raycast(ray, out hit, maxGhostSpawnDistance))
        {
            if (hit.distance > minGhostSpawnDistance)
            {
                float frustrumWidth = 2.0f * hit.distance * mainCamera.aspect *
                    Mathf.Tan(mainCamera.fieldOfView * 0.5f * Mathf.Deg2Rad);

                Vector3 initialPosition = mainCamera.transform.TransformPoint(
                    new Vector3(0.5f * frustrumWidth, hit.distance, 
                    mainCamera.transform.position.z + relHeight));

                while (true)
                {
                    yield return null;
                }
            }
        }
    }

    // Cast ray in from of camera to test distance to nearest collider in order to
    // spawn ghost in between RaycastHit and camera
}
