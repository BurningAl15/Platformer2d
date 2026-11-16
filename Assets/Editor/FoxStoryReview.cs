using System;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
public static class FoxStoryReview
{
    public static void Run()
    {
        var world=AssetDatabase.LoadAssetAtPath<WorldData>("Assets/Data/WorldData_World1.asset");
        int gems=0,clovers=0,fragments=0;
        for(int i=0;i<world.LevelCount;i++)
        {
            var scene=EditorSceneManager.OpenPreviewScene("Assets/Scenes/Levels/"+world.GetLevelScene(i)+".unity");
            try
            {
                var roots=scene.GetRootGameObjects();
                var managers=roots.SelectMany(r=>r.GetComponentsInChildren<LevelManager>(true)).ToArray();
                var manager=new SerializedObject(managers.Single());
                int count=roots.SelectMany(r=>r.GetComponentsInChildren<Pickup>(true)).Count(p=>p.IsGem());
                gems+=count;
                int c=roots.SelectMany(r=>r.GetComponentsInChildren<TrebolPickup>(true)).Count();
                int h=roots.SelectMany(r=>r.GetComponentsInChildren<HeartFragmentPickup>(true)).Count();
                clovers+=c;fragments+=h;
                bool bound=manager.FindProperty("currentWorldData").objectReferenceValue==world;
                bool panel=manager.FindProperty("worldCompleteController").objectReferenceValue!=null;
                Debug.Log($"FOX_STORY {scene.name} gems={count} clovers={c} fragments={h} world={bound} panel={panel}");
                if(!bound || (i==world.LevelCount-1 && !panel) || c!=1 || h!=(i<6?1:0))throw new Exception("Scene wiring: "+scene.name);
            }finally{ EditorSceneManager.ClosePreviewScene(scene); }
        }
        if(gems!=world.totalGems||clovers!=world.totalTrebols||world.MaxStars!=24)throw new Exception("World totals: "+gems+" "+clovers);
        Debug.Log($"FOX_STORY validated {world.LevelCount} levels, {gems} gems, {clovers} clovers and {fragments} heart fragments");
    }
}
