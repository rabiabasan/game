using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;

public class PlayAllClips : MonoBehaviour
{
    [Header("Klipleri buraya sürükle-bırak")]
    public List<AnimationClip> clips = new List<AnimationClip>();

    [Header("Ayarlar")]
    public bool loopPlaylist = true;
    public float extraGapSeconds = 0.05f; // klipler arasında minicik boşluk

    PlayableGraph graph;
    AnimationMixerPlayable mixer;
    AnimationPlayableOutput output;

    int index = 0;
    double clipEndTime = 0;

    void OnEnable()
    {
        if (clips == null || clips.Count == 0)
        {
            Debug.LogWarning("Klip listesi boş. Inspector'da klipleri ekle.");
            return;
        }

        graph = PlayableGraph.Create("ClipPlaylistGraph");
        graph.SetTimeUpdateMode(DirectorUpdateMode.GameTime);

        var animator = GetComponent<Animator>();
        output = AnimationPlayableOutput.Create(graph, "AnimOutput", animator);

        mixer = AnimationMixerPlayable.Create(graph, 1);
        output.SetSourcePlayable(mixer);

        PlayClip(0);
        graph.Play();
    }

    void Update()
    {
        if (!graph.IsValid() || clips.Count == 0) return;

        // klip bitince bir sonrakine geç
        if (graph.GetRootPlayable(0).GetTime() >= clipEndTime)
        {
            int next = index + 1;
            if (next >= clips.Count)
            {
                if (!loopPlaylist) { enabled = false; return; }
                next = 0;
            }
            PlayClip(next);
        }
    }

    void PlayClip(int i)
    {
        index = i;

        // Mixer girişini yeniden kur
        mixer.SetInputCount(1);
        var clipPlayable = AnimationClipPlayable.Create(graph, clips[index]);
        clipPlayable.SetApplyFootIK(true);

        mixer.ConnectInput(0, clipPlayable, 0, 1f);

        // süre hesabı
        double now = graph.GetRootPlayable(0).GetTime();
        clipEndTime = now + clips[index].length + extraGapSeconds;

        // klibi baştan başlat
        clipPlayable.SetTime(0);
    }

    void OnDisable()
    {
        if (graph.IsValid()) graph.Destroy();
    }
}