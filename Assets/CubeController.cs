using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Random = UnityEngine.Random;
using System.Collections;

public class CubeController : MonoBehaviour
{
    [SerializeField] private Slider slider;
    [SerializeField] private AudioClip moveAUD, matchAUD, matchHoverAUD;
    [SerializeField] private TextMeshProUGUI scoreTXT, highScoreTXT;
    [SerializeField] private Color hoverColor, colorStart100, colorEnd100, colorEnd1000, planeColor;
    [SerializeField] private Transform planeParent;
    [SerializeField] private GameObject cube, plane, gameOverText;
    [SerializeField] private List<Vector3> spawnPoints = new List<Vector3>();
    [SerializeField] private List<GameObject> cubes = new List<GameObject>();
    
    private AudioSource source;
    private Vector3 firstPos;
    private GameObject m_cube;
    private bool isPlanesSpawned, isMatched, isGameOver;
    private int score;

    private Cube matchCube = null, m_cubeComp = null;
    RaycastHit hit;

    //Oyun baþlamadan önce hareket pozisyonlarý belirlenir.
    private void Awake()
    {
        source = GetComponent<AudioSource>();
        GetVolume();
        SetSpawnPoints();
        GetHighScore();
    }

    private void SetSpawnPoints()
    {
        spawnPoints.Clear();

        for (int i = -2; i < 2; i++)
        {
            spawnPoints.Add(new Vector3(i, 0, -2));
            spawnPoints.Add(new Vector3(i, 0, -1));
            spawnPoints.Add(new Vector3(i, 0, 0));
            spawnPoints.Add(new Vector3(i, 0, 1));
        }

    }

    private void Start()
    {
        foreach (Vector3 point in spawnPoints)
        {
            if (!isPlanesSpawned)
            {
                Transform planeClone = Instantiate(plane, point, Quaternion.identity).transform;
                planeClone.parent = planeParent;
            }

        }

        isPlanesSpawned = true;
        SpawnCubes();
    }


    void Update()
    {
        if (Input.GetKeyUp(KeyCode.R))
            RestartGame();

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (!isGameOver) {
            if (Physics.Raycast(ray, out hit, Mathf.Infinity))
            {
                if (Input.GetMouseButtonDown(0))
                {
                    if (hit.collider.tag == "Cube")
                    {
                        if (m_cube == null)
                        {
                            m_cube = hit.collider.gameObject;
                            m_cubeComp = m_cube.GetComponent<Cube>();
                            StartCoroutine(m_cubeComp.HoverCor(m_cube, 1));
                            firstPos = m_cube.transform.position;
                            CheckDistance();
                        }
                    }
                }
                if (m_cube != null)
                {
                    Vector3 finalVector = new Vector3(Mathf.RoundToInt(hit.point.x), 0, Mathf.RoundToInt(hit.point.z));
                    float abs = Mathf.Abs((firstPos - finalVector).x) + Mathf.Abs((firstPos - finalVector).z);

                    if (Mathf.RoundToInt(hit.point.x) < 2f && Mathf.RoundToInt(hit.point.x) > -3f
                    && Mathf.RoundToInt(hit.point.z) < 2f && Mathf.RoundToInt(hit.point.z) > -3f
                    && abs <= 2)
                    {
                        Cube myCube = m_cube.GetComponent<Cube>();
                        int checkCubes = 0;

                        for (int i = 0; i < cubes.Count; i++)
                        {
                            if (cubes[i] != m_cube)
                            {
                                if (cubes[i].transform.position == finalVector)
                                {
                                    if (!isMatched)
                                        matchCube = cubes[i].GetComponent<Cube>();
                                }
                                else
                                    checkCubes++;
                            }
                        }

                        if (matchCube != null)
                            if (checkCubes == cubes.Count - 1)
                            {
                                if (matchCube.GetAnimState() == 1)
                                {
                                    matchCube.SetAnimState(2);
                                    m_cube.SetActive(true);
                                    matchCube.value = matchCube.value / 2;
                                    matchCube.text.text = "" + matchCube.value;
                                }

                                StartCoroutine(matchCube.HoverCor(matchCube.gameObject, 0));
                                matchCube = null;
                            }

                        if (matchCube == null)
                        {
                            m_cube.transform.position = finalVector;
                            isMatched = false;
                        }
                        else
                        {
                            // 2 Küp eþlenebilirse ama henüz fare býrakýlmadýysa
                            if (matchCube.value == myCube.value)
                            {
                                matchCube.SetAnimState(1);
                                matchCube.value = matchCube.value * 2;
                                matchCube.text.text = "" + matchCube.value;
                                source.PlayOneShot(matchHoverAUD);
                                StartCoroutine(matchCube.HoverCor(matchCube.gameObject, 1));
                                m_cube.transform.position = finalVector;
                                m_cube.SetActive(false);
                                UpdateScore(matchCube.value);
                                isMatched = true;
                            }
                        }
                    }
                }
                if (Input.GetMouseButtonUp(0))
                {
                    if (m_cube != null)
                    {
                        if (firstPos != m_cube.transform.position)

                        {
                            if (!isMatched)
                            {
                                spawnPoints.Remove(m_cube.transform.position);
                                spawnPoints.Add(firstPos);
                                StartCoroutine(m_cubeComp.HoverCor(m_cube, 0));
                            }
                            else
                            {
                                if (matchCube.GetAnimState() == 1)
                                    matchCube.SetAnimState(2);

                                source.PlayOneShot(matchAUD);
                                StartCoroutine(matchCube.HoverCor(matchCube.gameObject, 0));
                                setCubeChanges(matchCube);
                                spawnPoints.Add(firstPos);
                                cubes.Remove(m_cube);
                                matchCube = null;
                                Destroy(m_cube);
                            }
                            SpawnCubes();
                            source.PlayOneShot(moveAUD);
                        }
                        else
                            StartCoroutine(m_cubeComp.HoverCor(m_cube, 0));
                        m_cubeComp = null;
                        m_cube = null;
                    }

                    for (int i = 0; i < planeParent.childCount; i++)
                        planeParent.GetChild(i).GetComponent<Renderer>().material.color = planeColor;
                    for (int i = 0; i < cubes.Count; i++)
                        cubes[i].GetComponent<Cube>().text.text = "" + cubes[i].GetComponent<Cube>().value;

                }

            }
        }
    }

