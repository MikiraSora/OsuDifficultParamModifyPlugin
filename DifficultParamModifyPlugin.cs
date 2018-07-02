using OsuRTDataProvider;
using OsuRTDataProvider.Mods;
using Sync.Command;
using Sync.Plugins;
using Sync.Tools;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static OsuRTDataProvider.Mods.ModsInfo;

namespace DifficultParamModifyPlugin
{
    [SyncRequirePlugin(typeof(OsuRTDataProviderPlugin))]
    public class DifficultParamModifyPlugin : Plugin
    {
        OsuRTDataProviderPlugin ortdp_plugin;
        Logger<DifficultParamModifyPlugin> logger = new Logger<DifficultParamModifyPlugin>();
        static readonly string[] SUPPORT_PARAMS = new[] {"ar","od","cs","hp"};
        static readonly string[] SUPPORT_OSU_PARAMS = new[] { "ApproachRate", "OverallDifficulty", "CircleSize", "HPDrainRate" };
        private readonly HashSet<string> contain_mods = new HashSet<string>();

        Dictionary<string, float> modify_difficults = new Dictionary<string, float>(4);

        SourcesWrapper.ORTDP.RealtimeDataProvideWrapperBase source_wrapper;

        /// <summary>
        /// 是否已经修改，true的话就不用多次修改，false的话不用多次恢复
        /// </summary>
        bool is_modify = false;
        
        bool enable_modify = false;

        string current_osu_file;

        public DifficultParamModifyPlugin() : base("DifficultParamModifyPlugin", "DarkProjector")
        {
            EventBus.BindEvent<PluginEvents.LoadCompleteEvent>(OnPluginLoadComplete);

            EventBus.BindEvent<PluginEvents.InitCommandEvent>(e=>e.Commands.Dispatch.bind("modify_diff", HandleCommands, "修改难度值"));

            EventBus.BindEvent<PluginEvents.InitFilterEvent>(e => e.Filters.AddFilter(new Osu.IRCCommandFileter()));
        }

        #region Commands

        private bool HandleCommands(Arguments args)
        {
            if (args.Count == 0)
            {
                ShowHelp();
                return true;
            }

            if (args.Count == 2 && args[0] == "listen" && bool.TryParse(args[1], out bool sw))
            {
                ListenSwitch(sw);
                return true;
            }

            if (args.Count >= 2 && args[0] == "set")
            {
                args.RemoveAt(0);
                SetValue(args);
                return true;
            }

            return false;
        }

        private void ShowHelp()
        {

        }

        private void ListenSwitch(bool @switch)
        {
            if (@switch)
                StartModifyListen();
            else
                StopModifyListen();
        }

        private void SetValue(IEnumerable<string> vals)
        {
            string val = string.Empty;

            foreach (var p in vals)
            {
                val += p + ';';
            }

            SetValue(val);
        }

        #endregion

        private void StartModifyListen()
        {
            RestoreOsuFile(current_osu_file);
            current_osu_file = null;
            is_modify = false;
            enable_modify = true;
        }

        private void StopModifyListen()
        {
            RestoreOsuFile(current_osu_file);
            current_osu_file = null;
            is_modify = false;
            enable_modify = false;
        }

        public override void OnExit()
        {
            StopModifyListen();
        }

        private void OnPluginLoadComplete(PluginEvents.LoadCompleteEvent e)
        {
            ortdp_plugin = (from plugin in e.Host.EnumPluings() where plugin is OsuRTDataProviderPlugin select plugin as OsuRTDataProviderPlugin).FirstOrDefault();

            if (ortdp_plugin==null)
            {
                logger.LogInfomation($"找不到ORTDP插件，请输入命令\"plugins install provider\"并重启Sync");
                return;
            }

            source_wrapper = ortdp_plugin.ModsChangedAtListening ? new SourcesWrapper.ORTDP.RealtimeDataProviderModsWrapper(ortdp_plugin, this) : new SourcesWrapper.ORTDP.OsuRTDataProviderWrapper(ortdp_plugin, this);

            source_wrapper.OnTrigEvent += Source_wrapper_OnTrigEvent;

            source_wrapper.Attach();
        }

