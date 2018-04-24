using UnityEngine;
using System.Collections;

public class Scare03Controller : MonoBehaviour {

    [SerializeField] private GameObject door;
    [SerializeField] private GameObject lightBulb;
    [SerializeField] private Light flashlight;
    [SerializeField] private GameObject ghost;
    [SerializeField] private Transform playerSpawnPosition;

    [SerializeField] private const float WAIT_TIME = 5.0f;
    [SerializeField] private float scareSpawnDistance = 2.5f;
    [SerializeField] private float ghostSpawnDistance = 1.5f;
    [SerializeField] private float doorSlamDistance = 3.5f;

    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip scream;
    [SerializeField] private AudioClip shakyBreathing;
    [SerializeField] private AudioClip doorKnock;
    [SerializeField] private AudioClip voiceFromBehind;
    private AudioSource ghostAudioSource;

    private float duration;
    private bool playScare;
    private bool spawnGhost;
    private Camera mainCam;
    private GameObject player;
    private HangingLightController lightController;

    private Vector3 fromDirection;
    private Vector3 toDirection;

	// Use this for initialization
	void Start ()
    {
        duration = 0;
        playScare = false;
        spawnGhost = false;
        ghost.GetComponent<Renderer>().enabled = false;
        mainCam = Camera.main;
        player = GameObject.FindGameObjectWithTag("Player");
        lightController = lightBulb.GetComponent<HangingLightController>();
        ghostAudioSource = ghost.GetComponent<AudioSource>();
    }
	
	// Update is called once per frame
	void Update ()
    {
        if (!playScare && duration > WAIT_TIME)
        {
            playScare = true;
            StartCoroutine("Scare03");
        }

        // Rotate ghost to face player position
        if (playScare)
        {
            /*
            Vector3 relVec = ghost.transform.position - mainCam.transform.position;
            Vector3 targetVec = ghost.transform.position + relVec;
            ghost.transform.LookAt(targetVec);
            */
            Vector3 relVec = mainCam.transform.position - ghost.transform.position;
            ghost.transform.rotation = Quaternion.LookRotation(-relVec);
        }
    }

