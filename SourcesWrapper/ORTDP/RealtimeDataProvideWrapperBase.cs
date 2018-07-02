﻿using OsuRTDataProvider;
using OsuRTDataProvider.BeatmapInfo;
using OsuRTDataProvider.Mods;
using System;
using static OsuRTDataProvider.Listen.OsuListenerManager;

namespace DifficultParamModifyPlugin.SourcesWrapper.ORTDP
{
    internal abstract class RealtimeDataProvideWrapperBase : SourceWrapperBase<OsuRTDataProviderPlugin>
    {
        public ModsInfo current_mod;
        
        public event Action<string, int, int, bool> OnTrigEvent;

        protected int beatmapID, beatmapSetID;

        protected OsuStatus current_status;

        public string OsuFilePath;

        public RealtimeDataProvideWrapperBase(OsuRTDataProviderPlugin ref_plugin, DifficultParamModifyPlugin plugin) : base(ref_plugin, plugin)
        {
            
        }

        public void OnCurrentBeatmapChange(Beatmap beatmap)
        {
            if (beatmap == Beatmap.Empty || string.IsNullOrWhiteSpace(beatmap?.FilenameFull))
            {
                //fix empty beatmap
                return;
            }

            beatmapID = beatmap.BeatmapID;
            beatmapSetID = beatmap.BeatmapSetID;
            OsuFilePath = beatmap.FilenameFull;

            if (current_status == OsuStatus.Listening)
            {
                TrigListen();
            }
        }

        public void FireEvent(string path,int set_id,int id,bool output_type)
        {
            this.OnTrigEvent?.Invoke(path, set_id, id, output_type);
        }

        public abstract void OnCurrentModsChange(ModsInfo mod);

        /*
        protected BeatmapEntry GetCurrentBeatmap()
        {
            return new BeatmapEntry()
            {
                BeatmapId = beatmapID,
                BeatmapSetId = beatmapSetID,
                OsuFilePath = OsuFilePath
            };
        }
        */

        public abstract void OnStatusChange(OsuStatus last_status, OsuStatus status);

        protected void TrigListen()
        {
            CurrentOutputType = false;
            
            this.OnTrigEvent?.Invoke(OsuFilePath, this.beatmapSetID, this.beatmapID, CurrentOutputType);
        }

        public override void Detach()
        {
            RefPlugin.ListenerManager.OnBeatmapChanged -= OnCurrentBeatmapChange;
            RefPlugin.ListenerManager.OnStatusChanged -= OnStatusChange;
            RefPlugin.ListenerManager.OnModsChanged -= OnCurrentModsChange;
        }

        public override bool Attach()
        {
            RefPlugin.ListenerManager.OnBeatmapChanged += OnCurrentBeatmapChange;
            RefPlugin.ListenerManager.OnStatusChanged += OnStatusChange;
            RefPlugin.ListenerManager.OnModsChanged += OnCurrentModsChange;
            return true;
        }
    }
}