        private void RestoreOsuFile(string restore_target)
        {
            if (!is_modify)
                return;

            var restore_path = GetBackupFullName(restore_target);

            if (File.Exists(restore_path))
            {
                try
                {
                    File.Copy(restore_path, restore_target,true);
                    File.Delete(restore_path);

                    logger.LogInfomation($"恢复成功:{restore_target}");
                }
                catch (Exception e)
                {
                    logger.LogInfomation($"修改失败 {e.Message}:{restore_target}");
                }
                finally
                {
                    is_modify = false;
                }
            }
        }

        private static string GetBackupFullName(string modify_target)
        {
            if (!File.Exists(modify_target))
                return null;

            var file_info = new FileInfo(modify_target??string.Empty);
            var backup_file = Path.Combine(file_info.DirectoryName, $"_{file_info.Name}_");

            return backup_file;
        }

        private void ModifyOsuFile(string modify_target)
        {
            if (is_modify)
                return;

            var backup_file = GetBackupFullName(modify_target);

            try
            {
                File.Copy(modify_target, backup_file);

                using (var reader = new StreamReader(File.OpenRead(backup_file)))
                {
                    using (var writer = new StreamWriter(new FileStream(modify_target, FileMode.Truncate)))
                    {
                        while (!reader.EndOfStream)
                        {
                            var line = reader.ReadLine();

                            if (line == "[Difficulty]")
                            {
                                writer.WriteLine(line);

                                while (!string.IsNullOrWhiteSpace(line))
                                {
                                    line = reader.ReadLine();

                                    var data_arr = line.Trim().Split(':');

                                    if (data_arr.Length == 2)
                                    {
                                        for (int i = 0; i < SUPPORT_OSU_PARAMS.Length; i++)
                                        {
                                            if (SUPPORT_OSU_PARAMS[i]==data_arr[0]&&modify_difficults.ContainsKey(SUPPORT_PARAMS[i]))
                                            {
                                                contain_mods.Add(SUPPORT_PARAMS[i]);
                                                line = $"{SUPPORT_OSU_PARAMS[i]}:{modify_difficults[SUPPORT_PARAMS[i]]}";

                                                logger.LogInfomation($"modify value: {line}");
                                            }
                                        }
                                    }

                                    writer.WriteLine(line);
                                }

                                for (int i = 0; i < modify_difficults.Count; i++)
                                {
                                    var pair = modify_difficults.ElementAt(i);
                                    if (!contain_mods.Contains(pair.Key))
                                    {
                                        line = $"{SUPPORT_OSU_PARAMS[i]}:{pair.Value}";
                                        writer.WriteLine(line);


                                        logger.LogInfomation($"Add modify value: {line}");
                                    }
                                }

                                continue;
                            }

                            writer.WriteLine(line);
                        }
                    }
                }

                logger.LogInfomation($"修改完成:{modify_target}");
            }
            catch (Exception e)
            {
                logger.LogInfomation($"修改失败 {e.Message}:{modify_target}");
            }
            finally
            {
                is_modify = true;
            }
        }

        private void SetValue (string param)
        {
            var param_array = param.Replace(" ",string.Empty).ToLower().Split(';');

            foreach (var p in param_array)
            {
                var @params = p.Split(new[] { ':' },StringSplitOptions.RemoveEmptyEntries);
                if (@params.Length != 2)
                    continue;

                if (SUPPORT_PARAMS.Contains(@params[0]))
                {
                    if (float.TryParse(@params[1],out float value))
                    {
                        modify_difficults[@params[0]] = value;
                        logger.LogInfomation($"Modify {@params[0]}:{value}");
                    }
                    else
                    {
                        modify_difficults.Remove(@params[0]);
                        logger.LogInfomation($"Reser {@params[0]} to defualt");
                    }
                }
            }
        }

        #region Events Wrapper For OLSP

        public event Action<string, int, int, bool> OnBeatmapChanged;

        public ModsInfo CurrentMods { get => source_wrapper.current_mod; }
        
        private void Source_wrapper_OnTrigEvent(string osu_file, int set_id, int id, bool current_play)
        {
            OnChangedBeatmap(osu_file);
            OnBeatmapChanged?.Invoke(osu_file, set_id, id, current_play);
        }

        private void OnChangedBeatmap(string osu_file)
        {
            if (!enable_modify)
                return;

            if (osu_file != current_osu_file)
            {
                RestoreOsuFile(current_osu_file);
                ModifyOsuFile(osu_file);
            }

            current_osu_file = osu_file;
        }
        #endregion

        ~DifficultParamModifyPlugin()
        {
            StopModifyListen();
        }
    }
}
