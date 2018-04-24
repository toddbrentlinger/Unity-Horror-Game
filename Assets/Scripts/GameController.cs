using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;

public class GameController : MonoBehaviour {

    public Text cursorGUI;
    public Text actionPromptText;
    public Text titleText;
    public GameObject introCamera;

    private GameObject player;
    private UnityStandardAssets.Characters.FirstPerson.FirstPersonController fpc;
    private Camera mainCamera;
    private Animator introCameraAnimator;

    private bool gameOver;
    private bool restart;

    private LightmapData[] initialLightmapData;
    private Color initialAmbientLight;
    // private int lightMapIndex;
    private bool isLightsOn = true;

	// Use this for initialization
	void Start ()
    {
        // Enable cursor
        cursorGUI.enabled = true;

        // Set actionPrompt to default blank text
        actionPromptText.text = "";

        player = GameObject.FindGameObjectWithTag("Player");
        fpc = player.GetComponent<UnityStandardAssets.Characters.FirstPerson.FirstPersonController>();
        mainCamera = Camera.main;
        introCameraAnimator = introCamera.GetComponent<Animator>();

        gameOver = false;
        restart = false;

        initialLightmapData = LightmapSettings.lightmaps;
        initialAmbientLight = RenderSettings.ambientLight;

        // Lock cursor
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        StartCoroutine("Intro");
    }
	
	// Update is called once per frame
	void Update ()
    {
        if (Input.GetKeyDown(KeyCode.L))
        {
            // Toggle isLightsOn
            isLightsOn = !isLightsOn;

            // Set lightmapData
            LightmapSettings.lightmaps = isLightsOn ? initialLightmapData : new LightmapData[] {};

            // Set ambientLight
            RenderSettings.ambientLight = isLightsOn ? initialAmbientLight : Color.black;

            // Set lightSource on/off
            GameObject[] allLights = GameObject.FindGameObjectsWithTag("LightSource");
            foreach (GameObject lightSource in allLights)
                lightSource.GetComponent<Light>().enabled = isLightsOn;
        }

        if (!gameOver)
            return;

        if (gameOver && restart)
        {
            if (Input.GetKeyDown(KeyCode.R))
                SceneManager.LoadScene("Haunted House");
        }
	}

    IEnumerator Intro()
    {
        // Disable FirstPersonController
        fpc.disableFPC = true;

        // Activate introCamera
        introCamera.SetActive(true);

        // Activate title screen
        titleText.gameObject.SetActive(true);

        yield return new WaitForSeconds(3.0f);

        // Disable titleScreen
        titleText.gameObject.SetActive(false);

        yield return new WaitForSeconds(0.5f);

        // Play introCamera animation
        introCameraAnimator.Play("CameraIntro");

        // Wait for length of animation
        yield return new WaitForSeconds(6.0f);

        // Deactivate introCamera
        introCamera.SetActive(false);

        // Activate mainCamera
        mainCamera.gameObject.SetActive(true);

        // Enable FirstPersonController
        fpc.disableFPC = false;
    }

    void DisplayActionPrompt(string objectName)
    {
        actionPromptText.text = objectName;
    }
}
