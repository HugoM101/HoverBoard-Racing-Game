using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Splines;
using System.Collections.Generic;

public class PositionTracker : MonoBehaviour
{
    public static PositionTracker Instance { get; private set; }

    [System.Serializable]
    public class RacerData
    {
        public Transform racerTransform;
        public float progress;
        public float lastLapProgress;
        public int lapCount;
        public bool isPlayer;
        public int currentPosition;
    }

    public List<RacerData> racers = new();
    public SplineContainer spline;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        
        else
        {
            Destroy(gameObject);
        }
        
    }

    void FixedUpdate()
    {
        UpdateRacers();
        UpdatePosition();
    }

    //add new racer to list
    public void RegisterRacer(Transform racer, bool isPlayer, float startProgress)
    {
        racers.Add(new RacerData 
        {
            racerTransform = racer,
            progress = startProgress,
            lapCount = Mathf.FloorToInt(startProgress), //0.97 start prog would equal lap 0 
            isPlayer = isPlayer,
            currentPosition = 1,
            lastLapProgress = startProgress % 1f
        });
    }

    void UpdateRacers()
    {
        foreach (RacerData racer in racers)
        {
            float currentLapProgress = GetClosestProgress(racer.racerTransform.position);
            
            //calculating how much progress has been made since the last frame
            float progressIncrement = currentLapProgress - racer.lastLapProgress;
            
            if (progressIncrement < -0.5f)
            {
                progressIncrement += 1f;
            }

            else if (progressIncrement > 0.5f) //this is for if the racer is reversing to stop "cheating"
            {
                progressIncrement -= 1f;
            }

            racer.progress += progressIncrement;
            racer.lastLapProgress = currentLapProgress;

            racer.lapCount = Mathf.FloorToInt(racer.progress);
        }
    }


    int CalculatePosition(RacerData racerToCheck)
    {
        int position = 1;
        foreach (RacerData racer in racers)
        {
            if (racer != racerToCheck)
            {
                //if another racer has more laps then they are ahead so increment the position
                if (racer.progress > racerToCheck.progress)
                {
                    position++;
                } 
            }
        }

        return position;
    }

    void UpdatePosition()
    {
      
        foreach (RacerData racer in racers)
        {
            racer.currentPosition = CalculatePosition(racer);
            if (racer.isPlayer)
            {
              UIManager.Instance.UpdatePositionDisplay(racer.currentPosition, racer.lapCount);
            }
        }
    }

    float GetClosestProgress(Vector3 racerPosition)
    {
        float closestProgress = 0f;
        float currentClosestDistance = 999f; //starting point
        int pointsSampledAlongSpline = 1000;

        //loop through each point 
        //from 0 up to the total points
        for (int i = 0; i <= pointsSampledAlongSpline; i++)
        {
            float t = i / (float)pointsSampledAlongSpline; //dividing loop num to a progress val between 0 and 1
            Vector3 pointOnSpline = spline.EvaluatePosition(t); //gives the position in gameworld of that progress (t)
            float distance = Vector3.Distance(pointOnSpline, racerPosition);

            if (distance < currentClosestDistance)
            {
                currentClosestDistance = distance; //updated to the new closer one
                closestProgress = t;
            }
        }

        return closestProgress;
    }
}