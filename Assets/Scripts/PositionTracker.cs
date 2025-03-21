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
        public int lapCount; 
        public bool isPlayer;
    }

    public List<RacerData> racers = new();
    public SplineContainer spline;
    public GameObject checkpoint; 

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
            lapCount = 0,
            isPlayer = isPlayer
        });
    }

    void UpdateRacers()
    {
        foreach (RacerData racer in racers)
        {
            racer.progress = GetClosestProgress(racer.racerTransform.position);
        }
    }

    //to be used by the checkpoint script attached to the checkpoint object
    public void OnCheckpointTrigger(Collider other)
    {
        foreach (RacerData racer in racers)
        {
            if (other.transform == racer.racerTransform)
            {
                racer.lapCount++;
                break;
            }
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
                if (racer.lapCount > racerToCheck.lapCount)
                {
                    position++;
                } 

                //if laps are equal then check progress along spline
                else if (racer.lapCount == racerToCheck.lapCount && racer.progress > racerToCheck.progress)
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
            if (racer.isPlayer)
            {
                int position = CalculatePosition(racer);
                UIManager.Instance.UpdatePositionDisplay(position, racer.lapCount);
                break;
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