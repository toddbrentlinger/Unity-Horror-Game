using UnityEngine;
using System.Collections;

public class Scare02Controller : MonoBehaviour {

    [SerializeField] private AudioSource m_sound;
    [SerializeField] private const float WAIT_TIME = 15.0f;
    [SerializeField] private float ghostWalkDistance = 5.0f;

    private float duration;
    private bool playScare;
    private float playerPositionX;
    private float startPositionX;

	// Use this for initialization
	void Start ()
    {
        duration = 0;
        playScare = false;
        playerPositionX = transform.position.x;
        startPositionX = m_sound.transform.position.x;
	}
	
	// Update is called once per frame
	void Update ()
    {
        if (duration > WAIT_TIME && !playScare)
        {
            playScare = true;
            // StartCoroutine("Scare02");
            // StartCoroutine("SimpleScare02");
            StartCoroutine("AdvancedScare02");
        }
	}

    void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Player") && !playScare)
        {
            duration += Time.deltaTime;
            playerPositionX = other.transform.position.x;
        }
    }

    IEnumerator Scare02()
    {
        // Get size of box collider that triggers the scare
        /*
        Vector3 extents = GetComponent<Collider>().bounds.extents;

        if (playerPositionX > transform.position.x)
            startPositionX = playerPositionX - extents.x;
        else
            startPositionX = playerPositionX + extents.x;
        */
        float targetPositionX;
        if (playerPositionX > transform.position.x)
        {
            startPositionX = playerPositionX + ghostWalkDistance / 2;
            targetPositionX = startPositionX - ghostWalkDistance;
        }
        else
        {
            startPositionX = playerPositionX - ghostWalkDistance / 2;
            targetPositionX = startPositionX + ghostWalkDistance;
        }

        m_sound.transform.position = new Vector3(startPositionX, m_sound.transform.position.y,
            m_sound.transform.position.z);

        Vector3 target = new Vector3(targetPositionX, m_sound.transform.position.y,
            m_sound.transform.position.z);

        m_sound.Play();
        while (m_sound.isPlaying)
        {
            float clipLength = m_sound.clip.length;
            // float distance = extents.x;
            float distance = ghostWalkDistance;
            float speed = distance / clipLength;
            m_sound.transform.position = Vector3.MoveTowards(m_sound.transform.position,
                target, speed * Time.deltaTime);
            yield return null;
        }

        playScare = false;
        duration = 0;
    }

    IEnumerator SimpleScare02()
    {
        Transform player = GameObject.FindGameObjectWithTag("Player").transform;
        m_sound.transform.position = new Vector3(player.position.x, m_sound.transform.position.y,
            m_sound.transform.position.z);

        m_sound.Play();
        while (m_sound.isPlaying)
        {
            m_sound.transform.position = new Vector3(player.position.x, m_sound.transform.position.y,
                m_sound.transform.position.z);
            yield return null;
        }

        playScare = false;
        duration = 0;
    }

    IEnumerator AdvancedScare02()
    {
        Transform player = GameObject.FindGameObjectWithTag("Player").transform;

        if (playerPositionX > transform.position.x)
            startPositionX = playerPositionX - ghostWalkDistance / 2;
        else
            startPositionX = playerPositionX + ghostWalkDistance / 2;

        m_sound.transform.position = new Vector3(startPositionX, m_sound.transform.position.y,
            m_sound.transform.position.z);

        m_sound.Play();
        float targetPositionX;
        float startTime = Time.time;
        while (m_sound.isPlaying)
        {
            float t = (Time.time - startTime) / m_sound.clip.length;
            float targetDistance = Mathf.Lerp(0.0f, ghostWalkDistance, t);

            if (playerPositionX > transform.position.x)
                targetPositionX = player.position.x - ghostWalkDistance / 2 + targetDistance;
            else
                targetPositionX = player.position.x + ghostWalkDistance / 2 - targetDistance;

            m_sound.transform.position = new Vector3(targetPositionX, m_sound.transform.position.y, 
                m_sound.transform.position.z);

            yield return null;
        }

        playScare = false;
        duration = 0;
    }
}