    void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Player") && !playScare)
        {
            duration += Time.deltaTime;
        }
    }

    IEnumerator Scare03()
    {
        Ray ray;
        RaycastHit hit;
        Collider ghostCollider = ghost.GetComponent<Collider>();
        float maxRayLength = 10.0f;

        // Spawn ghost if player is facing away and at least scareSpawnDistance away
        Vector3 backward;
        while (!spawnGhost)
        {
            backward = player.transform.TransformDirection(Vector3.back);
            Debug.DrawRay(player.transform.position, backward * scareSpawnDistance);
            ray = new Ray(player.transform.position, backward);
            // Physics.Raycast(player.transform.position, backward, out hit)
            if (ghostCollider.Raycast(ray, out hit, maxRayLength))
            {
                if (hit.collider.tag == "Ghost" && hit.distance > scareSpawnDistance)
                    spawnGhost = true;
            }

            yield return null;
        }

        // Enable ghost and turn light off if player is facing ghost and at least ghostSpawnDistance away
        Renderer ghostRenderer = ghost.GetComponent<Renderer>();
        while (!ghostRenderer.enabled)
        {
            // Vector3 fwd = mainCam.transform.TransformDirection(Vector3.forward);
            ray = mainCam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
            Debug.DrawRay(mainCam.transform.position, mainCam.transform.TransformDirection(Vector3.forward * ghostSpawnDistance));
            if (ghostCollider.Raycast(ray, out hit, maxRayLength))
            {
                if (hit.collider.tag == "Ghost" && hit.distance > ghostSpawnDistance)
                {
                    lightController.ShatterLight();

                    while (lightController.GetLightOn())
                        yield return null;

                    ghostRenderer.enabled = true;
                }
            }

            yield return null;
        }

        yield return new WaitForSeconds(0.5f);

        // Quickly move ghost towards player position while screaming
        ghostAudioSource.PlayOneShot(scream);
        float minDistFromGhostToPlayer = 1.0f;
        float ghostSpeed = 8.0f;
        Vector3 initialGhostPosition = ghost.transform.position;
        float distFromGhostToPlayer = Vector3.Distance(ghost.transform.position, player.transform.position);
        while (distFromGhostToPlayer > minDistFromGhostToPlayer)
        {
            ghost.transform.position = Vector3.MoveTowards(ghost.transform.position,
                player.transform.position, ghostSpeed * Time.deltaTime);
            distFromGhostToPlayer = Vector3.Distance(ghost.transform.position, player.transform.position);
            yield return null;
        }

        // Disable ghost, turn flashlight off, turn off access to flashlight, reset parameters
        flashlight.enabled = false;
        flashlight.GetComponent<CameraFollower>().KeepLightOff(true);
        ghostRenderer.enabled = false;

        // Wait to continue audioClip briefly in complete darkness
        yield return new WaitForSeconds(0.5f);

        // Stop audioClip
        if (ghostAudioSource.isPlaying)
            ghostAudioSource.Stop();

        // Reset ghost position
        ghost.transform.position = initialGhostPosition;

        yield return new WaitForSeconds(1.5f);

        // Play shakyBreathing at a random location relative to the player
        float randAngle = Random.Range(0.0f, 359.9f);
        Quaternion randRotation = Quaternion.AngleAxis(randAngle, Vector3.up);

        float maxBreathingDistance = 2.0f;
        float minBreathingDistance = 0.0f;
        audioSource.transform.position = mainCam.transform.TransformPoint(randRotation * Vector3.forward 
            * maxBreathingDistance);
        Vector3 relVec = audioSource.transform.position - mainCam.transform.position;

        // Set audioSource to shakyBreathing
        audioSource.clip = shakyBreathing;

        // Volume parameters
        float minVolume = 0.5f; // volume at maxBreathingDistance
        float maxVolume = audioSource.volume; // volume at minBreathingDistance
        audioSource.volume = minVolume;

        // Spatial blend parameters
        float startSpatial = 1.0f; // 3D spatial blend
        float endSpatial = 0.0f; // 2D spatial blend
        audioSource.spatialBlend = startSpatial;

        audioSource.Play();
        float startTime = Time.time;
        while (audioSource.isPlaying)
        {
            // Time ratio of current clip duration to total length of clip
            float t = (Time.time - startTime) / audioSource.clip.length;

            // Change audioSource volume
            audioSource.volume = Mathf.MoveTowards(minVolume, maxVolume, t);

            // Change audioSource spatial blend
            audioSource.spatialBlend = Mathf.MoveTowards(startSpatial, endSpatial, t);

            // Change audioSource position
            float currentDistance = Mathf.MoveTowards(maxBreathingDistance, minBreathingDistance, t);
            audioSource.transform.position = mainCam.transform.position + relVec.normalized * currentDistance;

            yield return null;
        }

        // Reset audioSource parameters
        audioSource.volume = maxVolume;
        audioSource.spatialBlend = startSpatial;

        yield return new WaitForSeconds(0.5f);

        // Disable FirstPersonController
        UnityStandardAssets.Characters.FirstPerson.FirstPersonController fpc
            = player.GetComponent<UnityStandardAssets.Characters.FirstPerson.FirstPersonController>();
        fpc.disableFPC = true;

        yield return null;

        // Move player to new spawn position
        player.transform.position = playerSpawnPosition.position;

        // Rotate camera and player to playerSpawnPosition which looks at bottom of stairs
        player.transform.rotation = playerSpawnPosition.rotation;
        // mainCam.transform.rotation = playerSpawnPosition.rotation;
        // flashlight.transform.rotation = playerSpawnPosition.rotation;

        // Reset all input
        // Input.ResetInputAxes();

        // Turn flashlight and flashlight access back on
        flashlight.enabled = true;
        flashlight.GetComponent<CameraFollower>().KeepLightOff(false);
        
        yield return null;
        yield return new WaitForSeconds(1.0f);

        // Enable FirstPersonController
        fpc.ResetRotation(player.transform, mainCam.transform);
        fpc.disableFPC = false;

        // If player nears top of basement stairs, slam door shut
        bool slamDoor = false;
        while (!slamDoor)
        {
            Vector3 relVectorToDoor = door.transform.position - player.transform.position;
            if (relVectorToDoor.magnitude < doorSlamDistance)
                slamDoor = true;
            yield return null;
        }

        DoorControllerAdvanced doorController = door.GetComponent<DoorControllerAdvanced>();

        // Slam Door
        doorController.SetSlamDoor();

        // Lock door
        doorController.SetLockDoor(true);

        // Wait between door slamming and locking to door knock
        yield return new WaitForSeconds(2.5f);

        // Play doorKnock from basement door AudioSource
        audioSource.clip = doorKnock;
        audioSource.transform.position = door.GetComponent<Renderer>().bounds.center;
        audioSource.Play();

        while (audioSource.isPlaying)
            yield return null;

        yield return new WaitForSeconds(0.5f);

        // Unlock door
        doorController.SetLockDoor(false);

        // Slightly open basement door
        doorController.SetSlightOpenDoor();

        yield return new WaitForSeconds(10.0f);

        // Turn basement lightbulb on
        lightController.SetLightOn(true);
        
        playScare = false;
        spawnGhost = false;
        duration = 0;
    }
}
