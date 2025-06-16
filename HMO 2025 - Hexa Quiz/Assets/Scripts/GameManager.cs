using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.SocialPlatforms.Impl;
using UnityEngine.UI;
using UnityEngine.XR;

public class GameManager : MonoBehaviour
{
    MainDB db = new MainDB();
    //db indexes >> question number - Key, list[0] - question, list[1] - option1, list[2] - option2, list[3] - option3, list[4] - option4, list[5] - right answer index(string, do a int.parse)

    [Header("Panels")]
    public GameObject MenuPanel;
    public GameObject QuizPanel;
    public GameObject GOPanel;

    [Header("GameObjects")]
    public GameObject[] optionsBG;
    public GameObject nextButton;
    public GameObject timerBG;
    public GameObject questionBG;
    public Image[] stageLines;
    

    [Header("TextFields")]
    public TMP_Text question;
    public TMP_Text[] optionsText;
    public TMP_Text generateButtonText;
    public TMP_Text timerText;
    public TMP_Text notificationText;
    public TMP_Text scoreText;
    public TMP_Text finishText;

    [Header("Audio")]
    public AudioSource bgmAS;
    public AudioSource sfxAS;
    public AudioClip bgm;
    public AudioClip[] sfx;


    [Header("Variables")]
    [SerializeField] int timer = 20;
    [SerializeField] int questionPerPlayer = 6;
    [SerializeField] float finishPanelTime = 5f;
    
    private bool hasAnswered = false;
    private int currentCorrect = -1;
    //private int playerPos = -2;
    private bool quizOn = false;
    private bool menuOn = true;
    private bool timeUp = false;
    private bool TestingOn = false;
    //public int timeBonus = 0;
    
    //private int score = 0;
    private int currentQuestion = 0;

    private string[] bhaloKotha = { "That's Correct! Keep it up!", "That’s right! Well done!", "Correct! Great job!" };
    private string[] kharapKotha = { "Incorrect. Try the next one!", "Wrong answer. Don’t give up!"};

    private List<int> selectedQuestions = new List<int>();

    [Header("Colors")]
    //Color32 bluee = new Color32(15, 0, 128, 255);
    //Color32 offWhite = new Color32(255, 236, 212, 255);
    //Color32 orangeColor = new Color32(244, 111, 33, 255);
    Color32 whiteColor = new Color32(255, 255, 255, 255);

    private void Start()
    {
        MenuPanel.SetActive(true);
        QuizPanel.SetActive(false);
        GOPanel.SetActive(false);


        bgmAS.clip = bgm;
        bgmAS.loop = true;
        bgmAS.Play();
        menuOn = true;
    }

    private void Update()
    {
        if(!menuOn && Input.GetKeyDown(KeyCode.R))
        {
            RestartScene();
        }
        if(Input.GetKeyDown(KeyCode.Escape))
        {
            Application.Quit();
        }
        /*if (Input.GetKeyDown(KeyCode.T))
        {
            if(!TestingOn)
            {
                TestingOn = true;
                StartCoroutine(TestingQuestions());
            }
        }*/
    }

    public void GenerateQuestion()
    {        
        QuestionOptionAnimationReset();
        hasAnswered = false;
        timeUp = false;
        StopAllCoroutines();
        if(currentQuestion < questionPerPlayer)
        {
            currentQuestion++;            
            stageLines[currentQuestion - 1].color = whiteColor;
        }
        else if(currentQuestion == questionPerPlayer)
        {
            StartCoroutine(GameFinish());
            return;
        }
        ResetOptions();
        nextButton.SetActive(false);
        
        int randInt = Random.Range(0, db.table.Count);
        while(selectedQuestions.Contains(randInt))
        {
            randInt = Random.Range(0, db.table.Count);
        }
        selectedQuestions.Add(randInt);
        //generateButtonText.text = randInt.ToString();
        question.text = db.GetValueFromDB(randInt, 0);
        for(int i = 0; i < optionsText.Length; i++)
        {
            optionsText[i].text = db.GetValueFromDB(randInt, i+1);
        }
        currentCorrect = int.Parse(db.GetValueFromDB(randInt, 5));
        notificationText.text = "";
        ResetTimer();
        QuestionOptionAnimation();
    }