    private void GetVolume()
    {
        slider.value = PlayerPrefs.GetFloat("volume", 0.5f);
        AudioListener.volume = slider.value;
    }

    public void SetVolume()
    {
        PlayerPrefs.SetFloat("volume", slider.value);
        AudioListener.volume = slider.value;
    }

    //Skor yazýsýný güncelle.
    private void UpdateScore(int value)
    {
        score += value;
        scoreTXT.text = "" + score;

        if (score > GetHighScore())
            SetHighScore(score);
    }

    //Küpün hareket mesafesi 2 birimdir. Bu yüzden küp seçildiði zaman gidebileceði yerlerin rengi burada deðiþir.
    private void CheckDistance()
    {
        for (int i = 0; i < planeParent.childCount; i++)
        {
            float abs = Mathf.Abs((firstPos - planeParent.GetChild(i).position).x) + Mathf.Abs((firstPos - planeParent.GetChild(i).position).z);
            Color color = m_cubeComp.value < 150 ? Color.Lerp(colorStart100, colorEnd100, m_cubeComp.value / 100f) : Color.Lerp(colorEnd100, colorEnd1000, m_cubeComp.value / 1000f);

            if (abs <= 2)
                planeParent.GetChild(i).GetComponent<Renderer>().material.color = color;

            else
                planeParent.GetChild(i).GetComponent<Renderer>().material.color = Color.gray;
        }

        for (int i = 0; i < cubes.Count; i++)
        {
            float abs = Mathf.Abs((firstPos - cubes[i].transform.position).x) + Mathf.Abs((firstPos - cubes[i].transform.position).z);
            Cube cube = cubes[i].GetComponent<Cube>();

            if (abs > 2)
                cube.text.text = "-";
            else
                cube.text.text = "" + cube.value;
        }

    }

    //Her hareket sonrasý 1 adet küpün spawn iþlemi burada gerçekleþir.  
    private void SpawnCubes()
    {
        if (spawnPoints.Count > 1) {
            int random = Random.Range(0, spawnPoints.Count);
            Cube newCube = Instantiate(cube, spawnPoints[random], Quaternion.identity).GetComponent<Cube>();
            newCube.value = 4;

            setCubeChanges(newCube);
            spawnPoints.RemoveAt(random);
            cubes.Add(newCube.gameObject);

            //newCube.transform.parent = cubeParent;
        }
        else
        {
            Debug.Log("Game Over!");
            scoreTXT.transform.parent.gameObject.SetActive(false);
            gameOverText.SetActive(true);
            isGameOver = true;

        }
    }

    private void RestartGame()
    {
        scoreTXT.transform.parent.gameObject.SetActive(true);
        gameOverText.SetActive(false);

        score = 0;
        scoreTXT.text = "" + 0;

        for (int i = 0; i < cubes.Count; i++)
            Destroy(cubes[i]);

        isGameOver = false;
        cubes.Clear();
        SetSpawnPoints();
        SpawnCubes();

    }

    //Küpler birleþtiklerinde rengi ve yazý deðeri deðiþir.
    private void setCubeChanges(Cube cube)
    {
        Color color = cube.value < 150 ? Color.Lerp(colorStart100, colorEnd100, cube.value / 100f) : Color.Lerp(colorEnd100, colorEnd1000, cube.value / 1000f);
        Color aColor = color;
        aColor.r = aColor.r - 0.3f;
        Material mat = cube.gameObject.GetComponent<Renderer>().material;
        mat.SetColor("_firstColor", color);
        mat.SetColor("_secondColor", aColor);
        cube.text.text = "" + cube.value;
    }

    // High score belirle.
    private void SetHighScore(int value)
    {
        PlayerPrefs.SetInt("high_score", value);
        GetHighScore();
    }
    private int GetHighScore() {
        int val = PlayerPrefs.GetInt("high_score");
        highScoreTXT.text = "High Score: " + val;
        return val;
    }
}

