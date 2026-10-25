using UnityEngine;

public enum LevelRating
{
    None = 0,
    OneStar = 1,
    TwoStars = 2,
    ThreeStars = 3
}

public class LevelRatingSystem : MonoBehaviour
{
    [System.Serializable]
    public class LevelRequirements
    {
        public float targetTime;
        public int totalGems;
    }
    
    public LevelRating CalculateRating(float time, int gems, LevelRequirements req)
    {
        int stars = 1;
        
        if (gems >= req.totalGems)
            stars++;
            
        if (time <= req.targetTime)
            stars++;
            
        return (LevelRating)stars;
    }
}