    public void OnButtonPress(int buttonId)
    {
        //timeBonus = timer;
        StopAllCoroutines();
        quizOn = false;
        if(!hasAnswered)
        {
            optionsBG[buttonId].transform.GetChild(5).gameObject.SetActive(true);
            if (currentCorrect == buttonId)
            {
                //score = score + 10 + timeBonus;
                notificationText.text = bhaloKotha[Random.Range(0, bhaloKotha.Length)];                
                //scoreText.text = score.ToString();
                optionsBG[buttonId].transform.GetChild(0).gameObject.SetActive(true);
                PlaySFX(0);
            }
            else
            {
                notificationText.text = kharapKotha[Random.Range(0, kharapKotha.Length)];
                //score += 5;
                //scoreText.text = score.ToString();
                optionsBG[buttonId].transform.GetChild(1).gameObject.SetActive(true);
                //optionsText[buttonId].color = offWhitee;
                PaintOptions(currentCorrect);
                PlaySFX(1);
            }            
            nextButton.SetActive(true);
            timerBG.SetActive(false);
        }
        hasAnswered = true;
    }

    public void PaintOptions(int correctOption)
    {
        for (int i = 0; i < optionsBG.Length; i++)
        {
            if(i == correctOption)
            {
                if(!timeUp)
                {
                    optionsBG[i].transform.GetChild(0).gameObject.SetActive(true);
                }
                else
                {
                    optionsBG[i].transform.GetChild(2).gameObject.SetActive(true);
                }
                return;
            }
            /*else
            {
                optionsBG[i].transform.GetChild(1).gameObject.SetActive(true);
            }*/
        }
    }
    public void ResetOptions()
    {
        for (int i = 0; i < optionsBG.Length; i++)
        {
            optionsBG[i].transform.GetChild(0).gameObject.SetActive(false);
            optionsBG[i].transform.GetChild(1).gameObject.SetActive(false);
            optionsBG[i].transform.GetChild(2).gameObject.SetActive(false);
            optionsBG[i].transform.GetChild(5).gameObject.SetActive(false);
            //optionsText[i].color = bluee;
        }
    }

    public void PlayGameButton()
    {
        MenuPanel.SetActive(false);
        QuizPanel.SetActive(true);
        GOPanel.SetActive(false);
        GenerateQuestion();
        quizOn = true;       
    }

    IEnumerator Timer()
    {
        yield return new WaitForSeconds(1f);
        timer--;
        timerText.text = "Time Left\n" + timer.ToString();
        if(quizOn && timer > 0)
        {
            StartCoroutine(Timer());
        }
        else
        {
            StopCoroutine(Timer());
            timeUp = true;
            quizOn = false;
            hasAnswered = true;
            nextButton.SetActive(true);
            PaintOptions(currentCorrect);
            timer = 20;
            timerBG.SetActive(false);
            notificationText.text = "Time's Up!";
        }
    }

    public void ResetTimer()
    {
        quizOn = true;
        hasAnswered = false;
        timer = 20;
        timerBG.SetActive(true);
        timerText.text = "Time Left\n" + timer.ToString();
        StartCoroutine(Timer());
    }

    IEnumerator GameFinish()
    {
        MenuPanel.SetActive(false);
        QuizPanel.SetActive(false);
        GOPanel.SetActive(true);
        finishText.text = "Thanks for playing!";
        yield return new WaitForSeconds(finishPanelTime);
        RestartScene();
    }


    public void RestartScene()
    {
        SceneManager.LoadScene("Hexa Quiz");
    }

    public void QuestionOptionAnimation()
    {
        Sequence mySequence = DOTween.Sequence();
        mySequence.Append(questionBG.transform.DOMoveY(960+200, 0.3f));
        mySequence.Append(optionsBG[0].transform.DOMoveX(300, 0.2f));
        mySequence.Append(optionsBG[1].transform.DOMoveX(780, 0.2f));
        mySequence.Append(optionsBG[2].transform.DOMoveX(300, 0.2f));
        mySequence.Append(optionsBG[3].transform.DOMoveX(780, 0.2f));
    }

    public void QuestionOptionAnimationReset()
    {
        questionBG.transform.position = new Vector3(questionBG.transform.position.x, 960+1500f, questionBG.transform.position.z);
        optionsBG[0].transform.position = new Vector3(-450f, optionsBG[0].transform.position.y, optionsBG[0].transform.position.z);
        optionsBG[1].transform.position = new Vector3(1450, optionsBG[1].transform.position.y, optionsBG[1].transform.position.z);
        optionsBG[2].transform.position = new Vector3(-450f, optionsBG[2].transform.position.y, optionsBG[2].transform.position.z);
        optionsBG[3].transform.position = new Vector3(1450, optionsBG[3].transform.position.y, optionsBG[3].transform.position.z);
    }

    public void PlaySFX(int index)
    {
        sfxAS.clip = sfx[index];
        sfxAS.Play();
    }
    
}
