using OsuRTDataProvider;
using OsuRTDataProvider.Mods;
using static OsuRTDataProvider.Listen.OsuListenerManager;

namespace DifficultParamModifyPlugin.SourcesWrapper.ORTDP
{
    /// <summary>
    /// 支持选图界面获取Mod的版本
    /// </summary>
    internal class RealtimeDataProviderModsWrapper : OsuRTDataProviderWrapper
    {
        public RealtimeDataProviderModsWrapper(OsuRTDataProviderPlugin ref_plugin, DifficultParamModifyPlugin plugin) : base(ref_plugin, plugin)
        {
        }

        public override void OnCurrentModsChange(ModsInfo mod)
        {
            if (current_mod == mod)
                return;
            current_mod = mod;

            if (CurrentOutputType)
                return;

            //选图界面改变Mods会输出

            this.FireEvent(OsuFilePath, this.beatmapSetID, this.beatmapID, CurrentOutputType);
        }

        public override void OnStatusChange(OsuStatus last_status, OsuStatus status)
        {
            current_status = status;

            if (last_status == status) return;
            if ((status != OsuStatus.Playing) && (status != OsuStatus.Rank))
            {
                if (status == OsuStatus.Listening)
                {
                    TrigListen();
                }
                else
                {
                    FireEvent(null, 0, 0, false);
                }
            }
            else
            {
                CurrentOutputType = true;
                FireEvent(OsuFilePath, this.beatmapSetID, this.beatmapID, CurrentOutputType);
            }
        }
    }
}