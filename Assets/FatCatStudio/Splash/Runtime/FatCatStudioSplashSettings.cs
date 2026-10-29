using UnityEngine;

namespace FatCatStudio.Splash
{
    [CreateAssetMenu(
        fileName = "FatCatStudioSplashSettings",
        menuName = "FatCat Studio/Splash/Splash Settings")]
    public sealed class FatCatStudioSplashSettings : ScriptableObject
    {
        public enum SplashStyle
        {
            LIGHT,DARK
        }
        
        [SerializeField] private SplashStyle splashStyle = SplashStyle.LIGHT;
        
        [Header("Visual")]
        [SerializeField] private Color backgroundColor = Color.black;
        [SerializeField] private Color textColor = Color.white;
        [SerializeField] private Texture logo;

        public Color BackgroundColor => backgroundColor;
        public Color TextColor => textColor;
        public Texture Logo => logo;
        
        public SplashStyle SplashStyleEnum => splashStyle;
    }
}