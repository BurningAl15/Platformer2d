using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LSPlayerController : MonoBehaviour
{
    public MapPoint currentPoint;

    public float moveSpeed = 10f;
    
    void Start()
    {
        
    }

    public void SetCurrentPoint(MapPoint _currentMapPoint)
    {
        currentPoint = _currentMapPoint;
        transform.position = currentPoint.transform.position;
    }
    
    void Update()
    {
        transform.position = Vector3.MoveTowards(transform.position, currentPoint.transform.position,
            moveSpeed * Time.deltaTime);

        float currentDistance = Vector3.Distance(transform.position, currentPoint.transform.position);
        if (currentDistance< .025f)
        {
            if (Input.GetAxisRaw("Horizontal") > .5f)
            {
                if (currentPoint.right != null)
                    SetNextPoint(currentPoint.right);
            }
            else if (Input.GetAxisRaw("Horizontal") < -.5f)
            {
                if (currentPoint.left != null)
                    SetNextPoint(currentPoint.left);
            }
            else if (Input.GetAxisRaw("Vertical") > .5f)
            {
                if (currentPoint.up != null)
                    SetNextPoint(currentPoint.up);
            }
            else if (Input.GetAxisRaw("Vertical") < -.5f)
            {
                if (currentPoint.down != null)
                    SetNextPoint(currentPoint.down);
            }
            if (currentDistance<=0.01f)
            {
                if (currentPoint.isLevel && !currentPoint.isLocked)
                {
                    UI_SelectController._instance.Turn_On_Off(true);
                    UI_SelectController._instance.RenderLevelData(currentPoint.levelName, currentPoint.gemsCollected,
                        currentPoint.totalGems, currentPoint.bestTime, currentPoint.targetTime);
    
                    if (Input.GetButtonDown("Jump"))
                    {
                        LevelSelectManager._instance.Loading_GameplayScene(currentPoint.currentLevel);
                    }
                }
            }
        } 
    }


    
    void SetNextPoint(MapPoint nextPoint)
    {
        currentPoint = nextPoint;
        UI_SelectController._instance.Turn_On_Off(false);
    }
}
